namespace SchoolProject.ViewModels
{
    public class TeacherClassSessionViewModel
    {
        public int Id { get; set; }

        public int ClassId { get; set; }
        public string? ClassName { get; set; }

        public int SubjectId { get; set; }
        public string? SubjectName { get; set; }

        public int SessionId { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }

        public bool HasAttendance { get; set; }
    }
}