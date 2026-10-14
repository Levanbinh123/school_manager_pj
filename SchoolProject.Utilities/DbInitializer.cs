using Microsoft.AspNetCore.Identity;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SchoolProject.Models;
using SchoolProject.Repositories;
using Microsoft.EntityFrameworkCore;
namespace SchoolProject.Utilities
{
    public class DbInitializer : IDblnitilizer
    {
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _context;
        public DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }
        public void Initialize()
        {
            try
            {
                if (_context.Database.GetAppliedMigrations().Count() > 0)
                {
                    _context.Database.Migrate();
                }
            }
            catch (Exception)
            {
                throw;
            }
            if (!_roleManager.RoleExistsAsync(WebSiteRole.Website_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(WebSiteRole.Website_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(WebSiteRole.WebSite_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(WebSiteRole.Website_Student)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(WebSiteRole.Website_Teacher)).GetAwaiter().GetResult();
                _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        UserName = "admin@gmail.com",
                        Email = "admin@gmail.com"
                    }, "Admin@123"

                    ).GetAwaiter().GetResult();
                var appuser = _userManager.Users.Where(x => x.Email == "admin@gmail.com").FirstOrDefault();
                if (appuser != null)
                {
                    _userManager.AddToRoleAsync(appuser, WebSiteRole.Website_Admin).GetAwaiter().GetResult();
                }
            }
        }
    }
}