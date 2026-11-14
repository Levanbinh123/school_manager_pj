using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int MaxCredits { get; set; } = 30;
        public string AcademicYear { get; set; } = string.Empty;
        public int SemesterNumber { get; set; }
        // Đánh dấu học kỳ cuối cùng của năm học
        public bool IsFinalSemester { get; set; } = false;
        // Trạng thái học kỳ
        public SessionStatus Status { get; set; } = SessionStatus.Planned;
        public bool IsPromoted { get; set; } = false;
    
        public bool IsAcademicYearClosed { get; set; } = false;
        public DateTime? PromotedAt { get; set; }

        // Khóa ngoại liên kết với năm học
        public int? AcademicYearId { get; set; }

        // Navigation Property
        public AcademicYear? AcademicYearInfo { get; set; }
        public ICollection<ClassSession> ClassSessions { get; set; }
          = new HashSet<ClassSession>();

        public ICollection<TuitionRate> TuitionRates { get; set; }
            = new HashSet<TuitionRate>();

        public ICollection<TeacherSession> TeacherSessions { get; set; }
            = new HashSet<TeacherSession>();
    }
    public enum SessionStatus
    {
        Planned = 0,
        Active = 1,
        Completed = 2,
    }
}
