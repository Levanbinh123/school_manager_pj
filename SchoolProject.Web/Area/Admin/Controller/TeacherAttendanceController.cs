using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    [ApiController]
    [Route("api/teacher/attendance")]
    [Authorize(Roles = "Teacher")]
    public class TeacherAttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public TeacherAttendanceController(
            IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        private string GetUserId()
        {
            return User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException();
        }

        // POST /api/teacher/attendance
        [HttpPost]
        public async Task<IActionResult> Add(
            [FromBody] CreateAttendanceViewModel vm)
        {
            try
            {
                await _attendanceService.AddAttendance(
                    GetUserId(),
                    vm);

                return Ok(new
                {
                    message = "Điểm danh thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // GET /api/teacher/attendance/class-session/{id}
        [HttpGet("class-session/{id}")]
        public async Task<IActionResult> GetByClassSession(
            int id)
        {
            try
            {
                var result =
                    await _attendanceService
                        .GetByClassSession(
                            GetUserId(),
                            id);

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

        // PUT /api/teacher/attendance/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateAttendanceViewModel vm)
        {
            try
            {
                await _attendanceService.UpdateAttendance(
                    GetUserId(),
                    id,
                    vm);

                return Ok(new
                {
                    message =
                        "Cập nhật Attendance thành công"
                });
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
