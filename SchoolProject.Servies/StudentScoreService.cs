using Microsoft.EntityFrameworkCore;
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
            var teacherRepository =
                _unitOfWork.GenericRepository<Teacher>();

            var scoreRepository =
                _unitOfWork.GenericRepository<StudentScore>();

            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var classSessionRepository =
                _unitOfWork.GenericRepository<ClassSession>();


            var teacher =
                teacherRepository
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.KeyId == userId);

            if (teacher == null)
            {
                throw new Exception(
                    "Không tìm thấy Teacher.");
            }


            var scores =
                scoreRepository
                    .GetAll()
                    .ToList();


            var result =
                new List<StudentScoreViewModel>();


            foreach (var score in scores)
            {
                var enroll =
                    enrollRepository
                        .GetById(score.EnrollId);

                if (enroll == null)
                    continue;

                if (!enroll.ClassSessionId.HasValue)
                    continue;


                var classSession =
                    classSessionRepository
                        .GetById(
                            enroll.ClassSessionId.Value);

                if (classSession == null)
                    continue;


                if (classSession.HomeroomTeacherId
                    != teacher.Id)
                {
                    continue;
                }


                result.Add(
                    new StudentScoreViewModel(score)
                    {
                        SubjectId =
                            enroll.SubjectId
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
            if (vm == null)
            {
                throw new Exception(
                    "Dữ liệu không hợp lệ.");
            }

            if (vm.EnrollId <= 0)
            {
                throw new Exception(
                    "EnrollId không hợp lệ.");
            }


            var teacherRepository =
                _unitOfWork.GenericRepository<Teacher>();

            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var scoreRepository =
                _unitOfWork.GenericRepository<StudentScore>();

            var classSessionRepository =
                _unitOfWork.GenericRepository<ClassSession>();

            var sessionRepository =
                _unitOfWork.GenericRepository<Session>();


            // --------------------------------------------------------
            // Teacher
            // --------------------------------------------------------

            var teacher =
                teacherRepository
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.KeyId == userId);

            if (teacher == null)
            {
                throw new Exception(
                    "Không tìm thấy Teacher.");
            }


            // --------------------------------------------------------
            // Enroll
            // --------------------------------------------------------

            var enroll =
                enrollRepository
                    .GetById(vm.EnrollId);

            if (enroll == null)
            {
                throw new Exception(
                    "Không tìm thấy Enroll.");
            }


            // --------------------------------------------------------
            // ClassSession
            // --------------------------------------------------------

            if (!enroll.ClassSessionId.HasValue)
            {
                throw new Exception(
                    "Enroll chưa được gán vào ClassSession.");
            }


            var classSession =
                classSessionRepository
                    .GetById(
                        enroll.ClassSessionId.Value);

            if (classSession == null)
            {
                throw new Exception(
                    "Không tìm thấy ClassSession.");
            }


            // --------------------------------------------------------
            // Teacher phải dạy ClassSession
            // --------------------------------------------------------

            if (classSession.HomeroomTeacherId
                != teacher.Id)
            {
                throw new UnauthorizedAccessException(
                    "Bạn không có quyền tổng kết điểm cho lớp học này.");
            }


            // --------------------------------------------------------
            // Subject phải khớp
            // --------------------------------------------------------

            if (enroll.SubjectId
                != classSession.SubjectId)
            {
                throw new Exception(
                    "Subject của Enroll không khớp với ClassSession.");
            }


            // --------------------------------------------------------
            // Session
            // --------------------------------------------------------

            var session =
                sessionRepository
                    .GetById(classSession.SessionId);

            if (session == null)
            {
                throw new Exception(
                    "Không tìm thấy Session.");
            }


            // --------------------------------------------------------
            // Chỉ tổng kết sau khi Session kết thúc
            // --------------------------------------------------------

            if (DateTime.Now < session.End)
            {
                throw new Exception(
                    $"Chưa thể tổng kết điểm. " +
                    $"Session kết thúc vào " +
                    $"{session.End:dd/MM/yyyy HH:mm}.");
            }


            // --------------------------------------------------------
            // Kiểm tra đã có StudentScore chưa
            // --------------------------------------------------------

            var exists =
                scoreRepository
                    .GetAll()
                    .Any(x =>
                        x.EnrollId == enroll.Id);

            if (exists)
            {
                throw new Exception(
                    "Sinh viên này đã được tổng kết điểm.");
            }


            // --------------------------------------------------------
            // Tính điểm
            // --------------------------------------------------------

            var calculated =
                await CalculateStudentScore(
                    enroll,
                    classSession);


            // --------------------------------------------------------
            // Tạo StudentScore
            // --------------------------------------------------------

            var score = new StudentScore
            {
                EnrollId =
                    enroll.Id,

                AttendanceScore =
                    calculated.AttendanceScore,

                // Giữ QuizScore theo Model hiện tại.
                // Giá trị này lưu Assignment Average.
                QuizScore =
                    calculated.AssignmentAverage,

                MidtermScore = 0,

                FinalScore =
                    calculated.FinalExamScore,

                TotalScore =
                    calculated.TotalScore,

                LetterGrade =
                    calculated.LetterGrade
            };


            await scoreRepository.AddAsync(score);

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
            if (vm == null)
            {
                throw new Exception(
                    "Dữ liệu không hợp lệ.");
            }

            if (vm.EnrollId <= 0)
            {
                throw new Exception(
                    "EnrollId không hợp lệ.");
            }


            var teacherRepository =
                _unitOfWork.GenericRepository<Teacher>();

            var scoreRepository =
                _unitOfWork.GenericRepository<StudentScore>();

            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var classSessionRepository =
                _unitOfWork.GenericRepository<ClassSession>();

            var sessionRepository =
                _unitOfWork.GenericRepository<Session>();


            // --------------------------------------------------------
            // Teacher
            // --------------------------------------------------------

            var teacher =
                teacherRepository
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.KeyId == userId);

            if (teacher == null)
            {
                throw new Exception(
                    "Không tìm thấy Teacher.");
            }


            // --------------------------------------------------------
            // StudentScore
            // --------------------------------------------------------

            var score =
                scoreRepository
                    .GetById(id);

            if (score == null)
            {
                throw new Exception(
                    "Không tìm thấy StudentScore.");
            }


            // --------------------------------------------------------
            // Enroll
            // --------------------------------------------------------

            var enroll =
                enrollRepository
                    .GetById(vm.EnrollId);

            if (enroll == null)
            {
                throw new Exception(
                    "Không tìm thấy Enroll.");
            }


            // --------------------------------------------------------
            // Score phải thuộc Enroll
            // --------------------------------------------------------

            if (score.EnrollId != enroll.Id)
            {
                throw new Exception(
                    "StudentScore không thuộc Enroll này.");
            }


            // --------------------------------------------------------
            // ClassSession
            // --------------------------------------------------------

            if (!enroll.ClassSessionId.HasValue)
            {
                throw new Exception(
                    "Enroll chưa được gán vào ClassSession.");
            }


            var classSession =
                classSessionRepository
                    .GetById(
                        enroll.ClassSessionId.Value);

            if (classSession == null)
            {
                throw new Exception(
                    "Không tìm thấy ClassSession.");
            }


            // --------------------------------------------------------
            // Teacher
            // --------------------------------------------------------

            if (classSession.HomeroomTeacherId
                != teacher.Id)
            {
                throw new UnauthorizedAccessException(
                    "Bạn không có quyền cập nhật điểm cho lớp học này.");
            }


            // --------------------------------------------------------
            // Subject
            // --------------------------------------------------------

            if (enroll.SubjectId
                != classSession.SubjectId)
            {
                throw new Exception(
                    "Subject của Enroll không khớp với ClassSession.");
            }


            // --------------------------------------------------------
            // Session
            // --------------------------------------------------------

            var session =
                sessionRepository
                    .GetById(classSession.SessionId);

            if (session == null)
            {
                throw new Exception(
                    "Không tìm thấy Session.");
            }


            // --------------------------------------------------------
            // Chỉ update sau Session.End
            // --------------------------------------------------------

            if (DateTime.Now < session.End)
            {
                throw new Exception(
                    $"Chưa thể cập nhật điểm tổng kết. " +
                    $"Session kết thúc vào " +
                    $"{session.End:dd/MM/yyyy HH:mm}.");
            }


            // --------------------------------------------------------
            // Tính lại
            // --------------------------------------------------------

            var calculated =
                await CalculateStudentScore(
                    enroll,
                    classSession);


            score.AttendanceScore =
                calculated.AttendanceScore;

            score.QuizScore =
                calculated.AssignmentAverage;

            score.MidtermScore = 0;

            score.FinalScore =
                calculated.FinalExamScore;

            score.TotalScore =
                calculated.TotalScore;

            score.LetterGrade =
                calculated.LetterGrade;


            scoreRepository.Update(score);

            _unitOfWork.Save();
        }


        // ============================================================
        // 4. STUDENT - GET ALL SCORES
        // ============================================================

        public async Task<List<StudentScoreViewModel>>
            GetStudentScores(string userId)
        {
            var studentRepository =
                _unitOfWork.GenericRepository<Student>();

            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var scoreRepository =
                _unitOfWork.GenericRepository<StudentScore>();


            var student =
                studentRepository
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.KeyId == userId);

            if (student == null)
            {
                throw new Exception(
                    "Không tìm thấy Student.");
            }


            var enrolls =
                enrollRepository
                    .GetAll()
                    .Where(x =>
                        x.StudentId == student.Id)
                    .ToList();


            var enrollIds =
                enrolls
                    .Select(x => x.Id)
                    .ToList();


            var scores =
                scoreRepository
                    .GetAll()
                    .Where(x =>
                        enrollIds.Contains(
                            x.EnrollId))
                    .ToList();


            var result =
                new List<StudentScoreViewModel>();


            foreach (var score in scores)
            {
                var enroll =
                    enrolls.FirstOrDefault(x =>
                        x.Id == score.EnrollId);

                if (enroll == null)
                    continue;


                result.Add(
                    new StudentScoreViewModel(score)
                    {
                        SubjectId =
                            enroll.SubjectId
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
            var studentRepository =
                _unitOfWork.GenericRepository<Student>();

            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var scoreRepository =
                _unitOfWork.GenericRepository<StudentScore>();


            var student =
                studentRepository
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.KeyId == userId);

            if (student == null)
            {
                throw new Exception(
                    "Không tìm thấy Student.");
            }


            var enrolls =
                enrollRepository
                    .GetAll()
                    .Where(x =>
                        x.StudentId == student.Id &&
                        x.SubjectId == subjectId)
                    .ToList();


            var enrollIds =
                enrolls
                    .Select(x => x.Id)
                    .ToList();


            var scores =
                scoreRepository
                    .GetAll()
                    .Where(x =>
                        enrollIds.Contains(
                            x.EnrollId))
                    .ToList();


            var result =
                new List<StudentScoreViewModel>();


            foreach (var score in scores)
            {
                result.Add(
                    new StudentScoreViewModel(score)
                    {
                        SubjectId =
                            subjectId
                    });
            }


            return result;
        }


        // ============================================================
        // 6. CALCULATE STUDENT SCORE
        // ============================================================

        private async Task<StudentScoreResult>
            CalculateStudentScore(
                Enroll enroll,
                ClassSession classSession)
        {
            var assignmentRepository =
                _unitOfWork
                    .GenericRepository<Assignment>();

            var submissionRepository =
                _unitOfWork
                    .GenericRepository<Submission>();


            int subjectId =
                enroll.SubjectId;


            // ========================================================
            // A. ASSIGNMENT
            // ========================================================

            var assignments =
                assignmentRepository
                    .GetAll()
                    .Where(x =>
                        x.SubjectId == subjectId &&
                        x.ClassSessionId ==
                            classSession.Id &&
                        x.Type ==
                            AssignmentType.Assignment)
                    .OrderBy(x =>
                        x.DueDate)
                    .ToList();


            if (!assignments.Any())
            {
                throw new Exception(
                    "Subject chưa có Assignment.");
            }


            var assignmentIds =
                assignments
                    .Select(x => x.Id)
                    .ToList();


            var submissions =
                submissionRepository
                    .GetAll()
                    .Where(x =>
                        x.StudentId ==
                            enroll.StudentId &&
                        assignmentIds.Contains(
                            x.AssignmentId))
                    .ToList();


            var assignmentScores =
                new List<double>();


            foreach (var assignment in assignments)
            {
                var submission =
                    submissions.FirstOrDefault(x =>
                        x.AssignmentId ==
                        assignment.Id);


                // ----------------------------------------------------
                // Chưa nộp + chưa đến hạn
                // ----------------------------------------------------

                if (submission == null &&
                    assignment.DueDate >
                    DateTime.Now)
                {
                    throw new Exception(
                        $"Bài '{assignment.Title}' " +
                        $"chưa đến hạn và sinh viên chưa nộp.");
                }


                // ----------------------------------------------------
                // Chưa nộp + đã hết hạn
                // => 0 điểm
                // ----------------------------------------------------

                if (submission == null &&
                    assignment.DueDate <=
                    DateTime.Now)
                {
                    assignmentScores.Add(0);
                    continue;
                }


                // ----------------------------------------------------
                // Đã nộp nhưng chưa chấm
                // ----------------------------------------------------

                if (submission != null &&
                    !submission.Score.HasValue)
                {
                    throw new Exception(
                        $"Bài '{assignment.Title}' " +
                        $"đã nộp nhưng chưa được giáo viên chấm.");
                }


                // ----------------------------------------------------
                // Đã chấm
                // ----------------------------------------------------

                assignmentScores.Add(
                    submission!.Score!.Value);
            }


            double assignmentAverage =
                assignmentScores.Average();


            assignmentAverage =
                Math.Round(
                    assignmentAverage,
                    2);


            // ========================================================
            // B. FINAL EXAM
            // ========================================================

            var finalExams =
                assignmentRepository
                    .GetAll()
                    .Where(x =>
                        x.SubjectId == subjectId &&
                        x.ClassSessionId ==
                            classSession.Id &&
                        x.Type ==
                            AssignmentType.FinalExam)
                    .OrderBy(x =>
                        x.DueDate)
                    .ToList();


            if (finalExams.Count == 0)
            {
                throw new Exception(
                    "Subject chưa có Final Exam.");
            }


            if (finalExams.Count > 1)
            {
                throw new Exception(
                    "ClassSession có nhiều hơn 1 Final Exam.");
            }


            var finalExam =
                finalExams.First();


            var finalSubmission =
                submissionRepository
                    .GetAll()
                    .FirstOrDefault(x =>
                        x.StudentId ==
                            enroll.StudentId &&
                        x.AssignmentId ==
                            finalExam.Id);


            double finalExamScore;


            // --------------------------------------------------------
            // Chưa thi + chưa đến hạn
            // --------------------------------------------------------

            if (finalSubmission == null &&
                finalExam.DueDate >
                DateTime.Now)
            {
                throw new Exception(
                    "Final Exam chưa đến hạn.");
            }


            // --------------------------------------------------------
            // Không thi + quá hạn
            // => 0
            // --------------------------------------------------------

            if (finalSubmission == null &&
                finalExam.DueDate <=
                DateTime.Now)
            {
                finalExamScore = 0;
            }

            // --------------------------------------------------------
            // Đã thi nhưng chưa chấm
            // --------------------------------------------------------

            else if (finalSubmission != null &&
                     !finalSubmission.Score.HasValue)
            {
                throw new Exception(
                    "Final Exam đã nộp nhưng chưa được giáo viên chấm.");
            }

            // --------------------------------------------------------
            // Đã chấm
            // --------------------------------------------------------

            else
            {
                finalExamScore =
                    finalSubmission!.Score!.Value;
            }


            finalExamScore =
                Math.Round(
                    finalExamScore,
                    2);


            // ========================================================
            // C. ATTENDANCE
            // ========================================================

            double attendanceScore = 0;


            if (classSession.HasAttendance)
            {
                attendanceScore =
                    await CalculateAttendanceScore(
                        enroll,
                        classSession);
            }


            // ========================================================
            // D. TOTAL SCORE
            // ========================================================

            double totalScore;


            if (classSession.HasAttendance)
            {
                // Attendance = 10%
                // Assignment = 40%
                // Final = 50%

                totalScore =
                    (attendanceScore * 0.10)
                    +
                    (assignmentAverage * 0.40)
                    +
                    (finalExamScore * 0.50);
            }
            else
            {
                // Assignment = 50%
                // Final = 50%

                totalScore =
                    (assignmentAverage * 0.50)
                    +
                    (finalExamScore * 0.50);
            }


            totalScore =
                Math.Round(
                    totalScore,
                    2);


            // ========================================================
            // E. LETTER GRADE
            // ========================================================

            var letterGrade =
                CalculateLetterGrade(
                    totalScore);


            return new StudentScoreResult
            {
                AttendanceScore =
                    attendanceScore,

                AssignmentAverage =
                    assignmentAverage,

                FinalExamScore =
                    finalExamScore,

                TotalScore =
                    totalScore,

                LetterGrade =
                    letterGrade
            };
        }


        // ============================================================
        // 7. CALCULATE ATTENDANCE
        // ============================================================

        private async Task<double>
            CalculateAttendanceScore(
                Enroll enroll,
                ClassSession classSession)
        {
            if (!classSession.HasAttendance)
                return 0;


            if (classSession.TotalSessions <= 0)
            {
                throw new Exception(
                    "ClassSession chưa cấu hình TotalSessions.");
            }


            var attendanceRepository =
                _unitOfWork
                    .GenericRepository<Attendance>();


            var attendances =
                attendanceRepository
                    .GetAll()
                    .Where(x =>
                        x.EnrollId ==
                            enroll.Id &&
                        x.SubjectId ==
                            enroll.SubjectId)
                    .OrderBy(x =>
                        x.Date)
                    .ToList();


            // --------------------------------------------------------
            // Phải đủ số buổi
            // --------------------------------------------------------

            if (attendances.Count <
                classSession.TotalSessions)
            {
                throw new Exception(
                    $"Chưa đủ dữ liệu điểm danh: " +
                    $"{attendances.Count}/" +
                    $"{classSession.TotalSessions} buổi.");
            }


            // --------------------------------------------------------
            // Chỉ lấy đúng số buổi
            // --------------------------------------------------------

            var totalSessions =
                attendances
                    .Take(classSession.TotalSessions)
                    .ToList();


            var present =
                totalSessions.Count(x =>
                    x.Present);


            var attendanceScore =
                (double)present /
                classSession.TotalSessions *
                10;


            return Math.Round(
                attendanceScore,
                2);
        }


        // ============================================================
        // 8. LETTER GRADE
        // ============================================================

        private string CalculateLetterGrade(
            double total)
        {
            if (total >= 9.0)
                return "A";

            if (total >= 8.0)
                return "B";

            if (total >= 7.0)
                return "C";

            if (total >= 5.0)
                return "D";

            return "F";
        }
    }


    // ================================================================
    // SCORE RESULT
    // ================================================================

    public class StudentScoreResult
    {
        public double AttendanceScore { get; set; }

        public double AssignmentAverage { get; set; }

        public double FinalExamScore { get; set; }

        public double TotalScore { get; set; }

        public string LetterGrade { get; set; }
            = string.Empty;
    }
}