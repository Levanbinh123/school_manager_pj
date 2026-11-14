using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================
        // HELPER: TÌM TEACHER
        // =====================================================
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

        // =====================================================
        // HELPER: KIỂM TRA SCHEDULE VÀ QUYỀN TEACHER
        // =====================================================
        private Schedule GetAuthorizedSchedule(
       string userId,
       int scheduleId)
        {
            var teacher = GetTeacher(userId);

            var schedule = _unitOfWork
                .GenericRepository<Schedule>()
                .GetById(scheduleId);

            if (schedule == null)
                throw new Exception("Không tìm thấy Schedule.");

            var classSession = _unitOfWork
                .GenericRepository<ClassSession>()
                .GetById(schedule.ClassSessionId);

            if (classSession == null)
                throw new Exception("Không tìm thấy ClassSession.");

            // Nếu Schedule đã được phân công giáo viên,
            // kiểm tra giáo viên được phân công.
            if (schedule.TeacherId.HasValue)
            {
                if (schedule.TeacherId.Value != teacher.Id)
                    throw new Exception(
                        "Bạn không được phân công dạy lịch học này.");
            }
            // Nếu Schedule chưa có TeacherId,
            // sử dụng giáo viên chủ nhiệm của ClassSession.
            else if (classSession.HomeroomTeacherId != teacher.Id)
            {
                throw new Exception(
                    "Bạn không có quyền điểm danh lớp học này.");
            }

            if (!classSession.HasAttendance)
                throw new Exception(
                    "Lớp học này không sử dụng điểm danh.");

            return schedule;
        }

        // =====================================================
        // HELPER: KIỂM TRA ENROLL THUỘC SCHEDULE
        // =====================================================
        private Enroll GetValidEnroll(
            Schedule schedule,
            int enrollId)
        {
            var enroll = _unitOfWork
                .GenericRepository<Enroll>()
                .GetById(enrollId);

            if (enroll == null)
                throw new Exception("Không tìm thấy Enroll.");

            if (enroll.ClassSessionId != schedule.ClassSessionId)
                throw new Exception(
                    "Sinh viên không thuộc lớp học của Schedule này.");

            // Chỉ điểm danh sinh viên đã được duyệt đăng ký.
            if (enroll.Status != EnrollStatus.Approved)
                throw new Exception(
                    "Sinh viên chưa được duyệt đăng ký lớp học.");

            if (schedule.SubjectId.HasValue &&
                schedule.SubjectId.Value != enroll.SubjectId)
            {
                throw new Exception(
                    "Môn học của Schedule không khớp với Enroll.");
            }

            return enroll;
        }

        // =====================================================
        // HELPER: KIỂM TRA NGÀY HỌC
        // =====================================================
        private void ValidateAttendanceDate(
            Schedule schedule,
            DateTime date)
        {
            if (date == default)
                throw new Exception("Ngày điểm danh không hợp lệ.");

            if (date.DayOfWeek != schedule.Day)
            {
                throw new Exception(
                    $"Lịch học này diễn ra vào {schedule.Day}, " +
                    "ngày điểm danh không khớp.");
            }
        }

        // =====================================================
        // POST ATTENDANCE
        // Điểm danh một sinh viên trong một buổi học
        // =====================================================
        public async Task AddAttendance(
            string userId,
            CreateAttendanceViewModel vm)
        {
            var schedule = GetAuthorizedSchedule(
                userId,
                vm.ScheduleId);

            ValidateAttendanceDate(schedule, vm.Date);

            var enroll = GetValidEnroll(
                schedule,
                vm.EnrollId);

            var attendanceRepository =
                _unitOfWork.GenericRepository<Attendance>();

            var exists = attendanceRepository
                .GetAll()
                .Any(x =>
                    x.ScheduleId == vm.ScheduleId &&
                    x.EnrollId == vm.EnrollId &&
                    x.Date.Date == vm.Date.Date);

            if (exists)
                throw new Exception(
                    "Sinh viên đã được điểm danh trong buổi học này.");

            var attendance = new Attendance
            {
                ScheduleId = schedule.Id,
                EnrollId = enroll.Id,
                Date = vm.Date.Date,
                Present = vm.Present,
                Note = vm.Note
            };

            await attendanceRepository.AddAsync(attendance);

            _unitOfWork.Save();
        }

        // =====================================================
        // GET ATTENDANCE BY SCHEDULE
        // Lấy lịch sử điểm danh của một lịch học
        // =====================================================
        public Task<List<AttendanceViewModel>> GetBySchedule(
            string userId,
            int scheduleId)
        {
            var schedule = GetAuthorizedSchedule(
                userId,
                scheduleId);

            var attendances = _unitOfWork
                .GenericRepository<Attendance>()
                .GetAll()
                .Where(x => x.ScheduleId == schedule.Id)
                .OrderByDescending(x => x.Date)
                .ToList();

            var result = attendances
                .Select(x => new AttendanceViewModel(x))
                .ToList();

            return Task.FromResult(result);
        }

        // =====================================================
        // GET STUDENT ATTENDANCE
        // Sinh viên xem lịch sử điểm danh của mình
        // =====================================================
        public Task<List<AttendanceViewModel>> GetStudentAttendance(
            string userId)
        {
            var student = _unitOfWork
                .GenericRepository<Student>()
                .GetAll()
                .FirstOrDefault(x => x.KeyId == userId);

            if (student == null)
                throw new Exception("Không tìm thấy Student.");

            var enrollIds = _unitOfWork
                .GenericRepository<Enroll>()
                .GetAll()
                .Where(x => x.StudentId == student.Id)
                .Select(x => x.Id)
                .ToList();

            var attendances = _unitOfWork
                .GenericRepository<Attendance>()
                .GetAll()
                .Where(x => enrollIds.Contains(x.EnrollId))
                .OrderByDescending(x => x.Date)
                .ToList();

            var result = attendances
                .Select(x => new AttendanceViewModel(x))
                .ToList();

            return Task.FromResult(result);
        }

        // =====================================================
        // PUT ATTENDANCE
        // Cập nhật kết quả điểm danh
        // =====================================================
        public Task UpdateAttendance(
            string userId,
            int id,
            CreateAttendanceViewModel vm)
        {
            var attendanceRepository =
                _unitOfWork.GenericRepository<Attendance>();

            var attendance = attendanceRepository.GetById(id);

            if (attendance == null)
                throw new Exception("Không tìm thấy Attendance.");

            // Không cho chuyển bản ghi điểm danh sang Schedule khác.
            if (attendance.ScheduleId != vm.ScheduleId)
                throw new Exception(
                    "Không thể chuyển Attendance sang Schedule khác.");

            var schedule = GetAuthorizedSchedule(
                userId,
                attendance.ScheduleId);

            ValidateAttendanceDate(schedule, vm.Date);

            GetValidEnroll(schedule, attendance.EnrollId);

            var exists = attendanceRepository
                .GetAll()
                .Any(x =>
                    x.Id != id &&
                    x.ScheduleId == attendance.ScheduleId &&
                    x.EnrollId == attendance.EnrollId &&
                    x.Date.Date == vm.Date.Date);

            if (exists)
                throw new Exception(
                    "Sinh viên đã có bản ghi điểm danh trong ngày này.");

            // Không thay đổi ScheduleId và EnrollId.
            attendance.Date = vm.Date.Date;
            attendance.Present = vm.Present;
            attendance.Note = vm.Note;

            attendanceRepository.Update(attendance);

            _unitOfWork.Save();

            return Task.CompletedTask;
        }
        // BULK ATTENDANCE
        // Điểm danh nhiều sinh viên trong cùng một buổi học
        // =====================================================
        public async Task<BulkAttendanceResultViewModel> BulkAttendance(
            string userId,
            BulkAttendanceViewModel vm)
        {
            if (vm.Attendances == null || !vm.Attendances.Any())
                throw new Exception("Danh sách điểm danh không được để trống.");

            // 1. Kiểm tra Schedule và quyền giáo viên
            var schedule = GetAuthorizedSchedule(
                userId,
                vm.ScheduleId);

            // 2. Kiểm tra ngày học
            ValidateAttendanceDate(schedule, vm.Date);

            // 3. Không cho một EnrollId xuất hiện nhiều lần
            var duplicateEnrollIds = vm.Attendances
                .GroupBy(x => x.EnrollId)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();

            if (duplicateEnrollIds.Any())
            {
                throw new Exception(
                    "Danh sách có EnrollId bị trùng: " +
                    string.Join(", ", duplicateEnrollIds));
            }

            var attendanceRepository =
                _unitOfWork.GenericRepository<Attendance>();

            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            // 4. Kiểm tra toàn bộ Enroll trước khi ghi dữ liệu
            var enrollIds = vm.Attendances
                .Select(x => x.EnrollId)
                .ToList();

            var enrolls = enrollRepository
                .GetAll()
                .Where(x => enrollIds.Contains(x.Id))
                .ToList();

            if (enrolls.Count != enrollIds.Count)
                throw new Exception(
                    "Có sinh viên không tồn tại trong hệ thống.");

            foreach (var item in vm.Attendances)
            {
                GetValidEnroll(schedule, item.EnrollId);
            }

            // 5. Lấy các bản ghi điểm danh đã tồn tại
            var existingAttendances = attendanceRepository
                .GetAll()
                .Where(x =>
                    x.ScheduleId == schedule.Id &&
                    x.Date.Date == vm.Date.Date &&
                    enrollIds.Contains(x.EnrollId))
                .ToList();

            int created = 0;
            int updated = 0;

            // 6. Thêm mới hoặc cập nhật
            foreach (var item in vm.Attendances)
            {
                var existing = existingAttendances
                    .FirstOrDefault(x => x.EnrollId == item.EnrollId);

                if (existing == null)
                {
                    var attendance = new Attendance
                    {
                        ScheduleId = schedule.Id,
                        EnrollId = item.EnrollId,
                        Date = vm.Date.Date,
                        Present = item.Present,
                        Note = item.Note
                    };

                    await attendanceRepository.AddAsync(attendance);

                    created++;
                }
                else
                {
                    existing.Present = item.Present;
                    existing.Note = item.Note;

                    attendanceRepository.Update(existing);

                    updated++;
                }
            }

            // 7. Lưu một lần
            _unitOfWork.Save();

            return new BulkAttendanceResultViewModel
            {
                Total = vm.Attendances.Count,
                Created = created,
                Updated = updated,
                Message = "Điểm danh hàng loạt thành công."
            };
        }
        public Task<List<AttendanceStudentViewModel>> GetStudentsForAttendance(
    string userId,
    int scheduleId,
    DateTime date)
        {
            // 1. Kiểm tra Schedule và quyền giáo viên
            var schedule = GetAuthorizedSchedule(userId, scheduleId);

            // 2. Kiểm tra ngày học
            ValidateAttendanceDate(schedule, date);

            // 3. Lấy danh sách sinh viên đã được duyệt
            var enrolls = _unitOfWork.GenericRepository<Enroll>()
      .GetAll()
      .Where(x => x.ClassSessionId == schedule.ClassSessionId &&
                  x.Status == EnrollStatus.Approved)
      .ToList();

            var students = _unitOfWork.GenericRepository<Student>()
                .GetAll()
                .ToDictionary(x => x.Id);

            // 4. Lấy danh sách điểm danh đã tồn tại
            var enrollIds = enrolls.Select(x => x.Id).ToList();

            var attendances = _unitOfWork.GenericRepository<Attendance>()
                .GetAll()
                .Where(x => x.ScheduleId == scheduleId &&
                            x.Date.Date == date.Date &&
                            enrollIds.Contains(x.EnrollId))
                .ToList();

            var result = enrolls
                .Where(x => students.ContainsKey(x.StudentId))
                .Select(enroll =>
                {
                    students.TryGetValue(enroll.StudentId, out var student);

                    var attendance = attendances
                        .FirstOrDefault(x => x.EnrollId == enroll.Id);

                    return new AttendanceStudentViewModel
                    {
                        EnrollId = enroll.Id,
                        StudentId = student!.Id,
                        StudentName = $"{student.LastName} {student.FirstName}",
                        Present = attendance?.Present,
                        Note = attendance?.Note
                    };
                })
                .ToList();

            return Task.FromResult(result);
        }
    }
}