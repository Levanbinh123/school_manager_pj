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

        // ============================================================
        // GET ALL
        // ============================================================

        public async Task<List<ScheduleViewModel>> GetAll()
        {
            var schedules = await _unitOfWork
                .GenericRepository<Schedule>()
                .GetAllAsync();

            return schedules
                .Select(x => new ScheduleViewModel(x))
                .ToList();
        }

        // ============================================================
        // GET BY ID
        // ============================================================

        public async Task<ScheduleViewModel?> GetById(int id)
        {
            var schedule = await _unitOfWork
                .GenericRepository<Schedule>()
                .GetByIdAsync(x => x.Id == id);

            if (schedule == null)
            {
                return null;
            }

            return new ScheduleViewModel(schedule);
        }

        // ============================================================
        // ADD
        // ============================================================

        public async Task Add(CreateScheduleViewModel vm)
        {
            ValidateTime(vm);

            await ValidateReferences(vm);

            var repository =
                _unitOfWork.GenericRepository<Schedule>();

            var exists = repository.Exists(x =>
                x.ClassSessionId == vm.ClassSessionId &&
                x.Day == vm.Day &&
                x.StartTime == vm.StartTime
            );

            if (exists)
            {
                throw new Exception(
                    "Lịch học này đã tồn tại.");
            }

            var schedule = new Schedule
            {
                ClassSessionId = vm.ClassSessionId,
                SubjectId = vm.SubjectId,
                TeacherId = vm.TeacherId,
                Day = vm.Day,
                StartTime = vm.StartTime,
                EndTime = vm.EndTime,
                Room = string.IsNullOrWhiteSpace(vm.Room)
                    ? null
                    : vm.Room.Trim()
            };

            await repository.AddAsync(schedule);

            _unitOfWork.Save();
        }

        // ============================================================
        // UPDATE
        // ============================================================

        public async Task Update(
            int id,
            CreateScheduleViewModel vm)
        {
            ValidateTime(vm);

            await ValidateReferences(vm);

            var repository =
                _unitOfWork.GenericRepository<Schedule>();

            var schedule = await repository
                .GetByIdAsync(x => x.Id == id);

            if (schedule == null)
            {
                throw new Exception(
                    "Không tìm thấy Schedule.");
            }

            var exists = repository.Exists(x =>
                x.Id != id &&
                x.ClassSessionId == vm.ClassSessionId &&
                x.Day == vm.Day &&
                x.StartTime == vm.StartTime
            );

            if (exists)
            {
                throw new Exception(
                    "Lịch học này đã tồn tại.");
            }

            schedule.ClassSessionId =
                vm.ClassSessionId;

            schedule.SubjectId =
                vm.SubjectId;

            schedule.TeacherId =
                vm.TeacherId;

            schedule.Day =
                vm.Day;

            schedule.StartTime =
                vm.StartTime;

            schedule.EndTime =
                vm.EndTime;

            schedule.Room =
                string.IsNullOrWhiteSpace(vm.Room)
                    ? null
                    : vm.Room.Trim();

            repository.Update(schedule);

            _unitOfWork.Save();
        }

        // ============================================================
        // DELETE
        // ============================================================

        public async Task Delete(int id)
        {
            var repository =
                _unitOfWork.GenericRepository<Schedule>();

            var schedule = await repository
                .GetByIdAsync(x => x.Id == id);

            if (schedule == null)
            {
                throw new Exception(
                    "Không tìm thấy Schedule.");
            }

            repository.Delete(schedule);

            _unitOfWork.Save();
        }

        // ============================================================
        // VALIDATE TIME
        // ============================================================

        private void ValidateTime(
            CreateScheduleViewModel vm)
        {
            if (vm.StartTime >= vm.EndTime)
            {
                throw new Exception(
                    "StartTime phải nhỏ hơn EndTime.");
            }
        }

        // ============================================================
        // VALIDATE REFERENCES
        // ============================================================

        private async Task ValidateReferences(
            CreateScheduleViewModel vm)
        {
            // --------------------------------------------------------
            // Kiểm tra ClassSession
            // --------------------------------------------------------

            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(
                    x => x.Id == vm.ClassSessionId);

            if (classSession == null)
            {
                throw new Exception(
                    "Không tìm thấy ClassSession.");
            }

            // --------------------------------------------------------
            // Kiểm tra Subject
            // --------------------------------------------------------

            var subject = await _unitOfWork
                .GenericRepository<Subject>()
                .GetByIdAsync(
                    x => x.Id == vm.SubjectId);

            if (subject == null)
            {
                throw new Exception(
                    "Không tìm thấy Subject.");
            }

            // --------------------------------------------------------
            // Subject phải thuộc ClassSession
            // --------------------------------------------------------

            if (classSession.SubjectId != vm.SubjectId)
            {
                throw new Exception(
                    "Subject không thuộc ClassSession này.");
            }

            // --------------------------------------------------------
            // Kiểm tra Teacher
            // --------------------------------------------------------

            var teacher = await _unitOfWork
                .GenericRepository<Teacher>()
                .GetByIdAsync(
                    x => x.Id == vm.TeacherId);

            if (teacher == null)
            {
                throw new Exception(
                    "Không tìm thấy Teacher.");
            }
        }
    }
}