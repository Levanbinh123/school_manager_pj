using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SchoolProject.Models;
using SchoolProject.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SchoolProject.Servies
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // =========================
        // REGISTER
        // =========================
        public async Task<string> Register(RegisterViewModel vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var existingUser = await _userManager.FindByEmailAsync(vm.Email);

            if (existingUser != null)
                throw new Exception("Email đã được sử dụng.");

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName
            };

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(x => x.Description));

                throw new Exception(errors);
            }

            // Mặc định tài khoản mới là Student
            await _userManager.AddToRoleAsync(user, "Student");

            return user.Id;
        }

        // =========================
        // LOGIN
        // =========================
        public async Task<LoginResponseViewModel?> Login(LoginViewModel vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var user = await _userManager.FindByEmailAsync(vm.Email);

            if (user == null)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(
                user,
                vm.Password,
                false);

            if (!result.Succeeded)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id),

                new Claim(
                    ClaimTypes.Email,
                    user.Email ?? string.Empty),

                new Claim(
                    ClaimTypes.Name,
                    user.FullName ?? user.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role));
            }

            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("Jwt:Key chưa được cấu hình.");

            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return new LoginResponseViewModel
            {
                Token = tokenString,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles.ToList()
            };
        }

        // =========================
        // LOGOUT
        // =========================
        public async Task Logout(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException(
                    "UserId không hợp lệ.");

            await _signInManager.SignOutAsync();
        }

        // =========================
        // GET USER ID FROM JWT
        // =========================
        public string GetUserIdFromJwt()
        {
            var userId =
                _httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException(
                    "Không tìm thấy UserId trong JWT.");
            }

            return userId;
        }

        // =========================
        // GET STUDENT ID
        // =========================
        public async Task<int> GetStudentIdByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException(
                    "UserId không hợp lệ.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new UnauthorizedAccessException(
                    "Không tìm thấy tài khoản.");
            }

            if (user.StudentId == null)
            {
                throw new UnauthorizedAccessException(
                    "Tài khoản này chưa được liên kết với Student.");
            }

            return user.StudentId.Value;
        }
    }
}