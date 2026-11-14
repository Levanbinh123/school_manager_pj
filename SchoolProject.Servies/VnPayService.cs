
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        // 1. CREATE PAYMENT URL
        // =========================================================

        public async Task<string> CreatePaymentUrl(
            string userId,
            int feeId,
            string ipAddress)
        {
            // Tìm Student theo tài khoản đăng nhập
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(x => x.KeyId == userId);

            if (student == null)
                throw new Exception(
                    "Không tìm thấy Student của tài khoản.");

            // Tìm Fee thuộc Student này
            var fee = await _unitOfWork
                .GenericRepository<Fee>()
                .GetByIdAsync(x =>
                    x.Id == feeId &&
                    x.StudentId == student.Id);

            if (fee == null)
                throw new Exception(
                    "Không tìm thấy khoản học phí.");

            if (fee.Status == "Paid")
                throw new Exception(
                    "Khoản học phí này đã được thanh toán.");

            if (fee.TotalAmount <= 0)
                throw new Exception(
                    "Số tiền học phí không hợp lệ.");

            // Kiểm tra lớp học phần đang mở thanh toán
            await GetOpenPaymentClassSession(fee);

            // Kiểm tra không có giao dịch nào của Fee đã Paid
            var existingPayments = await _unitOfWork
                .GenericRepository<VnPayPayment>()
                .GetAllAsync(
                    filter: x => x.FeeId == fee.Id
                );

            if (existingPayments.Any(x => x.Status == "Paid"))
                throw new Exception(
                    "Khoản học phí đã được thanh toán.");

            // Sử dụng tổng học phí, bao gồm phí trễ hạn
            var paymentAmount = fee.TotalAmount;

            // Tạo mã giao dịch duy nhất
            var txnRef =
                $"{fee.Id}_{DateTime.Now:yyyyMMddHHmmssfff}_" +
                Guid.NewGuid().ToString("N");

            var payment = new VnPayPayment
            {
                FeeId = fee.Id,
                StudentId = student.Id,
                TxnRef = txnRef,
                Amount = paymentAmount,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            await _unitOfWork
                .GenericRepository<VnPayPayment>()
                .AddAsync(payment);

            _unitOfWork.Save();

            var createDate = DateTime.Now.ToString(
                "yyyyMMddHHmmss",
                CultureInfo.InvariantCulture);

            var expireDate = DateTime.Now
                .AddMinutes(15)
                .ToString(
                    "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture);

            // VNPay yêu cầu đơn vị tiền là VND * 100
            var amount = decimal.ToInt64(
                paymentAmount * 100
            ).ToString(CultureInfo.InvariantCulture);

            var vnpParams = new SortedDictionary<string, string>
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

            var queryString = BuildQueryString(vnpParams);

            var secureHash = HmacSha512(
                _settings.HashSecret,
                queryString);

            return
                $"{_settings.PaymentUrl}?" +
                $"{queryString}" +
                $"&vnp_SecureHash={secureHash}";
        }

        // =========================================================
        // 2. FIND CLASS SESSION AND CHECK PAYMENT PERIOD
        // =========================================================

        private async Task<ClassSession> GetOpenPaymentClassSession(
            Fee fee)
        {
            var enrollments = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync(
                    include: q => q.Include(x => x.ClassSession),
                    filter: x =>
                        x.StudentId == fee.StudentId &&
                        x.SubjectId == fee.SubjectId &&
                        x.SessionId == fee.SessionId &&
                        x.ClassSessionId != null &&
                        x.Status != EnrollStatus.Dropped
                );

            var now = DateTime.Now;

            var validClasses = enrollments
                .Where(x =>
                    x.ClassSession != null &&
                    x.ClassSession.Status ==
                        ClassSessionStatus.PaymentOpen &&
                    x.ClassSession.PaymentStartDate.HasValue &&
                    x.ClassSession.PaymentEndDate.HasValue &&
                    now >= x.ClassSession.PaymentStartDate.Value &&
                    now <= x.ClassSession.PaymentEndDate.Value)
                .Select(x => x.ClassSession!)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            if (validClasses.Count == 0)
            {
                throw new Exception(
                    "Lớp học phần chưa mở thanh toán hoặc đã hết hạn.");
            }

            if (validClasses.Count > 1)
            {
                throw new Exception(
                    "Có nhiều lớp học phần phù hợp với khoản học phí. " +
                    "Vui lòng kiểm tra dữ liệu Enroll.");
            }

            return validClasses[0];
        }

        // =========================================================
        // 3. RETURN URL
        // Return chỉ đọc kết quả, KHÔNG cập nhật database
        // =========================================================

        public async Task<(bool Success, int FeeId, string Message)>
            ProcessReturn(IQueryCollection query)
        {
            var validation = ValidateResponse(query);

            if (!validation.Valid)
            {
                return (
                    false,
                    0,
                    "Checksum không hợp lệ."
                );
            }

            if (query["vnp_TmnCode"].ToString() != _settings.TmnCode)
            {
                return (
                    false,
                    0,
                    "Merchant không hợp lệ."
                );
            }

            var txnRef = query["vnp_TxnRef"].ToString();

            if (string.IsNullOrWhiteSpace(txnRef))
            {
                return (
                    false,
                    0,
                    "Thiếu mã giao dịch."
                );
            }

            var payment = await _unitOfWork
                .GenericRepository<VnPayPayment>()
                .GetByIdAsync(x => x.TxnRef == txnRef);

            if (payment == null)
            {
                return (
                    false,
                    0,
                    "Không tìm thấy giao dịch."
                );
            }

            if (!IsAmountValid(payment, query))
            {
                return (
                    false,
                    payment.FeeId,
                    "Số tiền giao dịch không khớp."
                );
            }

            var responseCode =
                query["vnp_ResponseCode"].ToString();

            var transactionStatus =
                query["vnp_TransactionStatus"].ToString();

            if (responseCode == "00" &&
                transactionStatus == "00")
            {
                return (
                    true,
                    payment.FeeId,
                    "VNPay thông báo giao dịch thành công. " +
                    "Hệ thống đang xác nhận kết quả qua IPN."
                );
            }

            return (
                false,
                payment.FeeId,
                $"Thanh toán không thành công. Mã lỗi: {responseCode}"
            );
        }

        // =========================================================
        // 4. IPN
        // IPN mới là nơi cập nhật trạng thái thanh toán
        // =========================================================

        public async Task<(bool Success, string Message)>
            ProcessIpn(IQueryCollection query)
        {
            try
            {
                var validation = ValidateResponse(query);

                if (!validation.Valid)
                {
                    return (
                        false,
                        "Invalid checksum"
                    );
                }

                if (query["vnp_TmnCode"].ToString() != _settings.TmnCode)
                {
                    return (
                        false,
                        "Invalid merchant"
                    );
                }

                var txnRef = query["vnp_TxnRef"].ToString();

                if (string.IsNullOrWhiteSpace(txnRef))
                {
                    return (
                        false,
                        "Missing transaction reference"
                    );
                }

                var payment = await _unitOfWork
                    .GenericRepository<VnPayPayment>()
                    .GetByIdAsync(x => x.TxnRef == txnRef);

                if (payment == null)
                {
                    return (
                        false,
                        "Payment not found"
                    );
                }

                // Kiểm tra số tiền
                if (!IsAmountValid(payment, query))
                {
                    return (
                        false,
                        "Invalid amount"
                    );
                }

                // IPN gửi lại sau khi đã xác nhận
                if (payment.Status == "Paid")
                {
                    return (
                        true,
                        "Already confirmed"
                    );
                }

                var responseCode =
                    query["vnp_ResponseCode"].ToString();

                var transactionStatus =
                    query["vnp_TransactionStatus"].ToString();

                // Giao dịch thành công
                if (responseCode == "00" &&
                    transactionStatus == "00")
                {
                    await MarkPaymentSuccess(payment, query);

                    return (
                        true,
                        "Confirm Success"
                    );
                }

                // Giao dịch thất bại
                payment.Status = "Failed";

                _unitOfWork
                    .GenericRepository<VnPayPayment>()
                    .Update(payment);

                _unitOfWork.Save();

                return (
                    true,
                    "Payment failed"
                );
            }
            catch (Exception)
            {
                // Không trả chi tiết exception nội bộ cho VNPay
                return (
                    false,
                    "Internal server error"
                );
            }
        }

        // =========================================================
        // 5. MARK PAYMENT SUCCESS
        // =========================================================

        private async Task MarkPaymentSuccess(
            VnPayPayment payment,
            IQueryCollection query)
        {
            if (payment.Status == "Paid")
                return;

            var fee = await _unitOfWork
                .GenericRepository<Fee>()
                .GetByIdAsync(x => x.Id == payment.FeeId);

            if (fee == null)
                throw new Exception("Không tìm thấy Fee.");

            if (fee.StudentId != payment.StudentId)
                throw new Exception("Student không khớp.");

            if (fee.Status == "Paid")
            {
                throw new Exception(
                    "Fee đã được thanh toán bằng giao dịch khác.");
            }

            // Kiểm tra tổng học phí hiện tại
            if (fee.TotalAmount != payment.Amount)
            {
                throw new Exception(
                    "Tổng học phí hiện tại không khớp giao dịch.");
            }

            if (!IsAmountValid(payment, query))
            {
                throw new Exception(
                    "Số tiền VNPay không khớp.");
            }

            if (query["vnp_ResponseCode"].ToString() != "00" ||
                query["vnp_TransactionStatus"].ToString() != "00")
            {
                throw new Exception(
                    "Giao dịch VNPay chưa thành công.");
            }

            // Cập nhật VnPayPayment
            payment.Status = "Paid";

            payment.TransactionNo =
                query["vnp_TransactionNo"].ToString();

            payment.BankCode =
                query["vnp_BankCode"].ToString();

            payment.PaidAt = DateTime.Now;

            // Cập nhật Fee
            fee.Status = "Paid";
            fee.PaymentMethod = "VNPAY";

            fee.TransactionId =
                payment.TransactionNo;

            fee.PaidAt = DateTime.Now;

            // Tiền đã thu nhưng đang được giữ lại
            fee.FundsStatus = "Held";

            _unitOfWork
                .GenericRepository<VnPayPayment>()
                .Update(payment);

            _unitOfWork
                .GenericRepository<Fee>()
                .Update(fee);

            _unitOfWork.Save();
        }

        // =========================================================
        // 6. VALIDATE AMOUNT
        // =========================================================

        private bool IsAmountValid(
            VnPayPayment payment,
            IQueryCollection query)
        {
            var amount = query["vnp_Amount"].ToString();

            if (!long.TryParse(
                    amount,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var receivedAmount))
            {
                return false;
            }

            var expectedAmount = decimal.ToInt64(
                payment.Amount * 100
            );

            return receivedAmount == expectedAmount;
        }

        // =========================================================
        // 7. VALIDATE CHECKSUM
        // =========================================================

        private (bool Valid, string ResponseCode)
            ValidateResponse(IQueryCollection query)
        {
            var secureHash =
                query["vnp_SecureHash"].ToString();

            if (string.IsNullOrWhiteSpace(secureHash))
            {
                return (
                    false,
                    query["vnp_ResponseCode"].ToString()
                );
            }

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

            var signData = BuildQueryString(parameters);

            var checkHash = HmacSha512(
                _settings.HashSecret,
                signData
            );

            var valid = FixedTimeHashEquals(
                checkHash,
                secureHash
            );

            return (
                valid,
                query["vnp_ResponseCode"].ToString()
            );
        }

        // =========================================================
        // 8. BUILD QUERY STRING
        // =========================================================

        private string BuildQueryString(
            SortedDictionary<string, string> parameters)
        {
            return string.Join(
                "&",
                parameters.Select(x =>
                    $"{WebUtility.UrlEncode(x.Key)}=" +
                    $"{WebUtility.UrlEncode(x.Value)}")
            );
        }

        // =========================================================
        // 9. HMAC SHA512
        // =========================================================

        private string HmacSha512(
            string key,
            string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA512(keyBytes);

            var hash = hmac.ComputeHash(dataBytes);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        // =========================================================
        // 10. CONSTANT-TIME HASH COMPARISON
        // =========================================================

        private bool FixedTimeHashEquals(
            string first,
            string second)
        {
            try
            {
                var firstBytes = Convert.FromHexString(first);
                var secondBytes = Convert.FromHexString(second);

                return firstBytes.Length == secondBytes.Length &&
                       CryptographicOperations.FixedTimeEquals(
                           firstBytes,
                           secondBytes
                       );
            }
            catch
            {
                return false;
            }
        }
    }
}
