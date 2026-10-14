namespace SchoolProject.ViewModels
{
    public class TeacherStudentViewModel
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public DateTime DOB { get; set; }

        public int YearLevel { get; set; }

        public int MajorId { get; set; }

        public string? KeyId { get; set; }
    }
}