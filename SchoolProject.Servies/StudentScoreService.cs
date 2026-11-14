
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class StudentScoreService : IStudentScoreService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentScoreService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // 1. TEACHER - GET ALL SCORES
        // ============================================================

        public async Task<List<StudentScoreViewModel>>
            GetTeacherScores(string userId)
        {
            var teacher = _unitOfWork
                .GenericRepository<Teacher>()
                .GetAll()
                .FirstOrDefault(x => x.KeyId == userId);

            if (teacher == null)
                throw new Exception("Không tìm thấy Teacher.");

            var scores = _unitOfWork
                .GenericRepository<StudentScore>()
                .GetAll()
                .ToList();

            var result = new List<StudentScoreViewModel>();

            foreach (var score in scores)
            {
                var enroll = _unitOfWork
                    .GenericRepository<Enroll>()
                    .GetById(score.EnrollId);

                if (enroll == null ||
                    !enroll.ClassSessionId.HasValue)
                    continue;

                var classSession = _unitOfWork
                    .GenericRepository<ClassSession>()
                    .GetById(enroll.ClassSessionId.Value);

                if (classSession == null ||
                    classSession.HomeroomTeacherId != teacher.Id)
                    continue;

                result.Add(new StudentScoreViewModel(score)
                {
                    SubjectId = classSession.SubjectId
                });
            }

            return result;
        }

        // ============================================================
        // 2. TEACHER - ADD SCORE
        // ============================================================

        public async Task AddScore(
            string userId,
            CreateStudentScoreViewModel vm)
        {
            if (vm == null || vm.EnrollId <= 0)
                throw new Exception("Dữ liệu không hợp lệ.");

            var teacher = GetTeacher(userId);

            var enroll = _unitOfWork
                .GenericRepository<Enroll>()
                .GetById(vm.EnrollId);

            if (enroll == null)
                throw new Exception("Không tìm thấy Enroll.");

            if (enroll.Status != EnrollStatus.Approved)
                throw new Exception(
                    "Chỉ được nhập điểm cho sinh viên đã được duyệt vào lớp.");

            var classSession = GetClassSession(enroll);

            ValidateTeacherPermission(teacher, classSession);
            ValidateEnrollSubject(enroll, classSession);
            ValidateSessionEnded(classSession);

            var scoreRepository =
                _unitOfWork.GenericRepository<StudentScore>();

            bool exists = scoreRepository.GetAll()
                .Any(x => x.EnrollId == enroll.Id);

            if (exists)
                throw new Exception(
                    "Sinh viên này đã có kết quả tổng kết.");

            var calculated = await CalculateStudentScore(
                enroll,
                classSession);

            var score = new StudentScore
            {
                EnrollId = enroll.Id,

                AttendanceScore = calculated.AttendanceScore,
                QuizScore = calculated.AssignmentAverage,
                MidtermScore = calculated.MidtermExamScore,
                FinalScore = calculated.FinalExamScore,

                TotalScore = calculated.TotalScore,
                LetterGrade = calculated.LetterGrade,
                GPA = calculated.GPA,

                // Chưa chốt điểm
                IsFinalized = false
            };

            await scoreRepository.AddAsync(score);

            // Chưa cập nhật Enroll thành Completed/Failed.
            // Chờ giáo viên kiểm tra và chốt điểm.

            _unitOfWork.Save();
        }

        // ============================================================
        // 3. TEACHER - UPDATE SCORE
        // ============================================================

        public async Task UpdateScore(
       string userId,
       int id,
       CreateStudentScoreViewModel vm)
        {
            if (vm == null || vm.EnrollId <= 0)
                throw new Exception("Dữ liệu không hợp lệ.");

            var teacher = GetTeacher(userId);

            var scoreRepository =
                _unitOfWork.GenericRepository<StudentScore>();

            var score = scoreRepository.GetById(id);

            if (score == null)
                throw new Exception("Không tìm thấy StudentScore.");

            // Chỉ cho phép cập nhật bản nháp.
            if (score.IsFinalized)
            {
                throw new Exception(
                    "Điểm đã được chốt. Không được phép chỉnh sửa.");
            }

            var enroll = _unitOfWork
                .GenericRepository<Enroll>()
                .GetById(vm.EnrollId);

            if (enroll == null)
                throw new Exception("Không tìm thấy Enroll.");

            if (score.EnrollId != enroll.Id)
                throw new Exception(
                    "StudentScore không thuộc Enroll này.");

            if (enroll.Status != EnrollStatus.Approved)
                throw new Exception(
                    "Chỉ được chốt điểm cho sinh viên đang ở trạng thái Approved.");

            var classSession = GetClassSession(enroll);

            ValidateTeacherPermission(teacher, classSession);
            ValidateEnrollSubject(enroll, classSession);
            ValidateSessionEnded(classSession);

            // Tính toán lại toàn bộ kết quả.
            var calculated = await CalculateStudentScore(
                enroll,
                classSession);

            score.AttendanceScore = calculated.AttendanceScore;
            score.QuizScore = calculated.AssignmentAverage;
            score.MidtermScore = calculated.MidtermExamScore;
            score.FinalScore = calculated.FinalExamScore;

            score.TotalScore = calculated.TotalScore;
            score.LetterGrade = calculated.LetterGrade;
            score.GPA = calculated.GPA;

            // Chính thức chốt điểm.
            score.IsFinalized = true;

            enroll.Status = calculated.TotalScore >= 4.0
                ? EnrollStatus.Completed
                : EnrollStatus.Failed;

            _unitOfWork
                .GenericRepository<Enroll>()
                .Update(enroll);

            scoreRepository.Update(score);

            _unitOfWork.Save();
        }

        // ============================================================
        // 4. STUDENT - GET ALL SCORES
        // ============================================================

        public async Task<List<StudentScoreViewModel>>
            GetStudentScores(string userId)
        {
            var student = _unitOfWork
                .GenericRepository<Student>()
                .GetAll()
                .FirstOrDefault(x => x.KeyId == userId);

            if (student == null)
                throw new Exception("Không tìm thấy Student.");

            var enrolls = _unitOfWork
                .GenericRepository<Enroll>()
                .GetAll()
                .Where(x => x.StudentId == student.Id)
                .ToList();

            var enrollIds = enrolls.Select(x => x.Id).ToList();

            var scores = _unitOfWork
      .GenericRepository<StudentScore>()
      .GetAll()
      .Where(x =>
          enrollIds.Contains(x.EnrollId) &&
          x.IsFinalized)
      .ToList();

            var classSessions = _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAll()
                .ToList();

            var result = new List<StudentScoreViewModel>();

            foreach (var score in scores)
            {
                var enroll = enrolls.FirstOrDefault(
                    x => x.Id == score.EnrollId);

                if (enroll == null ||
                    !enroll.ClassSessionId.HasValue)
                    continue;

                var classSession = classSessions.FirstOrDefault(
                    x => x.Id == enroll.ClassSessionId.Value);

                if (classSession == null)
                    continue;

                result.Add(new StudentScoreViewModel(score)
                {
                    SubjectId = classSession.SubjectId
                });
            }

            return result;
        }

        // ============================================================
        // 5. STUDENT - GET SCORE BY SUBJECT
        // ============================================================

        public async Task<List<StudentScoreViewModel>>
            GetStudentScoresBySubject(
                string userId,
                int subjectId)
        {
            var student = _unitOfWork
                .GenericRepository<Student>()
                .GetAll()
                .FirstOrDefault(x => x.KeyId == userId);

            if (student == null)
                throw new Exception("Không tìm thấy Student.");

            var enrolls = _unitOfWork
                .GenericRepository<Enroll>()
                .GetAll()
                .Where(x =>
                    x.StudentId == student.Id &&
                    x.SubjectId == subjectId &&
                    x.ClassSessionId.HasValue)
                .ToList();

            var enrollIds = enrolls.Select(x => x.Id).ToList();

            var scores = _unitOfWork
      .GenericRepository<StudentScore>()
      .GetAll()
      .Where(x =>
          enrollIds.Contains(x.EnrollId) &&
          x.IsFinalized)
      .ToList();

            return scores.Select(score =>
                new StudentScoreViewModel(score)
                {
                    SubjectId = subjectId
                }).ToList();
        }

        // ============================================================
        // 6. HELPER METHODS
        // ============================================================

        private Teacher GetTeacher(string userId)
        {
            var teacher = _unitOfWork
                .GenericRepository<Teacher>()
                .GetAll()
                .FirstOrDefault(x => x.KeyId == userId);

            if (teacher == null)
                throw new Exception("Không tìm thấy Teacher.");

            return teacher;
        }

        private ClassSession GetClassSession(Enroll enroll)
        {
            if (!enroll.ClassSessionId.HasValue)
                throw new Exception(
                    "Enroll chưa được gán vào ClassSession.");

            var classSession = _unitOfWork
                .GenericRepository<ClassSession>()
                .GetById(enroll.ClassSessionId.Value);

            if (classSession == null)
                throw new Exception("Không tìm thấy ClassSession.");

            return classSession;
        }

        private void ValidateSessionEnded(ClassSession classSession)
        {
            var session = _unitOfWork
                .GenericRepository<Session>()
                .GetById(classSession.SessionId);

            if (session == null)
                throw new Exception("Không tìm thấy Session.");

            if (DateTime.Now < session.End)
            {
                throw new Exception(
                    $"Chưa thể tổng kết điểm. Session kết thúc vào " +
                    $"{session.End:dd/MM/yyyy HH:mm}.");
            }

            if (DateTime.Now.Date <= classSession.EndDate.Date)
            {
                throw new Exception(
                    $"Lớp học chưa kết thúc. Ngày kết thúc: " +
                    $"{classSession.EndDate:dd/MM/yyyy}.");
            }
        }

        // ============================================================
        // 7. CALCULATE STUDENT SCORE
        // ============================================================

        private async Task<StudentScoreResult> CalculateStudentScore(
            Enroll enroll,
            ClassSession classSession)
        {
            // A. GRADE CONFIGURATION

            var config = _unitOfWork
                .GenericRepository<GradeConfiguration>()
                .GetAll()
                .FirstOrDefault(x =>
                    x.ClassSessionId == classSession.Id);

            if (config == null)
                throw new Exception(
                    "ClassSession chưa được cấu hình trọng số điểm.");

            ValidateGradeConfiguration(config, classSession);

            // B. ASSIGNMENTS

            var assignments = _unitOfWork
                .GenericRepository<Assignment>()
                .GetAll()
                .Where(x =>
                    x.ClassSessionId == classSession.Id &&
                    x.Type == AssignmentType.Assignment)
                .OrderBy(x => x.DueDate)
                .ToList();

            double assignmentAverage = 0;

            if (config.QuizWeight > 0)
            {
                if (!assignments.Any())
                    throw new Exception(
                        "ClassSession chưa có Assignment.");

                var assignmentIds = assignments
                    .Select(x => x.Id)
                    .ToList();

                var submissions = _unitOfWork
                    .GenericRepository<Submission>()
                    .GetAll()
                    .Where(x =>
                        x.StudentId == enroll.StudentId &&
                        assignmentIds.Contains(x.AssignmentId))
                    .ToList();

                var assignmentScores = new List<double>();

                foreach (var assignment in assignments)
                {
                    var submission = submissions.FirstOrDefault(
                        x => x.AssignmentId == assignment.Id);

                    if (submission == null)
                    {
                        if (assignment.DueDate > DateTime.Now)
                        {
                            throw new Exception(
                                $"Bài '{assignment.Title}' chưa đến hạn " +
                                "và sinh viên chưa nộp.");
                        }

                        assignmentScores.Add(0);
                        continue;
                    }

                    if (!submission.Score.HasValue)
                    {
                        throw new Exception(
                            $"Bài '{assignment.Title}' đã nộp " +
                            "nhưng chưa được giáo viên chấm.");
                    }

                    ValidateScoreRange(
                        submission.Score.Value,
                        $"Điểm Assignment '{assignment.Title}'");

                    assignmentScores.Add(submission.Score.Value);
                }

                assignmentAverage = Math.Round(
                    assignmentScores.Average(), 2);
            }

            // C. MIDTERM

            double midtermExamScore = 0;

            if (config.MidtermWeight > 0)
            {
                midtermExamScore = await GetExamScore(
                    enroll,
                    classSession,
                    AssignmentType.MidtermExam,
                    "Midterm Exam");
            }

            // D. FINAL

            double finalExamScore = 0;

            if (config.FinalWeight > 0)
            {
                finalExamScore = await GetExamScore(
                    enroll,
                    classSession,
                    AssignmentType.FinalExam,
                    "Final Exam");
            }

            // E. ATTENDANCE

            double attendanceScore = 0;

            if (classSession.HasAttendance &&
                config.AttendanceWeight > 0)
            {
                attendanceScore = await CalculateAttendanceScore(
                    enroll,
                    classSession);
            }

            // F. WEIGHTED TOTAL - SCALE 10

            double totalScore =
                attendanceScore *
                    (double)config.AttendanceWeight / 100
                +
                assignmentAverage *
                    (double)config.QuizWeight / 100
                +
                midtermExamScore *
                    (double)config.MidtermWeight / 100
                +
                finalExamScore *
                    (double)config.FinalWeight / 100;

            totalScore = Math.Round(totalScore, 2);

            // G. CONVERT SCALE 10 TO SCALE 4

            var grade = ConvertGrade(totalScore);

            return new StudentScoreResult
            {
                AttendanceScore = attendanceScore,
                AssignmentAverage = assignmentAverage,
                MidtermExamScore = midtermExamScore,
                FinalExamScore = finalExamScore,

                TotalScore = totalScore,
                LetterGrade = grade.LetterGrade,
                GPA = grade.GPA
            };
        }

        // ============================================================
        // 8. GET MIDTERM / FINAL EXAM SCORE
        // ============================================================

        private async Task<double> GetExamScore(
            Enroll enroll,
            ClassSession classSession,
            AssignmentType examType,
            string examName)
        {
            var exams = _unitOfWork
                .GenericRepository<Assignment>()
                .GetAll()
                .Where(x =>
                    x.ClassSessionId == classSession.Id &&
                    x.Type == examType)
                .OrderBy(x => x.DueDate)
                .ToList();

            if (exams.Count == 0)
                throw new Exception(
                    $"ClassSession chưa có {examName}.");

            if (exams.Count > 1)
                throw new Exception(
                    $"ClassSession có nhiều hơn một {examName}.");

            var exam = exams.First();

            var submission = _unitOfWork
                .GenericRepository<Submission>()
                .GetAll()
                .FirstOrDefault(x =>
                    x.StudentId == enroll.StudentId &&
                    x.AssignmentId == exam.Id);

            if (submission == null)
            {
                if (exam.DueDate > DateTime.Now)
                    throw new Exception(
                        $"{examName} chưa đến hạn.");

                return 0;
            }

            if (!submission.Score.HasValue)
                throw new Exception(
                    $"{examName} đã nộp nhưng chưa được giáo viên chấm.");

            ValidateScoreRange(
                submission.Score.Value,
                $"Điểm {examName}");

            return Math.Round(submission.Score.Value, 2);
        }

        // ============================================================
        // 9. CALCULATE ATTENDANCE
        // ============================================================

        private async Task<double> CalculateAttendanceScore(
            Enroll enroll,
            ClassSession classSession)
        {
            if (!classSession.HasAttendance)
                return 0;

            if (classSession.TotalSessions <= 0)
                throw new Exception(
                    "ClassSession chưa cấu hình TotalSessions.");

            var scheduleIds = _unitOfWork
                .GenericRepository<Schedule>()
                .GetAll()
                .Where(x => x.ClassSessionId == classSession.Id)
                .Select(x => x.Id)
                .ToList();

            if (!scheduleIds.Any())
                throw new Exception(
                    "ClassSession chưa có Schedule.");

            var attendances = _unitOfWork
                .GenericRepository<Attendance>()
                .GetAll()
                .Where(x =>
                    x.EnrollId == enroll.Id &&
                    scheduleIds.Contains(x.ScheduleId))
                .OrderBy(x => x.Date)
                .ToList();

            if (attendances.Count < classSession.TotalSessions)
            {
                throw new Exception(
                    $"Chưa đủ dữ liệu điểm danh: " +
                    $"{attendances.Count}/" +
                    $"{classSession.TotalSessions} buổi.");
            }

            var totalSessions = attendances
                .Take(classSession.TotalSessions)
                .ToList();

            int present = totalSessions.Count(x => x.Present);

            double attendanceScore =
                (double)present /
                classSession.TotalSessions * 10;

            return Math.Round(attendanceScore, 2);
        }

        // ============================================================
        // 10. VALIDATIONS
        // ============================================================

        private void ValidateTeacherPermission(
            Teacher teacher,
            ClassSession classSession)
        {
            if (classSession.HomeroomTeacherId != teacher.Id)
            {
                throw new UnauthorizedAccessException(
                    "Bạn không có quyền thao tác điểm của lớp học này.");
            }
        }

        private void ValidateEnrollSubject(
            Enroll enroll,
            ClassSession classSession)
        {
            if (enroll.SubjectId != classSession.SubjectId)
            {
                throw new Exception(
                    "Subject của Enroll không khớp với ClassSession.");
            }
        }

        private void ValidateGradeConfiguration(
            GradeConfiguration config,
            ClassSession classSession)
        {
            var total =
                config.AttendanceWeight +
                config.QuizWeight +
                config.MidtermWeight +
                config.FinalWeight;

            if (config.AttendanceWeight < 0 ||
                config.QuizWeight < 0 ||
                config.MidtermWeight < 0 ||
                config.FinalWeight < 0)
            {
                throw new Exception(
                    "Trọng số điểm không được nhỏ hơn 0.");
            }

            if (config.AttendanceWeight > 100 ||
                config.QuizWeight > 100 ||
                config.MidtermWeight > 100 ||
                config.FinalWeight > 100)
            {
                throw new Exception(
                    "Mỗi trọng số không được vượt quá 100%.");
            }

            if (total != 100)
                throw new Exception(
                    "Tổng trọng số điểm phải bằng 100%.");

            if (!classSession.HasAttendance &&
                config.AttendanceWeight != 0)
            {
                throw new Exception(
                    "Lớp không sử dụng điểm danh. " +
                    "Trọng số chuyên cần phải bằng 0.");
            }
        }

        private void ValidateScoreRange(
            double score,
            string fieldName)
        {
            if (score < 0 || score > 10)
            {
                throw new Exception(
                    $"{fieldName} phải nằm trong khoảng từ 0 đến 10.");
            }
        }

        // ============================================================
        // 11. CONVERT GRADE
        // ============================================================

        private (string LetterGrade, double GPA) ConvertGrade(
            double score)
        {
            if (score >= 8.5)
                return ("A", 4.0);

            if (score >= 8.0)
                return ("B+", 3.5);

            if (score >= 7.0)
                return ("B", 3.0);

            if (score >= 6.5)
                return ("C+", 2.5);

            if (score >= 5.5)
                return ("C", 2.0);

            if (score >= 5.0)
                return ("D+", 1.5);

            if (score >= 4.0)
                return ("D", 1.0);

            return ("F", 0.0);
        }
    }

    // ================================================================
    // SCORE RESULT
    // ================================================================

    public class StudentScoreResult
    {
        public double AttendanceScore { get; set; }

        public double AssignmentAverage { get; set; }

        public double MidtermExamScore { get; set; }

        public double FinalExamScore { get; set; }

        public double TotalScore { get; set; }

        public string LetterGrade { get; set; } = string.Empty;

        public double GPA { get; set; }
    }
}
