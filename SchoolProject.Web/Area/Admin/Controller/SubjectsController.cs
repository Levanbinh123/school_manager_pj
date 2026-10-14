using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;

namespace SchoolProject.Web.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SubjectsController : ControllerBase
    {
        private readonly ISubjectService _subjectService;

        public SubjectsController(
            ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        // =====================================================
        // GET ALL
        // GET: api/admin/Subjects
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result =
                await _subjectService.GetAll();

            return Ok(result);
        }

        // =====================================================
        // GET BY ID
        // GET: api/admin/Subjects/1
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Subject ID không hợp lệ"
                });
            }

            var result =
                await _subjectService.GetById(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy Subject"
                });
            }

            return Ok(result);
        }

        // =====================================================
        // ADD
        // POST: api/admin/Subjects
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Add(
            [FromBody] CreateSubjectViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _subjectService.Add(vm);

                return Ok(new
                {
                    message = "Thêm Subject thành công"
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

        // =====================================================
        // UPDATE
        // PUT: api/admin/Subjects/1
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateSubjectViewModel vm)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Subject ID không hợp lệ"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _subjectService.Update(
                    id,
                    vm);

                return Ok(new
                {
                    message = "Cập nhật Subject thành công"
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

        // =====================================================
        // DELETE
        // DELETE: api/admin/Subjects/1
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Subject ID không hợp lệ"
                });
            }

            try
            {
                await _subjectService.Delete(id);

                return Ok(new
                {
                    message = "Xóa Subject thành công"
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