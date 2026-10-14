using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =====================================================
        // POST ATTENDANCE
        // =====================================================
        public async Task AddAttendance(
            string userId,
            CreateAttendanceViewModel vm)
        {
            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var attendanceRepository =
                _unitOfWork.GenericRepository<Attendance>();

            var classSessionRepository =
                _unitOfWork.GenericRepository<ClassSession>();

            var teacherRepository =
                _unitOfWork.GenericRepository<Teacher>();

            // =================================================
            // 1. TÌM TEACHER
            // =================================================

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

            // =================================================
            // 2. KIỂM TRA ENROLL
            // =================================================

            var enroll =
                enrollRepository
                    .GetById(vm.EnrollId);

            if (enroll == null)
            {
                throw new Exception(
                    "Không tìm thấy Enroll.");
            }

            // =================================================
            // 3. KIỂM TRA SUBJECT
            // =================================================

            if (!vm.SubjectId.HasValue)
            {
                throw new Exception(
                    "SubjectId không được để trống.");
            }

            if (vm.SubjectId.Value != enroll.SubjectId)
            {
                throw new Exception(
                    "Môn học không khớp với đăng ký của sinh viên.");
            }

            // =================================================
            // 4. ENROLL PHẢI CÓ CLASS SESSION
            // =================================================

            if (enroll.ClassSessionId == null)
            {
                throw new Exception(
                    "Sinh viên chưa được xếp vào ClassSession.");
            }

            // =================================================
            // 5. LẤY CLASS SESSION
            // =================================================

            var classSession =
                classSessionRepository
                    .GetById(
                        enroll.ClassSessionId.Value);

            if (classSession == null)
            {
                throw new Exception(
                    "Không tìm thấy ClassSession.");
            }

            // =================================================
            // 6. KIỂM TRA SUBJECT CỦA CLASS SESSION
            // =================================================

            if (classSession.SubjectId != enroll.SubjectId)
            {
                throw new Exception(
                    "Môn học của ClassSession không khớp với Enroll.");
            }

            // =================================================
            // 7. KIỂM TRA CÓ SỬ DỤNG ĐIỂM DANH KHÔNG
            // =================================================

            if (!classSession.HasAttendance)
            {
                throw new Exception(
                    "Môn học này không sử dụng điểm danh.");
            }

            // =================================================
            // 8. KIỂM TRA TEACHER
            // =================================================

            if (classSession.HomeroomTeacherId != teacher.Id)
            {
                throw new Exception(
                    "Teacher không phụ trách lớp học này.");
            }

            // =================================================
            // 9. KIỂM TRA ĐÃ ĐIỂM DANH CHƯA
            // =================================================

            var exists =
                attendanceRepository
                    .GetAll()
                    .Any(x =>
                        x.EnrollId == vm.EnrollId &&
                        x.SubjectId == vm.SubjectId.Value &&
                        x.Date.Date == vm.Date.Date);

            if (exists)
            {
                throw new Exception(
                    "Student đã được điểm danh trong ngày này.");
            }

            // =================================================
            // 10. TẠO ATTENDANCE
            // =================================================

            var attendance = new Attendance
            {
                EnrollId = vm.EnrollId,
                SubjectId = vm.SubjectId.Value,
                Date = vm.Date,
                Present = vm.Present,
                Note = vm.Note
            };

            await attendanceRepository
                .AddAsync(attendance);

            _unitOfWork.Save();
        }


        // =====================================================
        // GET BY CLASS SESSION
        // =====================================================
        public async Task<List<AttendanceViewModel>>
            GetByClassSession(
                string userId,
                int classSessionId)
        {
            var teacherRepository =
                _unitOfWork.GenericRepository<Teacher>();

            var classSessionRepository =
                _unitOfWork.GenericRepository<ClassSession>();

            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var attendanceRepository =
                _unitOfWork.GenericRepository<Attendance>();

            // =================================================
            // 1. TÌM TEACHER
            // =================================================

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

            // =================================================
            // 2. TÌM CLASS SESSION
            // =================================================

            var classSession =
                classSessionRepository
                    .GetById(classSessionId);

            if (classSession == null)
            {
                throw new Exception(
                    "Không tìm thấy ClassSession.");
            }

            // =================================================
            // 3. KIỂM TRA CÓ SỬ DỤNG ĐIỂM DANH KHÔNG
            // =================================================

            if (!classSession.HasAttendance)
            {
                throw new Exception(
                    "Môn học này không sử dụng điểm danh.");
            }

            // =================================================
            // 4. KIỂM TRA TEACHER
            // =================================================

            if (classSession.HomeroomTeacherId != teacher.Id)
            {
                throw new Exception(
                    "Teacher không phụ trách lớp học này.");
            }

            // =================================================
            // 5. LẤY ENROLL
            // =================================================

            var enrolls =
                enrollRepository
                    .GetAll()
                    .Where(x =>
                        x.ClassSessionId ==
                        classSessionId)
                    .ToList();

            var enrollIds =
                enrolls
                    .Select(x => x.Id)
                    .ToList();

            // =================================================
            // 6. LẤY ATTENDANCE
            // =================================================

            var attendances =
                attendanceRepository
                    .GetAll()
                    .Where(x =>
                        enrollIds.Contains(x.EnrollId))
                    .ToList();

            return attendances
                .Select(x =>
                    new AttendanceViewModel(x))
                .ToList();
        }


        // =====================================================
        // GET STUDENT ATTENDANCE
        // =====================================================
        public async Task<List<AttendanceViewModel>>
            GetStudentAttendance(
                string userId)
        {
            var studentRepository =
                _unitOfWork.GenericRepository<Student>();

            var enrollRepository =
                _unitOfWork.GenericRepository<Enroll>();

            var attendanceRepository =
                _unitOfWork.GenericRepository<Attendance>();

            // =================================================
            // 1. TÌM STUDENT
            // =================================================

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

            // =================================================
            // 2. LẤY ENROLL CỦA STUDENT
            // =================================================

            var enrollIds =
                enrollRepository
                    .GetAll()
                    .Where(x =>
                        x.StudentId == student.Id)
                    .Select(x => x.Id)
                    .ToList();

            // =================================================
            // 3. LẤY ATTENDANCE
            // =================================================

            var attendances =
                attendanceRepository
                    .GetAll()
                    .Where(x =>
                        enrollIds.Contains(x.EnrollId))
                    .ToList();

            return attendances
                .Select(x =>
                    new AttendanceViewModel(x))
                .ToList();
        }


        // =====================================================
        // PUT ATTENDANCE
        // =====================================================
        public async Task UpdateAttendance(
            string userId,
            int id,
            CreateAttendanceViewModel vm)
        {
            var attendanceRepository =
                _unitOfWork
                    .GenericRepository<Attendance>();

            var enrollRepository =
                _unitOfWork
                    .GenericRepository<Enroll>();

            var classSessionRepository =
                _unitOfWork
                    .GenericRepository<ClassSession>();

            var teacherRepository =
                _unitOfWork
                    .GenericRepository<Teacher>();

            // =================================================
            // 1. TÌM TEACHER
            // =================================================

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

            // =================================================
            // 2. TÌM ATTENDANCE
            // =================================================

            var attendance =
                attendanceRepository
                    .GetById(id);

            if (attendance == null)
            {
                throw new Exception(
                    "Không tìm thấy Attendance.");
            }

            // =================================================
            // 3. TÌM ENROLL
            // =================================================

            var enroll =
                enrollRepository
                    .GetById(
                        attendance.EnrollId);

            if (enroll == null)
            {
                throw new Exception(
                    "Không tìm thấy Enroll.");
            }

            // =================================================
            // 4. KIỂM TRA ENROLL CÓ CLASS SESSION
            // =================================================

            if (enroll.ClassSessionId == null)
            {
                throw new Exception(
                    "Enroll chưa có ClassSession.");
            }

            // =================================================
            // 5. TÌM CLASS SESSION
            // =================================================

            var classSession =
                classSessionRepository
                    .GetById(
                        enroll.ClassSessionId.Value);

            if (classSession == null)
            {
                throw new Exception(
                    "Không tìm thấy ClassSession.");
            }

            // =================================================
            // 6. KIỂM TRA CÓ SỬ DỤNG ĐIỂM DANH KHÔNG
            // =================================================

            if (!classSession.HasAttendance)
            {
                throw new Exception(
                    "Môn học này không sử dụng điểm danh.");
            }

            // =================================================
            // 7. KIỂM TRA TEACHER
            // =================================================

            if (classSession.HomeroomTeacherId != teacher.Id)
            {
                throw new Exception(
                    "Teacher không phụ trách lớp học này.");
            }

            // =================================================
            // 8. KIỂM TRA SUBJECT
            // =================================================

            if (!vm.SubjectId.HasValue)
            {
                throw new Exception(
                    "SubjectId không được để trống.");
            }

            if (vm.SubjectId.Value != enroll.SubjectId)
            {
                throw new Exception(
                    "Môn học không khớp với đăng ký của sinh viên.");
            }

            if (classSession.SubjectId != enroll.SubjectId)
            {
                throw new Exception(
                    "Môn học của ClassSession không khớp với Enroll.");
            }

            // =================================================
            // 9. KIỂM TRA TRÙNG ATTENDANCE
            // =================================================

            var exists =
                attendanceRepository
                    .GetAll()
                    .Any(x =>
                        x.Id != id &&
                        x.EnrollId == attendance.EnrollId &&
                        x.SubjectId == attendance.SubjectId &&
                        x.Date.Date == vm.Date.Date);

            if (exists)
            {
                throw new Exception(
                    "Student đã có điểm danh trong ngày này.");
            }

            // =================================================
            // 10. CẬP NHẬT
            // =================================================

            // Không đổi EnrollId
            attendance.EnrollId =
                enroll.Id;

            // Không đổi SubjectId
            attendance.SubjectId =
                enroll.SubjectId;

            attendance.Date =
                vm.Date;

            attendance.Present =
                vm.Present;

            attendance.Note =
                vm.Note;

            attendanceRepository.Update(
                attendance);

            _unitOfWork.Save();
        }
    }
}