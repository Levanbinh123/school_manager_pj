using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IAuthService
    {
        Task<string> Register(RegisterViewModel vm);

        Task<LoginResponseViewModel?> Login(LoginViewModel vm);

        Task Logout(string userId);

        string GetUserIdFromJwt();
        Task<int> GetStudentIdByUserId(
            string userId);
    }
}