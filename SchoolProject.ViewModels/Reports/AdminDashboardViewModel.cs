
namespace SchoolProject.ViewModels
{
    public class AdminDashboardViewModel
    {
        // ================================
        // SINH VIÊN
        // ================================

        public int TotalStudents { get; set; }

        public int StudyingStudents { get; set; }

        public int SuspendedStudents { get; set; }

        public int OnLeaveStudents { get; set; }

        public int DroppedOutStudents { get; set; }

        public int GraduatedStudents { get; set; }


        // ================================
        // GIÁO VIÊN
        // ================================

        public int TotalTeachers { get; set; }


        // ================================
        // LỚP HỌC
        // ================================

        public int TotalClasses { get; set; }

        public int PlannedClasses { get; set; }

        public int RegistrationClasses { get; set; }

        public int ConfirmedClasses { get; set; }

        public int InProgressClasses { get; set; }

        public int CompletedClasses { get; set; }

        public int CancelledClasses { get; set; }


        // ================================
        // HỌC KỲ
        // ================================

        public int TotalSessions { get; set; }

        public int ActiveSessions { get; set; }

        public int CompletedSessions { get; set; }


        // ================================
        // HỌC PHÍ
        // ================================

        public int TotalFees { get; set; }

        public int PaidFees { get; set; }

        public int UnpaidFees { get; set; }

        public decimal TotalExpectedAmount { get; set; }

        public decimal TotalPaidAmount { get; set; }

        public decimal TotalHeldAmount { get; set; }

        public decimal TotalTransferredAmount { get; set; }


        // ================================
        // THỜI GIAN THỐNG KÊ
        // ================================

        public DateTime GeneratedAt { get; set; }
            = DateTime.Now;
    }
}
