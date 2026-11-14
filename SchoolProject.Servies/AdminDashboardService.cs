
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;
using System.Linq;

namespace SchoolProject.Servies
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminDashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminDashboardViewModel> GetDashboard()
        {
            // 1. Lấy dữ liệu từ database
            var students = await _unitOfWork
                .GenericRepository<Student>()
                .GetAllAsync();

            var teachers = await _unitOfWork
                .GenericRepository<Teacher>()
                .GetAllAsync();

            var classes = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAllAsync();

            var sessions = await _unitOfWork
                .GenericRepository<Session>()
                .GetAllAsync();

            var fees = await _unitOfWork
                .GenericRepository<Fee>()
                .GetAllAsync();

            // 2. Khởi tạo Dashboard
            var dashboard = new AdminDashboardViewModel
            {
                GeneratedAt = DateTime.Now
            };

            // 3. Thống kê sinh viên
            // Không tính sinh viên đã bị xóa mềm
            var activeStudents = students
                .Where(x => !x.IsDeleted)
                .ToList();

            dashboard.TotalStudents = activeStudents.Count;

            dashboard.StudyingStudents = activeStudents.Count(
                x => x.Status == StudentStatus.Studying);

            dashboard.SuspendedStudents = activeStudents.Count(
                x => x.Status == StudentStatus.Suspended);

            dashboard.OnLeaveStudents = activeStudents.Count(
                x => x.Status == StudentStatus.OnLeave);

            dashboard.DroppedOutStudents = activeStudents.Count(
                x => x.Status == StudentStatus.DroppedOut);

            dashboard.GraduatedStudents = activeStudents.Count(
                x => x.Status == StudentStatus.Graduated);

            // 4. Thống kê giảng viên
            dashboard.TotalTeachers = teachers.Count();

            // 5. Thống kê lớp học
            dashboard.TotalClasses = classes.Count();

            dashboard.PlannedClasses = classes.Count(
                x => x.Status == ClassSessionStatus.Planned);

            // Các trạng thái thuộc giai đoạn đăng ký và thanh toán
            dashboard.RegistrationClasses = classes.Count(x =>
                x.Status == ClassSessionStatus.OpenForRegistration ||
                x.Status == ClassSessionStatus.Full ||
                x.Status == ClassSessionStatus.RegistrationClosed ||
                x.Status == ClassSessionStatus.AwaitingPaymentApproval ||
                x.Status == ClassSessionStatus.PaymentOpen ||
                x.Status == ClassSessionStatus.PaymentClosed);

            dashboard.ConfirmedClasses = classes.Count(
                x => x.Status == ClassSessionStatus.Confirmed);

            dashboard.InProgressClasses = classes.Count(
                x => x.Status == ClassSessionStatus.InProgress);

            dashboard.CompletedClasses = classes.Count(
                x => x.Status == ClassSessionStatus.Completed);

            dashboard.CancelledClasses = classes.Count(
                x => x.Status == ClassSessionStatus.Cancelled);

            // 6. Thống kê học kỳ
            dashboard.TotalSessions = sessions.Count();

            dashboard.ActiveSessions = sessions.Count(
                x => x.Status == SessionStatus.Active);

            dashboard.CompletedSessions = sessions.Count(
                x => x.Status == SessionStatus.Completed);

            // 7. Thống kê học phí
            dashboard.TotalFees = fees.Count();

            var paidFees = fees
                .Where(x => x.Status == "Paid")
                .ToList();

            dashboard.PaidFees = paidFees.Count;

            // Bao gồm tất cả khoản phí chưa có trạng thái Paid
            dashboard.UnpaidFees = fees.Count(
                x => x.Status != "Paid");

            // Tổng số tiền học phí phải thu
            dashboard.TotalExpectedAmount = fees.Sum(
                x => x.TotalAmount);

            // Tổng số tiền sinh viên đã thanh toán
            dashboard.TotalPaidAmount = paidFees.Sum(
                x => x.TotalAmount);

            // Tiền đã thu nhưng vẫn đang được giữ
            dashboard.TotalHeldAmount = fees
                .Where(x =>
                    x.Status == "Paid" &&
                    x.FundsStatus == "Held")
                .Sum(x => x.TotalAmount);

            // Tiền đã chuyển về tài khoản nhà trường
            dashboard.TotalTransferredAmount = fees
                .Where(x => x.FundsStatus == "Transferred")
                .Sum(x => x.TotalAmount);

            return dashboard;
        }
    }
}
