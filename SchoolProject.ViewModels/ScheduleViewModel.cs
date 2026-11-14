namespace SchoolProject.ViewModels
{
    public class ScheduleViewModel
    {
        public int Id { get; set; }

        public int ClassSessionId { get; set; }

        public int? SubjectId { get; set; }
        public string? SubjectName { get; set; }

        public int ClassId { get; set; }
        public string? ClassName { get; set; }

        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ScheduleViewModel()
        {
        }
    }
}