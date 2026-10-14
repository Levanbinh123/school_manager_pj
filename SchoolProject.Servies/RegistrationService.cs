
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

        // =====================================================
        // GET AVAILABLE CLASS SESSIONS
        // =====================================================
        public async Task<List<ClassSessionViewModel>>
            GetAvailableClassSessions(
                int studentId,
                int sessionId)
        {
            // 1. CHECK STUDENT
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(x => x.Id == studentId);

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên.");

            // 2. CHECK SESSION
            var session = await _unitOfWork
                .GenericRepository<Session>()
                .GetByIdAsync(x => x.Id == sessionId);

            if (session == null)
                throw new Exception("Không tìm thấy Session.");

            // 3. LẤY SUBJECT ĐÃ ĐĂNG KÝ
            var registeredSubjectIds = _unitOfWork
                .GenericRepository<Enroll>()
                .GetAll()
                .Where(x =>
                    x.StudentId == studentId &&
                    x.SessionId == sessionId)
                .Select(x => x.SubjectId)
                .ToHashSet();

            // 4. LẤY CLASS SESSION
            var allClassSessions = _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAll(
                    include: query => query
                        .Include(x => x.Class)
                        .Include(x => x.Subject)
                            .ThenInclude(x => x!.SubjectYears)
                        .Include(x => x.Session)
                        .Include(x => x.HomeroomTeacher))
                .ToList();

            // 5. FILTER
            var availableClassSessions = new List<ClassSession>();

            foreach (var x in allClassSessions)
            {
                if (x.SessionId != sessionId)
                    continue;

                if (x.Subject == null)
                    continue;

                // CHECK MAJOR
                if (x.Subject.MajorId != student.MajorId)
                    continue;

                // CHECK YEAR LEVEL
                var yearOk = x.Subject.SubjectYears.Any(y =>
                    y.YearLevel <= student.YearLevel);

                if (!yearOk)
                    continue;

                // CHECK ĐÃ ĐĂNG KÝ
                if (registeredSubjectIds.Contains(x.SubjectId))
                    continue;

                // CHECK SESSION SUBJECT
                var sessionSubject = _unitOfWork
                    .GenericRepository<SessionSubject>()
                    .GetAll()
                    .FirstOrDefault(ss =>
                        ss.SessionId == sessionId &&
                        ss.SubjectId == x.SubjectId);

                if (sessionSubject == null || !sessionSubject.IsOpen)
                    continue;

                availableClassSessions.Add(x);
            }

            // 6. RETURN VIEW MODEL
            return availableClassSessions
                .Select(x => new ClassSessionViewModel
                {
                    Id = x.Id,
                    ClassId = x.ClassId,
                    ClassName = x.Class?.Name,
                    SubjectId = x.SubjectId,
                    SubjectName = x.Subject?.Name,
                    SessionId = x.SessionId,
                    HomeroomTeacherId = x.HomeroomTeacherId,
                    HomeroomTeacherName = x.HomeroomTeacher != null
                        ? $"{x.HomeroomTeacher.FirstName} {x.HomeroomTeacher.LastName}"
                        : null
                })
                .ToList();
        }

        // =====================================================
        // REGISTER CLASS SESSION + AUTO CREATE FEE
        // =====================================================
        public async Task<bool> RegisterClassSession(
            int studentId,
            int classSessionId)
        {
            // 1. LẤY STUDENT
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(x => x.Id == studentId);

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên.");

            // 2. LẤY CLASS SESSION
            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(
                    x => x.Id == classSessionId,
                    include: query => query
                        .Include(x => x.Subject)
                            .ThenInclude(x => x!.SubjectYears)
                        .Include(x => x.Session)
                        .Include(x => x.Class)
                        .Include(x => x.HomeroomTeacher));

            if (classSession == null)
                throw new Exception("Không tìm thấy ClassSession.");

            // 3. CHECK SUBJECT
            if (classSession.Subject == null)
                throw new Exception("ClassSession chưa có môn học.");

            var subject = classSession.Subject;

            // 4. CHECK SESSION
            if (classSession.Session == null)
                throw new Exception("ClassSession chưa có Session.");

            var session = classSession.Session;

            // 5. CHECK MAJOR
            if (subject.MajorId != student.MajorId)
                throw new Exception(
                    "Lớp học không thuộc chuyên ngành của sinh viên.");

            // 6. CHECK YEAR LEVEL
            var eligible = subject.SubjectYears.Any(x =>
                x.YearLevel <= student.YearLevel);

            if (!eligible)
                throw new Exception(
                    "Sinh viên chưa đủ năm để đăng ký môn này.");

            // 7. CHECK SESSION SUBJECT
            var sessionSubject = _unitOfWork
                .GenericRepository<SessionSubject>()
                .GetAll()
                .FirstOrDefault(x =>
                    x.SubjectId == classSession.SubjectId &&
                    x.SessionId == classSession.SessionId &&
                    x.IsOpen);

            if (sessionSubject == null)
                throw new Exception(
                    "Môn học chưa mở đăng ký trong học kỳ này.");

            // 8. CHECK DUPLICATE
            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var exists = enrollRepository.Exists(x =>
                x.StudentId == studentId &&
                x.SubjectId == classSession.SubjectId &&
                x.SessionId == classSession.SessionId);

            if (exists)
                throw new Exception(
                    "Sinh viên đã đăng ký môn học này.");

            // 9. TÍNH TỔNG TÍN CHỈ
            var currentEnrollments = enrollRepository
                .GetAll(include: query =>
                    query.Include(x => x.Subject))
                .Where(x =>
                    x.StudentId == studentId &&
                    x.SessionId == session.Id)
                .ToList();

            var currentCredits = currentEnrollments
                .Where(x => x.Subject != null)
                .Sum(x => x.Subject!.Credits);

            var newSubjectCredits = subject.Credits;

            if (newSubjectCredits <= 0)
                throw new Exception(
                    "Số tín chỉ của môn học không hợp lệ.");

            var totalCredits = currentCredits + newSubjectCredits;

            if (totalCredits > session.MaxCredits)
            {
                throw new Exception(
                    $"Bạn đã đăng ký {currentCredits} tín chỉ. " +
                    $"Môn {subject.Name} có {newSubjectCredits} tín chỉ. " +
                    $"Tổng là {totalCredits} tín chỉ, vượt quá giới hạn " +
                    $"{session.MaxCredits} tín chỉ của {session.Name}.");
            }

            // 10. KIỂM TRA ĐƠN GIÁ TÍN CHỈ
            var tuitionRateRepository =
                _unitOfWork.GenericRepository<TuitionRate>();

            var tuitionRate = tuitionRateRepository
                .GetAll()
                .FirstOrDefault(x =>
                    x.SessionId == classSession.SessionId);

            if (tuitionRate == null)
                throw new Exception(
                    "Học kỳ này chưa được cấu hình đơn giá tín chỉ.");

            if (tuitionRate.PricePerCredit <= 0)
                throw new Exception(
                    "Đơn giá tín chỉ không hợp lệ.");

            // 11. KIỂM TRA FEE ĐÃ TỒN TẠI CHƯA
            var feeRepository =
                _unitOfWork.GenericRepository<Fee>();

            var existingFee = feeRepository
                .GetAll()
                .FirstOrDefault(x =>
                    x.StudentId == studentId &&
                    x.SubjectId == classSession.SubjectId &&
                    x.SessionId == classSession.SessionId);

            // 12. TẠO ENROLL
            var enroll = new Enroll
            {
                StudentId = studentId,
                SubjectId = classSession.SubjectId,
                SessionId = classSession.SessionId,
                ClassSessionId = classSession.Id
            };

            await enrollRepository.AddAsync(enroll);

            // 13. TỰ ĐỘNG TẠO FEE
            if (existingFee == null)
            {
                var credits = subject.Credits;
                var pricePerCredit = tuitionRate.PricePerCredit;
                var amount = credits * pricePerCredit;

                var fee = new Fee
                {
                    StudentId = studentId,
                    SubjectId = classSession.SubjectId,
                    SessionId = classSession.SessionId,
                    Credits = credits,
                    PricePerCredit = pricePerCredit,
                    Amount = amount,
                    DueDate = DateTime.Now.AddDays(30),
                    Status = "Unpaid"
                };

                feeRepository.Add(fee);
            }

            // 14. SAVE
            _unitOfWork.Save();

            return true;
        }

        // =====================================================
        // GET MY ENROLLMENTS
        // =====================================================
        public async Task<List<EnrollViewModel>>
            GetMyEnrollments(
                int studentId,
                int sessionId)
        {
            // CHECK STUDENT
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(x => x.Id == studentId);

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên.");

            // CHECK SESSION
            var session = await _unitOfWork
                .GenericRepository<Session>()
                .GetByIdAsync(x => x.Id == sessionId);

            if (session == null)
                throw new Exception("Không tìm thấy Session.");

            // GET ENROLLS
            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync(
                    include: query => query
                        .Include(x => x.Subject)
                        .Include(x => x.Session)
                        .Include(x => x.ClassSession)
                            .ThenInclude(x => x!.Class)
                        .Include(x => x.ClassSession)
                            .ThenInclude(x => x!.HomeroomTeacher));

            // FILTER
            return enrolls
                .Where(x =>
                    x.StudentId == studentId &&
                    x.SessionId == sessionId)
                .Select(x => new EnrollViewModel(x))
                .ToList();
        }

        // =====================================================
        // CANCEL REGISTRATION
        // =====================================================
        public async Task<bool> CancelRegistration(
            int studentId,
            int classSessionId)
        {
            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var enroll = await enrollRepository.GetByIdAsync(x =>
                x.StudentId == studentId &&
                x.ClassSessionId == classSessionId);

            if (enroll == null)
                throw new Exception(
                    "Không tìm thấy đăng ký lớp học.");

            // KIỂM TRA HỌC PHÍ
            var feeRepository =
                _unitOfWork.GenericRepository<Fee>();

            var fee = feeRepository.GetAll()
                .FirstOrDefault(x =>
                    x.StudentId == studentId &&
                    x.SubjectId == enroll.SubjectId &&
                    x.SessionId == enroll.SessionId);

            if (fee != null && fee.Status == "Paid")
            {
                throw new Exception(
                    "Không thể hủy đăng ký vì học phí đã thanh toán. " +
                    "Vui lòng xử lý hoàn phí theo quy định của trường.");
            }

            // XÓA FEE CHƯA THANH TOÁN KHI HỦY ĐĂNG KÝ
            if (fee != null)
            {
                feeRepository.Delete(fee);
            }

            // XÓA ENROLL
            enrollRepository.Delete(enroll);

            _unitOfWork.Save();

            return true;
        }
    }
}
