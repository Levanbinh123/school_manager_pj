
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using SchoolProject.Repositories;
using SchoolProject.Servies;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/payment/vnpay")]
    public class VnPayController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly IAuthService _authService;
        private readonly VnPaySettings _settings;

        public VnPayController(
            IVnPayService vnPayService,
            IAuthService authService,
            IOptions<VnPaySettings> options)
        {
            _vnPayService = vnPayService;
            _authService = authService;
            _settings = options.Value;
        }

        // =====================================================
        // 1. CREATE PAYMENT URL
        // Student gọi API để tạo URL thanh toán
        // =====================================================

        [HttpPost("create/{feeId:int}")]
        [Authorize]
        public async Task<IActionResult> CreatePayment(int feeId)
        {
            try
            {
                if (feeId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "FeeId không hợp lệ."
                    });
                }

                // Lấy UserId từ JWT
                var userId = _authService.GetUserIdFromJwt();

                // Lấy IP người gọi API
                var ipAddress = HttpContext.Connection
                    .RemoteIpAddress?
                    .MapToIPv4()
                    .ToString() ?? "127.0.0.1";

                // Tạo URL thanh toán
                var paymentUrl = await _vnPayService.CreatePaymentUrl(
                    userId,
                    feeId,
                    ipAddress
                );

                return Ok(new
                {
                    success = true,
                    feeId = feeId,
                    paymentUrl = paymentUrl,
                    message = "Tạo URL thanh toán thành công."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
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
        // 2. RETURN URL
        // VNPay chuyển người dùng về frontend
        // Không cập nhật database tại đây
        // =====================================================

        [HttpGet("return")]
        [AllowAnonymous]
        public async Task<IActionResult> Return()
        {
            var result = await _vnPayService.ProcessReturn(
                Request.Query
            );

            var url =
                $"{_settings.FrontendReturnUrl}" +
                $"?success={result.Success.ToString().ToLowerInvariant()}" +
                $"&feeId={result.FeeId}" +
                $"&message={Uri.EscapeDataString(result.Message)}";

            return Redirect(url);
        }

        // =====================================================
        // 3. IPN URL
        // VNPay gọi để xác nhận kết quả giao dịch
        // =====================================================

        [HttpGet("ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> Ipn()
        {
            var result = await _vnPayService.ProcessIpn(
                Request.Query
            );

            string responseCode;

            if (result.Success)
            {
                responseCode =
                    result.Message == "Already confirmed"
                        ? "02"
                        : "00";
            }
            else
            {
                responseCode = result.Message switch
                {
                    "Payment not found" => "01",
                    "Invalid amount" => "04",
                    "Invalid checksum" => "97",
                    _ => "99"
                };
            }

            return Ok(new
            {
                RspCode = responseCode,
                Message = result.Message
            });
        }
    }
}
