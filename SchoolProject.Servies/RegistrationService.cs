using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegistrationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        private static bool HoldsSeat(Enroll enroll)
        {
            return enroll.Status != EnrollStatus.Dropped &&
                   enroll.Status != EnrollStatus.PaymentExpired;
        }

        // ============================================================
        // 1. GET AVAILABLE CLASS SESSIONS
        // ============================================================
        public async Task<List<ClassSessionViewModel>> GetAvailableClassSessions(
            int studentId,
            int sessionId)
        {
            var student = await _unitOfWork.GenericRepository<Student>()
                .GetByIdAsync(x => x.Id == studentId);

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên.");

            var session = await _unitOfWork.GenericRepository<Session>()
                .GetByIdAsync(x => x.Id == sessionId);

            if (session == null)
                throw new Exception("Không tìm thấy Session.");

            DateTime now = DateTime.Now;

            var enrolls = _unitOfWork.GenericRepository<Enroll>()
                .GetAll(x => x.StudentId == studentId && x.SessionId == sessionId)
                .ToList();

            // Chỉ chặn môn đang giữ chỗ hoặc đã có kết quả.
            // Dropped/PaymentExpired có thể được đăng ký lại bằng cách tái sử dụng Enroll cũ.
            var unavailableSubjectIds = enrolls
                .Where(x => HoldsSeat(x) ||
                            x.Status == EnrollStatus.Completed ||
                            x.Status == EnrollStatus.Failed)
                .Select(x => x.SubjectId)
                .ToHashSet();

            var classSessions = _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAll(
                    x => x.SessionId == sessionId,
                    include: query => query
                        .Include(x => x.Class)
                        .Include(x => x.Subject).ThenInclude(x => x!.MajorSubjects)
                        .Include(x => x.HomeroomTeacher)
                        .Include(x => x.Enrolls))
                .ToList();

            var result = new List<ClassSessionViewModel>();

            foreach (var classSession in classSessions)
            {
                if (classSession.Status != ClassSessionStatus.OpenForRegistration)
                    continue;

                if (now < classSession.RegistrationStart ||
                    now >= classSession.RegistrationEnd)
                    continue;

                if (classSession.Subject == null)
                    continue;

                int actualCount = classSession.Enrolls?
                    .Where(HoldsSeat)
                    .Select(x => x.StudentId)
                    .Distinct()
                    .Count() ?? 0;

                if (actualCount >= classSession.MaxStudents)
                    continue;

                var majorSubject = classSession.Subject.MajorSubjects
                    .FirstOrDefault(x => x.MajorId == student.MajorId);

                if (majorSubject == null || majorSubject.YearLevel > student.YearLevel)
                    continue;

                if (unavailableSubjectIds.Contains(classSession.SubjectId))
                    continue;

                //bool isOpenInSession = _unitOfWork
                //    .GenericRepository<SessionSubject>()
                //    .Exists(x => x.SessionId == sessionId &&
                //                 x.SubjectId == classSession.SubjectId &&
                //                 x.IsOpen);

                //if (!isOpenInSession)
                //    continue;

                result.Add(new ClassSessionViewModel
                {
                    Id = classSession.Id,
                    ClassId = classSession.ClassId,
                    ClassName = classSession.Class?.Name,
                    SubjectId = classSession.SubjectId,
                    SubjectName = classSession.Subject.Name,
                    SessionId = classSession.SessionId,
                    HomeroomTeacherId = classSession.HomeroomTeacherId,
                    HomeroomTeacherName = classSession.HomeroomTeacher == null
                        ? null
                        : $"{classSession.HomeroomTeacher.FirstName} {classSession.HomeroomTeacher.LastName}"
                });
            }

            return result;
        }

        // ============================================================
        // 2. REGISTER CLASS SESSION + CREATE/REUSE FEE
        // ============================================================
        public async Task<bool> RegisterClassSession(int studentId, int classSessionId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            var student = await _unitOfWork.GenericRepository<Student>()
                .GetByIdAsync(x => x.Id == studentId);

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên.");

            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(
                    x => x.Id == classSessionId,
                    include: query => query
                        .Include(x => x.Subject).ThenInclude(x => x!.MajorSubjects)
                        .Include(x => x.Session)
                        .Include(x => x.Enrolls));

            if (classSession == null)
                throw new Exception("Không tìm thấy ClassSession.");

            if (classSession.Subject == null || classSession.Session == null)
                throw new Exception("ClassSession chưa được cấu hình môn học hoặc học kỳ.");

            DateTime now = DateTime.Now;

            if (classSession.Status != ClassSessionStatus.OpenForRegistration)
                throw new Exception("Lớp học phần hiện không mở đăng ký.");

            if (now < classSession.RegistrationStart || now >= classSession.RegistrationEnd)
                throw new Exception("Hiện không nằm trong thời hạn đăng ký lớp học phần.");

            var enrollRepository = _unitOfWork.GenericRepository<Enroll>();
            var feeRepository = _unitOfWork.GenericRepository<Fee>();
            var classRepository = _unitOfWork.GenericRepository<ClassSession>();

            var currentEnrolls = enrollRepository
                .GetAll(x => x.ClassSessionId == classSessionId)
                .ToList();

            int actualCount = currentEnrolls
                .Where(HoldsSeat)
                .Select(x => x.StudentId)
                .Distinct()
                .Count();

            if (actualCount >= classSession.MaxStudents)
                throw new Exception("Lớp học phần đã đủ số lượng sinh viên.");

            var subject = classSession.Subject;
            var session = classSession.Session;

            var majorSubject = subject.MajorSubjects
                .FirstOrDefault(x => x.MajorId == student.MajorId);

            if (majorSubject == null)
                throw new Exception("Môn học không thuộc chuyên ngành của sinh viên.");

            if (majorSubject.YearLevel > student.YearLevel)
                throw new Exception("Sinh viên chưa đủ năm để đăng ký môn này.");

            //bool sessionSubjectIsOpen = _unitOfWork
            //    .GenericRepository<SessionSubject>()
            //    .Exists(x => x.SessionId == session.Id &&
            //                 x.SubjectId == subject.Id &&
            //                 x.IsOpen);

            //if (!sessionSubjectIsOpen)
            //    throw new Exception("Môn học chưa mở đăng ký trong học kỳ này.");

            // Unique index StudentId + SubjectId + SessionId:
            // tái sử dụng Enroll đã Dropped/PaymentExpired thay vì tạo bản ghi thứ hai.
            var existingEnroll = await enrollRepository.GetByIdAsync(
                x => x.StudentId == studentId &&
                     x.SubjectId == subject.Id &&
                     x.SessionId == session.Id,
                include: query => query
                    .Include(x => x.Attendances)
                    .Include(x => x.StudentScores));

            if (existingEnroll != null)
            {
                if (existingEnroll.Status != EnrollStatus.Dropped &&
                    existingEnroll.Status != EnrollStatus.PaymentExpired)
                    throw new Exception("Sinh viên đã đăng ký môn học này.");

                if (existingEnroll.Attendances.Any() || existingEnroll.StudentScores.Any())
                    throw new Exception("Không thể đăng ký lại vì đăng ký cũ đã phát sinh điểm danh hoặc điểm số.");
            }

            // Tính tín chỉ của các môn còn hiệu lực trong học kỳ.
            var currentStudentEnrolls = enrollRepository
                .GetAll(
                    x => x.StudentId == studentId && x.SessionId == session.Id,
                    include: query => query.Include(x => x.Subject))
                .Where(HoldsSeat)
                .ToList();

            int currentCredits = currentStudentEnrolls
                .Where(x => x.Subject != null)
                .Sum(x => x.Subject!.Credits);

            if (subject.Credits <= 0)
                throw new Exception("Số tín chỉ của môn học không hợp lệ.");

            int totalCredits = currentCredits + subject.Credits;
            if (totalCredits > session.MaxCredits)
            {
                throw new Exception(
                    $"Bạn đã đăng ký {currentCredits} tín chỉ. Môn {subject.Name} có {subject.Credits} tín chỉ. " +
                    $"Tổng {totalCredits} tín chỉ vượt quá giới hạn {session.MaxCredits} tín chỉ của {session.Name}.");
            }

            var tuitionRate = _unitOfWork.GenericRepository<TuitionRate>()
                .GetAll(x => x.SessionId == session.Id)
                .FirstOrDefault();

            if (tuitionRate == null || tuitionRate.PricePerCredit <= 0)
                throw new Exception("Học kỳ này chưa được cấu hình đơn giá tín chỉ hợp lệ.");

            var existingFee = await feeRepository.GetByIdAsync(
                x => x.StudentId == studentId &&
                     x.SubjectId == subject.Id &&
                     x.SessionId == session.Id);

            if (existingFee != null &&
                (existingFee.Status != "Unpaid" || existingFee.FundsStatus != "NotReceived"))
            {
                throw new Exception(
                    "Học phí của môn này đã phát sinh giao dịch hoặc đã thanh toán; không thể tạo đăng ký mới tự động.");
            }

            if (existingEnroll == null)
            {
                existingEnroll = new Enroll
                {
                    StudentId = studentId,
                    SubjectId = subject.Id,
                    SessionId = session.Id,
                    ClassSessionId = classSession.Id,
                    Status = EnrollStatus.Registered,
                    RegisteredAt = now
                };
                await enrollRepository.AddAsync(existingEnroll);
            }
            else
            {
                existingEnroll.ClassSessionId = classSession.Id;
                existingEnroll.Status = EnrollStatus.Registered;
                existingEnroll.RegisteredAt = now;
                enrollRepository.Update(existingEnroll);
            }

            if (existingFee == null)
            {
                decimal amount = subject.Credits * tuitionRate.PricePerCredit;

                var fee = new Fee
                {
                    StudentId = studentId,
                    SubjectId = subject.Id,
                    SessionId = session.Id,
                    Credits = subject.Credits,
                    PricePerCredit = tuitionRate.PricePerCredit,
                    Amount = amount,
                    LateFee = 0,
                    TotalAmount = amount,
                    // Ngày thanh toán chính thức sẽ được ClassService thiết lập
                    // khi Admin mở đợt thanh toán sau khi đóng đăng ký.
                    PaymentStartDate = classSession.PaymentStartDate ?? classSession.RegistrationEnd,
                    DueDate = classSession.PaymentEndDate ?? classSession.RegistrationEnd.AddDays(7),
                    Status = "Unpaid",
                    FundsStatus = "NotReceived"
                };

                await feeRepository.AddAsync(fee);
            }

            classSession.EnrolledCount = actualCount + 1;
            if (classSession.EnrolledCount >= classSession.MaxStudents)
                classSession.Status = ClassSessionStatus.Full;

            classRepository.Update(classSession);

            await _unitOfWork.SaveAsync();
            await transaction.CommitAsync();

            return true;
        }

        // ============================================================
        // 3. GET MY ENROLLMENTS
        // ============================================================
        public async Task<List<EnrollViewModel>> GetMyEnrollments(int studentId, int sessionId)
        {
            var studentExists = _unitOfWork.GenericRepository<Student>()
                .Exists(x => x.Id == studentId);

            if (!studentExists)
                throw new Exception("Không tìm thấy sinh viên.");

            var sessionExists = _unitOfWork.GenericRepository<Session>()
                .Exists(x => x.Id == sessionId);

            if (!sessionExists)
                throw new Exception("Không tìm thấy Session.");

            var enrolls = await _unitOfWork.GenericRepository<Enroll>()
                .GetAllAsync(
                    include: query => query
                        .Include(x => x.Subject)
                        .Include(x => x.Session)
                        .Include(x => x.ClassSession).ThenInclude(x => x!.Class)
                        .Include(x => x.ClassSession).ThenInclude(x => x!.HomeroomTeacher),
                    filter: x => x.StudentId == studentId && x.SessionId == sessionId);

            return enrolls.Select(x => new EnrollViewModel(x)).ToList();
        }

        // ============================================================
        // 4. CANCEL REGISTRATION
        // ============================================================
        public async Task<bool> CancelRegistration(int studentId, int classSessionId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            var enrollRepository = _unitOfWork.GenericRepository<Enroll>();
            var feeRepository = _unitOfWork.GenericRepository<Fee>();
            var classRepository = _unitOfWork.GenericRepository<ClassSession>();

            var enroll = await enrollRepository.GetByIdAsync(
                x => x.StudentId == studentId && x.ClassSessionId == classSessionId,
                include: query => query
                    .Include(x => x.Attendances)
                    .Include(x => x.StudentScores));

            if (enroll == null)
                throw new Exception("Không tìm thấy đăng ký lớp học.");

            if (enroll.Status != EnrollStatus.Registered)
                throw new Exception("Chỉ được hủy đăng ký khi đăng ký còn ở trạng thái Registered.");

            if (enroll.Attendances.Any() || enroll.StudentScores.Any())
                throw new Exception("Không thể hủy vì đăng ký đã phát sinh điểm danh hoặc điểm số.");

            var classSession = await classRepository.GetByIdAsync(
                x => x.Id == classSessionId);

            if (classSession == null)
                throw new Exception("Không tìm thấy lớp học phần.");

            DateTime now = DateTime.Now;
            if ((classSession.Status != ClassSessionStatus.OpenForRegistration &&
                 classSession.Status != ClassSessionStatus.Full) ||
                now < classSession.RegistrationStart ||
                now >= classSession.RegistrationEnd)
            {
                throw new Exception("Chỉ được hủy đăng ký trong thời hạn đăng ký lớp học phần.");
            }

            var fee = await feeRepository.GetByIdAsync(
                x => x.StudentId == studentId &&
                     x.SubjectId == enroll.SubjectId &&
                     x.SessionId == enroll.SessionId);

            if (fee != null &&
                (fee.Status != "Unpaid" || fee.FundsStatus != "NotReceived"))
            {
                throw new Exception(
                    "Không thể hủy tự động vì học phí đã phát sinh giao dịch. Vui lòng liên hệ phòng kế toán.");
            }

            // Giữ lịch sử đăng ký và Fee để không vi phạm unique index.
            enroll.Status = EnrollStatus.Dropped;
            enrollRepository.Update(enroll);

            var remainingEnrolls = enrollRepository
                .GetAll(x => x.ClassSessionId == classSessionId)
                .Where(x => x.Id != enroll.Id && HoldsSeat(x))
                .Select(x => x.StudentId)
                .Distinct()
                .Count();

            classSession.EnrolledCount = remainingEnrolls;
            if (classSession.Status == ClassSessionStatus.Full &&
                remainingEnrolls < classSession.MaxStudents)
            {
                classSession.Status = ClassSessionStatus.OpenForRegistration;
            }

            classRepository.Update(classSession);

            await _unitOfWork.SaveAsync();
            await transaction.CommitAsync();

            return true;
        }
    }
}
