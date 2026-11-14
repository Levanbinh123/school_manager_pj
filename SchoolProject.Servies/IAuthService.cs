using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IAuthService
    {
       

        Task<LoginResponseViewModel?> Login(LoginViewModel vm);

        Task Logout(string userId);

        string GetUserIdFromJwt();
        Task<int> GetStudentIdByUserId(
            string userId);
        // Tạo tài khoản Accountant
        Task<string> CreateAccountant(AccountantRegisterViewModel vm);
    }
}