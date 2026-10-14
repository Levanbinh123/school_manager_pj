namespace SchoolProject.Models
{
    public class Fee
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int SubjectId { get; set; }

        public int SessionId { get; set; }

        // Snapshot tại thời điểm tạo học phí
        public int Credits { get; set; }

        public decimal PricePerCredit { get; set; }

        public decimal Amount { get; set; }

        public DateTime DueDate { get; set; }

        // Unpaid / Pending / Paid
        public string Status { get; set; } = "Unpaid";

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public DateTime? PaidAt { get; set; }

        // Navigation
        public Student Student { get; set; } = null!;

        public Subject Subject { get; set; } = null!;

        public Session Session { get; set; } = null!;

        public ICollection<VnPayPayment> VnPayPayments { get; set; }
            = new HashSet<VnPayPayment>();
    }
}