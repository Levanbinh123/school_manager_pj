using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
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

        public StudentRegistrationController(
            IRegistrationService registrationService,
            IAuthService authService)
        {
            _registrationService = registrationService;
            _authService = authService;
        }

        // =====================================================
        // GET USER ID FROM JWT
        // =====================================================

        private string GetUserId()
        {
            return User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException(
                    "Không xác định được User.");
        }


        // =====================================================
        // GET AVAILABLE CLASS SESSIONS
        //
        // GET:
        // api/student/registration/class-sessions?sessionId=1
        //
        // Student sẽ thấy:
        // Subject + Class + Teacher
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
                var userId = _authService.GetUserIdFromJwt();

                var studentId =
                    await _authService.GetStudentIdByUserId(userId);

                var result =
                    await _registrationService
                        .GetAvailableClassSessions(
                            studentId,
                            sessionId);

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
        // REGISTER CLASS SESSION
        //
        // POST:
        // api/student/registration
        //
        // Body:
        // {
        //     "classSessionId": 1
        // }
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterClassSessionRequest request)
        {
            if (request.ClassSessionId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ClassSession ID không hợp lệ."
                });
            }

            try
            {
                var userId = GetUserId();

                var studentId =
                    await _authService
                        .GetStudentIdByUserId(userId);

                var result =
                    await _registrationService
                        .RegisterClassSession(
                            studentId,
                            request.ClassSessionId);

                return Ok(new
                {
                    success = result,
                    message = "Đăng ký lớp học thành công."
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
        // GET MY ENROLLMENTS
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
                var userId = GetUserId();

                var studentId =
                    await _authService
                        .GetStudentIdByUserId(userId);

                var result =
                    await _registrationService
                        .GetMyEnrollments(
                            studentId,
                            sessionId);

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
        // CANCEL REGISTRATION
        //
        // DELETE:
        // api/student/registration
        //
        // Body:
        // {
        //     "classSessionId": 1
        // }
        // =====================================================

        [HttpDelete]
        public async Task<IActionResult> CancelRegistration(
            [FromBody] RegisterClassSessionRequest request)
        {
            if (request.ClassSessionId <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "ClassSession ID không hợp lệ."
                });
            }

            try
            {
                var userId = GetUserId();

                var studentId =
                    await _authService
                        .GetStudentIdByUserId(userId);

                var result =
                    await _registrationService
                        .CancelRegistration(
                            studentId,
                            request.ClassSessionId);

                return Ok(new
                {
                    success = result,
                    message = "Hủy đăng ký thành công."
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
        // REQUEST MODEL
        // =====================================================

        public class RegisterClassSessionRequest
        {
            public int ClassSessionId { get; set; }
        }
    }
}