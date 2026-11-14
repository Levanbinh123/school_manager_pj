
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(
            IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: api/admin/dashboard
        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                var result = await _dashboardService.GetDashboard();

                return Ok(new
                {
                    success = true,
                    message = "Lấy dữ liệu Dashboard thành công.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể lấy dữ liệu Dashboard.",
                    error = ex.Message
                });
            }
        }
    }
}
