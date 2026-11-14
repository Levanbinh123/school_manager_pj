using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Models
{
    public class CashPayment
    {
        public int Id { get; set; }

        public int FeeId { get; set; }

        public int StudentId { get; set; }

        // Mã biên lai
        public string ReceiptCode { get; set; }
            = string.Empty;

        // Số tiền thực tế thu
        public decimal Amount { get; set; }

        // Người xác nhận thu tiền
        public string ReceivedByUserId { get; set; }
            = string.Empty;

        public DateTime ReceivedAt { get; set; }
            = DateTime.Now;

        public string? Note { get; set; }

        public Fee Fee { get; set; } = null!;

        public Student Student { get; set; } = null!;
    }
}
