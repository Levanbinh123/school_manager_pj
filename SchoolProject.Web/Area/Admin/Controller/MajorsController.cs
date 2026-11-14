using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/majors")]
    [Authorize(Roles = "Admin")]
    public class MajorsController : ControllerBase
    {
        private readonly IMajorService _majorService;

        public MajorsController(IMajorService majorService)
        {
            _majorService = majorService;
        }

        // GET: api/admin/majors
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _majorService.GetAll();
            return Ok(result);
        }

        // GET: api/admin/majors/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Major ID không hợp lệ." });

            var result = await _majorService.GetById(id);

            if (result == null)
                return NotFound(new { message = "Không tìm thấy ngành học." });

            return Ok(result);
        }

        // POST: api/admin/majors
        [HttpPost]
        public async Task<IActionResult> Add(
            [FromBody] CreateMajorViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _majorService.Add(vm);

                return Ok(new
                {
                    message = "Thêm ngành học thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/admin/majors/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateMajorViewModel vm)
        {
            if (id <= 0)
                return BadRequest(new { message = "Major ID không hợp lệ." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _majorService.Update(id, vm);

                return Ok(new
                {
                    message = "Cập nhật ngành học thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/admin/majors/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Major ID không hợp lệ." });

            try
            {
                await _majorService.Delete(id);

                return Ok(new
                {
                    message = "Xóa ngành học thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
