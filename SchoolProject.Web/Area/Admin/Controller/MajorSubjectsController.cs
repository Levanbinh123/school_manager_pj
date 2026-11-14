using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/major-subjects")]
    [Authorize(Roles = "Admin")]
    public class MajorSubjectsController : ControllerBase
    {
        private readonly IMajorSubjectService _service;

        public MajorSubjectsController(
            IMajorSubjectService service)
        {
            _service = service;
        }

        // ==========================================
        // GET: api/admin/major-subjects
        // Lấy toàn bộ môn đã gán vào các ngành
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result =
                    await _service.GetAllMajorSubjects();

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

        // ==========================================
        // GET: api/admin/major-subjects/major/1
        // Lấy các môn đã gán cho ngành ID = 1
        // ==========================================
        [HttpGet("major/{majorId:int}")]
        public async Task<IActionResult> GetByMajor(
            int majorId)
        {
            try
            {
                var result =
                    await _service.GetMajorSubjectsByMajorId(
                        majorId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
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

        // ==========================================
        // POST: api/admin/major-subjects
        // Gán môn học vào ngành
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> AssignSubject(
            [FromBody] AssignSubjectToMajorViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.AssignSubject(vm);

                return Ok(new
                {
                    message = "Gán môn học vào ngành thành công."
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

        // ==========================================
        // DELETE: api/admin/major-subjects/1/2
        // Gỡ môn ID = 2 khỏi ngành ID = 1
        // ==========================================
        [HttpDelete("{majorId:int}/{subjectId:int}")]
        public async Task<IActionResult> RemoveSubject(
            int majorId,
            int subjectId)
        {
            try
            {
                await _service.RemoveSubject(
                    majorId,
                    subjectId);

                return Ok(new
                {
                    message = "Đã gỡ môn học khỏi ngành."
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