namespace SchoolProject.ViewModels
{
    public class LoginResponseViewModel
    {
        public string Token { get; set; }

        public string UserId { get; set; }

        public string Email { get; set; }

        public string? FullName { get; set; }

        public IList<string> Roles { get; set; }
    }
}