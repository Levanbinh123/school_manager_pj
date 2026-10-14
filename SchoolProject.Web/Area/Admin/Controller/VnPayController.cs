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
        private readonly VnPaySettings _settings;

        public VnPayController(
            IVnPayService vnPayService,
            IOptions<VnPaySettings> options)
        {
            _vnPayService = vnPayService;
            _settings = options.Value;
        }

        // =====================================================
        // RETURN URL
        // =====================================================

        [HttpGet("return")]
        [AllowAnonymous]
        public async Task<IActionResult> Return()
        {
            var result =
                await _vnPayService
                    .ProcessReturn(Request.Query);

            var url =
                $"{_settings.FrontendReturnUrl}" +
                $"?success={result.Success}" +
                $"&feeId={result.FeeId}" +
                $"&message={Uri.EscapeDataString(result.Message)}";

            return Redirect(url);
        }

        // =====================================================
        // IPN
        // =====================================================

        [HttpGet("ipn")]
        [AllowAnonymous]
        public async Task<IActionResult> Ipn()
        {
            var result =
                await _vnPayService
                    .ProcessIpn(Request.Query);

            return Ok(new
            {
                RspCode =
                    result.Success
                        ? "00"
                        : "97",

                Message = result.Message
            });
        }
    }
}
