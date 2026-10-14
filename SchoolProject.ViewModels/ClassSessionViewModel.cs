using SchoolProject.Models;

namespace SchoolProject.ViewModels
{
    public class ClassSessionViewModel
    {
        public int Id { get; set; }

        public int ClassId { get; set; }
        public string? ClassName { get; set; }

        public int SubjectId { get; set; }
        public string? SubjectName { get; set; }

        public int SessionId { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public int? HomeroomTeacherId { get; set; }
        public string? HomeroomTeacherName { get; set; }

        public bool HasAttendance { get; set; }

        public int TotalSessions { get; set; }

        public ClassSessionViewModel()
        {
        }

        public ClassSessionViewModel(ClassSession entity)
        {
            Id = entity.Id;

            ClassId = entity.ClassId;
            ClassName = entity.Class?.Name;

            SubjectId = entity.SubjectId;
            SubjectName = entity.Subject?.Name;

            SessionId = entity.SessionId;

            Start = entity.Session?.Start ?? default;
            End = entity.Session?.End ?? default;

            HomeroomTeacherId = entity.HomeroomTeacherId;

            HomeroomTeacherName =
                entity.HomeroomTeacher == null
                    ? null
                    : $"{entity.HomeroomTeacher.FirstName} {entity.HomeroomTeacher.LastName}";

            HasAttendance = entity.HasAttendance;

            TotalSessions = entity.TotalSessions;
        }
    }
}