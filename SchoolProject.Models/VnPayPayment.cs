namespace SchoolProject.Models
{
    /*
       * Trạng thái giao dịch thanh toán:
       *
       * Pending       : Đã tạo giao dịch và URL thanh toán,
       *                 đang chờ VNPay xác nhận kết quả.
       *
       * Paid          : Thanh toán thành công,
       *                 VNPay đã xác nhận qua IPN hợp lệ.
       *
       * Failed        : Giao dịch thanh toán thất bại,
       *                 bị từ chối hoặc không thành công.
       *
       * RefundPending : Giao dịch đã thanh toán thành công,
       *                 đang chờ xử lý hoàn tiền.
       *
       * Refunded      : Đã hoàn tiền thành công cho sinh viên.
       */
    public class VnPayPayment
    {
        public int Id { get; set; }

        public int FeeId { get; set; }

        public int StudentId { get; set; }

        public string TxnRef { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        // Pending / Paid / Failed
        // RefundPending / Refunded
        public string Status { get; set; } = "Pending";

        public string? TransactionNo { get; set; }

        public string? BankCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        // Thông tin hoàn tiền
        public string? RefundTransactionNo { get; set; }

        public DateTime? RefundedAt { get; set; }

        public Fee Fee { get; set; } = null!;

        public Student Student { get; set; } = null!;
    }
}