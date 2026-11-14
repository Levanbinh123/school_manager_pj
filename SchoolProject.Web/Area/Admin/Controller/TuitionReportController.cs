
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/reports/tuition")]
    [Authorize(Roles = "Admin")]
    public class TuitionReportController : ControllerBase
    {
        private readonly ITuitionReportService _tuitionReportService;

        public TuitionReportController(
            ITuitionReportService tuitionReportService)
        {
            _tuitionReportService = tuitionReportService;
        }

        // ============================================================
        // 1. GET TUITION OVERVIEW
        // GET: api/admin/reports/tuition/overview
        // ============================================================

        [HttpGet("overview")]
        public async Task<IActionResult> GetTuitionOverview()
        {
            try
            {
                var result = await _tuitionReportService
                    .GetTuitionOverview();

                return Ok(new
                {
                    success = true,
                    message = "Lấy báo cáo học phí thành công.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // 2. GET TUITION BY SESSION
        // GET: api/admin/reports/tuition/session/1
        // ============================================================

        [HttpGet("session/{sessionId:int}")]
        public async Task<IActionResult> GetTuitionBySession(
            int sessionId)
        {
            try
            {
                var result = await _tuitionReportService
                    .GetTuitionBySession(sessionId);

                return Ok(new
                {
                    success = true,
                    message = "Lấy báo cáo học phí theo học kỳ thành công.",
                    data = result
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
