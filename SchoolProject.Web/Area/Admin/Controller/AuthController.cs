
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;
using SchoolProject.Utilities;
using System.Security.Claims;

namespace SchoolProject.Web.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

      

        // =========================
        // LOGIN
        // =========================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.Login(vm);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Email or password is incorrect"
                });
            }

            return Ok(result);
        }

        // =========================
        // LOGOUT
        // =========================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _authService.Logout(userId);

            return Ok(new
            {
                message = "Logout successfully"
            });
        }

        // =========================
        // CREATE ACCOUNTANT
        // ADMIN ONLY
        // =========================
        [HttpPost("create-accountant")]
        [Authorize(Roles = WebSiteRole.Website_Admin)]
        public async Task<IActionResult> CreateAccountant(
            [FromBody] AccountantRegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = await _authService.CreateAccountant(vm);

                return Ok(new
                {
                    message = "Tạo tài khoản Accountant thành công.",
                    userId = userId,
                    email = vm.Email,
                    role = WebSiteRole.Website_Accountant
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
