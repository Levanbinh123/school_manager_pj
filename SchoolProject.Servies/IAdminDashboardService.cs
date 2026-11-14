
using SchoolProject.ViewModels;


namespace SchoolProject.Servies
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboard();
    }
}
