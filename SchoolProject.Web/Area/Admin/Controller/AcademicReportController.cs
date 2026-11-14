
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/reports/academic")]
    [Authorize(Roles = "Admin")]
    public class AcademicReportController : ControllerBase
    {
        private readonly IAcademicReportService _reportService;

        public AcademicReportController(
            IAcademicReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: api/admin/reports/academic/session/1
        [HttpGet("session/{sessionId:int}")]
        public async Task<IActionResult> GetSessionReport(int sessionId)
        {
            try
            {
                var result = await _reportService
                    .GetSessionReport(sessionId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/admin/reports/academic/year/1
        [HttpGet("year/{academicYearId:int}")]
        public async Task<IActionResult> GetAcademicYearReport(
            int academicYearId)
        {
            try
            {
                var result = await _reportService
                    .GetAcademicYearReport(academicYearId);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
