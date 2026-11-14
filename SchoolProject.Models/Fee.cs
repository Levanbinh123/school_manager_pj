namespace SchoolProject.Models
{
    public class Fee
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public int SessionId { get; set; }

        public int Credits { get; set; }

        // Giá một tín chỉ tại thời điểm tạo học phí
        public decimal PricePerCredit { get; set; }

        // Học phí gốc
        public decimal Amount { get; set; }

        // Tiền phạt thanh toán trễ
        public decimal LateFee { get; set; } = 0;

        // Tổng tiền phải thanh toán
        public decimal TotalAmount { get; set; }
        public DateTime PaymentStartDate { get; set; }

        public DateTime DueDate { get; set; }

        // Unpaid / Pending / Paid / Overdue
        public string Status { get; set; } = "Unpaid";

        // NotReceived / Held / TransferPending /
        // Transferred / RefundPending / Refunded
        public string FundsStatus { get; set; } = "NotReceived";

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public DateTime? PaidAt { get; set; }

        public DateTime? TransferredAt { get; set; }

        public DateTime? RefundedAt { get; set; }

        public Student Student { get; set; } = null!;

        public Subject Subject { get; set; } = null!;

        public Session Session { get; set; } = null!;

        public ICollection<VnPayPayment> VnPayPayments { get; set; }
            = new HashSet<VnPayPayment>();
        public ICollection<SePayPayment> SePayPayments { get; set; }
    = new HashSet<SePayPayment>();

    }
}