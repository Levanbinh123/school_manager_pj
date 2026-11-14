
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/student/registration")]
    [Authorize(Roles = "Student")]
    public class StudentRegistrationController : ControllerBase
    {
        private readonly IRegistrationService _registrationService;
        private readonly IAuthService _authService;
        private readonly ILogger<StudentRegistrationController> _logger;

        public StudentRegistrationController(
            IRegistrationService registrationService,
            IAuthService authService,
            ILogger<StudentRegistrationController> logger)
        {
            _registrationService = registrationService;
            _authService = authService;
            _logger = logger;
        }

        // =====================================================
        // GET USER ID FROM JWT
        // =====================================================

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "Không xác định được User từ JWT.");
        }

        // =====================================================
        // GET STUDENT ID FROM JWT
        // =====================================================

        private async Task<int> GetStudentId()
        {
            var userId = GetUserId();

            var studentId = await _authService
                .GetStudentIdByUserId(userId);

            if (studentId <= 0)
            {
                throw new UnauthorizedAccessException(
                    "Tài khoản này chưa được liên kết với Student.");
            }

            return studentId;
        }

        // =====================================================
        // HANDLE ERRORS
        // =====================================================

        private IActionResult HandleError(Exception ex)
        {
            if (ex is UnauthorizedAccessException)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message
                });
            }

            _logger.LogError(
                ex,
                "Lỗi xảy ra trong StudentRegistrationController.");

            return StatusCode(500, new
            {
                success = false,
                message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."
            });
        }

        // =====================================================
        // 1. GET AVAILABLE CLASS SESSIONS
        //
        // GET:
        // api/student/registration/class-sessions?sessionId=1
        // =====================================================

        [HttpGet("class-sessions")]
        public async Task<IActionResult> GetAvailableClassSessions(
            [FromQuery] int sessionId)
        {
            if (sessionId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Session ID không hợp lệ."
                });
            }

            try
            {
                var studentId = await GetStudentId();

                var result = await _registrationService
                    .GetAvailableClassSessions(studentId, sessionId);

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách lớp học phần thành công.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        // =====================================================
        // 2. REGISTER CLASS SESSION
        //
        // POST:
        // api/student/registration
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterClassSessionRequest request)
        {
            try
            {
                var studentId = await GetStudentId();

                var result = await _registrationService
                    .RegisterClassSession(
                        studentId,
                        request.ClassSessionId);

                return Ok(new
                {
                    success = result,
                    message = result
                        ? "Đăng ký lớp học thành công."
                        : "Đăng ký lớp học không thành công."
                });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        // =====================================================
        // 3. GET MY ENROLLMENTS
        //
        // GET:
        // api/student/registration/my-subjects?sessionId=1
        // =====================================================

        [HttpGet("my-subjects")]
        public async Task<IActionResult> GetMyEnrollments(
            [FromQuery] int sessionId)
        {
            if (sessionId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Session ID không hợp lệ."
                });
            }

            try
            {
                var studentId = await GetStudentId();

                var result = await _registrationService
                    .GetMyEnrollments(studentId, sessionId);

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách môn học đã đăng ký thành công.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        // =====================================================
        // 4. CANCEL REGISTRATION
        //
        // DELETE:
        // api/student/registration
        // =====================================================

        [HttpDelete]
        public async Task<IActionResult> CancelRegistration(
            [FromBody] RegisterClassSessionRequest request)
        {
            try
            {
                var studentId = await GetStudentId();

                var result = await _registrationService
                    .CancelRegistration(
                        studentId,
                        request.ClassSessionId);

                return Ok(new
                {
                    success = result,
                    message = result
                        ? "Hủy đăng ký lớp học thành công."
                        : "Hủy đăng ký lớp học không thành công."
                });
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        // =====================================================
        // REQUEST MODEL
        // =====================================================

        public class RegisterClassSessionRequest
        {
            [Range(
                1,
                int.MaxValue,
                ErrorMessage = "ClassSessionId phải lớn hơn 0.")]
            public int ClassSessionId { get; set; }
        }
    }
}
