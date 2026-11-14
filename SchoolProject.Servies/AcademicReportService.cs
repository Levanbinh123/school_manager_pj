
using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;
using AcademicYearModel = SchoolProject.Models.AcademicYear;

namespace SchoolProject.Servies
{
    public class AcademicReportService : IAcademicReportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AcademicReportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================
        // 1. BÁO CÁO HỌC KỲ
        // =========================================
        public async Task<SessionReportViewModel> GetSessionReport(
            int sessionId)
        {
            var session = await _unitOfWork
                .GenericRepository<Session>()
                .GetByIdAsync(
                    x => x.Id == sessionId,
                    include: q => q.Include(x => x.AcademicYearInfo));

            if (session == null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy học kỳ.");
            }

            var classes = (await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAllAsync())
                .Where(x => x.SessionId == sessionId)
                .ToList();

            var enrollments = (await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync())
                .Where(x => x.SessionId == sessionId)
                .ToList();

            var enrollIds = enrollments
                .Select(x => x.Id)
                .ToHashSet();

            var scores = (await _unitOfWork
                .GenericRepository<StudentScore>()
                .GetAllAsync())
                .Where(x =>
                    enrollIds.Contains(x.EnrollId) &&
                    x.IsFinalized)
                .ToList();

            // Lấy mỗi kết quả tổng kết của một lượt đăng ký
            var finalizedScores = scores
                .GroupBy(x => x.EnrollId)
                .Select(x => x.First())
                .ToList();

            var passedScores = finalizedScores
                .Where(x => x.TotalScore >= 4.0)
                .ToList();

            var failedScores = finalizedScores
                .Where(x => x.TotalScore < 4.0)
                .ToList();

            int totalGraded = finalizedScores.Count;

            return new SessionReportViewModel
            {
                SessionId = session.Id,
                SessionName = session.Name,
                AcademicYear = session.AcademicYearInfo == null
                    ? string.Empty
                    : $"{session.AcademicYearInfo.StartDate.Year}-{session.AcademicYearInfo.EndDate.Year}",

                SemesterNumber = session.SemesterNumber,
                Status = session.Status.ToString(),

                // Lớp học phần
                TotalClasses = classes.Count,

                CompletedClasses = classes.Count(x =>
                    x.Status == ClassSessionStatus.Completed),

                CancelledClasses = classes.Count(x =>
                    x.Status == ClassSessionStatus.Cancelled),

                InProgressClasses = classes.Count(x =>
                    x.Status == ClassSessionStatus.InProgress),

                // Đăng ký
                TotalEnrollments = enrollments.Count,

                TotalStudents = enrollments
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count(),

                ApprovedEnrollments = enrollments.Count(x =>
                    x.Status == EnrollStatus.Approved),

                CompletedStudents = enrollments.Count(x =>
                    x.Status == EnrollStatus.Completed),

                FailedStudents = enrollments.Count(x =>
                    x.Status == EnrollStatus.Failed),

                DroppedStudents = enrollments.Count(x =>
                    x.Status == EnrollStatus.Dropped),

                PaymentExpiredStudents = enrollments.Count(x =>
                    x.Status == EnrollStatus.PaymentExpired),

                PendingEnrollments = enrollments.Count(x =>
                    x.Status == EnrollStatus.Registered ||
                    x.Status == EnrollStatus.Approved),

                // Điểm
                TotalFinalizedScores = totalGraded,

                MissingFinalizedScores = enrollments.Count(x =>
                    x.Status == EnrollStatus.Approved &&
                    !finalizedScores.Any(s => s.EnrollId == x.Id)),

                AverageTotalScore = totalGraded > 0
                    ? Math.Round(
                        finalizedScores.Average(x => x.TotalScore), 2)
                    : null,

                AverageGPA = totalGraded > 0
                    ? Math.Round(
                        finalizedScores.Average(x => x.GPA), 2)
                    : null,

                PassedStudents = passedScores.Count,
                FailedScoreStudents = failedScores.Count,

                PassRate = totalGraded > 0
                    ? Math.Round(
                        passedScores.Count * 100.0 / totalGraded, 2)
                    : 0,

                FailRate = totalGraded > 0
                    ? Math.Round(
                        failedScores.Count * 100.0 / totalGraded, 2)
                    : 0
            };
        }

        // =========================================
        // 2. BÁO CÁO NĂM HỌC
        // =========================================
        public async Task<AcademicYearReportViewModel> GetAcademicYearReport(
            int academicYearId)
        {
            var academicYear = await _unitOfWork
                .GenericRepository<AcademicYearModel>()
                .GetByIdAsync(
                    x => x.Id == academicYearId,
                    include: q => q.Include(x => x.Sessions));

            if (academicYear == null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy năm học.");
            }

            var sessionReports = new List<SessionReportViewModel>();

            foreach (var session in academicYear.Sessions
                .OrderBy(x => x.SemesterNumber))
            {
                var report = await GetSessionReport(session.Id);
                sessionReports.Add(report);
            }

            var sessionIds = academicYear.Sessions
                .Select(x => x.Id)
                .ToHashSet();

            var enrollments = (await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync())
                .Where(x => sessionIds.Contains(x.SessionId))
                .ToList();

            var enrollIds = enrollments
                .Select(x => x.Id)
                .ToHashSet();

            var finalizedScores = (await _unitOfWork
                .GenericRepository<StudentScore>()
                .GetAllAsync())
                .Where(x =>
                    enrollIds.Contains(x.EnrollId) &&
                    x.IsFinalized)
                .GroupBy(x => x.EnrollId)
                .Select(x => x.First())
                .ToList();

            int totalGraded = finalizedScores.Count;

            var passedScores = finalizedScores
                .Where(x => x.TotalScore >= 4.0)
                .ToList();

            var failedScores = finalizedScores
                .Where(x => x.TotalScore < 4.0)
                .ToList();

            return new AcademicYearReportViewModel
            {
                AcademicYearId = academicYear.Id,

                AcademicYearName =
                    $"{academicYear.StartDate.Year}-{academicYear.EndDate.Year}",

                StartDate = academicYear.StartDate,
                EndDate = academicYear.EndDate,
                Status = academicYear.Status.ToString(),

                TotalSessions = academicYear.Sessions.Count,

                CompletedSessions = academicYear.Sessions.Count(x =>
                    x.Status == SessionStatus.Completed),

                TotalClasses = sessionReports.Sum(x => x.TotalClasses),

                TotalStudents = enrollments
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count(),

                TotalEnrollments = enrollments.Count,

                PassedStudents = passedScores.Count,
                FailedStudents = failedScores.Count,

                DroppedStudents = enrollments.Count(x =>
                    x.Status == EnrollStatus.Dropped),

                PaymentExpiredStudents = enrollments.Count(x =>
                    x.Status == EnrollStatus.PaymentExpired),

                AverageTotalScore = totalGraded > 0
                    ? Math.Round(
                        finalizedScores.Average(x => x.TotalScore), 2)
                    : null,

                AverageGPA = totalGraded > 0
                    ? Math.Round(
                        finalizedScores.Average(x => x.GPA), 2)
                    : null,

                PassRate = totalGraded > 0
                    ? Math.Round(
                        passedScores.Count * 100.0 / totalGraded, 2)
                    : 0,

                Sessions = sessionReports
            };
        }
    }
}
