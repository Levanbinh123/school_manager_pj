using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;
using AcademicYearModel = SchoolProject.Models.AcademicYear;

namespace SchoolProject.Servies
{
    public class SessionService : ISessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================
        // 1. CREATE SESSION
        // =========================================
        public async Task<SessionViewModel> Add(
            CreateSessionViewModel vm)
        {
            ValidateDates(vm.Start, vm.End);

            if (vm.MaxCredits <= 0)
            {
                throw new ArgumentException(
                    "Số tín chỉ tối đa phải lớn hơn 0.");
            }

            if (vm.SemesterNumber <= 0)
            {
                throw new ArgumentException(
                    "Số thứ tự học kỳ phải lớn hơn 0.");
            }

            var academicYearRepository =
                _unitOfWork.GenericRepository<AcademicYearModel>();

            var academicYear = await academicYearRepository.GetByIdAsync(
                x => x.Id == vm.AcademicYearId);

            if (academicYear == null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy năm học.");
            }

            if (academicYear.Status == AcademicYearStatus.Completed ||
                academicYear.Status == AcademicYearStatus.Closed)
            {
                throw new InvalidOperationException(
                    "Không thể thêm học kỳ vào năm học đã hoàn thành hoặc đã đóng.");
            }

            // Học kỳ phải nằm trong khoảng thời gian năm học
            if (vm.Start.Date < academicYear.StartDate.Date ||
                vm.End.Date > academicYear.EndDate.Date)
            {
                throw new InvalidOperationException(
                    "Thời gian học kỳ phải nằm trong thời gian của năm học.");
            }

            var sessionRepository =
                _unitOfWork.GenericRepository<Session>();

            // Không trùng số thứ tự trong cùng năm học
            bool duplicatedNumber = sessionRepository.Exists(x =>
                x.AcademicYearId == vm.AcademicYearId &&
                x.SemesterNumber == vm.SemesterNumber);

            if (duplicatedNumber)
            {
                throw new InvalidOperationException(
                    "Số thứ tự học kỳ đã tồn tại trong năm học này.");
            }

            // Không cho hai học kỳ trùng thời gian
            bool overlapping = sessionRepository.Exists(x =>
                x.AcademicYearId == vm.AcademicYearId &&
                vm.Start.Date <= x.End.Date &&
                vm.End.Date >= x.Start.Date);

            if (overlapping)
            {
                throw new InvalidOperationException(
                    "Thời gian học kỳ bị trùng với học kỳ khác.");
            }

            var session = new Session
            {
                Name = vm.Name.Trim(),
                Start = vm.Start.Date,
                End = vm.End.Date,
                MaxCredits = vm.MaxCredits,

                // Chỉ gán khóa ngoại
                AcademicYearId = vm.AcademicYearId,

                SemesterNumber = vm.SemesterNumber,
                IsFinalSemester = vm.IsFinalSemester,
                Status = SessionStatus.Planned,
                IsPromoted = false,
                PromotedAt = null
            };

            await sessionRepository.AddAsync(session);
            await _unitOfWork.SaveAsync();

            return new SessionViewModel(session);
        }

        // =========================================
        // 2. UPDATE SESSION
        // =========================================
        public async Task<SessionViewModel> Update(
            int sessionId,
            UpdateSessionViewModel vm)
        {
            ValidateDates(vm.Start, vm.End);

            if (vm.MaxCredits <= 0)
            {
                throw new ArgumentException(
                    "Số tín chỉ tối đa phải lớn hơn 0.");
            }

            if (vm.SemesterNumber <= 0)
            {
                throw new ArgumentException(
                    "Số thứ tự học kỳ phải lớn hơn 0.");
            }

            var sessionRepository =
                _unitOfWork.GenericRepository<Session>();

            var session = await sessionRepository.GetByIdAsync(
                x => x.Id == sessionId,
                include: q => q.Include(x => x.AcademicYearInfo),
                disabledTracking: false);

            if (session == null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy học kỳ.");
            }

            if (session.Status != SessionStatus.Planned)
            {
                throw new InvalidOperationException(
                    "Chỉ được chỉnh sửa học kỳ ở trạng thái Planned.");
            }

            if (session.AcademicYearInfo == null)
            {
                throw new InvalidOperationException(
                    "Học kỳ chưa được liên kết với năm học.");
            }

            if (session.AcademicYearInfo.Status == AcademicYearStatus.Closed ||
                session.AcademicYearInfo.Status == AcademicYearStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Không thể sửa học kỳ thuộc năm học đã hoàn thành hoặc đã đóng.");
            }

            if (vm.Start.Date < session.AcademicYearInfo.StartDate.Date ||
                vm.End.Date > session.AcademicYearInfo.EndDate.Date)
            {
                throw new InvalidOperationException(
                    "Thời gian học kỳ phải nằm trong thời gian của năm học.");
            }

            bool duplicatedNumber = sessionRepository.Exists(x =>
                x.Id != sessionId &&
                x.AcademicYearId == session.AcademicYearId &&
                x.SemesterNumber == vm.SemesterNumber);

            if (duplicatedNumber)
            {
                throw new InvalidOperationException(
                    "Số thứ tự học kỳ đã tồn tại trong năm học này.");
            }

            bool overlapping = sessionRepository.Exists(x =>
                x.Id != sessionId &&
                x.AcademicYearId == session.AcademicYearId &&
                vm.Start.Date <= x.End.Date &&
                vm.End.Date >= x.Start.Date);

            if (overlapping)
            {
                throw new InvalidOperationException(
                    "Thời gian học kỳ bị trùng với học kỳ khác.");
            }

            session.Name = vm.Name.Trim();
            session.Start = vm.Start.Date;
            session.End = vm.End.Date;
            session.MaxCredits = vm.MaxCredits;
            session.SemesterNumber = vm.SemesterNumber;
            session.IsFinalSemester = vm.IsFinalSemester;

            sessionRepository.Update(session);

            await _unitOfWork.SaveAsync();

            return new SessionViewModel(session);
        }

