using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using System.Security.Claims;

namespace SchoolProject.Web.Area.Student.Controller
{
    [ApiController]
    [Route("api/student/fees")]
    [Authorize(Roles = "Student")]
    public class StudentFeeController : ControllerBase
    {
        private readonly IFeeService _feeService;
        private readonly IVnPayService _vnPayService;

        public StudentFeeController(
            IFeeService feeService,
            IVnPayService vnPayService)
        {
            _feeService = feeService;
            _vnPayService = vnPayService;
        }

        // =====================================================
        // GET USER ID
        // =====================================================

        private string GetUserId()
        {
            return User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value
                ?? throw new UnauthorizedAccessException(
                    "Không xác định được User.");
        }

        // =====================================================
        // GET MY FEES
        // GET: api/student/fees
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetMyFees()
        {
            try
            {
                var userId = GetUserId();

                var result =
                    await _feeService.GetMyFees(userId);

                return Ok(new
                {
                    success = true,
                    data = result
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
        // GET MY FEE BY ID
        // GET: api/student/fees/{id}
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Fee ID không hợp lệ."
                });
            }

            try
            {
                var userId = GetUserId();

                var result =
                    await _feeService.GetMyFee(
                        userId,
                        id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy học phí."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
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
        // CREATE VNPAY PAYMENT
        // POST: api/student/fees/{id}/payment/vnpay
        // =====================================================

        [HttpPost("{id}/payment/vnpay")]
        public async Task<IActionResult> PayVnPay(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Fee ID không hợp lệ."
                });
            }

            try
            {
                var userId = GetUserId();

                var ipAddress =
                    HttpContext
                        .Connection
                        .RemoteIpAddress?
                        .ToString()
                    ?? "127.0.0.1";

                var paymentUrl =
                    await _vnPayService
                        .CreatePaymentUrl(
                            userId,
                            id,
                            ipAddress);

                return Ok(new
                {
                    success = true,
                    paymentUrl
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
    }
}