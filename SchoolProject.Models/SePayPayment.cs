using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class SePayPayment
    {
        public int Id { get; set; }

        // Liên kết học phí
        public int FeeId { get; set; }

        // Liên kết sinh viên
        public int StudentId { get; set; }

        // Mã hóa đơn gửi sang SePay
        public string OrderInvoiceNumber { get; set; }
            = string.Empty;

        // ID đơn hàng do SePay trả về
        public string? SePayOrderId { get; set; }

        // ID giao dịch do SePay trả về
        public string? TransactionId { get; set; }

        // Số tiền giao dịch
        public decimal Amount { get; set; }

        // Pending / Paid / Failed / Refunded
        public string Status { get; set; } = "Pending";

        // BANK_TRANSFER / CARD...
        public string? PaymentMethod { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        public DateTime? RefundedAt { get; set; }

        // Navigation
        public Fee Fee { get; set; } = null!;

        public Student Student { get; set; } = null!;
    }
}
