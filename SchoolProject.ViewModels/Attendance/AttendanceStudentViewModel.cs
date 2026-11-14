namespace SchoolProject.ViewModels
{
    public class AttendanceStudentViewModel
    {
        public int EnrollId { get; set; }

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public bool? Present { get; set; }

        public string? Note { get; set; }
    }
}