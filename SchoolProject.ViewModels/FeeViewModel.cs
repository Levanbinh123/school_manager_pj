
using SchoolProject.Models;

namespace SchoolProject.ViewModels
{
    public class FeeViewModel
    {
        public FeeViewModel(Fee fee)
        {
            Id = fee.Id;

            StudentId = fee.StudentId;
            SubjectId = fee.SubjectId;
            SessionId = fee.SessionId;

            Credits = fee.Credits;
            PricePerCredit = fee.PricePerCredit;

            Amount = fee.Amount;
            DueDate = fee.DueDate;

            Status = fee.Status;
            PaymentMethod = fee.PaymentMethod;
            TransactionId = fee.TransactionId;
            PaidAt = fee.PaidAt;
        }

        public int Id { get; set; }

        public int StudentId { get; set; }
        public string? StudentName { get; set; }

        public int SubjectId { get; set; }
        public string? SubjectName { get; set; }

        public int SessionId { get; set; }

        public int Credits { get; set; }

        public decimal PricePerCredit { get; set; }

        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public DateTime? PaidAt { get; set; }
    }
}
