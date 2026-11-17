using SchoolProject.Models;

namespace SchoolProject.ViewModels
{
    public class EnrollViewModel
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int SubjectId { get; set; }

        public string? SubjectName { get; set; }

        public int SessionId { get; set; }

        public string? SessionName { get; set; }

        public int? ClassSessionId { get; set; }

        public string? ClassName { get; set; }
        public int? ClassSessionStatus { get; set; }

        public EnrollViewModel()
        {
        }

        public EnrollViewModel(Enroll enroll)
        {
            Id = enroll.Id;

            StudentId = enroll.StudentId;

            SubjectId = enroll.SubjectId;

            SubjectName =
                enroll.Subject?.Name;

            SessionId = enroll.SessionId;

            ClassSessionId =
                enroll.ClassSessionId;

            ClassName =
                enroll.ClassSession?.Class?.Name;
            ClassSessionStatus=enroll.ClassSession==null?null:(int)enroll.ClassSession.Status;
        }
    }
}