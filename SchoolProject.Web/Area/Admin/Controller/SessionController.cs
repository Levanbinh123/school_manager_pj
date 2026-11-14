using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace SchoolProject.Web.Controllers
{
    [Route("api/admin/sessions")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }

        // GET: api/admin/sessions
        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _sessionService.GetAll();

            return Ok(result);
        }

        // GET: api/admin/sessions/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _sessionService.GetById(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy học kỳ."
                });
            }

            return Ok(result);
        }

        // GET: api/admin/sessions/by-academic-year/1
        [HttpGet("by-academic-year/{academicYearId:int}")]
        public IActionResult GetByAcademicYear(int academicYearId)
        {
            var result =
                _sessionService.GetByAcademicYear(academicYearId);

            return Ok(result);
        }

        // POST: api/admin/sessions
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateSessionViewModel vm)
        {
            try
            {
                var result = await _sessionService.Add(vm);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/admin/sessions/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateSessionViewModel vm)
        {
            try
            {
                var result = await _sessionService.Update(id, vm);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/admin/sessions/1/activate
        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _sessionService.Activate(id);

                return Ok(new
                {
                    message = "Kích hoạt học kỳ thành công."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/admin/sessions/1/complete
        [HttpPut("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                await _sessionService.Complete(id);

                return Ok(new
                {
                    message = "Hoàn thành học kỳ thành công."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/admin/sessions/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _sessionService.Delete(id);

                return Ok(new
                {
                    message = "Xóa học kỳ thành công."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}