using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using System.Security.Claims;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/student/attendance")]
    [Authorize(Roles = "Student")]
    public class StudentAttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public StudentAttendanceController(
            IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // GET /api/student/attendance
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId =
                    User.FindFirst(
                        ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                    return Unauthorized();

                var result =
                    await _attendanceService
                        .GetStudentAttendance(userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
