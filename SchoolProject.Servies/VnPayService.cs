using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SchoolProject.Models;
using SchoolProject.Repositories;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SchoolProject.Servies
{
    public class VnPayService : IVnPayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly VnPaySettings _settings;

        public VnPayService(
            IUnitOfWork unitOfWork,
            IOptions<VnPaySettings> options)
        {
            _unitOfWork = unitOfWork;
            _settings = options.Value;
        }

        // =========================================================
        // CREATE PAYMENT URL
        // =========================================================

        public async Task<string> CreatePaymentUrl(
            string userId,
            int feeId,
            string ipAddress)
        {
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(x => x.KeyId == userId);

            if (student == null)
                throw new Exception(
                    "Không tìm thấy Student của tài khoản");

            var fee = await _unitOfWork
                .GenericRepository<Fee>()
                .GetByIdAsync(x =>
                    x.Id == feeId &&
                    x.StudentId == student.Id);

            if (fee == null)
                throw new Exception(
                    "Không tìm thấy khoản học phí");

            if (fee.Status == "Paid")
                throw new Exception(
                    "Khoản học phí này đã được thanh toán");

            if (fee.Amount <= 0)
                throw new Exception(
                    "Số tiền học phí không hợp lệ");

            // Tạo mã giao dịch duy nhất
            var txnRef =
                $"{fee.Id}_{DateTime.Now:yyyyMMddHHmmssfff}";

            var payment = new VnPayPayment
            {
                FeeId = fee.Id,
                StudentId = student.Id,
                TxnRef = txnRef,
                Amount = fee.Amount,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            await _unitOfWork
                .GenericRepository<VnPayPayment>()
                .AddAsync(payment);

            _unitOfWork.Save();

            var createDate =
                DateTime.Now.ToString(
                    "yyyyMMddHHmmss");

            var expireDate =
                DateTime.Now
                    .AddMinutes(15)
                    .ToString("yyyyMMddHHmmss");

            var amount =
                ((long)(fee.Amount * 100))
                .ToString();

            var vnpParams =
                new SortedDictionary<string, string>
                {
                    ["vnp_Version"] = "2.1.0",
                    ["vnp_Command"] = "pay",
                    ["vnp_TmnCode"] = _settings.TmnCode,
                    ["vnp_Amount"] = amount,
                    ["vnp_CurrCode"] = "VND",
                    ["vnp_TxnRef"] = txnRef,
                    ["vnp_OrderInfo"] =
                        $"Thanh toan hoc phi Fee {fee.Id}",
                    ["vnp_OrderType"] = "other",
                    ["vnp_Locale"] = "vn",
                    ["vnp_ReturnUrl"] = _settings.ReturnUrl,
                    ["vnp_IpAddr"] =
                        string.IsNullOrWhiteSpace(ipAddress)
                            ? "127.0.0.1"
                            : ipAddress,
                    ["vnp_CreateDate"] = createDate,
                    ["vnp_ExpireDate"] = expireDate
                };

            var queryString =
                string.Join(
                    "&",
                    vnpParams.Select(x =>
                        $"{WebUtility.UrlEncode(x.Key)}=" +
                        $"{WebUtility.UrlEncode(x.Value)}"));

            var secureHash =
                HmacSha512(
                    _settings.HashSecret,
                    queryString);

            return
                $"{_settings.PaymentUrl}?" +
                $"{queryString}" +
                $"&vnp_SecureHash={secureHash}";
        }

        // =========================================================
        // RETURN
        // =========================================================

        public async Task<(bool Success, int FeeId, string Message)>
            ProcessReturn(IQueryCollection query)
        {
            var result =
                ValidateResponse(query);

            if (!result.Valid)
            {
                return (
                    false,
                    0,
                    "Checksum không hợp lệ");
            }

            var txnRef =
                query["vnp_TxnRef"].ToString();

            var responseCode =
                query["vnp_ResponseCode"].ToString();

            var transactionStatus =
                query["vnp_TransactionStatus"].ToString();

            var payment =
                await _unitOfWork
                    .GenericRepository<VnPayPayment>()
                    .GetByIdAsync(x =>
                        x.TxnRef == txnRef);

            if (payment == null)
            {
                return (
                    false,
                    0,
                    "Không tìm thấy giao dịch");
            }

            if (responseCode == "00" &&
                transactionStatus == "00")
            {
                await MarkPaymentSuccess(
                    payment,
                    query);

                return (
                    true,
                    payment.FeeId,
                    "Thanh toán thành công");
            }

            payment.Status = "Failed";

            _unitOfWork
                .GenericRepository<VnPayPayment>()
                .Update(payment);

            _unitOfWork.Save();

            return (
                false,
                payment.FeeId,
                $"Thanh toán thất bại. " +
                $"ResponseCode: {responseCode}");
        }

        // =========================================================
        // IPN
        // =========================================================

        public async Task<(bool Success, string Message)>
            ProcessIpn(IQueryCollection query)
        {
            var result =
                ValidateResponse(query);

            if (!result.Valid)
            {
                return (
                    false,
                    "Invalid checksum");
            }

            var txnRef =
                query["vnp_TxnRef"].ToString();

            var responseCode =
                query["vnp_ResponseCode"].ToString();

            var transactionStatus =
                query["vnp_TransactionStatus"].ToString();

            var payment =
                await _unitOfWork
                    .GenericRepository<VnPayPayment>()
                    .GetByIdAsync(x =>
                        x.TxnRef == txnRef);

            if (payment == null)
            {
                return (
                    false,
                    "Payment not found");
            }

            // Đã xử lý rồi
            if (payment.Status == "Paid")
            {
                return (
                    true,
                    "Already confirmed");
            }

            if (responseCode == "00" &&
                transactionStatus == "00")
            {
                await MarkPaymentSuccess(
                    payment,
                    query);

                return (
                    true,
                    "Confirm Success");
            }

            payment.Status = "Failed";

            _unitOfWork
                .GenericRepository<VnPayPayment>()
                .Update(payment);

            _unitOfWork.Save();

            return (
                true,
                "Payment failed");
        }

        // =========================================================
        // MARK SUCCESS
        // =========================================================

        private async Task MarkPaymentSuccess(
            VnPayPayment payment,
            IQueryCollection query)
        {
            if (payment.Status == "Paid")
                return;

            var fee =
                await _unitOfWork
                    .GenericRepository<Fee>()
                    .GetByIdAsync(x =>
                        x.Id == payment.FeeId);

            if (fee == null)
                throw new Exception(
                    "Không tìm thấy Fee");

            var amount =
                query["vnp_Amount"].ToString();

            var expectedAmount =
                ((long)(payment.Amount * 100))
                .ToString();

            // Kiểm tra số tiền
            if (amount != expectedAmount)
            {
                throw new Exception(
                    "Số tiền thanh toán không khớp");
            }

            payment.Status = "Paid";

            payment.TransactionNo =
                query["vnp_TransactionNo"]
                    .ToString();

            payment.BankCode =
                query["vnp_BankCode"]
                    .ToString();

            payment.PaidAt = DateTime.Now;

            fee.Status = "Paid";
            fee.PaymentMethod = "VNPAY";
            fee.TransactionId =
                payment.TransactionNo;
            fee.PaidAt = DateTime.Now;

            _unitOfWork
                .GenericRepository<VnPayPayment>()
                .Update(payment);

            _unitOfWork
                .GenericRepository<Fee>()
                .Update(fee);

            _unitOfWork.Save();
        }

        // =========================================================
        // VALIDATE CHECKSUM
        // =========================================================

        private (
            bool Valid,
            string ResponseCode)
            ValidateResponse(IQueryCollection query)
        {
            var secureHash =
                query["vnp_SecureHash"]
                    .ToString();

            var parameters =
                new SortedDictionary<string, string>();

            foreach (var item in query)
            {
                if (item.Key == "vnp_SecureHash" ||
                    item.Key == "vnp_SecureHashType")
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(item.Value))
                {
                    parameters[item.Key] =
                        item.Value.ToString();
                }
            }

            var signData =
                string.Join(
                    "&",
                    parameters.Select(x =>
                        $"{WebUtility.UrlEncode(x.Key)}=" +
                        $"{WebUtility.UrlEncode(x.Value)}"));

            var checkHash =
                HmacSha512(
                    _settings.HashSecret,
                    signData);

            var valid =
                string.Equals(
                    checkHash,
                    secureHash,
                    StringComparison.OrdinalIgnoreCase);

            return (
                valid,
                query["vnp_ResponseCode"]
                    .ToString());
        }

        private string HmacSha512(
            string key,
            string data)
        {
            var keyBytes =
                Encoding.UTF8.GetBytes(key);

            var dataBytes =
                Encoding.UTF8.GetBytes(data);

            using var hmac =
                new HMACSHA512(keyBytes);

            var hash =
                hmac.ComputeHash(dataBytes);

            return Convert.ToHexString(
                    hash)
                .ToLowerInvariant();
        }
    }
}