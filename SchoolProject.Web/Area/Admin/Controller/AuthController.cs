    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SchoolProject.Servies;
    using SchoolProject.ViewModels;

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
            // REGISTER
            // =========================

            [HttpPost("register")]
            public async Task<IActionResult> Register(
                [FromBody] RegisterViewModel vm)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                try
                {
                    var userId =
                        await _authService.Register(vm);

                    return Ok(new
                    {
                        message = "Register successfully",
                        userId = userId
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());

                    return BadRequest(new
                    {
                        message = ex.Message,
                        innerException = ex.InnerException?.Message,
                        innerException2 = ex.InnerException?.InnerException?.Message
                    });
                }
            }


            // =========================
            // LOGIN
            // =========================

            [HttpPost("login")]
            public async Task<IActionResult> Login(
                [FromBody] LoginViewModel vm)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result =
                    await _authService.Login(vm);

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
                var userId =
                    User.FindFirst(
                        System.Security.Claims.ClaimTypes.NameIdentifier
                    )?.Value;

                if (userId == null)
                {
                    return Unauthorized();
                }

                await _authService.Logout(userId);

                return Ok(new
                {
                    message = "Logout successfully"
                });
            }
        }
    }