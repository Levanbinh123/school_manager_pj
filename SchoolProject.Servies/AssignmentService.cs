using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        // =====================================================
        // TEACHER - NOT GRADED SUBMISSIONS
        // =====================================================

        public async Task<List<SubmissionNotGradedViewModel>>
            GetNotGradedSubmissions(
                string userId,
                int assignmentId)
        {
            var teacher = await GetTeacherByUserId(userId);

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x =>
                    x.Id == assignmentId &&
                    x.TeacherId == teacher.Id);

            if (assignment == null)
            {
                throw new Exception(
                    "Không tìm thấy Assignment hoặc bạn không có quyền xem.");
            }

            var submissions = await _unitOfWork
                .GenericRepository<Submission>()
                .GetAllAsync();

            var students = await _unitOfWork
                .GenericRepository<Student>()
                .GetAllAsync();

            var result = submissions
                .Where(x =>
                    x.AssignmentId == assignmentId &&
                    !x.Score.HasValue)
                .Select(x =>
                {
                    var student = students
                        .FirstOrDefault(s =>
                            s.Id == x.StudentId);

                    return new SubmissionNotGradedViewModel
                    {
                        SubmissionId = x.Id,

                        AssignmentId = x.AssignmentId,

                        StudentId = x.StudentId,

                        StudentName = student == null
                            ? null
                            : student.FirstName + " " +
                              student.LastName,

                        FileUrl = x.FileUrl,

                        SubmittedAt = x.SubmittedAt,

                        Score = x.Score
                    };
                })
                .ToList();

            return result;
        }
        // =====================================================
        // TEACHER - STUDENTS NOT SUBMITTED
        // =====================================================

        public async Task<List<StudentNotSubmittedViewModel>>
            GetNotSubmittedStudents(
                string userId,
                int assignmentId)
        {
            var teacher = await GetTeacherByUserId(userId);

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x =>
                    x.Id == assignmentId &&
                    x.TeacherId == teacher.Id);

            if (assignment == null)
            {
                throw new Exception(
                    "Không tìm thấy Assignment hoặc bạn không có quyền xem.");
            }

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var students = await _unitOfWork
                .GenericRepository<Student>()
                .GetAllAsync();

            var submissions = await _unitOfWork
                .GenericRepository<Submission>()
                .GetAllAsync();

            // Sinh viên trong ClassSession
            var studentIds = enrolls
                .Where(x =>
                    x.ClassSessionId == assignment.ClassSessionId)
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            // Sinh viên đã nộp
            var submittedStudentIds = submissions
                .Where(x =>
                    x.AssignmentId == assignmentId)
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
            GetAssignmentSummary(
                string userId,
                int assignmentId)
        {
            var teacher = await GetTeacherByUserId(userId);

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x =>
                    x.Id == assignmentId &&
                    x.TeacherId == teacher.Id);

            if (assignment == null)
            {
                throw new Exception(
                    "Không tìm thấy Assignment hoặc bạn không có quyền xem.");
            }

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var submissions = await _unitOfWork
                .GenericRepository<Submission>()
                .GetAllAsync();

            // Sinh viên thuộc ClassSession
            var studentIds = enrolls
                .Where(x =>
                    x.ClassSessionId == assignment.ClassSessionId)
                .Select(x => x.StudentId)
                .Distinct()
                .ToList();

            // Bài nộp của Assignment
            var assignmentSubmissions = submissions
                .Where(x =>
                    x.AssignmentId == assignmentId)
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

                SubmittedStudents =
                    submittedStudentIds.Count,

                NotSubmittedStudents =
                    studentIds
                        .Except(submittedStudentIds)
                        .Count(),

                GradedSubmissions =
                    assignmentSubmissions
                        .Count(x => x.Score.HasValue),

                NotGradedSubmissions =
                    assignmentSubmissions
                        .Count(x => !x.Score.HasValue)
            };
        }
        // =====================================================
        // TEACHER - GET STUDENTS BY CLASS SESSION
        // =====================================================

        public async Task<List<TeacherStudentViewModel>>
            GetStudentsByClassSession(
                string userId,
                int classSessionId)
        {
            var teacher = await GetTeacherByUserId(userId);

            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(x =>
                    x.Id == classSessionId);

            if (classSession == null)
            {
                throw new Exception(
                    "Không tìm thấy ClassSession.");
            }

            if (classSession.HomeroomTeacherId != teacher.Id)
            {
                throw new Exception(
                    "Bạn không phải giáo viên dạy ClassSession này.");
            }

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var students = await _unitOfWork
                .GenericRepository<Student>()
                .GetAllAsync();

            var studentIds = enrolls
                .Where(x =>
                    x.ClassSessionId == classSessionId)
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
                .Where(x =>
                    x.HomeroomTeacherId == teacher.Id)
                .Select(x =>
                {
                    var classItem = classes
                        .FirstOrDefault(c => c.Id == x.ClassId);

                    var subject = subjects
                        .FirstOrDefault(s => s.Id == x.SubjectId);

                    var session = sessions
                        .FirstOrDefault(s => s.Id == x.SessionId);

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
                        TeacherName =
                            teacher.FirstName + " " +
                            teacher.LastName,

                        HasAttendance = x.HasAttendance
                    };
                })
                .ToList();
        }

        // =====================================================
        // TEACHER - ASSIGNMENT
        // =====================================================

        public async Task AddAssignment(
            string userId,
            CreateAssignmentViewModel vm)
        {
            var teacher = await GetTeacherByUserId(userId);

            await ValidateAssignment(vm);

            var assignment = new Assignment
            {
                Title = vm.Title.Trim(),
                Description = vm.Description,
                DueDate = vm.DueDate,
                SubjectId = vm.SubjectId,
                TeacherId = teacher.Id,
                ClassSessionId = vm.ClassSessionId,
                Type = vm.Type
            };

            await _unitOfWork
                .GenericRepository<Assignment>()
                .AddAsync(assignment);

            _unitOfWork.Save();
        }

        public async Task<List<AssignmentViewModel>>
            GetTeacherAssignments(string userId)
        {
            var teacher = await GetTeacherByUserId(userId);

            var assignments = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetAllAsync();

            return assignments
                .Where(x => x.TeacherId == teacher.Id)
                .Select(x => new AssignmentViewModel(x))
                .ToList();
        }

        public async Task<AssignmentViewModel?>
            GetTeacherAssignmentById(
                string userId,
                int id)
        {
            var teacher = await GetTeacherByUserId(userId);

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x =>
                    x.Id == id &&
                    x.TeacherId == teacher.Id);

            if (assignment == null)
                return null;

            return new AssignmentViewModel(assignment);
        }

        public async Task UpdateAssignment(
            string userId,
            int id,
            CreateAssignmentViewModel vm)
        {
            var teacher = await GetTeacherByUserId(userId);

            await ValidateAssignment(vm);

            var repository =
                _unitOfWork.GenericRepository<Assignment>();

            var assignment = await repository.GetByIdAsync(x =>
                x.Id == id &&
                x.TeacherId == teacher.Id);

            if (assignment == null)
                throw new Exception(
                    "Không tìm thấy Assignment");

            assignment.Title = vm.Title.Trim();
            assignment.Description = vm.Description;
            assignment.DueDate = vm.DueDate;
            assignment.SubjectId = vm.SubjectId;
            assignment.ClassSessionId = vm.ClassSessionId;
            assignment.Type = vm.Type;

            repository.Update(assignment);

            _unitOfWork.Save();
        }

        public async Task DeleteAssignment(
            string userId,
            int id)
        {
            var teacher = await GetTeacherByUserId(userId);

            var repository =
                _unitOfWork.GenericRepository<Assignment>();

            var assignment = await repository.GetByIdAsync(x =>
                x.Id == id &&
                x.TeacherId == teacher.Id);

            if (assignment == null)
                throw new Exception(
                    "Không tìm thấy Assignment");

            repository.Delete(assignment);

            _unitOfWork.Save();
        }


        // =====================================================
        // STUDENT - ASSIGNMENT
        // =====================================================

        public async Task<List<AssignmentViewModel>>
            GetStudentAssignments(string userId)
        {
            var student = await GetStudentByUserId(userId);

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var classSessionIds = enrolls
                .Where(x => x.StudentId == student.Id)
                .Select(x => x.ClassSessionId)
                .ToList();

            var assignments = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetAllAsync();

            return assignments
                .Where(x =>
                    classSessionIds.Contains(x.ClassSessionId))
                .Select(x => new AssignmentViewModel(x))
                .ToList();
        }

        public async Task<AssignmentViewModel?>
            GetStudentAssignmentById(
                string userId,
                int id)
        {
            var student = await GetStudentByUserId(userId);

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var classSessionIds = enrolls
                .Where(x => x.StudentId == student.Id)
                .Select(x => x.ClassSessionId)
                .ToList();

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x =>
                    x.Id == id &&
                    classSessionIds.Contains(x.ClassSessionId));

            if (assignment == null)
                return null;

            return new AssignmentViewModel(assignment);
        }


        // =====================================================
        // STUDENT - SUBMIT ASSIGNMENT
        // =====================================================

        public async Task SubmitAssignment(
            string userId,
            int assignmentId,
            SubmitAssignmentRequest request)
        {
            // ---------------------------------------------
            // 1. Tìm Student
            // ---------------------------------------------

            var student = await GetStudentByUserId(userId);

            // ---------------------------------------------
            // 2. Tìm Assignment
            // ---------------------------------------------

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x =>
                    x.Id == assignmentId);

            if (assignment == null)
                throw new Exception(
                    "Không tìm thấy Assignment");


            // ---------------------------------------------
            // 3. Kiểm tra Student thuộc ClassSession
            // ---------------------------------------------

            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            var enrolled = enrolls.Any(x =>
                x.StudentId == student.Id &&
                x.ClassSessionId == assignment.ClassSessionId);

            if (!enrolled)
                throw new Exception(
                    "Student không thuộc lớp của Assignment này");


            // ---------------------------------------------
            // 4. Kiểm tra hạn nộp
            // ---------------------------------------------

            if (assignment.DueDate < DateTime.Now)
                throw new Exception(
                    "Đã quá hạn nộp bài");


            // ---------------------------------------------
            // 5. Kiểm tra đã nộp chưa
            // ---------------------------------------------

            var submissionRepository =
                _unitOfWork.GenericRepository<Submission>();

            var exists = submissionRepository.Exists(x =>
                x.AssignmentId == assignmentId &&
                x.StudentId == student.Id);

            if (exists)
                throw new Exception(
                    "Student đã nộp bài Assignment này");


            // ---------------------------------------------
            // 6. Kiểm tra File
            // ---------------------------------------------

            if (request == null)
                throw new Exception(
                    "Request không được để trống.");

            var file = request.File;

            if (file == null || file.Length == 0)
                throw new Exception(
                    "Vui lòng chọn file.");


            // ---------------------------------------------
            // 7. Kiểm tra loại File
            // ---------------------------------------------

            var allowedExtensions = new[]
            {
                ".doc",
                ".docx",
                ".pdf",
                ".jpg",
                ".jpeg",
                ".png"
            };

            var extension =
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception(
                    "Chỉ cho phép file Word, PDF hoặc hình ảnh.");
            }


            // ---------------------------------------------
            // 8. Kiểm tra dung lượng
            // Maximum = 10MB
            // ---------------------------------------------

            const long maxFileSize =
                10 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                throw new Exception(
                    "File không được vượt quá 10MB.");
            }


            // ---------------------------------------------
            // 9. Tạo thư mục upload
            // ---------------------------------------------

            var uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "assignments");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }


            // ---------------------------------------------
            // 10. Tạo tên file mới
            // ---------------------------------------------

            var fileName =
                $"{Guid.NewGuid()}{extension}";


            // ---------------------------------------------
            // 11. Đường dẫn lưu file
            // ---------------------------------------------

            var filePath = Path.Combine(
                uploadFolder,
                fileName);


            // ---------------------------------------------
            // 12. Lưu file
            // ---------------------------------------------

            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }


            // ---------------------------------------------
            // 13. Tạo FileUrl
            // ---------------------------------------------

            var fileUrl =
                $"/uploads/assignments/{fileName}";


            // ---------------------------------------------
            // 14. Tạo Submission
            // ---------------------------------------------

            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = student.Id,
                FileUrl = fileUrl,
                SubmittedAt = DateTime.Now,
                Score = null
            };


            // ---------------------------------------------
            // 15. Lưu Submission vào Database
            // ---------------------------------------------

            await submissionRepository
                .AddAsync(submission);

            _unitOfWork.Save();
        }


        // =====================================================
        // TEACHER - SUBMISSION
        // =====================================================

        public async Task<List<SubmissionViewModel>>
            GetSubmissions(
                string userId,
                int assignmentId)
        {
            var teacher =
                await GetTeacherByUserId(userId);

            // ---------------------------------------------
            // Kiểm tra Assignment thuộc Teacher
            // ---------------------------------------------

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x =>
                    x.Id == assignmentId &&
                    x.TeacherId == teacher.Id);

            if (assignment == null)
                throw new Exception(
                    "Không tìm thấy Assignment");


            // ---------------------------------------------
            // Lấy Submissions
            // ---------------------------------------------

            var submissions = await _unitOfWork
                .GenericRepository<Submission>()
                .GetAllAsync();

            return submissions
                .Where(x =>
                    x.AssignmentId == assignmentId)
                .Select(x =>
                    new SubmissionViewModel(x))
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
            var teacher =
                await GetTeacherByUserId(userId);


            // ---------------------------------------------
            // Kiểm tra điểm
            // ---------------------------------------------

            if (request.Score < 0 ||
                request.Score > 10)
            {
                throw new Exception(
                    "Điểm phải từ 0 đến 10");
            }


            // ---------------------------------------------
            // Tìm Submission
            // ---------------------------------------------

            var submission = await _unitOfWork
                .GenericRepository<Submission>()
                .GetByIdAsync(x =>
                    x.Id == submissionId);

            if (submission == null)
                throw new Exception(
                    "Không tìm thấy bài nộp");


            // ---------------------------------------------
            // Kiểm tra Assignment thuộc Teacher
            // ---------------------------------------------

            var assignment = await _unitOfWork
                .GenericRepository<Assignment>()
                .GetByIdAsync(x =>
                    x.Id == submission.AssignmentId &&
                    x.TeacherId == teacher.Id);

            if (assignment == null)
                throw new Exception(
                    "Bạn không có quyền chấm bài này");


            // ---------------------------------------------
            // Cập nhật điểm
            // ---------------------------------------------

            submission.Score = request.Score;

            _unitOfWork
                .GenericRepository<Submission>()
                .Update(submission);

            _unitOfWork.Save();
        }


        // =====================================================
        // PRIVATE - GET TEACHER
        // =====================================================

        private async Task<Teacher>
            GetTeacherByUserId(string userId)
        {
            var teacher = await _unitOfWork
                .GenericRepository<Teacher>()
                .GetByIdAsync(x =>
                    x.KeyId == userId);

            if (teacher == null)
                throw new Exception(
                    "Không tìm thấy Teacher của tài khoản");

            return teacher;
        }


        // =====================================================
        // PRIVATE - GET STUDENT
        // =====================================================

        private async Task<Student>
            GetStudentByUserId(string userId)
        {
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(x =>
                    x.KeyId == userId);

            if (student == null)
                throw new Exception(
                    "Không tìm thấy Student của tài khoản");

            return student;
        }


        // =====================================================
        // VALIDATE ASSIGNMENT
        // =====================================================

        private async Task ValidateAssignment(
            CreateAssignmentViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Title))
                throw new Exception(
                    "Title không được để trống");


            // ---------------------------------------------
            // Kiểm tra Subject
            // ---------------------------------------------

            var subject = await _unitOfWork
                .GenericRepository<Subject>()
                .GetByIdAsync(x =>
                    x.Id == vm.SubjectId);

            if (subject == null)
                throw new Exception(
                    "Không tìm thấy Subject");


            // ---------------------------------------------
            // Kiểm tra ClassSession
            // ---------------------------------------------

            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(x =>
                    x.Id == vm.ClassSessionId);

            if (classSession == null)
                throw new Exception(
                    "Không tìm thấy ClassSession");
        }
    }
}