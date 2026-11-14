using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/tuition-rates")]
    [Authorize(Roles = "Admin")]
    public class TuitionRateController : ControllerBase
    {
        private readonly ITuitionRateService _service;

        public TuitionRateController(ITuitionRateService service)
        {
            _service = service;
        }

        // GET: api/admin/tuition-rates
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rates = await _service.GetAllAsync();
            return Ok(rates);
        }

        // GET: api/admin/tuition-rates/session/1
        [HttpGet("session/{sessionId:int}")]
        public async Task<IActionResult> GetBySession(int sessionId)
        {
            var rate = await _service.GetBySessionIdAsync(sessionId);

            if (rate == null)
            {
                return NotFound(new
                {
                    message = "Năm học chưa có mức học phí."
                });
            }

            return Ok(rate);
        }

        // POST: api/admin/tuition-rates
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateTuitionRateRequest request)
        {
            try
            {
                var rate = await _service.CreateAsync(
                    request.SessionId,
                    request.PricePerCredit);

                return CreatedAtAction(
                    nameof(GetBySession),
                    new { sessionId = rate.SessionId },
                    rate);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT: api/admin/tuition-rates/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateTuitionRateRequest request)
        {
            try
            {
                bool updated = await _service.UpdateAsync(
                    id,
                    request.PricePerCredit);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy mức học phí."
                    });
                }

                return Ok(new
                {
                    message = "Cập nhật học phí thành công."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/admin/tuition-rates/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy mức học phí."
                });
            }

            return Ok(new
            {
                message = "Xóa mức học phí thành công."
            });
        }
    }

    public class CreateTuitionRateRequest
    {
        public int SessionId { get; set; }

        public decimal PricePerCredit { get; set; }
    }

    public class UpdateTuitionRateRequest
    {
        public decimal PricePerCredit { get; set; }
    }
}
