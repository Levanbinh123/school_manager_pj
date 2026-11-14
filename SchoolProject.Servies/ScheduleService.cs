
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScheduleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =========================================
        // GET ALL
        // =========================================
        public async Task<List<ScheduleViewModel>> GetAll()
        {
            var schedules = await _unitOfWork
                .GenericRepository<Schedule>()
                .GetAllAsync();

            var result = new List<ScheduleViewModel>();

            foreach (var schedule in schedules)
            {
                result.Add(await MapToViewModel(schedule));
            }

            return result;
        }

        // =========================================
        // GET BY ID
        // =========================================
        public async Task<ScheduleViewModel?> GetById(int id)
        {
            var schedule = await _unitOfWork
                .GenericRepository<Schedule>()
                .GetByIdAsync(x => x.Id == id);

            if (schedule == null)
            {
                return null;
            }

            return await MapToViewModel(schedule);
        }

        // =========================================
        // ADD
        // =========================================
        public async Task Add(CreateScheduleViewModel vm)
        {
            // 1. Kiểm tra ClassSession
            var classSession = await ValidateReferences(vm);

            // 2. Kiểm tra giáo viên được phân công
            if (!classSession.HomeroomTeacherId.HasValue)
            {
                throw new Exception(
                    "ClassSession chưa được phân công giáo viên.");
            }

            int teacherId = classSession.HomeroomTeacherId.Value;

            // 3. Lấy giờ bắt đầu và kết thúc từ StudyPeriod
            var (startTime, endTime) = await ResolveStudyTime(vm);

            if (startTime >= endTime)
            {
                throw new Exception(
                    "Giờ bắt đầu phải nhỏ hơn giờ kết thúc.");
            }

            // 4. Kiểm tra trùng lịch
            var repository =
                _unitOfWork.GenericRepository<Schedule>();

            var schedules = await repository.GetAllAsync();

            var classSessions = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAllAsync();

            bool hasConflict = schedules.Any(x =>
            {
                // Chỉ kiểm tra lịch cùng ngày và giao nhau về thời gian
                if (x.Day != vm.Day ||
                    x.StartTime >= endTime ||
                    x.EndTime <= startTime)
                {
                    return false;
                }

                var existingSession = classSessions.FirstOrDefault(
                    cs => cs.Id == x.ClassSessionId);

                if (existingSession == null)
                {
                    return false;
                }

                // Trùng lớp học phần, lớp hoặc giáo viên
                return x.ClassSessionId == classSession.Id
                    || existingSession.ClassId == classSession.ClassId
                    || existingSession.HomeroomTeacherId == teacherId;
            });

            if (hasConflict)
            {
                throw new Exception(
                    "Không thể tạo lịch: lớp học phần, lớp " +
                    "hoặc giáo viên đã có lịch trùng giờ.");
            }

            // 5. Tạo Schedule
            // Môn học, lớp và giáo viên lấy từ ClassSession
            var schedule = new Schedule
            {
                ClassSessionId = classSession.Id,
                Day = vm.Day,
                StartTime = startTime,
                EndTime = endTime
            };

            // 6. Lưu database
            await repository.AddAsync(schedule);
            _unitOfWork.Save();
        }

        // =========================================
        // UPDATE
        // =========================================
        public async Task Update(
            int id,
            CreateScheduleViewModel vm)
        {
            var repository =
                _unitOfWork.GenericRepository<Schedule>();

            // 1. Tìm lịch cần sửa
            var schedule = await repository
                .GetByIdAsync(x => x.Id == id);

            if (schedule == null)
            {
                throw new Exception("Không tìm thấy Schedule.");
            }

            // 2. Kiểm tra ClassSession
            var classSession = await ValidateReferences(vm);

            if (!classSession.HomeroomTeacherId.HasValue)
            {
                throw new Exception(
                    "ClassSession chưa được phân công giáo viên.");
            }

            int teacherId = classSession.HomeroomTeacherId.Value;

            // 3. Xác định giờ học
            var (startTime, endTime) = await ResolveStudyTime(vm);

            if (startTime >= endTime)
            {
                throw new Exception(
                    "Giờ bắt đầu phải nhỏ hơn giờ kết thúc.");
            }

            // 4. Kiểm tra trùng với các lịch khác
            var schedules = await repository.GetAllAsync();

            var classSessions = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAllAsync();

            bool hasConflict = schedules.Any(x =>
            {
                // Không so sánh với chính lịch đang sửa
                if (x.Id == id ||
                    x.Day != vm.Day ||
                    x.StartTime >= endTime ||
                    x.EndTime <= startTime)
                {
                    return false;
                }

                var existingSession = classSessions.FirstOrDefault(
                    cs => cs.Id == x.ClassSessionId);

                if (existingSession == null)
                {
                    return false;
                }

                return x.ClassSessionId == classSession.Id
                    || existingSession.ClassId == classSession.ClassId
                    || existingSession.HomeroomTeacherId == teacherId;
            });

            if (hasConflict)
            {
                throw new Exception(
                    "Không thể cập nhật lịch: lớp học phần, lớp " +
                    "hoặc giáo viên đã có lịch trùng giờ.");
            }

            // 5. Cập nhật lịch
            schedule.ClassSessionId = classSession.Id;
            schedule.Day = vm.Day;
            schedule.StartTime = startTime;
            schedule.EndTime = endTime;

            repository.Update(schedule);
            _unitOfWork.Save();
        }

        // =========================================
        // DELETE
        // =========================================
        public async Task Delete(int id)
        {
            var repository =
                _unitOfWork.GenericRepository<Schedule>();

            var schedule = await repository
                .GetByIdAsync(x => x.Id == id);

            if (schedule == null)
            {
                throw new Exception("Không tìm thấy Schedule.");
            }

            repository.Delete(schedule);
            _unitOfWork.Save();
        }

        // =========================================
        // MAP SCHEDULE TO VIEWMODEL
        // Lấy thông tin môn, lớp và giáo viên
        // thông qua ClassSession
        // =========================================
        private async Task<ScheduleViewModel> MapToViewModel(
            Schedule schedule)
        {
            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(x => x.Id == schedule.ClassSessionId);

            if (classSession == null)
            {
                throw new Exception(
                    $"Không tìm thấy ClassSession ID " +
                    $"{schedule.ClassSessionId}.");
            }

            var classEntity = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == classSession.ClassId);

            var subject = await _unitOfWork
                .GenericRepository<Subject>()
                .GetByIdAsync(x => x.Id == classSession.SubjectId);

            Teacher? teacher = null;

            if (classSession.HomeroomTeacherId.HasValue)
            {
                teacher = await _unitOfWork
                    .GenericRepository<Teacher>()
                    .GetByIdAsync(
                        x => x.Id == classSession.HomeroomTeacherId.Value);
            }

            return new ScheduleViewModel
            {
                Id = schedule.Id,
                ClassSessionId = schedule.ClassSessionId,

                SubjectId = classSession.SubjectId,
                SubjectName = subject?.Name,

                ClassId = classSession.ClassId,
                ClassName = classEntity?.Name,

                TeacherId = classSession.HomeroomTeacherId,
                TeacherName = teacher?.LastName,

                Day = schedule.Day,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                StartDate=classSession.StartDate,
                EndDate=classSession.EndDate,
            };
        }

        // =========================================
        // RESOLVE STUDY TIME
        // Chuyển tiết bắt đầu/kết thúc thành giờ
        // =========================================
        private async Task<(TimeSpan StartTime, TimeSpan EndTime)>
            ResolveStudyTime(CreateScheduleViewModel vm)
        {
            if (vm.StartPeriodId <= 0 ||
                vm.EndPeriodId <= 0)
            {
                throw new Exception(
                    "Vui lòng chọn tiết bắt đầu và tiết kết thúc hợp lệ.");
            }

            var periodRepository =
                _unitOfWork.GenericRepository<StudyPeriod>();

            // Lấy tiết bắt đầu
            var startPeriod = await periodRepository
                .GetByIdAsync(x => x.Id == vm.StartPeriodId);

            if (startPeriod == null || !startPeriod.IsActive)
            {
                throw new Exception(
                    "Tiết bắt đầu không tồn tại hoặc đã bị vô hiệu hóa.");
            }

            // Lấy tiết kết thúc
            var endPeriod = await periodRepository
                .GetByIdAsync(x => x.Id == vm.EndPeriodId);

            if (endPeriod == null || !endPeriod.IsActive)
            {
                throw new Exception(
                    "Tiết kết thúc không tồn tại hoặc đã bị vô hiệu hóa.");
            }

            // Kiểm tra thứ tự tiết
            if (startPeriod.PeriodNumber > endPeriod.PeriodNumber)
            {
                throw new Exception(
                    "Tiết bắt đầu phải nhỏ hơn hoặc bằng tiết kết thúc.");
            }

            // Lấy các tiết hoạt động trong khoảng
            var allPeriods = await periodRepository.GetAllAsync();

            var selectedPeriods = allPeriods
                .Where(p =>
                    p.IsActive &&
                    p.PeriodNumber >= startPeriod.PeriodNumber &&
                    p.PeriodNumber <= endPeriod.PeriodNumber)
                .OrderBy(p => p.PeriodNumber)
                .ToList();

            if (selectedPeriods.Count == 0)
            {
                throw new Exception(
                    "Không tìm thấy các tiết học đã chọn.");
            }

            // Đảm bảo đúng tiết đầu và tiết cuối
            if (selectedPeriods.First().Id != startPeriod.Id ||
                selectedPeriods.Last().Id != endPeriod.Id)
            {
                throw new Exception(
                    "Khoảng tiết học không hợp lệ.");
            }

            // Kiểm tra tính liên tiếp và thời gian liền nhau
            for (int i = 1; i < selectedPeriods.Count; i++)
            {
                var previous = selectedPeriods[i - 1];
                var current = selectedPeriods[i];

                if (current.PeriodNumber !=
                    previous.PeriodNumber + 1)
                {
                    throw new Exception(
                        "Các tiết học được chọn không liên tiếp.");
                }

                if (previous.EndTime != current.StartTime)
                {
                    throw new Exception(
                        "Giờ kết thúc tiết trước phải bằng " +
                        "giờ bắt đầu tiết tiếp theo.");
                }
            }

            return (
                selectedPeriods.First().StartTime,
                selectedPeriods.Last().EndTime
            );
        }

        // =========================================
        // VALIDATE REFERENCES
        // Kiểm tra ClassSession và lớp
        // =========================================
        // =========================================
        private async Task<ClassSession> ValidateReferences(
            CreateScheduleViewModel vm)
        {
            // 1. Kiểm tra ClassSession có tồn tại không
            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(x => x.Id == vm.ClassSessionId);

            if (classSession == null)
            {
                throw new Exception(
                    "Không tìm thấy ClassSession.");
            }

            // 2. Chỉ cho phép tạo/cập nhật TKB khi lớp đã Confirmed
            //if (classSession.Status != ClassSessionStatus.Confirmed)
            //{
            //    throw new Exception(
            //        "Chỉ được tạo hoặc cập nhật thời khóa biểu " +
            //        "khi lớp học phần đã được xác nhận.");
            //}

            // 3. Kiểm tra lớp học có tồn tại không
            var classEntity = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == classSession.ClassId);

            if (classEntity == null)
            {
                throw new Exception(
                    "Không tìm thấy lớp học.");
            }

            return classSession;
        }

        // =========================================
        // STUDENT: GET MY SCHEDULE
        // =========================================
        public async Task<List<ScheduleViewModel>>
            GetStudentScheduleByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "Không tìm thấy UserId.");
            }

            // 1. Tìm Student theo tài khoản đăng nhập
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(x =>
                    x.KeyId == userId &&
                    !x.IsDeleted);

            if (student == null)
            {
                throw new UnauthorizedAccessException(
                    "Tài khoản này chưa được liên kết với học sinh.");
            }

            // 2. Lấy các đăng ký của học sinh
            var enrolls = await _unitOfWork
                .GenericRepository<Enroll>()
                .GetAllAsync();

            // Chỉ lấy đăng ký đã được duyệt hoặc đã hoàn thành
            var classSessionIds = enrolls
                .Where(x =>
                    x.StudentId == student.Id &&
                    x.ClassSessionId.HasValue &&
                    (x.Status == EnrollStatus.Approved ||
                     x.Status == EnrollStatus.Completed))
                .Select(x => x.ClassSessionId!.Value)
                .Distinct()
                .ToList();

            if (!classSessionIds.Any())
            {
                return new List<ScheduleViewModel>();
            }

            // 3. Lấy các lớp học phần hợp lệ
            var classSessions = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAllAsync();

            var validClassSessionIds = classSessions
                .Where(x =>
                    classSessionIds.Contains(x.Id) &&
                    x.Status != ClassSessionStatus.Cancelled)
                .Select(x => x.Id)
                .ToList();

            if (!validClassSessionIds.Any())
            {
                return new List<ScheduleViewModel>();
            }

            // 4. Lấy lịch học của các lớp học phần đó
            var schedules = await _unitOfWork
                .GenericRepository<Schedule>()
                .GetAllAsync();

            var mySchedules = schedules
                .Where(x => validClassSessionIds.Contains(x.ClassSessionId))
                .OrderBy(x => x.Day)
                .ThenBy(x => x.StartTime)
                .ToList();

            // 5. Chuyển sang ScheduleViewModel
            var result = new List<ScheduleViewModel>();

            foreach (var schedule in mySchedules)
            {
                result.Add(await MapToViewModel(schedule));
            }

            return result;
        }


        // =========================================
        // TEACHER: GET MY TEACHING SCHEDULE
        // =========================================
        public async Task<List<ScheduleViewModel>>
            GetTeacherScheduleByUserId(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException(
                    "Không tìm thấy UserId.");
            }

            // 1. Tìm Teacher theo tài khoản đăng nhập
            var teacher = await _unitOfWork
                .GenericRepository<Teacher>()
                .GetByIdAsync(x => x.KeyId == userId);

            if (teacher == null)
            {
                throw new UnauthorizedAccessException(
                    "Tài khoản này chưa được liên kết với giáo viên.");
            }

            // 2. Lấy các ClassSession được phân công cho giáo viên
            var classSessions = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetAllAsync();

            var myClassSessionIds = classSessions
                .Where(x =>
                    x.HomeroomTeacherId == teacher.Id &&
                    x.Status != ClassSessionStatus.Cancelled)
                .Select(x => x.Id)
                .ToList();

            if (!myClassSessionIds.Any())
            {
                return new List<ScheduleViewModel>();
            }

            // 3. Lấy lịch dạy
            var schedules = await _unitOfWork
                .GenericRepository<Schedule>()
                .GetAllAsync();

            var mySchedules = schedules
                .Where(x => myClassSessionIds.Contains(x.ClassSessionId))
                .OrderBy(x => x.Day)
                .ThenBy(x => x.StartTime)
                .ToList();

            // 4. Chuyển sang ScheduleViewModel
            var result = new List<ScheduleViewModel>();

            foreach (var schedule in mySchedules)
            {
                result.Add(await MapToViewModel(schedule));
            }

            return result;
        }
    }
}
