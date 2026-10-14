using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;

namespace SchoolProject.Web.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public ScheduleController(
            IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // GET: api/admin/Schedule
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _scheduleService.GetAll();

            return Ok(result);
        }

        // GET: api/admin/Schedule/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Schedule ID không hợp lệ"
                });
            }

            var result =
                await _scheduleService.GetById(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy Schedule"
                });
            }

            return Ok(result);
        }

        // POST: api/admin/Schedule
        [HttpPost]
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

        // PUT: api/admin/Schedule/1
        [HttpPut("{id}")]
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

        // DELETE: api/admin/Schedule/1
        [HttpDelete("{id}")]
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
    }
}