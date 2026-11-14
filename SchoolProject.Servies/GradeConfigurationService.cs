
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class GradeConfigurationService
        : IGradeConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GradeConfigurationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ============================================================
        // GET TEACHER
        // ============================================================

        private Teacher GetTeacher(string userId)
        {
            var teacher = _unitOfWork
                .GenericRepository<Teacher>()
                .GetAll()
                .FirstOrDefault(x => x.KeyId == userId);

            if (teacher == null)
                throw new Exception("Không tìm thấy giáo viên.");

            return teacher;
        }

        // ============================================================
        // GET CLASS SESSION + CHECK TEACHER
        // ============================================================

        private ClassSession GetAuthorizedClassSession(
            string userId,
            int classSessionId)
        {
            var teacher = GetTeacher(userId);

            var classSession = _unitOfWork
                .GenericRepository<ClassSession>()
                .GetById(classSessionId);

            if (classSession == null)
                throw new Exception("Không tìm thấy lớp học phần.");

            if (classSession.HomeroomTeacherId != teacher.Id)
            {
                throw new UnauthorizedAccessException(
                    "Bạn không được phân công lớp học phần này.");
            }

            return classSession;
        }

        // ============================================================
        // VALIDATE WEIGHTS
        // ============================================================

        private void ValidateWeights(
            CreateGradeConfigurationViewModel vm,
            ClassSession classSession)
        {
            if (vm == null)
                throw new Exception("Dữ liệu không được để trống.");

            if (vm.AttendanceWeight < 0 ||
                vm.QuizWeight < 0 ||
                vm.MidtermWeight < 0 ||
                vm.FinalWeight < 0)
            {
                throw new Exception(
                    "Trọng số không được âm.");
            }

            if (vm.AttendanceWeight > 100 ||
                vm.QuizWeight > 100 ||
                vm.MidtermWeight > 100 ||
                vm.FinalWeight > 100)
            {
                throw new Exception(
                    "Trọng số không được vượt quá 100%.");
            }

            var total =
                vm.AttendanceWeight +
                vm.QuizWeight +
                vm.MidtermWeight +
                vm.FinalWeight;

            if (total != 100)
            {
                throw new Exception(
                    $"Tổng trọng số phải bằng 100%. Hiện tại: {total}%.");
            }

            if (!classSession.HasAttendance &&
                vm.AttendanceWeight != 0)
            {
                throw new Exception(
                    "Lớp không sử dụng điểm danh. " +
                    "Trọng số chuyên cần phải bằng 0.");
            }
        }

        // ============================================================
        // CHECK EXISTING SCORES
        // ============================================================

        private bool HasStudentScores(int classSessionId)
        {
            var enrollIds = _unitOfWork
                .GenericRepository<Enroll>()
                .GetAll()
                .Where(x => x.ClassSessionId == classSessionId)
                .Select(x => x.Id)
                .ToHashSet();

            return _unitOfWork
                .GenericRepository<StudentScore>()
                .GetAll()
                .Any(x => enrollIds.Contains(x.EnrollId));
        }

        // ============================================================
        // MAP VIEW MODEL
        // ============================================================

        private GradeConfigurationViewModel ToViewModel(
            GradeConfiguration config)
        {
            return new GradeConfigurationViewModel
            {
                Id = config.Id,
                ClassSessionId = config.ClassSessionId,
                AttendanceWeight = config.AttendanceWeight,
                QuizWeight = config.QuizWeight,
                MidtermWeight = config.MidtermWeight,
                FinalWeight = config.FinalWeight
            };
        }

        // ============================================================
        // CREATE
        // ============================================================

        public async Task<GradeConfigurationViewModel> Create(
            string userId,
            CreateGradeConfigurationViewModel vm)
        {
            if (vm == null)
                throw new Exception("Dữ liệu không được để trống.");

            var classSession = GetAuthorizedClassSession(
                userId,
                vm.ClassSessionId);

            ValidateWeights(vm, classSession);

            var repo = _unitOfWork
                .GenericRepository<GradeConfiguration>();

            bool exists = repo.GetAll().Any(
                x => x.ClassSessionId == vm.ClassSessionId);

            if (exists)
            {
                throw new Exception(
                    "Lớp học phần đã có cấu hình trọng số.");
            }

            var config = new GradeConfiguration
            {
                ClassSessionId = vm.ClassSessionId,
                AttendanceWeight = vm.AttendanceWeight,
                QuizWeight = vm.QuizWeight,
                MidtermWeight = vm.MidtermWeight,
                FinalWeight = vm.FinalWeight,
                CreatedAt = DateTime.Now
            };

            await repo.AddAsync(config);

            _unitOfWork.Save();

            return ToViewModel(config);
        }

        // ============================================================
        // UPDATE
        // ============================================================

        public Task<GradeConfigurationViewModel> Update(
            string userId,
            int id,
            CreateGradeConfigurationViewModel vm)
        {
            if (vm == null)
                throw new Exception("Dữ liệu không được để trống.");

            var classSession = GetAuthorizedClassSession(
                userId,
                vm.ClassSessionId);

            ValidateWeights(vm, classSession);

            var repo = _unitOfWork
                .GenericRepository<GradeConfiguration>();

            var config = repo.GetById(id);

            if (config == null)
                throw new Exception(
                    "Không tìm thấy cấu hình điểm.");

            if (config.ClassSessionId != vm.ClassSessionId)
            {
                throw new Exception(
                    "Không thể chuyển cấu hình sang lớp học phần khác.");
            }

            if (HasStudentScores(config.ClassSessionId))
            {
                throw new Exception(
                    "Lớp đã có điểm tổng kết. " +
                    "Không thể thay đổi trọng số.");
            }

            config.AttendanceWeight = vm.AttendanceWeight;
            config.QuizWeight = vm.QuizWeight;
            config.MidtermWeight = vm.MidtermWeight;
            config.FinalWeight = vm.FinalWeight;
            config.UpdatedAt = DateTime.Now;

            repo.Update(config);

            _unitOfWork.Save();

            return Task.FromResult(ToViewModel(config));
        }

        // ============================================================
        // GET BY CLASS SESSION
        // ============================================================

        public Task<GradeConfigurationViewModel> GetByClassSession(
            string userId,
            int classSessionId)
        {
            GetAuthorizedClassSession(userId, classSessionId);

            var config = _unitOfWork
                .GenericRepository<GradeConfiguration>()
                .GetAll()
                .FirstOrDefault(x =>
                    x.ClassSessionId == classSessionId);

            if (config == null)
                throw new Exception(
                    "Lớp học phần chưa thiết lập trọng số.");

            return Task.FromResult(ToViewModel(config));
        }
    }
}
