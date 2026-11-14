using SchoolProject.Models;

namespace SchoolProject.ViewModels
{
    public class ClassSessionPaymentSummaryViewModel
    {
        public int ClassSessionId { get; set; }

        public string? ClassName { get; set; }

        public string? SubjectName { get; set; }

        public int TotalStudents { get; set; }

        public int PaidCount { get; set; }

        public int UnpaidCount { get; set; }

        public decimal CollectedAmount { get; set; }

        public decimal UnpaidAmount { get; set; }

        public DateTime? PaymentStartDate { get; set; }

        public DateTime? PaymentEndDate { get; set; }

        public ClassSessionStatus Status { get; set; }
    }
}