        // =========================================
        // 3. GET ALL SESSIONS
        // =========================================
        public IEnumerable<SessionViewModel> GetAll()
        {
            var sessions = _unitOfWork
                .GenericRepository<Session>()
                .GetAll(
                    orderBy: q => q
                        .OrderByDescending(x => x.Start)
                        .ThenBy(x => x.SemesterNumber),
                    include: q => q.Include(x => x.AcademicYearInfo)
                );

            return sessions.Select(x => new SessionViewModel(x));
        }

        // =========================================
        // 4. GET SESSION BY ID
        // =========================================
        public async Task<SessionViewModel?> GetById(int sessionId)
        {
            var session = await _unitOfWork
                .GenericRepository<Session>()
                .GetByIdAsync(
                    x => x.Id == sessionId,
                    include: q => q.Include(x => x.AcademicYearInfo)
                );

            if (session == null)
            {
                return null;
            }

            return new SessionViewModel(session);
        }

        // =========================================
        // 5. GET SESSIONS BY ACADEMIC YEAR
        // =========================================
        public IEnumerable<SessionViewModel> GetByAcademicYear(
            int academicYearId)
        {
            var sessions = _unitOfWork
                .GenericRepository<Session>()
                .GetAll(
                    filter: x => x.AcademicYearId == academicYearId,
                    orderBy: q => q.OrderBy(x => x.SemesterNumber),
                    include: q => q.Include(x => x.AcademicYearInfo)
                );

            return sessions.Select(x => new SessionViewModel(x));
        }

        // =========================================
        // 6. ACTIVATE SESSION
        // =========================================
        public async Task Activate(int sessionId)
        {
            var sessionRepository =
                _unitOfWork.GenericRepository<Session>();

            var session = await sessionRepository.GetByIdAsync(
                x => x.Id == sessionId,
                include: q => q.Include(x => x.AcademicYearInfo),
                disabledTracking: false);

            if (session == null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy học kỳ.");
            }

            if (session.Status != SessionStatus.Planned)
            {
                throw new InvalidOperationException(
                    "Chỉ có thể kích hoạt học kỳ ở trạng thái Planned.");
            }

            if (session.AcademicYearInfo == null)
            {
                throw new InvalidOperationException(
                    "Học kỳ chưa được liên kết với năm học.");
            }

            if (session.AcademicYearInfo.Status != AcademicYearStatus.Active ||
                !session.AcademicYearInfo.IsCurrent)
            {
                throw new InvalidOperationException(
                    "Năm học chưa được kích hoạt.");
            }

            if (DateTime.Today < session.Start.Date)
            {
                throw new InvalidOperationException(
                    "Chưa đến ngày bắt đầu học kỳ.");
            }

            if (DateTime.Today > session.End.Date)
            {
                throw new InvalidOperationException(
                    "Học kỳ đã qua ngày kết thúc.");
            }

            bool hasOtherActiveSession =
                sessionRepository.Exists(x =>
                    x.Id != sessionId &&
                    x.AcademicYearId == session.AcademicYearId &&
                    x.Status == SessionStatus.Active);

            if (hasOtherActiveSession)
            {
                throw new InvalidOperationException(
                    "Đã có học kỳ khác đang hoạt động trong năm học này.");
            }

            session.Status = SessionStatus.Active;

            sessionRepository.Update(session);
            await _unitOfWork.SaveAsync();
        }

        // =========================================
        // 7. COMPLETE SESSION
        // =========================================

