
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.Utilities;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SchoolProject.Servies
{
    public class SePayService : ISePayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly SePaySettings _settings;

        public SePayService(
            IUnitOfWork unitOfWork,
            IOptions<SePaySettings> options)
        {
            _unitOfWork = unitOfWork;
            _settings = options.Value;
        }

        // =====================================================
        // 1. CREATE PAYMENT
        // =====================================================

        public async Task<string> CreatePayment(
            string userId,
            int feeId)
        {
            // Tìm sinh viên theo tài khoản đăng nhập
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(
                    filter: x => x.KeyId == userId
                );

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên.");

            // Tìm học phí của sinh viên
            var fee = await _unitOfWork
                .GenericRepository<Fee>()
                .GetByIdAsync(
                    filter: x =>
                        x.Id == feeId &&
                        x.StudentId == student.Id
                );

            if (fee == null)
                throw new Exception("Không tìm thấy học phí.");

            if (fee.Status == "Paid")
                throw new Exception("Học phí này đã được thanh toán.");

            if (fee.TotalAmount <= 0)
                throw new Exception("Số tiền thanh toán không hợp lệ.");

            // Kiểm tra lớp có đang mở thanh toán không
            await GetOpenPaymentClassSession(fee);

            // Kiểm tra VNPay đã thanh toán chưa
            var vnPayPaid = _unitOfWork
      .GenericRepository<VnPayPayment>()
      .Exists(x =>
          x.FeeId == fee.Id &&
          x.Status == "Paid"
      );
            if (vnPayPaid)
                throw new Exception(
                    "Học phí đã được thanh toán qua VNPay."
                );

            // Lấy giao dịch SePay
            var sePayPayments = await _unitOfWork
                .GenericRepository<SePayPayment>()
                .GetAllAsync(
                    filter: x => x.FeeId == fee.Id,
                    disabledTracking: false
                );

            if (sePayPayments.Any(x => x.Status == "Paid"))
                throw new Exception(
                    "Học phí đã được thanh toán qua SePay."
                );

            var now = DateTime.Now;

            // Tái sử dụng giao dịch Pending còn hạn
            var pendingPayment = sePayPayments
                .Where(x => x.Status == "Pending")
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            if (pendingPayment != null &&
                now - pendingPayment.CreatedAt <
                TimeSpan.FromMinutes(15))
            {
                return pendingPayment.OrderInvoiceNumber;
            }

            // Hết hạn giao dịch cũ
            foreach (var oldPayment in sePayPayments
                .Where(x => x.Status == "Pending"))
            {
                oldPayment.Status = "Expired";

                _unitOfWork
                    .GenericRepository<SePayPayment>()
                    .Update(oldPayment);
            }

            // Tạo mã hóa đơn mới
            var invoiceNumber =
                $"SPF{fee.Id}_{now:yyyyMMddHHmmssfff}_" +
                Guid.NewGuid().ToString("N");

            var payment = new SePayPayment
            {
                FeeId = fee.Id,
                StudentId = student.Id,
                OrderInvoiceNumber = invoiceNumber,
                Amount = fee.TotalAmount,
                Status = "Pending",
                CreatedAt = now
            };

            await _unitOfWork
                .GenericRepository<SePayPayment>()
                .AddAsync(payment);

            await _unitOfWork.SaveAsync();

            return invoiceNumber;
        }

        // =====================================================
        // 2. CHECK PAYMENT CLASS SESSION
        // =====================================================

        private async Task<ClassSession>
            GetOpenPaymentClassSession(Fee fee)
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
                    now <= x.ClassSession.PaymentEndDate.Value
                )
                .Select(x => x.ClassSession!)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();

            if (validClasses.Count == 0)
            {
                throw new Exception(
                    "Không tìm thấy lớp đang trong thời gian thanh toán."
                );
            }

            if (validClasses.Count > 1)
            {
                throw new Exception(
                    "Sinh viên có nhiều lớp phù hợp. " +
                    "Vui lòng kiểm tra đăng ký."
                );
            }

            return validClasses[0];
        }

        // =====================================================
        // 3. GENERATE CHECKOUT HTML
        // =====================================================

        public async Task<string> GenerateCheckoutHtml(
            string orderInvoiceNumber)
        {
            var payment = await _unitOfWork
                .GenericRepository<SePayPayment>()
                .GetByIdAsync(
                    filter: x =>
                        x.OrderInvoiceNumber == orderInvoiceNumber
                );

            if (payment == null)
                throw new Exception("Không tìm thấy giao dịch SePay.");

            if (payment.Status != "Pending")
                throw new Exception(
                    "Giao dịch không còn ở trạng thái chờ thanh toán."
                );

            if (payment.Amount <= 0 ||
                payment.Amount != decimal.Truncate(payment.Amount))
            {
                throw new Exception(
                    "Số tiền thanh toán phải là số nguyên VND hợp lệ."
                );
            }

            if (string.IsNullOrWhiteSpace(_settings.MerchantId) ||
                string.IsNullOrWhiteSpace(_settings.SecretKey) ||
                string.IsNullOrWhiteSpace(_settings.CheckoutUrl))
            {
                throw new Exception(
                    "Cấu hình SePay chưa đầy đủ."
                );
            }

            // =================================================
            // TẠO CALLBACK URL CÓ KÈM MÃ HÓA ĐƠN
            // =================================================

            var successUrl = QueryHelpers.AddQueryString(
                _settings.ReturnUrl,
                new Dictionary<string, string?>
                {
                    ["order_invoice_number"] =
                        payment.OrderInvoiceNumber,
                    ["result"] = "success"
                });

            var errorUrl = QueryHelpers.AddQueryString(
                _settings.ReturnUrl,
                new Dictionary<string, string?>
                {
                    ["order_invoice_number"] =
                        payment.OrderInvoiceNumber,
                    ["result"] = "error"
                });

            var cancelUrl = QueryHelpers.AddQueryString(
                _settings.ReturnUrl,
                new Dictionary<string, string?>
                {
                    ["order_invoice_number"] =
                        payment.OrderInvoiceNumber,
                    ["result"] = "cancel"
                });

            // =================================================
            // CHECKOUT FIELDS
            // =================================================

            var fields = new Dictionary<string, string>
            {
                ["order_amount"] =
                    payment.Amount.ToString(
                        "0",
                        CultureInfo.InvariantCulture
                    ),

                ["merchant"] = _settings.MerchantId,

                ["currency"] = "VND",

                ["operation"] = "PURCHASE",

                ["order_description"] =
                    $"Thanh toan hoc phi {payment.FeeId}",

                ["order_invoice_number"] =
                    payment.OrderInvoiceNumber,

                ["success_url"] = successUrl,

                ["error_url"] = errorUrl,

                ["cancel_url"] = cancelUrl
            };

            // =================================================
            // TẠO CHỮ KÝ HMAC SHA256
            // Thứ tự trường phải đúng theo tài liệu SePay
            // =================================================

            var signatureFields = new[]
            {
                "order_amount",
                "merchant",
                "currency",
                "operation",
                "order_description",
                "order_invoice_number",
                "customer_id",
                "payment_method",
                "success_url",
                "error_url",
                "cancel_url"
            };

            var signatureData = string.Join(
                ",",
                signatureFields
                    .Where(fields.ContainsKey)
                    .Select(key => $"{key}={fields[key]}")
            );

            var signature = GenerateSignature(
                signatureData,
                _settings.SecretKey
            );

            fields["signature"] = signature;

            // =================================================
            // TẠO HTML FORM TỰ ĐỘNG POST SANG SEPAY
            // =================================================

            var html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine(
                "<meta charset='UTF-8'>"
            );
            html.AppendLine(
                "<title>SePay Checkout</title>"
            );
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            html.AppendLine(
                $"<form id='sepayForm' method='POST' " +
                $"action='{System.Net.WebUtility.HtmlEncode(_settings.CheckoutUrl)}'>"
            );

            foreach (var field in fields)
            {
                html.AppendLine(
                    $"<input type='hidden' " +
                    $"name='{System.Net.WebUtility.HtmlEncode(field.Key)}' " +
                    $"value='{System.Net.WebUtility.HtmlEncode(field.Value)}' />"
                );
            }

            html.AppendLine("</form>");

            html.AppendLine(
                "<p>Đang chuyển đến cổng thanh toán SePay...</p>"
            );

            html.AppendLine(
                "<script>document.getElementById('sepayForm').submit();</script>"
            );

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        // =====================================================
        // 4. GENERATE SIGNATURE
        // =====================================================

        private string GenerateSignature(
            string data,
            string secretKey)
        {
            using var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(secretKey)
            );

            var hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(data)
            );

            return Convert.ToBase64String(hash);
        }

        // =====================================================
        // 5. PROCESS RETURN
        // RETURN KHÔNG TỰ CẬP NHẬT PAID
        // =====================================================

        public async Task<(
            bool Success,
            int FeeId,
            string Message
        )> ProcessReturn(string orderInvoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(orderInvoiceNumber))
            {
                return (
                    false,
                    0,
                    "Không nhận được mã hóa đơn."
                );
            }

            var payment = await _unitOfWork
                .GenericRepository<SePayPayment>()
                .GetByIdAsync(
                    filter: x =>
                        x.OrderInvoiceNumber == orderInvoiceNumber
                );

            if (payment == null)
            {
                return (
                    false,
                    0,
                    "Không tìm thấy giao dịch."
                );
            }

            if (payment.Status == "Paid")
            {
                return (
                    true,
                    payment.FeeId,
                    "Thanh toán thành công."
                );
            }

            return (
                false,
                payment.FeeId,
                "Đã quay lại từ SePay. " +
                "Hệ thống đang chờ IPN xác nhận thanh toán."
            );
        }

        // =====================================================
        // 6. PROCESS IPN
        // =====================================================

        public async Task<(bool Success, string Message)> ProcessIpn(
            string requestBody,
            string? secretKey)
        {
            try
            {
                // ---------------------------------------------
                // XÁC THỰC IPN SECRET KEY
                // ---------------------------------------------

                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    return (
                        false,
                        "Request không gửi header X-Secret-Key."
                    );
                }

                if (string.IsNullOrWhiteSpace(_settings.IpnSecretKey))
                {
                    return (
                        false,
                        "Ứng dụng chưa đọc được SePay:IpnSecretKey từ cấu hình."
                    );
                }

                var receivedKey =
                    Encoding.UTF8.GetBytes(secretKey);

                var configuredKey =
                    Encoding.UTF8.GetBytes(_settings.IpnSecretKey);

                if (receivedKey.Length != configuredKey.Length ||
                    !CryptographicOperations.FixedTimeEquals(
                        receivedKey,
                        configuredKey))
                {
                    return (
                        false,
                        "IPN Secret Key không hợp lệ."
                    );
                }

                // ---------------------------------------------
                // ĐỌC JSON
                // ---------------------------------------------

                using var document =
                    JsonDocument.Parse(requestBody);

                var root = document.RootElement;

                var notificationType =
                    GetString(root, "notification_type");

                if (!root.TryGetProperty(
                    "order",
                    out var order))
                {
                    return (
                        false,
                        "IPN không có thông tin order."
                    );
                }

                if (!root.TryGetProperty(
                    "transaction",
                    out var transaction))
                {
                    return (
                        false,
                        "IPN không có thông tin transaction."
                    );
                }

                var invoice =
                    GetString(order, "order_invoice_number");

                if (string.IsNullOrWhiteSpace(invoice))
                {
                    return (
                        false,
                        "Không tìm thấy mã hóa đơn trong IPN."
                    );
                }

                // ---------------------------------------------
                // TÌM GIAO DỊCH
                // ---------------------------------------------

                var payment = await _unitOfWork
                    .GenericRepository<SePayPayment>()
                    .GetByIdAsync(
                        filter: x =>
                            x.OrderInvoiceNumber == invoice
                    );

                if (payment == null)
                {
                    return (
                        false,
                        "Không tìm thấy giao dịch SePay."
                    );
                }

                // ---------------------------------------------
                // IDEMPOTENT: IPN GỬI LẶP
                // ---------------------------------------------

                if (payment.Status == "Paid")
                {
                    return (
                        true,
                        "Giao dịch đã được xác nhận trước đó."
                    );
                }

                if (payment.Status != "Pending" && notificationType != "TRANSACTION_VOID")
                {
                    return (
                        false,
                        "Giao dịch không còn ở trạng thái chờ thanh toán."
                    );
                }

                // ---------------------------------------------
                // TRANSACTION VOID
                // ---------------------------------------------

                if (notificationType == "TRANSACTION_VOID")
                {
                    payment.Status = "Voided";

                    _unitOfWork
                        .GenericRepository<SePayPayment>()
                        .Update(payment);

                    await _unitOfWork.SaveAsync();

                    return (
                        true,
                        "Giao dịch đã bị hủy."
                    );
                }

                // ---------------------------------------------
                // CHỈ XỬ LÝ ORDER_PAID
                // ---------------------------------------------

                if (notificationType != "ORDER_PAID")
                {
                    return (
                        false,
                        "Loại thông báo IPN không được hỗ trợ."
                    );
                }

                var orderStatus =
                    GetString(order, "order_status");

                var orderCurrency =
                    GetString(order, "order_currency");

                var transactionStatus =
                    GetString(transaction, "transaction_status");

                var transactionId =
                    GetString(transaction, "transaction_id");

                var paymentMethod =
                    GetString(transaction, "payment_method");

                var orderAmount =
                    GetDecimal(order, "order_amount");

                var transactionAmount =
                    GetDecimal(transaction, "transaction_amount");

                // ---------------------------------------------
                // KIỂM TRA TRẠNG THÁI
                // ---------------------------------------------

                if (orderStatus != "CAPTURED")
                {
                    return (
                        false,
                        "Đơn hàng chưa ở trạng thái CAPTURED."
                    );
                }

                if (transactionStatus != "APPROVED")
                {
                    return (
                        false,
                        "Giao dịch chưa được APPROVED."
                    );
                }

                if (orderCurrency != "VND")
                {
                    return (
                        false,
                        "Đơn hàng không sử dụng VND."
                    );
                }

                if (orderAmount != payment.Amount ||
                    transactionAmount != payment.Amount)
                {
                    return (
                        false,
                        "Số tiền IPN không khớp với giao dịch."
                    );
                }

                if (string.IsNullOrWhiteSpace(transactionId))
                {
                    return (
                        false,
                        "Thiếu mã giao dịch từ SePay."
                    );
                }

                // ---------------------------------------------
                // TÌM FEE
                // ---------------------------------------------

                var fee = await _unitOfWork
                    .GenericRepository<Fee>()
                    .GetByIdAsync(
                        filter: x =>
                            x.Id == payment.FeeId &&
                            x.StudentId == payment.StudentId
                    );

                if (fee == null)
                {
                    return (
                        false,
                        "Không tìm thấy học phí tương ứng."
                    );
                }

                if (fee.Status == "Cancelled")
                {
                    return (
                        false,
                        "Học phí đã bị hủy cùng lớp học phần."
                    );
                }

                if (fee.Status == "Paid")
                {
                    return (
                        false,
                        "Học phí đã được thanh toán bằng giao dịch khác."
                    );
                }

                if (fee.TotalAmount != payment.Amount)
                {
                    return (
                        false,
                        "Số tiền giao dịch không còn khớp với học phí."
                    );
                }

                // ---------------------------------------------
                // CẬP NHẬT SEPAY PAYMENT
                // ---------------------------------------------

                payment.Status = "Paid";
                payment.SePayOrderId =
                    GetString(order, "order_id");

                payment.TransactionId = transactionId;
                payment.PaymentMethod = paymentMethod;
                payment.PaidAt = DateTime.Now;

                _unitOfWork
                    .GenericRepository<SePayPayment>()
                    .Update(payment);

                // ---------------------------------------------
                // CẬP NHẬT FEE
                // ---------------------------------------------

                fee.Status = "Paid";
                fee.PaymentMethod = "SEPAY";
                fee.TransactionId = transactionId;
                fee.PaidAt = DateTime.Now;
                fee.FundsStatus = "Held";

                _unitOfWork
                    .GenericRepository<Fee>()
                    .Update(fee);

                await _unitOfWork.SaveAsync();

                return (
                    true,
                    "Thanh toán SePay thành công. Học phí đã được cập nhật."
                );
            }
            catch (JsonException)
            {
                return (
                    false,
                    "Dữ liệu IPN không phải JSON hợp lệ."
                );
            }
            catch (Exception ex)
            {
                return (
                    false,
                    "Lỗi xử lý IPN: " + ex.Message
                );
            }
        }

        // =====================================================
        // 7. JSON HELPER
        // =====================================================

        private static string GetString(
            JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                propertyName,
                out var property))
            {
                return string.Empty;
            }

            if (property.ValueKind == JsonValueKind.String)
            {
                return property.GetString() ?? string.Empty;
            }

            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.ToString();
            }

            return string.Empty;
        }

        private static decimal GetDecimal(
            JsonElement element,
            string propertyName)
        {
            if (!element.TryGetProperty(
                propertyName,
                out var property))
            {
                return 0;
            }

            if (property.ValueKind == JsonValueKind.Number &&
                property.TryGetDecimal(out var number))
            {
                return number;
            }

            if (property.ValueKind == JsonValueKind.String &&
                decimal.TryParse(
                    property.GetString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var stringNumber))
            {
                return stringNumber;
            }

            return 0;
        }
    }
}
