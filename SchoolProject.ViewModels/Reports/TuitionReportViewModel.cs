
namespace SchoolProject.ViewModels
{
    // ============================================================
    // 1. BÁO CÁO TỔNG QUAN HỌC PHÍ
    // ============================================================

    public class TuitionReportViewModel
    {
        // Tổng số hóa đơn
        public int TotalFees { get; set; }

        // Tổng số sinh viên có hóa đơn
        public int TotalStudents { get; set; }

        // Tổng học phí gốc
        public decimal TotalAmount { get; set; }

        // Tổng tiền phạt
        public decimal TotalLateFee { get; set; }

        // Tổng số tiền cần thu
        public decimal TotalExpectedAmount { get; set; }

        // Đã thanh toán
        public int PaidFees { get; set; }

        // Chưa thanh toán
        public int UnpaidFees { get; set; }

        // Đang chờ thanh toán
        public int PendingFees { get; set; }

        // Quá hạn
        public int OverdueFees { get; set; }

        // Tổng tiền thực tế đã thu
        public decimal TotalPaidAmount { get; set; }

        // Tổng tiền chưa thu
        public decimal TotalUnpaidAmount { get; set; }

        // Tổng tiền đang quá hạn
        public decimal TotalOverdueAmount { get; set; }

        // ========================================================
        // QUẢN LÝ DÒNG TIỀN
        // ========================================================

        // Tiền đang giữ
        public decimal TotalHeldAmount { get; set; }

        // Tiền đã chuyển cho nhà trường
        public decimal TotalTransferredAmount { get; set; }

        // Tiền đang chờ chuyển
        public decimal TotalTransferPendingAmount { get; set; }

        // Tiền đang chờ hoàn
        public decimal TotalRefundPendingAmount { get; set; }

        // Tiền đã hoàn
        public decimal TotalRefundedAmount { get; set; }

        // ========================================================
        // THỐNG KÊ PHƯƠNG THỨC THANH TOÁN
        // ========================================================

        public int SePayPaymentCount { get; set; }

        public decimal SePayPaidAmount { get; set; }

        // Thời điểm lập báo cáo
        public DateTime GeneratedAt { get; set; } = DateTime.Now;
    }


    // ============================================================
    // 2. BÁO CÁO HỌC PHÍ THEO HỌC KỲ
    // ============================================================

    public class TuitionSessionReportViewModel
    {
        public int SessionId { get; set; }

        public string SessionName { get; set; } = string.Empty;

        public string AcademicYear { get; set; } = string.Empty;

        public int SemesterNumber { get; set; }

        public int TotalFees { get; set; }

        public int TotalStudents { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal TotalExpectedAmount { get; set; }

        public int PaidFees { get; set; }

        public int UnpaidFees { get; set; }

        public int OverdueFees { get; set; }

        public decimal TotalPaidAmount { get; set; }

        public decimal TotalUnpaidAmount { get; set; }

        public decimal TotalHeldAmount { get; set; }

        public decimal TotalTransferredAmount { get; set; }
    }
}
