using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class SessionReportViewModel
    {
        public int SessionId { get; set; }

        public string SessionName { get; set; } = string.Empty;

        public string AcademicYear { get; set; } = string.Empty;

        public int SemesterNumber { get; set; }

        public string Status { get; set; } = string.Empty;

        // Lớp học phần
        public int TotalClasses { get; set; }
        public int CompletedClasses { get; set; }
        public int CancelledClasses { get; set; }
        public int InProgressClasses { get; set; }

        // Sinh viên
        public int TotalEnrollments { get; set; }
        public int TotalStudents { get; set; }
        public int ApprovedEnrollments { get; set; }
        public int CompletedStudents { get; set; }
        public int FailedStudents { get; set; }
        public int DroppedStudents { get; set; }
        public int PaymentExpiredStudents { get; set; }
        public int PendingEnrollments { get; set; }

        // Điểm số
        public int TotalFinalizedScores { get; set; }
        public int MissingFinalizedScores { get; set; }

        public double? AverageTotalScore { get; set; }
        public double? AverageGPA { get; set; }

        public int PassedStudents { get; set; }
        public int FailedScoreStudents { get; set; }

        public double PassRate { get; set; }
        public double FailRate { get; set; }
    }
}
