
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;
using System.Security.Claims;

namespace SchoolProject.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(
            IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // =========================================
        // ADMIN: GET ALL SCHEDULES
        // GET: api/Schedule
        // =========================================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _scheduleService.GetAll();

            return Ok(result);
        }

        // =========================================
        // ADMIN: GET SCHEDULE BY ID
        // GET: api/Schedule/1
        // =========================================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Schedule ID không hợp lệ"
                });
            }

            var result = await _scheduleService.GetById(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy Schedule"
                });
            }

            return Ok(result);
        }

        // =========================================
        // ADMIN: ADD SCHEDULE
        // POST: api/Schedule
        // =========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(
            [FromBody] CreateScheduleViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _scheduleService.Add(vm);

                return Ok(new
                {
                    message = "Thêm Schedule thành công"
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

        // =========================================
        // ADMIN: UPDATE SCHEDULE
        // PUT: api/Schedule/1
        // =========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateScheduleViewModel vm)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Schedule ID không hợp lệ"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _scheduleService.Update(id, vm);

                return Ok(new
                {
                    message = "Cập nhật Schedule thành công"
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

        // =========================================
        // ADMIN: DELETE SCHEDULE
        // DELETE: api/Schedule/1
        // =========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Schedule ID không hợp lệ"
                });
            }

            try
            {
                await _scheduleService.Delete(id);

                return Ok(new
                {
                    message = "Xóa Schedule thành công"
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

        // =========================================
        // STUDENT: GET MY SCHEDULE
        // GET: api/Schedule/my
        // =========================================
        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyStudentSchedule()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "Không tìm thấy UserId trong JWT."
                });
            }

            try
            {
                var result = await _scheduleService
                    .GetStudentScheduleByUserId(userId);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
        }

        // =========================================
        // TEACHER: GET MY TEACHING SCHEDULE
        // GET: api/Schedule/my-teaching
        // =========================================
        [HttpGet("my-teaching")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyTeacherSchedule()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "Không tìm thấy UserId trong JWT."
                });
            }

            try
            {
                var result = await _scheduleService
                    .GetTeacherScheduleByUserId(userId);

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
