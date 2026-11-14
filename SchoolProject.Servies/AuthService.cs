
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using SchoolProject.Models;
using SchoolProject.ViewModels;
using SchoolProject.Utilities;
using SchoolProject.Repositories;

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
        private readonly ApplicationDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        // =========================================
        // CREATE ACCOUNTANT
        // ADMIN ONLY VIA CONTROLLER
        // =========================================
        public async Task<string> CreateAccountant(
            AccountantRegisterViewModel vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            if (string.IsNullOrWhiteSpace(vm.FullName))
                throw new Exception("Họ tên không được để trống.");

            if (string.IsNullOrWhiteSpace(vm.Email))
                throw new Exception("Email không được để trống.");

            if (string.IsNullOrWhiteSpace(vm.Password))
                throw new Exception("Mật khẩu không được để trống.");

            var email = vm.Email.Trim();

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
                throw new Exception("Email đã được sử dụng.");

            // Kiểm tra role Accountant đã tồn tại chưa
            var roleExists = await _context.Roles
                .AnyAsync(x => x.Name == WebSiteRole.Website_Accountant);

            if (!roleExists)
            {
                throw new Exception(
                    "Role Accountant chưa tồn tại. Vui lòng kiểm tra DbInitializer.");
            }

            // Bắt đầu transaction
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = vm.FullName.Trim()
                };

                // BƯỚC 1: Tạo tài khoản Identity
                var createResult = await _userManager.CreateAsync(
                    user,
                    vm.Password);

                if (!createResult.Succeeded)
                {
                    throw new Exception(
                        "Tạo tài khoản thất bại: " +
                        string.Join("; ",
                            createResult.Errors.Select(x => x.Description)));
                }

                // BƯỚC 2: Gán role Accountant
                var roleResult = await _userManager.AddToRoleAsync(
                    user,
                    WebSiteRole.Website_Accountant);

                if (!roleResult.Succeeded)
                {
                    throw new Exception(
                        "Gán role Accountant thất bại: " +
                        string.Join("; ",
                            roleResult.Errors.Select(x => x.Description)));
                }

                // BƯỚC 3: Commit khi cả hai thao tác thành công
                await transaction.CommitAsync();

                return user.Id;
            }
            catch
            {
                // Có lỗi thì rollback toàn bộ transaction
                await transaction.RollbackAsync();

                throw;
            }
        }

        // =========================================
        // LOGIN
        // =========================================
        public async Task<LoginResponseViewModel?> Login(
            LoginViewModel vm)
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

            // Lấy danh sách role
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

            // Thêm role vào JWT
            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role));
            }

            var jwtKey = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                throw new Exception(
                    "Jwt:Key chưa được cấu hình.");
            }

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

            var tokenString =
                new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseViewModel
            {
                Token = tokenString,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = roles.ToList()
            };
        }

        // =========================================
        // LOGOUT
        // =========================================
        public async Task Logout(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "UserId không hợp lệ.");
            }

            await _signInManager.SignOutAsync();
        }

        // =========================================
        // GET USER ID FROM JWT
        // =========================================
        public string GetUserIdFromJwt()
        {
            var userId = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "Không tìm thấy UserId trong JWT.");
            }

            return userId;
        }

        // =========================================
        // GET STUDENT ID BY USER ID
        // =========================================
        public async Task<int> GetStudentIdByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
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
