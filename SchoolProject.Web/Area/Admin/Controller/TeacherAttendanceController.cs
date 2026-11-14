using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;
using System.Security.Claims;

namespace SchoolProject.Controllers
{
    [Route("api/teacher/attendance")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class TeacherAttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public TeacherAttendanceController(
            IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        // Lấy Identity UserId từ JWT
        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // 1. Điểm danh một sinh viên
        [HttpPost]
        public async Task<IActionResult> AddAttendance(
            [FromBody] CreateAttendanceViewModel vm)
        {
            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new
                    {
                        message = "Token không hợp lệ hoặc không có UserId."
                    });

                await _attendanceService.AddAttendance(userId, vm);

                return Ok(new
                {
                    message = "Điểm danh sinh viên thành công."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. Điểm danh nhiều sinh viên trong một lần
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkAttendance(
            [FromBody] BulkAttendanceViewModel vm)
        {
            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new
                    {
                        message = "Token không hợp lệ hoặc không có UserId."
                    });

                var result = await _attendanceService.BulkAttendance(
                    userId, vm);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 3. Xem lịch sử điểm danh theo Schedule
        [HttpGet("schedule/{scheduleId:int}")]
        public async Task<IActionResult> GetBySchedule(int scheduleId)
        {
            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new
                    {
                        message = "Token không hợp lệ hoặc không có UserId."
                    });

                var result = await _attendanceService.GetBySchedule(
                    userId, scheduleId);

                return Ok(new
                {
                    scheduleId,
                    total = result.Count,
                    attendances = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 4. Lấy danh sách sinh viên để điểm danh theo ngày
        [HttpGet("schedule/{scheduleId:int}/students")]
        public async Task<IActionResult> GetStudentsForAttendance(
            int scheduleId,
            [FromQuery] DateTime date)
        {
            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new
                    {
                        message = "Token không hợp lệ hoặc không có UserId."
                    });

                if (date == default)
                    return BadRequest(new
                    {
                        message = "Vui lòng cung cấp ngày điểm danh."
                    });

                var result =
                    await _attendanceService.GetStudentsForAttendance(
                        userId, scheduleId, date);

                return Ok(new
                {
                    scheduleId,
                    date = date.Date,
                    totalStudents = result.Count,
                    students = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 5. Cập nhật điểm danh
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAttendance(
            int id,
            [FromBody] CreateAttendanceViewModel vm)
        {
            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new
                    {
                        message = "Token không hợp lệ hoặc không có UserId."
                    });

                await _attendanceService.UpdateAttendance(
                    userId, id, vm);

                return Ok(new
                {
                    message = "Cập nhật điểm danh thành công."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}