
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class TuitionReportService : ITuitionReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TuitionReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // 1. BÁO CÁO TỔNG QUAN HỌC PHÍ
        // ============================================================

        public async Task<TuitionReportViewModel> GetTuitionOverview()
        {
            var fees = await _unitOfWork
                .GenericRepository<Fee>()
                .GetAllAsync();

            var sePayPayments = await _unitOfWork
                .GenericRepository<SePayPayment>()
                .GetAllAsync();

            // Hóa đơn của lớp đã hủy không còn là khoản phải thu.
            var activeFees = fees
                .Where(x => x.Status != "Cancelled")
                .ToList();

            var paidFees = activeFees
                .Where(x => x.Status == "Paid")
                .ToList();

            var unpaidFees = activeFees
                .Where(x => x.Status == "Unpaid")
                .ToList();

            var pendingFees = activeFees
                .Where(x => x.Status == "Pending")
                .ToList();

            // Hóa đơn quá hạn:
            // Chưa thanh toán và đã vượt DueDate
            var overdueFees = activeFees
                .Where(x =>
                    x.Status != "Paid" &&
                    x.DueDate < DateTime.Now)
                .ToList();

            var result = new TuitionReportViewModel
            {
                // Tổng quan
                TotalFees = activeFees.Count,

                TotalStudents = activeFees
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count(),

                TotalAmount = activeFees.Sum(x => x.Amount),

                TotalLateFee = activeFees.Sum(x => x.LateFee),

                TotalExpectedAmount = activeFees.Sum(x => x.TotalAmount),

                // Trạng thái hóa đơn
                PaidFees = paidFees.Count,

                UnpaidFees = unpaidFees.Count,

                PendingFees = pendingFees.Count,

                OverdueFees = overdueFees.Count,

                // Tiền học phí
                TotalPaidAmount = paidFees.Sum(x => x.TotalAmount),

                TotalUnpaidAmount = activeFees
                    .Where(x => x.Status != "Paid")
                    .Sum(x => x.TotalAmount),

                TotalOverdueAmount = overdueFees
                    .Sum(x => x.TotalAmount),

                // Dòng tiền
                TotalHeldAmount = activeFees
                    .Where(x =>
                        x.FundsStatus == "Held" &&
                        x.Status == "Paid")
                    .Sum(x => x.TotalAmount),

                TotalTransferredAmount = activeFees
                    .Where(x =>
                        x.FundsStatus == "Transferred")
                    .Sum(x => x.TotalAmount),

                TotalTransferPendingAmount = activeFees
                    .Where(x =>
                        x.FundsStatus == "TransferPending")
                    .Sum(x => x.TotalAmount),

                TotalRefundPendingAmount = activeFees
                    .Where(x =>
                        x.FundsStatus == "RefundPending")
                    .Sum(x => x.TotalAmount),

                TotalRefundedAmount = activeFees
                    .Where(x =>
                        x.FundsStatus == "Refunded")
                    .Sum(x => x.TotalAmount),

                // SePay
                SePayPaymentCount = sePayPayments
                    .Count(x => x.Status == "Paid"),

                SePayPaidAmount = sePayPayments
                    .Where(x => x.Status == "Paid")
                    .Sum(x => x.Amount),

                GeneratedAt = DateTime.Now
            };

            return result;
        }


        // ============================================================
        // 2. BÁO CÁO HỌC PHÍ THEO HỌC KỲ
        // ============================================================

        public async Task<TuitionSessionReportViewModel> GetTuitionBySession(
            int sessionId)
        {
            // Kiểm tra Session
            var session = await _unitOfWork
                .GenericRepository<Session>()
                .GetByIdAsync(x => x.Id == sessionId);

            if (session == null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy học kỳ.");
            }

            // Lấy học phí của Session
            var fees = (await _unitOfWork
                .GenericRepository<Fee>()
                .GetAllAsync())
                .Where(x => x.SessionId == sessionId)
                .ToList();

            var activeFees = fees
                .Where(x => x.Status != "Cancelled")
                .ToList();

            var paidFees = activeFees
                .Where(x => x.Status == "Paid")
                .ToList();

            var unpaidFees = activeFees
                .Where(x => x.Status != "Paid")
                .ToList();

            var overdueFees = activeFees
                .Where(x =>
                    x.Status != "Paid" &&
                    x.DueDate < DateTime.Now)
                .ToList();

            var result = new TuitionSessionReportViewModel
            {
                SessionId = session.Id,

                SessionName = session.Name,

                AcademicYear = session.AcademicYear,

                SemesterNumber = session.SemesterNumber,

                TotalFees = activeFees.Count,

                TotalStudents = activeFees
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count(),

                TotalAmount = activeFees.Sum(x => x.Amount),

                TotalExpectedAmount = activeFees.Sum(x => x.TotalAmount),

                PaidFees = paidFees.Count,

                UnpaidFees = unpaidFees.Count,

                OverdueFees = overdueFees.Count,

                TotalPaidAmount = paidFees
                    .Sum(x => x.TotalAmount),

                TotalUnpaidAmount = unpaidFees
                    .Sum(x => x.TotalAmount),

                TotalHeldAmount = activeFees
                    .Where(x =>
                        x.Status == "Paid" &&
                        x.FundsStatus == "Held")
                    .Sum(x => x.TotalAmount),

                TotalTransferredAmount = activeFees
                    .Where(x =>
                        x.FundsStatus == "Transferred")
                    .Sum(x => x.TotalAmount)
            };

            return result;
        }
    }
}