        public async Task Complete(int sessionId)
        {
            var sessionRepository =
                _unitOfWork.GenericRepository<Session>();

            var session = await sessionRepository.GetByIdAsync(
                x => x.Id == sessionId,
                disabledTracking: false);

            if (session == null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy học kỳ.");
            }

            if (session.Status != SessionStatus.Active)
            {
                throw new InvalidOperationException(
                    "Chỉ có thể hoàn thành học kỳ đang Active.");
            }

            if (DateTime.Today <= session.End.Date)
            {
                throw new InvalidOperationException(
                    "Chưa đến ngày hoàn thành học kỳ.");
            }

            // Kiểm tra toàn bộ dữ liệu trước khi kết thúc
            await ValidateSessionCanBeCompleted(sessionId);

            session.Status = SessionStatus.Completed;

            sessionRepository.Update(session);

            await _unitOfWork.SaveAsync();
        }
        private async Task ValidateSessionCanBeCompleted(int sessionId)
        {
            // 1. Lấy các lớp học phần thuộc học kỳ
            var classSessions = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAllAsync();

            var sessionClasses = classSessions
                .Where(x => x.SessionId == sessionId)
                .ToList();

            // 2. Tất cả lớp phải hoàn thành hoặc bị hủy
            bool hasUncompletedClass = sessionClasses.Any(x =>
                x.Status != ClassSessionStatus.Completed &&
                x.Status != ClassSessionStatus.Cancelled);

            if (hasUncompletedClass)
            {
                throw new InvalidOperationException(
                    "Không thể kết thúc học kỳ vì vẫn còn lớp học phần chưa hoàn thành hoặc chưa bị hủy.");
            }

            // 3. Lấy đăng ký của học kỳ
            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var sessionEnrolls = enrolls
                .Where(x => x.SessionId == sessionId)
                .ToList();

            // 4. Không cho kết thúc nếu còn đăng ký chưa xử lý
            bool hasUnprocessedEnroll = sessionEnrolls.Any(x =>
                x.Status == EnrollStatus.Registered ||
                x.Status == EnrollStatus.Approved);

            if (hasUnprocessedEnroll)
            {
                throw new InvalidOperationException(
                    "Không thể kết thúc học kỳ vì vẫn còn sinh viên chưa được xử lý đăng ký hoặc chưa tổng kết điểm.");
            }

            // 5. Kiểm tra kết quả tổng kết đã được chốt
            var completedEnrollIds = sessionEnrolls
                .Where(x =>
                    x.Status == EnrollStatus.Completed ||
                    x.Status == EnrollStatus.Failed)
                .Select(x => x.Id)
                .ToHashSet();

            var scores = await _unitOfWork
                .GenericRepository<StudentScore>()
                .GetAllAsync();

            var finalizedEnrollIds = scores
                .Where(x =>
                    completedEnrollIds.Contains(x.EnrollId) &&
                    x.IsFinalized)
                .Select(x => x.EnrollId)
                .ToHashSet();

            bool hasMissingFinalizedScore = completedEnrollIds
                .Any(id => !finalizedEnrollIds.Contains(id));

            if (hasMissingFinalizedScore)
            {
                throw new InvalidOperationException(
                    "Không thể kết thúc học kỳ vì vẫn còn sinh viên chưa có điểm tổng kết được chốt.");
            }
        }

        // =========================================
        // 8. DELETE SESSION
        // =========================================
        public async Task Delete(int sessionId)
        {
            var sessionRepository =
                _unitOfWork.GenericRepository<Session>();

            var session = await sessionRepository.GetByIdAsync(
                x => x.Id == sessionId,
                disabledTracking: false);

            if (session == null)
            {
                throw new KeyNotFoundException(
                    "Không tìm thấy học kỳ.");
            }

            if (session.Status != SessionStatus.Planned)
            {
                throw new InvalidOperationException(
                    "Chỉ được xóa học kỳ ở trạng thái Planned.");
            }

            bool hasClassSessions = _unitOfWork
                .GenericRepository<ClassSession>()
                .Exists(x => x.SessionId == sessionId);

            bool hasEnrollments = _unitOfWork
                .GenericRepository<Enroll>()
                .Exists(x => x.SessionId == sessionId);

            bool hasFees = _unitOfWork
                .GenericRepository<Fee>()
                .Exists(x => x.SessionId == sessionId);

            bool hasTuitionRates = _unitOfWork
                .GenericRepository<TuitionRate>()
                .Exists(x => x.SessionId == sessionId);

            bool hasTeacherSessions = _unitOfWork
                .GenericRepository<TeacherSession>()
                .Exists(x => x.SessionId == sessionId);

            bool hasSessionSubjects = _unitOfWork
                .GenericRepository<SessionSubject>()
                .Exists(x => x.SessionId == sessionId);

            if (hasClassSessions ||
                hasEnrollments ||
                hasFees ||
                hasTuitionRates ||
                hasTeacherSessions ||
                hasSessionSubjects)
            {
                throw new InvalidOperationException(
                    "Không thể xóa học kỳ vì đã có dữ liệu lớp học phần, đăng ký, học phí hoặc môn học liên quan.");
            }

            sessionRepository.Delete(session);

            await _unitOfWork.SaveAsync();
        }

        // =========================================
        // 9. VALIDATE DATES
        // =========================================
        private static void ValidateDates(
            DateTime start,
            DateTime end)
        {
            if (start == default || end == default)
            {
                throw new ArgumentException(
                    "Ngày bắt đầu hoặc ngày kết thúc không hợp lệ.");
            }

            if (end.Date <= start.Date)
            {
                throw new ArgumentException(
                    "Ngày kết thúc phải sau ngày bắt đầu.");
            }
        }
    }
}