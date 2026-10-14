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
        // CREATE CLASS
        // =====================================================
        public async Task AddClass(CreateClassViewModel vm)
        {
            // =================================================
            // 1. CHECK DUPLICATE CLASS
            // =================================================
            var exists = _unitOfWork
                .GenericRepository<Class>()
                .Exists(x => x.Name == vm.Name);

            if (exists)
            {
                throw new Exception(
                    "Class already exists.");
            }


            // =================================================
            // 2. CREATE CLASS
            // =================================================
            var model = new Class
            {
                Name = vm.Name
            };


            // =================================================
            // 3. ADD DATABASE
            // =================================================
            await _unitOfWork
                .GenericRepository<Class>()
                .AddAsync(model);

            _unitOfWork.Save();
        }


        // =====================================================
        // UPDATE CLASS
        // =====================================================
        public async Task UpdateClass(
            int id,
            CreateClassViewModel vm)
        {
            // =================================================
            // 1. CHECK DUPLICATE CLASS
            // =================================================
            var exists = _unitOfWork
                .GenericRepository<Class>()
                .Exists(x =>
                    x.Id != id &&
                    x.Name == vm.Name);

            if (exists)
            {
                throw new Exception(
                    "Class already exists.");
            }


            // =================================================
            // 2. GET CLASS
            // =================================================
            var model = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == id);

            if (model == null)
            {
                throw new Exception(
                    "Class not found");
            }


            // =================================================
            // 3. UPDATE
            // =================================================
            model.Name = vm.Name;


            // =================================================
            // 4. SAVE
            // =================================================
            _unitOfWork
                .GenericRepository<Class>()
                .Update(model);

            _unitOfWork.Save();
        }


        // =====================================================
        // DELETE CLASS
        // =====================================================
        public async Task DeleteClass(int id)
        {
            // =================================================
            // 1. GET CLASS
            // =================================================
            var model = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == id);

            if (model == null)
            {
                throw new Exception(
                    "Class not found");
            }


            // =================================================
            // 2. DELETE
            // =================================================
            _unitOfWork
                .GenericRepository<Class>()
                .Delete(model);

            _unitOfWork.Save();
        }


        // =====================================================
        // GET CLASS BY ID
        // =====================================================
        public async Task<ClassViewModel?> GetClassById(int id)
        {
            // =================================================
            // GET CLASS
            // =================================================
            var model = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == id);

            if (model == null)
            {
                return null;
            }


            // =================================================
            // RETURN VIEW MODEL
            // =================================================
            return new ClassViewModel
            {
                Id = model.Id,
                Name = model.Name
            };
        }


        // =====================================================
        // GET ALL CLASS
        // =====================================================
        public async Task<List<ClassViewModel>> GetAllClass()
        {
            // =================================================
            // GET ALL CLASS
            // =================================================
            var modelList = _unitOfWork
                .GenericRepository<Class>()
                .GetAll()
                .ToList();


            // =================================================
            // RETURN VIEW MODEL
            // =================================================
            return modelList
                .Select(x => new ClassViewModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToList();
        }


        // =====================================================
        // CREATE CLASS SESSION
        //
        // ClassSession =
        // Class + Subject + Session + Teacher
        //
        // Teacher có thể NULL
        // =====================================================
        public async Task<ClassSessionViewModel> AddClassSession(
            CreateClassSessionViewModel vm)
        {
            // =================================================
            // 1. CHECK CLASS
            // =================================================
            var classEntity = await _unitOfWork
                .GenericRepository<Class>()
                .GetByIdAsync(x => x.Id == vm.ClassId);

            if (classEntity == null)
            {
                throw new Exception(
                    "Không tìm thấy lớp.");
            }


            // =================================================
            // 2. CHECK SUBJECT
            // =================================================
            var subject = await _unitOfWork
                .GenericRepository<Subject>()
                .GetByIdAsync(x => x.Id == vm.SubjectId);

            if (subject == null)
            {
                throw new Exception(
                    "Không tìm thấy môn học.");
            }


            // =================================================
            // 3. CHECK SESSION
            // =================================================
            var session = await _unitOfWork
                .GenericRepository<Session>()
                .GetByIdAsync(x => x.Id == vm.SessionId);

            if (session == null)
            {
                throw new Exception(
                    "Không tìm thấy học kỳ.");
            }


            // =================================================
            // 4. CHECK TEACHER
            //
            // TeacherId có thể NULL
            // =================================================
            if (vm.HomeroomTeacherId.HasValue)
            {
                var teacher = await _unitOfWork
                    .GenericRepository<Teacher>()
                    .GetByIdAsync(
                        x => x.Id == vm.HomeroomTeacherId.Value);

                if (teacher == null)
                {
                    throw new Exception(
                        "Không tìm thấy giáo viên.");
                }
            }


            // =================================================
            // 5. CHECK DUPLICATE
            //
            // Một Class + Subject + Session
            // chỉ có một ClassSession
            // =================================================
            var exists = _unitOfWork
                .GenericRepository<ClassSession>()
                .Exists(x =>
                    x.ClassId == vm.ClassId &&
                    x.SubjectId == vm.SubjectId &&
                    x.SessionId == vm.SessionId);

            if (exists)
            {
                throw new Exception(
                    "Lớp này đã có ClassSession cho môn học và session này.");
            }


            // =================================================
            // 6. CREATE CLASS SESSION
            // =================================================
            var classSession = new ClassSession
            {
                ClassId = vm.ClassId,
                SubjectId = vm.SubjectId,
                SessionId = vm.SessionId,
                HomeroomTeacherId = vm.HomeroomTeacherId,
                HasAttendance = vm.HasAttendance,
                TotalSessions = vm.TotalSessions
            };


            // =================================================
            // 7. ADD DATABASE
            // =================================================
            _unitOfWork
                .GenericRepository<ClassSession>()
                .Add(classSession);

            _unitOfWork.Save();


            // =================================================
            // 8. LOAD AGAIN
            // =================================================
            var result = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(
                    x => x.Id == classSession.Id,
                    include: query => query
                        .Include(x => x.Class)
                        .Include(x => x.Subject)
                        .Include(x => x.Session)
                        .Include(x => x.HomeroomTeacher)
                );

            if (result == null)
            {
                throw new Exception(
                    "Không thể lấy ClassSession vừa tạo.");
            }


            // =================================================
            // 9. RETURN VIEW MODEL
            // =================================================
            return new ClassSessionViewModel(result);
        }


        // =====================================================
        // GET CLASS SESSIONS
        //
        // Lấy tất cả ClassSession của một Class
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
                )
                .Where(x => x.ClassId == classId)
                .ToList();


            // =================================================
            // RETURN VIEW MODEL
            // =================================================
            return list
                .Select(x => new ClassSessionViewModel
                {
                    Id = x.Id,

                    ClassId = x.ClassId,
                    ClassName = x.Class?.Name,

                    SubjectId = x.SubjectId,
                    SubjectName = x.Subject?.Name,

                    SessionId = x.SessionId,

                    HomeroomTeacherId = x.HomeroomTeacherId
                ,
                    HasAttendance = x.HasAttendance
                })
                .ToList();
        }


        // =====================================================
        // ADD STUDENTS TO CLASS SESSION
        //
        // Flow:
        // Student đăng ký Subject
        //        ↓
        // Enroll
        //        ↓
        // Admin có thể xếp vào ClassSession
        // =====================================================
        public async Task<int> AddStudentsToClassSession(
            int classSessionId,
            List<int> studentList)
        {
            // =================================================
            // 1. CHECK CLASS SESSION
            // =================================================
            var classSession = await _unitOfWork
                .GenericRepository<ClassSession>()
                .GetByIdAsync(
                    x => x.Id == classSessionId);

            if (classSession == null)
            {
                throw new Exception(
                    "Không tìm thấy ClassSession.");
            }


            int count = 0;


            // =================================================
            // 2. LOOP STUDENTS
            // =================================================
            foreach (var studentId in studentList)
            {
                // =============================================
                // CHECK STUDENT
                // =============================================
                var student = await _unitOfWork
                    .GenericRepository<Student>()
                    .GetByIdAsync(
                        x => x.Id == studentId);

                if (student == null)
                {
                    throw new Exception(
                        $"Không tìm thấy sinh viên ID = {studentId}.");
                }


                // =============================================
                // FIND EXISTING ENROLL
                //
                // Student phải đăng ký đúng Subject + Session
                // =============================================
                var enroll = await _unitOfWork
                    .GenericRepository<Enroll>()
                    .GetByIdAsync(
                        x =>
                            x.StudentId == studentId &&
                            x.SubjectId == classSession.SubjectId &&
                            x.SessionId == classSession.SessionId
                    );

                if (enroll == null)
                {
                    throw new Exception(
                        $"Sinh viên ID = {studentId} chưa đăng ký môn này.");
                }


                // =============================================
                // ALREADY IN THIS CLASS
                // =============================================
                if (enroll.ClassSessionId == classSessionId)
                {
                    continue;
                }


                // =============================================
                // UPDATE CLASS SESSION
                // =============================================
                enroll.ClassSessionId = classSessionId;

                _unitOfWork
                    .GenericRepository<Enroll>()
                    .Update(enroll);

                count++;
            }


            // =================================================
            // 3. SAVE
            // =================================================
            _unitOfWork.Save();


            return count;
        }
    }
}