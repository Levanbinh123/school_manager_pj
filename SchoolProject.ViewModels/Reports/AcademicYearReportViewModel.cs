using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public class AcademicYearReportViewModel
    {
        public int AcademicYearId { get; set; }

        public string AcademicYearName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public int TotalSessions { get; set; }

        public int CompletedSessions { get; set; }

        public int TotalClasses { get; set; }

        public int TotalStudents { get; set; }

        public int TotalEnrollments { get; set; }

        public int PassedStudents { get; set; }

        public int FailedStudents { get; set; }

        public int DroppedStudents { get; set; }

        public int PaymentExpiredStudents { get; set; }

        public double? AverageTotalScore { get; set; }

        public double? AverageGPA { get; set; }

        public double PassRate { get; set; }

        public List<SessionReportViewModel> Sessions { get; set; }
            = new List<SessionReportViewModel>();
    }
}
