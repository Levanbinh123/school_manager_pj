namespace SchoolProject.Models
{
    public class VnPayPayment
    {
        public int Id { get; set; }

        public int FeeId { get; set; }

        public int StudentId { get; set; }

        // Mã giao dịch do hệ thống của mình tạo
        public string TxnRef { get; set; } = string.Empty;

        // Số tiền thanh toán
        public decimal Amount { get; set; }

        // Pending / Paid / Failed
        public string Status { get; set; } = "Pending";

        // Mã giao dịch VNPAY trả về
        public string? TransactionNo { get; set; }

        // Ngân hàng
        public string? BankCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        public Fee Fee { get; set; } = null!;

        public Student Student { get; set; } = null!;
    }
}