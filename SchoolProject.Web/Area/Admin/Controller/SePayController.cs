using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SchoolProject.Servies;
using SchoolProject.Utilities;
using System.Text;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/payment/sepay")]
    public class SePayController : ControllerBase
    {
        private readonly ISePayService _sePayService;
        private readonly IAuthService _authService;
        private readonly SePaySettings _settings;

        public SePayController(
            ISePayService sePayService,
            IAuthService authService,
            IOptions<SePaySettings> options)
        {
            _sePayService = sePayService;
            _authService = authService;
            _settings = options.Value;
        }

        // =====================================================
        // 1. CREATE PAYMENT
        // POST: api/payment/sepay/create/{feeId}
        // =====================================================

        [Authorize]
        [HttpPost("create/{feeId:int}")]
        public async Task<IActionResult> CreatePayment(int feeId)
        {
            try
            {
                var userId = _authService.GetUserIdFromJwt();

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Không xác định được tài khoản đăng nhập."
                    });
                }

                var invoiceNumber =
                    await _sePayService.CreatePayment(userId, feeId);

                var checkoutUrl =
                    $"{Request.Scheme}://{Request.Host}" +
                    $"/api/payment/sepay/checkout/" +
                    Uri.EscapeDataString(invoiceNumber);

                return Ok(new
                {
                    success = true,
                    message = "Tạo giao dịch SePay thành công.",
                    feeId,
                    orderInvoiceNumber = invoiceNumber,
                    checkoutUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // 2. CHECKOUT
        // GET: api/payment/sepay/checkout/{invoice}
        // Trả HTML form tự động chuyển sang SePay
        // =====================================================

        [AllowAnonymous]
        [HttpGet("checkout/{invoice}")]
        public async Task<IActionResult> Checkout(string invoice)
        {
            try
            {
                var html =
                    await _sePayService.GenerateCheckoutHtml(invoice);

                return Content(
                    html,
                    "text/html",
                    Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // 3. RETURN
        // GET: api/payment/sepay/return
        // Chỉ đọc trạng thái, không cập nhật database
        // =====================================================


        [AllowAnonymous]
        [HttpGet("return")]
        public async Task<IActionResult> Return()
        {
            var invoice = Request.Query["order_invoice_number"].ToString();
            var resultType = Request.Query["result"].ToString();

            if (string.IsNullOrWhiteSpace(invoice))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không nhận được mã hóa đơn."
                });
            }

            try
            {
                // Xử lý kết quả trả về từ SePay
                var result = await _sePayService.ProcessReturn(invoice);

                var frontendUrl = _settings.FrontendReturnUrl;

                if (string.IsNullOrWhiteSpace(frontendUrl))
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Chưa cấu hình FrontendReturnUrl."
                    });
                }

                // Tạo URL chuyển hướng về Flutter FE
                var redirectUrl = QueryHelpers.AddQueryString(
                    frontendUrl,
                    new Dictionary<string, string>
                    {
                        ["status"] = result.Success ? "success" : "failed",
                        ["feeId"] = result.FeeId.ToString(),
                        ["invoice"] = invoice,
                        ["message"] = result.Message,
                        ["gatewayResult"] = resultType
                    });

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SePay Return Error: {ex}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi khi xử lý kết quả thanh toán."
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("ipn")]
        [Consumes("application/json")]
        public async Task<IActionResult> Ipn()
        {
            try
            {
                // 1. Đọc raw JSON body
                string requestBody;

                using (var reader = new StreamReader(
                    Request.Body,
                    Encoding.UTF8))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                // 2. Đọc IPN Secret Key từ Header
                var secretKey =
                    Request.Headers["X-Secret-Key"].FirstOrDefault();

                // 3. Kiểm tra Header
                Console.WriteLine(
                    $"IPN Header Exists: {!string.IsNullOrWhiteSpace(secretKey)}");

                Console.WriteLine(
                    $"IPN Body Exists: {!string.IsNullOrWhiteSpace(requestBody)}");

                if (string.IsNullOrWhiteSpace(secretKey))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Request không gửi header X-Secret-Key."
                    });
                }

                // 4. Xử lý IPN
                var result = await _sePayService.ProcessIpn(
                    requestBody,
                    secretKey);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message
                    });
                }

                // 5. Trả HTTP 200
                return Ok(new
                {
                    success = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SePay IPN Error: {ex.Message}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error"
                });
            }
        }
    }
    }
