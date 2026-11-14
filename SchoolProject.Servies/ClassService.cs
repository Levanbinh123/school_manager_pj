using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClassService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================
        // 1. VALIDATE SESSION + ACADEMIC YEAR
        // =====================================================
        private async Task<Session> ValidateSession(
            int sessionId,
            bool requireActive = false)
        {
            var session = await _unitOfWork
                .GenericRepository<Session>()
                .GetByIdAsync(
                    x => x.Id == sessionId,
                    include: query => query
                        .Include(x => x.AcademicYearInfo)
                );

            if (session == null)
                throw new Exception("Không tìm thấy học kỳ.");

            if (session.Status.ToString() == "Completed")
                throw new Exception(
                    "Học kỳ đã hoàn thành, không thể tiếp tục thao tác.");

            if (session.AcademicYearInfo == null)
                throw new Exception("Không tìm thấy năm học của học kỳ.");

            if (session.AcademicYearInfo.Status != AcademicYearStatus.Active ||
                !session.AcademicYearInfo.IsCurrent)
            {
                throw new Exception(
                    "Năm học chưa được kích hoạt hoặc không còn là năm học hiện tại.");
            }

            if (requireActive &&
                session.Status.ToString() != "Active")
            {
                throw new Exception("Học kỳ chưa được kích hoạt.");
            }

            return session;
        }

        // =====================================================
        // 2. VALIDATE TEACHER
        // =====================================================
        private async Task ValidateTeacher(int? teacherId)
        {
            if (!teacherId.HasValue)
                return;

            var teacher = await _unitOfWork
                .GenericRepository<Teacher>()
                .GetByIdAsync(x => x.Id == teacherId.Value);

            if (teacher == null)
                throw new Exception("Không tìm thấy giáo viên.");
        }

        // =====================================================
        // 3. LOAD CLASS SESSION
        // =====================================================
        private async Task<ClassSession?> GetClassSessionEntity(int id)
        {
            return await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(
                    x => x.Id == id,
                    include: query => query
                        .Include(x => x.Class)
                        .Include(x => x.Subject)
                        .Include(x => x.Session)
                            .ThenInclude(x => x.AcademicYearInfo)
                        .Include(x => x.HomeroomTeacher)
                        .Include(x => x.Enrolls)
                );
        }

        // =====================================================
        // 4. COUNT STUDENTS
        // =====================================================
        private int GetActualEnrolledCount(ClassSession entity)
        {
            return entity.Enrolls?
                .Where(x =>
                    x.Status != EnrollStatus.Dropped &&
                    x.Status != EnrollStatus.PaymentExpired)
                .Select(x => x.StudentId)
                .Distinct()
                .Count() ?? 0;
        }

        // =====================================================
        // 5. GET FEES OF CLASS SESSION
        // =====================================================
        private List<Fee> GetClassSessionFees(ClassSession entity)
        {
            var studentIds = entity.Enrolls?
                .Where(x => x.Status != EnrollStatus.Dropped)
                .Select(x => x.StudentId)
                .Distinct()
                .ToList() ?? new List<int>();

            if (studentIds.Count == 0)
                return new List<Fee>();

            return _unitOfWork
                .GenericRepository<Fee>()
                .GetAll()
                .Where(x =>
                    studentIds.Contains(x.StudentId) &&
                    x.SubjectId == entity.SubjectId &&
                    x.SessionId == entity.SessionId)
                .ToList();
        }

        // =====================================================
        // 6. AUTOMATIC STATUS SYNCHRONIZATION
        // =====================================================
        private bool SyncClassSessionStatus(ClassSession entity)
        {
            bool changed = false;
            DateTime now = DateTime.Now;

            int actualCount = GetActualEnrolledCount(entity);

            if (entity.EnrolledCount != actualCount)
            {
                entity.EnrolledCount = actualCount;
                changed = true;
            }

            if (entity.Status == ClassSessionStatus.Cancelled ||
                entity.Status == ClassSessionStatus.Completed)
            {
                return changed;
            }

            // PLANNED -> OPEN / REGISTRATION CLOSED
            if (entity.Status == ClassSessionStatus.Planned)
            {
                if (now >= entity.RegistrationEnd)
                {
                    entity.Status =
                        ClassSessionStatus.RegistrationClosed;

                    changed = true;
                }
                else if (now >= entity.RegistrationStart)
                {
                    entity.Status =
                        ClassSessionStatus.OpenForRegistration;

                    changed = true;
                }
            }

            // OPEN / FULL -> REGISTRATION CLOSED
            if ((entity.Status == ClassSessionStatus.OpenForRegistration ||
                 entity.Status == ClassSessionStatus.Full) &&
                now >= entity.RegistrationEnd)
            {
                entity.Status =
                    ClassSessionStatus.RegistrationClosed;

                changed = true;
            }

            // OPEN -> FULL
            if (entity.Status == ClassSessionStatus.OpenForRegistration &&
                actualCount >= entity.MaxStudents)
            {
                entity.Status = ClassSessionStatus.Full;
                changed = true;
            }

            // FULL -> OPEN
            if (entity.Status == ClassSessionStatus.Full &&
                actualCount < entity.MaxStudents &&
                now < entity.RegistrationEnd)
            {
                entity.Status =
                    ClassSessionStatus.OpenForRegistration;

                changed = true;
            }

            // REGISTRATION CLOSED -> AWAITING PAYMENT APPROVAL
            if (entity.Status == ClassSessionStatus.RegistrationClosed &&
                now >= entity.RegistrationEnd &&
                actualCount >= entity.MinStudents)
            {
                entity.Status =
                    ClassSessionStatus.AwaitingPaymentApproval;

                changed = true;
            }

            // PAYMENT OPEN -> PAYMENT CLOSED
            if (entity.Status == ClassSessionStatus.PaymentOpen &&
                entity.PaymentEndDate.HasValue &&
                now >= entity.PaymentEndDate.Value)
            {
                entity.Status = ClassSessionStatus.PaymentClosed;
                changed = true;
            }

            // CONFIRMED -> IN PROGRESS
            if (entity.Status == ClassSessionStatus.Confirmed &&
                now.Date >= entity.StartDate.Date)
            {
                entity.Status = ClassSessionStatus.InProgress;
                changed = true;
            }

            // IN PROGRESS -> COMPLETED
            if (entity.Status == ClassSessionStatus.InProgress &&
                now.Date > entity.EndDate.Date)
            {
                // Lấy toàn bộ sinh viên đã được duyệt vào lớp,
                // bao gồm cả sinh viên đã hoàn thành và sinh viên bị trượt.
                var officialEnrolls = entity.Enrolls?
                    .Where(x =>
                        x.Status == EnrollStatus.Approved ||
                        x.Status == EnrollStatus.Completed ||
                        x.Status == EnrollStatus.Failed)
                    .ToList() ?? new List<Enroll>();

                if (officialEnrolls.Count > 0)
                {
                    var enrollIds = officialEnrolls
                        .Select(x => x.Id)
                        .ToList();

                    var finalizedEnrollIds = _unitOfWork
                        .GenericRepository<StudentScore>()
                        .GetAll()
                        .Where(x =>
                            enrollIds.Contains(x.EnrollId) &&
                            x.IsFinalized)
                        .Select(x => x.EnrollId)
                        .Distinct()
                        .ToHashSet();

                    bool allStudentsFinalized = officialEnrolls.All(
                        x => finalizedEnrollIds.Contains(x.Id));

                    if (allStudentsFinalized)
                    {
                        entity.Status = ClassSessionStatus.Completed;
                        changed = true;
                    }
                }
            }
            return changed;
        }

        // =====================================================
        // 7. MAP CLASS SESSION
        // =====================================================
        private ClassSessionViewModel MapClassSession(
            ClassSession entity)
        {
            return new ClassSessionViewModel
            {
                Id = entity.Id,

                ClassId = entity.ClassId,
                ClassName = entity.Class?.Name,

                SubjectId = entity.SubjectId,
                SubjectName = entity.Subject?.Name,

                SessionId = entity.SessionId,

                Start = entity.Session?.Start ?? default,
                End = entity.Session?.End ?? default,

                HomeroomTeacherId = entity.HomeroomTeacherId,

                HomeroomTeacherName = entity.HomeroomTeacher == null
                    ? null
                    : $"{entity.HomeroomTeacher.FirstName} " +
                      $"{entity.HomeroomTeacher.LastName}".Trim(),

                HasAttendance = entity.HasAttendance,

                TotalSessions = entity.TotalSessions,
                MinStudents = entity.MinStudents,
                MaxStudents = entity.MaxStudents,

                StartDate = entity.StartDate,
                EndDate = entity.EndDate,

                RegistrationStart = entity.RegistrationStart,
                RegistrationEnd = entity.RegistrationEnd,

                PaymentStartDate = entity.PaymentStartDate,
                PaymentEndDate = entity.PaymentEndDate,

                Status = entity.Status,
                EnrolledCount = entity.EnrolledCount
            };
        }

        // =====================================================
        // 8. CREATE CLASS
        // =====================================================
        public async Task AddClass(CreateClassViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Name))
                throw new Exception("Tên lớp không được để trống.");

            string name = vm.Name.Trim();

            bool exists = _unitOfWork
                .GenericRepository<Class>()
                .Exists(x => x.Name == name);

            if (exists)
                throw new Exception("Lớp đã tồn tại.");

            var model = new Class
            {
                Name = name
            };

            await _unitOfWork
                .GenericRepository<Class>()
                .AddAsync(model);

            _unitOfWork.Save();
        }

        // =====================================================
        // 9. UPDATE CLASS
        // =====================================================
        public async Task UpdateClass(
            int id,
            CreateClassViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Name))
                throw new Exception("Tên lớp không được để trống.");

            string name = vm.Name.Trim();

            bool exists = _unitOfWork
                .GenericRepository<Class>()
                .Exists(x => x.Id != id && x.Name == name);

            if (exists)
                throw new Exception("Lớp đã tồn tại.");

            var model = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == id);

            if (model == null)
                throw new Exception("Không tìm thấy lớp.");

            model.Name = name;

            _unitOfWork
                .GenericRepository<Class>()
                .Update(model);

            _unitOfWork.Save();
        }

        // =====================================================
        // 10. DELETE CLASS
        // =====================================================
        public async Task DeleteClass(int id)
        {
            var model = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == id);

            if (model == null)
                throw new Exception("Không tìm thấy lớp.");

            bool hasClassSessions = _unitOfWork
                .GenericRepository<ClassSession>()
                .Exists(x => x.ClassId == id);

            if (hasClassSessions)
                throw new Exception(
                    "Không thể xóa lớp vì lớp đã có lớp học phần.");

            _unitOfWork
                .GenericRepository<Class>()
                .Delete(model);

            _unitOfWork.Save();
        }

        // =====================================================
        // 11. GET CLASS BY ID
        // =====================================================
        public async Task<ClassViewModel?> GetClassById(int id)
        {
            var model = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == id);

            if (model == null)
                return null;

            return new ClassViewModel
            {
                Id = model.Id,
                Name = model.Name
            };
        }

        // =====================================================
        // 12. GET ALL CLASS
        // =====================================================
        public async Task<List<ClassViewModel>> GetAllClass()
        {
            var modelList = _unitOfWork
                .GenericRepository<Class>()
                .GetAll()
                .ToList();

            return modelList.Select(x => new ClassViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }

        // =====================================================
        // 13. CREATE CLASS SESSION
        // =====================================================
        public async Task<ClassSessionViewModel> AddClassSession(
            CreateClassSessionViewModel vm)
        {
            var classEntity = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == vm.ClassId);

            if (classEntity == null)
                throw new Exception("Không tìm thấy lớp.");

            var subject = await _unitOfWork
                .GenericRepository<Subject>()
                .GetByIdAsync(x => x.Id == vm.SubjectId);

            if (subject == null)
                throw new Exception("Không tìm thấy môn học.");

            var session = await ValidateSession(
                vm.SessionId,
                requireActive: false);

            var academicYear = session.AcademicYearInfo!;

            await ValidateTeacher(vm.HomeroomTeacherId);

            if (vm.TotalSessions < 1 || vm.TotalSessions > 1000)
                throw new Exception(
                    "Tổng số buổi học phải từ 1 đến 1000.");

            if (vm.MinStudents < 1 ||
                vm.MaxStudents < 1 ||
                vm.MinStudents > vm.MaxStudents)
            {
                throw new Exception("Sĩ số lớp học không hợp lệ.");
            }

            if (vm.StartDate == default ||
                vm.EndDate == default ||
                vm.RegistrationStart == default ||
                vm.RegistrationEnd == default)
            {
                throw new Exception(
                    "Vui lòng nhập đầy đủ thời gian.");
            }

            if (vm.StartDate.Date > vm.EndDate.Date)
                throw new Exception(
                    "Ngày bắt đầu học phải trước ngày kết thúc.");

            if (vm.RegistrationStart >= vm.RegistrationEnd)
                throw new Exception(
                    "Thời gian bắt đầu đăng ký phải trước thời gian kết thúc đăng ký.");

            if (vm.RegistrationEnd > vm.StartDate)
                throw new Exception(
                    "Thời gian đăng ký phải kết thúc trước ngày bắt đầu học.");

            if (vm.StartDate.Date < session.Start.Date ||
                vm.EndDate.Date > session.End.Date)
            {
                throw new Exception(
                    "Thời gian học của lớp phải nằm trong thời gian học kỳ.");
            }

            if (vm.RegistrationStart.Date <
                    academicYear.StartDate.Date ||
                vm.RegistrationEnd.Date >
                    academicYear.EndDate.Date)
            {
                throw new Exception(
                    "Thời gian đăng ký phải nằm trong thời gian năm học.");
            }

            bool exists = _unitOfWork
                .GenericRepository<ClassSession>()
                .Exists(x =>
                    x.ClassId == vm.ClassId &&
                    x.SubjectId == vm.SubjectId &&
                    x.SessionId == vm.SessionId);

            if (exists)
                throw new Exception(
                    "Lớp đã có lớp học phần cho môn học và học kỳ này.");

            var classSession = new ClassSession
            {
                ClassId = vm.ClassId,
                SubjectId = vm.SubjectId,
                SessionId = vm.SessionId,

                HomeroomTeacherId = vm.HomeroomTeacherId,

                HasAttendance = vm.HasAttendance,
                TotalSessions = vm.TotalSessions,

                MinStudents = vm.MinStudents,
                MaxStudents = vm.MaxStudents,

                StartDate = vm.StartDate,
                EndDate = vm.EndDate,

                RegistrationStart = vm.RegistrationStart,
                RegistrationEnd = vm.RegistrationEnd,

                PaymentStartDate = null,
                PaymentEndDate = null,

                EnrolledCount = 0,

                Status = ClassSessionStatus.Planned
            };

            await _unitOfWork
                .GenericRepository<ClassSession>()
                .AddAsync(classSession);

            _unitOfWork.Save();

            var result = await GetClassSessionEntity(classSession.Id);

            if (result == null)
                throw new Exception(
                    "Không thể lấy lớp học phần vừa tạo.");

            return MapClassSession(result);
        }

        // =====================================================
        // 14. GET CLASSES WAITING FOR ADMIN PAYMENT APPROVAL
        // =====================================================
        public async Task<List<ClassSessionViewModel>>
            GetClassesAwaitingPaymentApproval()
        {
            var list = _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAll(
                    include: query => query
                        .Include(x => x.Class)
                        .Include(x => x.Subject)
                        .Include(x => x.Session)
                        .Include(x => x.HomeroomTeacher)
                        .Include(x => x.Enrolls)
                )
                .Where(x =>
                    x.Status ==
                    ClassSessionStatus.AwaitingPaymentApproval)
                .ToList();

            return list.Select(MapClassSession).ToList();
        }

        // =====================================================
        // 15. ADMIN APPROVES PAYMENT PERIOD - 7 DAYS
        // =====================================================
        public async Task ApprovePaymentPeriod(int id)
        {
            var entity = await GetClassSessionEntity(id);

            if (entity == null)
                throw new Exception("Không tìm thấy lớp học phần.");

            SyncClassSessionStatus(entity);

            if (entity.Status !=
                ClassSessionStatus.AwaitingPaymentApproval)
            {
                throw new Exception(
                    "Lớp không ở trạng thái chờ Admin duyệt thanh toán.");
            }

            int actualCount = GetActualEnrolledCount(entity);

            if (actualCount < entity.MinStudents)
            {
                throw new Exception(
                    $"Chưa đủ sĩ số tối thiểu: {actualCount}/{entity.MinStudents}.");
            }

            if (actualCount > entity.MaxStudents)
                throw new Exception("Sĩ số vượt quá giới hạn lớp.");

            var enrolls = entity.Enrolls?
                .Where(x => x.Status != EnrollStatus.Dropped)
                .ToList() ?? new List<Enroll>();

            if (enrolls.Count == 0)
                throw new Exception("Lớp chưa có sinh viên đăng ký.");

            // Tìm học phí của các sinh viên trong lớp
            var studentIds = enrolls
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            var fees = _unitOfWork
                .GenericRepository<Fee>()
                .GetAll()
                .Where(x =>
                    studentIds.Contains(x.StudentId) &&
                    x.SubjectId == entity.SubjectId &&
                    x.SessionId == entity.SessionId)
                .ToList();

            // Không mở thanh toán nếu còn sinh viên chưa có Fee
            var feeStudentIds = fees
                .Select(x => x.StudentId)
                .Distinct()
                .ToHashSet();

            var missingStudentIds = studentIds
                .Where(x => !feeStudentIds.Contains(x))
                .ToList();

            if (missingStudentIds.Count > 0)
            {
                throw new Exception(
                    "Một số sinh viên chưa có thông tin học phí. " +
                    "StudentId: " +
                    string.Join(", ", missingStudentIds));
            }

            DateTime paymentStart = DateTime.Now;
            DateTime paymentEnd = paymentStart.AddDays(7);

            // Ghi thời gian thanh toán cho lớp
            entity.PaymentStartDate = paymentStart;
            entity.PaymentEndDate = paymentEnd;
            entity.Status = ClassSessionStatus.PaymentOpen;
            entity.EnrolledCount = actualCount;

            // Đồng bộ thời hạn thanh toán cho từng Fee
            foreach (var fee in fees)
            {
                if (fee.Status == "Paid")
                    continue;

                fee.PaymentStartDate = paymentStart;
                fee.DueDate = paymentEnd;

                // Không tự chuyển Fee sang Paid hoặc Pending.
                // Trạng thái thanh toán được cập nhật bởi VNPay/CashPayment.
                _unitOfWork
                    .GenericRepository<Fee>()
                    .Update(fee);
            }

            _unitOfWork
                .GenericRepository<ClassSession>()
                .Update(entity);

            _unitOfWork.Save();
        }

        // =====================================================
        // 16. GET PAYMENT SUMMARY
        // =====================================================
        public async Task<ClassSessionPaymentSummaryViewModel>
            GetPaymentSummary(int classSessionId)
        {
            var entity = await GetClassSessionEntity(classSessionId);

            if (entity == null)
                throw new Exception("Không tìm thấy lớp học phần.");

            var enrolls = entity.Enrolls?
                .Where(x => x.Status != EnrollStatus.Dropped)
                .ToList() ?? new List<Enroll>();

            var fees = GetClassSessionFees(entity);

            int paidCount = fees.Count(x => x.Status == "Paid");

            int unpaidCount = enrolls.Count - paidCount;

            decimal collectedAmount = fees
                .Where(x => x.Status == "Paid")
                .Sum(x => x.TotalAmount);

            decimal unpaidAmount = fees
                .Where(x => x.Status != "Paid")
                .Sum(x => x.TotalAmount);

            return new ClassSessionPaymentSummaryViewModel
            {
                ClassSessionId = entity.Id,
                ClassName = entity.Class?.Name,
                SubjectName = entity.Subject?.Name,

                TotalStudents = enrolls.Count,
                PaidCount = paidCount,
                UnpaidCount = Math.Max(0, unpaidCount),

                CollectedAmount = collectedAmount,
                UnpaidAmount = unpaidAmount,

                PaymentStartDate = entity.PaymentStartDate,
                PaymentEndDate = entity.PaymentEndDate,

                Status = entity.Status
            };
        }

        // =====================================================
        // 17. ADMIN CONFIRMS CLASS
        // =====================================================
        public async Task ConfirmClassSession(int id)
        {
            var entity = await GetClassSessionEntity(id);

            if (entity == null)
                throw new Exception("Không tìm thấy lớp học phần.");

            SyncClassSessionStatus(entity);

            if (entity.Status != ClassSessionStatus.PaymentClosed)
            {
                throw new Exception(
                    "Chỉ được xác nhận lớp sau khi thời hạn thanh toán kết thúc.");
            }

            await ValidateSession(
                entity.SessionId,
                requireActive: true);

            var enrolls = entity.Enrolls?
                .Where(x => x.Status != EnrollStatus.Dropped)
                .ToList() ?? new List<Enroll>();

            var studentIds = enrolls
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            var fees = _unitOfWork
                .GenericRepository<Fee>()
                .GetAll()
                .Where(x =>
                    studentIds.Contains(x.StudentId) &&
                    x.SubjectId == entity.SubjectId &&
                    x.SessionId == entity.SessionId)
                .ToList();

            // Chỉ lấy sinh viên có học phí Paid
            var paidStudentIds = fees
                .Where(x => x.Status == "Paid")
                .Select(x => x.StudentId)
                .ToHashSet();

            int paidCount = enrolls
                .Count(x => paidStudentIds.Contains(x.StudentId));

            if (paidCount < entity.MinStudents)
            {
                throw new Exception(
                    $"Số sinh viên đã thanh toán chưa đủ tối thiểu. " +
                    $"Đã thanh toán: {paidCount}/{entity.MinStudents}.");
            }

            // Chỉ duyệt sinh viên đã thanh toán
            // Phân loại sinh viên theo trạng thái thanh toán
            foreach (var enroll in enrolls)
            {
                if (paidStudentIds.Contains(enroll.StudentId))
                {
                    // Đã thanh toán: được phép học
                    enroll.Status = EnrollStatus.Approved;
                }
                else
                {
                    // Chưa thanh toán: hết hạn đăng ký lớp này
                    enroll.Status = EnrollStatus.PaymentExpired;

                    // Gỡ khỏi lớp học phần
                    enroll.ClassSessionId = null;
                }

                _unitOfWork
                    .GenericRepository<Enroll>()
                    .Update(enroll);
            }

            // Sinh viên chưa trả tiền giữ trạng thái Registered.
            // Không được đưa vào danh sách học chính thức.

            entity.EnrolledCount = paidCount;
            entity.Status = ClassSessionStatus.Confirmed;

            _unitOfWork
                .GenericRepository<ClassSession>()
                .Update(entity);

            _unitOfWork.Save();
        }

        // =====================================================
        // 18. ADMIN CANCELS CLASS
        // =====================================================
        public async Task CancelClassSession(int id)
        {
            var entity = await GetClassSessionEntity(id);

            if (entity == null)
                throw new Exception("Không tìm thấy lớp học phần.");

            if (entity.Status == ClassSessionStatus.Completed)
                throw new Exception(
                    "Không thể hủy lớp đã hoàn thành.");

            if (entity.Status == ClassSessionStatus.Cancelled)
                throw new Exception("Lớp đã được hủy.");

            if (entity.Status == ClassSessionStatus.InProgress)
                throw new Exception(
                    "Không thể hủy lớp đang diễn ra bằng chức năng này.");

            var fees = GetClassSessionFees(entity);

            foreach (var fee in fees)
            {
                if (fee.Status == "Paid" &&
                    fee.FundsStatus != "Refunded")
                {
                    // Chỉ đánh dấu chờ hoàn tiền.
                    // Chưa được coi là đã hoàn tiền thực tế.
                    fee.FundsStatus = "RefundPending";

                    _unitOfWork
                        .GenericRepository<Fee>()
                        .Update(fee);
                }
            }

            entity.Status = ClassSessionStatus.Cancelled;

            _unitOfWork
                .GenericRepository<ClassSession>()
                .Update(entity);

            _unitOfWork.Save();
        }

        // =====================================================
        // 19. GET CLASS SESSIONS BY CLASS ID
        // =====================================================
        public async Task<List<ClassSessionViewModel>> GetClassSessions(
            int classId)
        {
            var list = _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAll(
                    include: query => query
                        .Include(x => x.Class)
                        .Include(x => x.Subject)
                        .Include(x => x.Session)
                        .Include(x => x.HomeroomTeacher)
                        .Include(x => x.Enrolls)
                )
                .Where(x => x.ClassId == classId)
                .ToList();

            bool changed = false;

            foreach (var entity in list)
            {
                if (SyncClassSessionStatus(entity))
                {
                    _unitOfWork
                        .GenericRepository<ClassSession>()
                        .Update(entity);

                    changed = true;
                }
            }

            if (changed)
                _unitOfWork.Save();

            return list.Select(MapClassSession).ToList();
        }

        // =====================================================
        // 20. GET ALL CLASS SESSIONS
        // =====================================================
        public async Task<List<ClassSessionViewModel>> GetAllClassSessions()
        {
            var list = _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAll(
                    include: query => query
                        .Include(x => x.Class)
                        .Include(x => x.Subject)
                        .Include(x => x.Session)
                        .Include(x => x.HomeroomTeacher)
                        .Include(x => x.Enrolls)
                )
                .ToList();

            bool changed = false;

            foreach (var entity in list)
            {
                if (SyncClassSessionStatus(entity))
                {
                    _unitOfWork
                        .GenericRepository<ClassSession>()
                        .Update(entity);

                    changed = true;
                }
            }

            if (changed)
                _unitOfWork.Save();

            return list.Select(MapClassSession).ToList();
        }

        // =====================================================
        // 21. ADD STUDENTS TO CLASS SESSION
        // =====================================================
        public async Task<int> AddStudentsToClassSession(
            int classSessionId,
            List<int> studentList)
        {
            if (studentList == null || studentList.Count == 0)
                throw new Exception(
                    "Danh sách sinh viên không được để trống.");

            var classSession =
                await GetClassSessionEntity(classSessionId);

            if (classSession == null)
                throw new Exception(
                    "Không tìm thấy lớp học phần.");

            await ValidateSession(
                classSession.SessionId,
                requireActive: true);

            SyncClassSessionStatus(classSession);

            if (classSession.Status !=
                ClassSessionStatus.OpenForRegistration)
            {
                throw new Exception(
                    "Lớp hiện không ở trạng thái nhận đăng ký.");
            }

            DateTime now = DateTime.Now;

            if (now < classSession.RegistrationStart ||
                now >= classSession.RegistrationEnd)
            {
                throw new Exception(
                    "Chưa đến thời gian đăng ký hoặc đã hết hạn đăng ký.");
            }

            var enrollsToAdd = new List<Enroll>();

            var distinctStudentIds = studentList
                .Distinct()
                .ToList();

            foreach (int studentId in distinctStudentIds)
            {
                var student = await _unitOfWork
                    .GenericRepository<Student>()
                    .GetByIdAsync(x => x.Id == studentId);

                if (student == null)
                    throw new Exception(
                        $"Không tìm thấy sinh viên ID = {studentId}.");

                var enroll = await _unitOfWork
                    .GenericRepository<Enroll>()
                    .GetByIdAsync(
                        x =>
                            x.StudentId == studentId &&
                            x.SubjectId == classSession.SubjectId &&
                            x.SessionId == classSession.SessionId
                    );

                if (enroll == null)
                    throw new Exception(
                        $"Sinh viên ID = {studentId} chưa đăng ký môn học này.");

                if (enroll.Status == EnrollStatus.Dropped)
                    throw new Exception(
                        $"Sinh viên ID = {studentId} đã hủy đăng ký môn học.");

                if (enroll.ClassSessionId == classSessionId)
                    continue;

                if (enroll.ClassSessionId.HasValue)
                    throw new Exception(
                        $"Sinh viên ID = {studentId} đã được xếp vào lớp khác.");

                enrollsToAdd.Add(enroll);
            }

            int currentCount = GetActualEnrolledCount(classSession);

            if (currentCount + enrollsToAdd.Count >
                classSession.MaxStudents)
            {
                throw new Exception(
                    $"Không đủ chỗ. Sĩ số hiện tại: {currentCount}, " +
                    $"số muốn thêm: {enrollsToAdd.Count}, " +
                    $"tối đa: {classSession.MaxStudents}.");
            }

            foreach (var enroll in enrollsToAdd)
            {
                enroll.ClassSessionId = classSessionId;

                _unitOfWork
                    .GenericRepository<Enroll>()
                    .Update(enroll);
            }

            classSession.EnrolledCount =
                currentCount + enrollsToAdd.Count;

            if (classSession.EnrolledCount >= classSession.MaxStudents)
            {
                classSession.Status = ClassSessionStatus.Full;
            }

            _unitOfWork
                .GenericRepository<ClassSession>()
                .Update(classSession);

            _unitOfWork.Save();

            return enrollsToAdd.Count;
        }
    }
}