
using Microsoft.AspNetCore.Http;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;

        private const long MaxFileSize = 10 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".doc", ".docx", ".pdf", ".jpg", ".jpeg", ".png"
        };

        public AssignmentService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        // =====================================================
        // TEACHER - NOT GRADED SUBMISSIONS
        // =====================================================

        public async Task<List<SubmissionNotGradedViewModel>>
            GetNotGradedSubmissions(string userId, int assignmentId)
        {
            await GetTeacherAssignment(userId, assignmentId);

            var submissions = await _unitOfWork
                .GenericRepository<Submission>()
                .GetAllAsync();

            var students = await _unitOfWork
                .GenericRepository<Student>()
                .GetAllAsync();

            return submissions
                .Where(x => x.AssignmentId == assignmentId &&
                            !x.Score.HasValue)
                .Select(x =>
                {
                    var student = students.FirstOrDefault(
                        s => s.Id == x.StudentId);

                    return new SubmissionNotGradedViewModel
                    {
                        SubmissionId = x.Id,
                        AssignmentId = x.AssignmentId,
                        StudentId = x.StudentId,
                        StudentName = student == null
                            ? null
                            : student.FirstName + " " + student.LastName,

                        FileUrl = _fileStorage.GetReadUrl(x.FileUrl),
                        SubmittedAt = x.SubmittedAt,
                        Score = x.Score
                    };
                })
                .ToList();
        }

        // =====================================================
        // TEACHER - STUDENTS NOT SUBMITTED
        // =====================================================

        public async Task<List<StudentNotSubmittedViewModel>>
            GetNotSubmittedStudents(string userId, int assignmentId)
        {
            var assignment = await GetTeacherAssignment(userId, assignmentId);

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var students = await _unitOfWork
                .GenericRepository<Student>()
                .GetAllAsync();

            var submissions = await _unitOfWork
                .GenericRepository<Submission>()
                .GetAllAsync();

            var studentIds = enrolls
                .Where(x =>
                    x.ClassSessionId == assignment.ClassSessionId &&
                    x.Status == EnrollStatus.Approved)
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            var submittedStudentIds = submissions
                .Where(x => x.AssignmentId == assignmentId)
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            return students
                .Where(x =>
                    studentIds.Contains(x.Id) &&
                    !submittedStudentIds.Contains(x.Id))
                .Select(x => new StudentNotSubmittedViewModel
                {
                    StudentId = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    ClassSessionId = assignment.ClassSessionId
                })
                .ToList();
        }

        // =====================================================
        // TEACHER - ASSIGNMENT SUMMARY
        // =====================================================

        public async Task<AssignmentSummaryViewModel>
            GetAssignmentSummary(string userId, int assignmentId)
        {
            var assignment = await GetTeacherAssignment(userId, assignmentId);

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var submissions = await _unitOfWork
                .GenericRepository<Submission>()
                .GetAllAsync();

            var studentIds = enrolls
                .Where(x =>
                    x.ClassSessionId == assignment.ClassSessionId &&
                    x.Status == EnrollStatus.Approved)
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            var assignmentSubmissions = submissions
                .Where(x => x.AssignmentId == assignmentId)
                .ToList();

            var submittedStudentIds = assignmentSubmissions
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            return new AssignmentSummaryViewModel
            {
                AssignmentId = assignment.Id,
                Title = assignment.Title,
                TotalStudents = studentIds.Count,
                SubmittedStudents = submittedStudentIds.Count(
                    x => studentIds.Contains(x)),
                NotSubmittedStudents = studentIds
                    .Except(submittedStudentIds)
                    .Count(),
                GradedSubmissions = assignmentSubmissions
                    .Count(x => x.Score.HasValue),
                NotGradedSubmissions = assignmentSubmissions
                    .Count(x => !x.Score.HasValue)
            };
        }

        // =====================================================
        // TEACHER - GET STUDENTS BY CLASS SESSION
        // =====================================================

        public async Task<List<TeacherStudentViewModel>>
            GetStudentsByClassSession(string userId, int classSessionId)
        {
            var teacher = await GetTeacherByUserId(userId);

            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(x => x.Id == classSessionId);

            if (classSession == null)
                throw new Exception("Không tìm thấy ClassSession.");

            if (classSession.HomeroomTeacherId != teacher.Id)
                throw new UnauthorizedAccessException(
                    "Bạn không phải giáo viên phụ trách ClassSession này.");

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var students = await _unitOfWork
                .GenericRepository<Student>()
                .GetAllAsync();

            var studentIds = enrolls
                .Where(x =>
                    x.ClassSessionId == classSessionId &&
                    x.Status == EnrollStatus.Approved)
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            return students
                .Where(x => studentIds.Contains(x.Id))
                .Select(x => new TeacherStudentViewModel
                {
                    Id = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    DOB = x.DOB,
                    YearLevel = x.YearLevel,
                    MajorId = x.MajorId,
                    KeyId = x.KeyId
                })
                .ToList();
        }

        // =====================================================
        // TEACHER - GET CLASS SESSIONS
        // =====================================================

        public async Task<List<TeacherClassSessionViewModel>>
            GetTeacherClassSessions(string userId)
        {
            var teacher = await GetTeacherByUserId(userId);

            var classSessions = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAllAsync();

            var classes = await _unitOfWork
                .GenericRepository<Class>()
                .GetAllAsync();

            var subjects = await _unitOfWork
                .GenericRepository<Subject>()
                .GetAllAsync();

            var sessions = await _unitOfWork
                .GenericRepository<Session>()
                .GetAllAsync();

            return classSessions
                .Where(x => x.HomeroomTeacherId == teacher.Id)
                .Select(x =>
                {
                    var classItem = classes.FirstOrDefault(
                        c => c.Id == x.ClassId);

                    var subject = subjects.FirstOrDefault(
                        s => s.Id == x.SubjectId);

                    var session = sessions.FirstOrDefault(
                        s => s.Id == x.SessionId);

                    return new TeacherClassSessionViewModel
                    {
                        Id = x.Id,
                        ClassId = x.ClassId,
                        ClassName = classItem?.Name,
                        SubjectId = x.SubjectId,
                        SubjectName = subject?.Name,
                        SessionId = x.SessionId,
                        Start = session?.Start ?? default,
                        End = session?.End ?? default,
                        TeacherId = x.HomeroomTeacherId,
                        TeacherName = teacher.FirstName + " " + teacher.LastName,
                        HasAttendance = x.HasAttendance
                    };
                })
                .ToList();
        }

        // =====================================================
        // TEACHER - ADD ASSIGNMENT
        // =====================================================

        public async Task AddAssignment(
            string userId,
            CreateAssignmentViewModel vm,
            IFormFile? file)
        {
            var classSession = await ValidateAssignment(userId, vm);

            await EnsureClassSessionNotFinalized(classSession.Id);

            string? uploadedKey = null;

            try
            {
                if (file != null && file.Length > 0)
                {
                    ValidateFile(file);

                    uploadedKey = await _fileStorage.UploadAsync(
                        file,
                        "assignments");
                }

                var assignment = new Assignment
                {
                    Title = vm.Title.Trim(),
                    Description = vm.Description,
                    DueDate = vm.DueDate,
                    ClassSessionId = classSession.Id,
                    Type = vm.Type,
                    AttachmentUrl = uploadedKey
                };

                await _unitOfWork
                    .GenericRepository<Assignment>()
                    .AddAsync(assignment);

                _unitOfWork.Save();
            }
            catch
            {
                if (uploadedKey != null)
                    await _fileStorage.DeleteAsync(uploadedKey);

                throw;
            }
        }

        // =====================================================
        // TEACHER - GET ASSIGNMENTS
        // =====================================================

        public async Task<List<AssignmentViewModel>>
            GetTeacherAssignments(string userId)
        {
            var teacher = await GetTeacherByUserId(userId);

            var classSessions = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAllAsync();

            var teacherClassSessionIds = classSessions
                .Where(x => x.HomeroomTeacherId == teacher.Id)
                .Select(x => x.Id)
                .ToList();

            var assignments = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetAllAsync();

            return assignments
                .Where(x => teacherClassSessionIds.Contains(x.ClassSessionId))
                .Select(x =>
                {
                    var vm = new AssignmentViewModel(x);
                    vm.AttachmentUrl = _fileStorage.GetReadUrl(x.AttachmentUrl);
                    return vm;
                })
                .ToList();
        }

        // =====================================================
        // TEACHER - GET ASSIGNMENT BY ID
        // =====================================================

        public async Task<AssignmentViewModel?>
            GetTeacherAssignmentById(string userId, int id)
        {
            var assignment = await GetTeacherAssignment(userId, id);

            var vm = new AssignmentViewModel(assignment);
            vm.AttachmentUrl = _fileStorage.GetReadUrl(
                assignment.AttachmentUrl);

            return vm;
        }

        // =====================================================
        // TEACHER - UPDATE ASSIGNMENT
        // =====================================================

        public async Task UpdateAssignment(
            string userId,
            int id,
            CreateAssignmentViewModel vm,
            IFormFile? file)
        {
            var assignment = await GetTeacherAssignment(userId, id);

            var classSession = await ValidateAssignment(userId, vm);

            await EnsureClassSessionNotFinalized(assignment.ClassSessionId);

            if (assignment.ClassSessionId != classSession.Id)
            {
                await EnsureClassSessionNotFinalized(classSession.Id);

                var submissions = await _unitOfWork
                    .GenericRepository<Submission>()
                    .GetAllAsync();

                if (submissions.Any(x => x.AssignmentId == id))
                {
                    throw new Exception(
                        "Không thể chuyển ClassSession khi Assignment đã có bài nộp.");
                }
            }

            var oldAttachmentKey = assignment.AttachmentUrl;
            string? newAttachmentKey = null;

            try
            {
                if (file != null && file.Length > 0)
                {
                    ValidateFile(file);

                    newAttachmentKey = await _fileStorage.UploadAsync(
                        file,
                        "assignments");
                }

                assignment.Title = vm.Title.Trim();
                assignment.Description = vm.Description;
                assignment.DueDate = vm.DueDate;
                assignment.ClassSessionId = classSession.Id;
                assignment.Type = vm.Type;

                if (newAttachmentKey != null)
                    assignment.AttachmentUrl = newAttachmentKey;

                _unitOfWork
                    .GenericRepository<Assignment>()
                    .Update(assignment);

                _unitOfWork.Save();
            }
            catch
            {
                if (newAttachmentKey != null)
                    await _fileStorage.DeleteAsync(newAttachmentKey);

                throw;
            }

            // Chỉ xóa file cũ sau khi database lưu thành công.
            if (newAttachmentKey != null &&
                !string.IsNullOrWhiteSpace(oldAttachmentKey))
            {
                await _fileStorage.DeleteAsync(oldAttachmentKey);
            }
        }

        // =====================================================
        // TEACHER - DELETE ASSIGNMENT
        // =====================================================

        public async Task DeleteAssignment(string userId, int id)
        {
            var assignment = await GetTeacherAssignment(userId, id);

            await EnsureClassSessionNotFinalized(
                assignment.ClassSessionId);

            var submissions = await _unitOfWork
                .GenericRepository<Submission>()
                .GetAllAsync();

            if (submissions.Any(x => x.AssignmentId == id))
            {
                throw new Exception(
                    "Không thể xóa Assignment đã có bài nộp.");
            }

            var attachmentKey = assignment.AttachmentUrl;

            _unitOfWork
                .GenericRepository<Assignment>()
                .Delete(assignment);

            _unitOfWork.Save();

            if (!string.IsNullOrWhiteSpace(attachmentKey))
                await _fileStorage.DeleteAsync(attachmentKey);
        }

        // =====================================================
        // STUDENT - GET ASSIGNMENTS
        // =====================================================

        public async Task<List<AssignmentViewModel>>
            GetStudentAssignments(string userId)
        {
            var student = await GetStudentByUserId(userId);

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var classSessionIds = enrolls
                .Where(x =>
                    x.StudentId == student.Id &&
                    x.Status == EnrollStatus.Approved &&
                    x.ClassSessionId.HasValue)
                .Select(x => x.ClassSessionId!.Value)
                .Distinct()
                .ToList();

            var assignments = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetAllAsync();

            return assignments
                .Where(x => classSessionIds.Contains(x.ClassSessionId))
                .Select(x =>
                {
                    var vm = new AssignmentViewModel(x);
                    vm.AttachmentUrl = _fileStorage.GetReadUrl(x.AttachmentUrl);
                    return vm;
                })
                .ToList();
        }

        // =====================================================
        // STUDENT - GET ASSIGNMENT BY ID
        // =====================================================

        public async Task<AssignmentViewModel?>
            GetStudentAssignmentById(string userId, int id)
        {
            var student = await GetStudentByUserId(userId);

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var classSessionIds = enrolls
                .Where(x =>
                    x.StudentId == student.Id &&
                    x.Status == EnrollStatus.Approved &&
                    x.ClassSessionId.HasValue)
                .Select(x => x.ClassSessionId!.Value)
                .Distinct()
                .ToList();

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x =>
                    x.Id == id &&
                    classSessionIds.Contains(x.ClassSessionId));

            if (assignment == null)
                return null;

            var vm = new AssignmentViewModel(assignment);
            vm.AttachmentUrl = _fileStorage.GetReadUrl(
                assignment.AttachmentUrl);

            return vm;
        }

        // =====================================================
        // STUDENT - SUBMIT ASSIGNMENT
        // =====================================================

        public async Task SubmitAssignment(
            string userId,
            int assignmentId,
            SubmitAssignmentRequest request)
        {
            var student = await GetStudentByUserId(userId);

            if (request == null)
                throw new Exception("Request không được để trống.");

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x => x.Id == assignmentId);

            if (assignment == null)
                throw new Exception("Không tìm thấy Assignment.");

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var enrolled = enrolls.Any(x =>
                x.StudentId == student.Id &&
                x.ClassSessionId == assignment.ClassSessionId &&
                x.Status == EnrollStatus.Approved);

            if (!enrolled)
            {
                throw new UnauthorizedAccessException(
                    "Bạn không có quyền nộp bài cho ClassSession này.");
            }

            if (DateTime.Now > assignment.DueDate)
                throw new Exception("Đã quá hạn nộp bài.");

            var submissionRepository =
                _unitOfWork.GenericRepository<Submission>();

            if (submissionRepository.Exists(x =>
                x.AssignmentId == assignmentId &&
                x.StudentId == student.Id))
            {
                throw new Exception("Bạn đã nộp bài Assignment này.");
            }

            var file = request.File;

            if (file == null || file.Length == 0)
                throw new Exception("Vui lòng chọn file.");

            ValidateFile(file);

            string? uploadedKey = null;

            try
            {
                uploadedKey = await _fileStorage.UploadAsync(
                    file,
                    "submissions");

                var submission = new Submission
                {
                    AssignmentId = assignmentId,
                    StudentId = student.Id,
                    FileUrl = uploadedKey,
                    SubmittedAt = DateTime.Now,
                    Score = null
                };

                await submissionRepository.AddAsync(submission);

                _unitOfWork.Save();
            }
            catch
            {
                if (uploadedKey != null)
                    await _fileStorage.DeleteAsync(uploadedKey);

                throw;
            }
        }

        // =====================================================
        // TEACHER - GET SUBMISSIONS
        // =====================================================

        public async Task<List<SubmissionViewModel>>
            GetSubmissions(string userId, int assignmentId)
        {
            await GetTeacherAssignment(userId, assignmentId);

            var submissions = await _unitOfWork
                .GenericRepository<Submission>()
                .GetAllAsync();

            return submissions
                .Where(x => x.AssignmentId == assignmentId)
                .Select(x =>
                {
                    var vm = new SubmissionViewModel(x);
                    vm.FileUrl = _fileStorage.GetReadUrl(x.FileUrl);
                    return vm;
                })
                .ToList();
        }

        // =====================================================
        // TEACHER - GRADE SUBMISSION
        // =====================================================

        public async Task GradeSubmission(
            string userId,
            int submissionId,
            GradeSubmissionRequest request)
        {
            if (request == null)
                throw new Exception("Request không được để trống.");

            if (request.Score < 0 || request.Score > 10)
                throw new Exception("Điểm phải từ 0 đến 10.");

            var submission = await _unitOfWork
                .GenericRepository<Submission>()
                .GetByIdAsync(x => x.Id == submissionId);

            if (submission == null)
                throw new Exception("Không tìm thấy bài nộp.");

            var assignment = await GetTeacherAssignment(
                userId,
                submission.AssignmentId);

            await EnsureClassSessionNotFinalized(
                assignment.ClassSessionId);

            submission.Score = request.Score;

            _unitOfWork
                .GenericRepository<Submission>()
                .Update(submission);

            _unitOfWork.Save();
        }

        // =====================================================
        // PRIVATE - VALIDATE FILE
        // =====================================================

        private static void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File không hợp lệ.");

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                throw new Exception(
                    "Chỉ cho phép file Word, PDF hoặc hình ảnh.");
            }

            if (file.Length > MaxFileSize)
            {
                throw new Exception(
                    "File không được vượt quá 10MB.");
            }
        }

        // =====================================================
        // PRIVATE - CHECK CLASS SESSION FINALIZED
        // =====================================================

        private async Task EnsureClassSessionNotFinalized(
            int classSessionId)
        {
            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var eligibleEnrolls = enrolls
                .Where(x =>
                    x.ClassSessionId == classSessionId &&
                    (x.Status == EnrollStatus.Approved ||
                     x.Status == EnrollStatus.Completed ||
                     x.Status == EnrollStatus.Failed))
                .ToList();

            if (!eligibleEnrolls.Any())
                return;

            var enrollIds = eligibleEnrolls
                .Select(x => x.Id)
                .ToHashSet();

            var scores = await _unitOfWork
                .GenericRepository<StudentScore>()
                .GetAllAsync();

            var finalizedEnrollIds = scores
                .Where(x =>
                    enrollIds.Contains(x.EnrollId) &&
                    x.IsFinalized)
                .Select(x => x.EnrollId)
                .ToHashSet();

            bool allFinalized = eligibleEnrolls.All(
                x => finalizedEnrollIds.Contains(x.Id));

            if (allFinalized)
            {
                throw new InvalidOperationException(
                    "Tất cả sinh viên trong lớp đã được tổng kết điểm. " +
                    "Không thể thay đổi Assignment hoặc chấm bài.");
            }
        }

        // =====================================================
        // PRIVATE - GET TEACHER
        // =====================================================

        private async Task<Teacher> GetTeacherByUserId(string userId)
        {
            var teacher = await _unitOfWork
                .GenericRepository<Teacher>()
                .GetByIdAsync(x => x.KeyId == userId);

            if (teacher == null)
            {
                throw new UnauthorizedAccessException(
                    "Không tìm thấy Teacher của tài khoản.");
            }

            return teacher;
        }

        // =====================================================
        // PRIVATE - GET STUDENT
        // =====================================================

        private async Task<Student> GetStudentByUserId(string userId)
        {
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(x => x.KeyId == userId);

            if (student == null)
            {
                throw new UnauthorizedAccessException(
                    "Không tìm thấy Student của tài khoản.");
            }

            return student;
        }

        // =====================================================
        // PRIVATE - GET ASSIGNMENT OWNED BY TEACHER
        // =====================================================

        private async Task<Assignment> GetTeacherAssignment(
            string userId,
            int assignmentId)
        {
            var teacher = await GetTeacherByUserId(userId);

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x => x.Id == assignmentId);

            if (assignment == null)
                throw new Exception("Không tìm thấy Assignment.");

            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(x => x.Id == assignment.ClassSessionId);

            if (classSession == null ||
                classSession.HomeroomTeacherId != teacher.Id)
            {
                throw new UnauthorizedAccessException(
                    "Bạn không có quyền thao tác Assignment này.");
            }

            return assignment;
        }

        // =====================================================
        // PRIVATE - VALIDATE ASSIGNMENT
        // =====================================================

        private async Task<ClassSession> ValidateAssignment(
            string userId,
            CreateAssignmentViewModel vm)
        {
            if (vm == null)
                throw new Exception("Dữ liệu không được để trống.");

            if (string.IsNullOrWhiteSpace(vm.Title))
                throw new Exception("Title không được để trống.");

            if (vm.DueDate <= DateTime.Now)
            {
                throw new Exception(
                    "Hạn nộp phải lớn hơn thời điểm hiện tại.");
            }

            var teacher = await GetTeacherByUserId(userId);

            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(x => x.Id == vm.ClassSessionId);

            if (classSession == null)
                throw new Exception("Không tìm thấy ClassSession.");

            if (classSession.HomeroomTeacherId != teacher.Id)
            {
                throw new UnauthorizedAccessException(
                    "Bạn không phải giáo viên phụ trách ClassSession này.");
            }

            return classSession;
        }
    }
}
