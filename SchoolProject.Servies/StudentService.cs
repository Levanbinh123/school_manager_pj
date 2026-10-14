using Microsoft.AspNetCore.Identity;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================================================
        // GET ALL STUDENTS
        // PAGINATION + SEARCH
        // ============================================================

        public PagedResult<StudentViewModel> GetAll(
            int pageNumber,
            int pageSize,
            string? search = null)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            var students =
                _unitOfWork
                    .GenericRepository<Student>()
                    .GetAll();

            // --------------------------------------------------------
            // SEARCH
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                students = students.Where(x =>
                    x.FirstName.Contains(search) ||
                    x.LastName.Contains(search) ||
                    (
                        x.FirstName + " " + x.LastName
                    ).Contains(search) ||
                    x.KeyId.Contains(search)
                );
            }

            // --------------------------------------------------------
            // TOTAL
            // --------------------------------------------------------

            int totalCount =
                students.Count();

            // --------------------------------------------------------
            // PAGINATION
            // --------------------------------------------------------

            int skip =
                (pageNumber - 1) * pageSize;

            var studentList =
                students
                    .OrderBy(x => x.Id)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

            // --------------------------------------------------------
            // CONVERT
            // --------------------------------------------------------

            var vmList =
                studentList
                    .Select(x =>
                        new StudentViewModel(x))
                    .ToList();

            return new PagedResult<StudentViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // ============================================================
        // GET STUDENT BY ID
        // ============================================================

        public StudentViewModel? GetById(
            int id)
        {
            var student =
                _unitOfWork
                    .GenericRepository<Student>()
                    .GetById(id);

            if (student == null)
            {
                return null;
            }

            return new StudentViewModel(student);
        }

        // ============================================================
        // ADD STUDENT
        // ============================================================
        // Admin tạo Student:
        //
        // - FirstName
        // - LastName
        // - DOB
        // - DateOfJoin
        // - Email
        // - Password
        // - MajorId
        // - YearLevel
        //
        // MajorId và YearLevel được Admin thiết lập ở đây.
        // ============================================================

        public async Task AddStudent(
            CreateStudentViewModel student)
        {
            if (student == null)
                throw new Exception("Thông tin Student không được để trống.");

            // ============================================================
            // 1. CHECK YEAR LEVEL
            // ============================================================

            if (student.YearLevel < 1 || student.YearLevel > 4)
                throw new Exception("YearLevel phải từ 1 đến 4.");

            // ============================================================
            // 2. CHECK MAJOR
            // ============================================================

            var major = _unitOfWork
                .GenericRepository<Major>()
                .GetById(student.MajorId);

            if (major == null)
                throw new Exception("Không tìm thấy Major.");

            // ============================================================
            // 3. CHECK EMAIL
            // ============================================================

            var existingUser = await _userManager
                .FindByEmailAsync(student.Email);

            if (existingUser != null)
                throw new Exception("Email đã tồn tại.");

            // ============================================================
            // 4. TẠO STUDENT TRƯỚC
            // ============================================================

            var model = student.ConvertModel(student);

            await _unitOfWork
                .GenericRepository<Student>()
                .AddAsync(model);

            _unitOfWork.Save();

            // ============================================================
            // 5. KIỂM TRA STUDENT ĐÃ CÓ ID
            // ============================================================

            if (model.Id <= 0)
                throw new Exception("Không thể tạo Student.");

            // ============================================================
            // 6. TẠO APPLICATION USER
            // ============================================================

            var appUser = new ApplicationUser
            {
                UserName = student.Email,
                Email = student.Email,
                FullName = $"{student.FirstName} {student.LastName}",
                StudentId = model.Id
            };

            var result = await _userManager
                .CreateAsync(appUser, student.Password);

            if (!result.Succeeded)
            {
                // Xóa Student nếu tạo User thất bại
                _unitOfWork
                    .GenericRepository<Student>()
                    .Delete(model);

                _unitOfWork.Save();

                var errors = string.Join(
                    ", ",
                    result.Errors.Select(x => x.Description));

                throw new Exception(errors);
            }

            // ============================================================
            // 7. TẠO ROLE STUDENT NẾU CHƯA CÓ
            // ============================================================

            if (!await _roleManager.RoleExistsAsync("Student"))
            {
                var roleResult = await _roleManager
                    .CreateAsync(new IdentityRole("Student"));

                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(appUser);

                    _unitOfWork
                        .GenericRepository<Student>()
                        .Delete(model);

                    _unitOfWork.Save();

                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(x => x.Description));

                    throw new Exception(errors);
                }
            }

            // ============================================================
            // 8. ADD ROLE STUDENT
            // ============================================================

            var roleAddResult = await _userManager
                .AddToRoleAsync(appUser, "Student");

            if (!roleAddResult.Succeeded)
            {
                await _userManager.DeleteAsync(appUser);

                _unitOfWork
                    .GenericRepository<Student>()
                    .Delete(model);

                _unitOfWork.Save();

                var errors = string.Join(
                    ", ",
                    roleAddResult.Errors.Select(x => x.Description));

                throw new Exception(errors);
            }

            // ============================================================
            // 9. LƯU STUDENT
            // ============================================================

            // KeyId lưu ApplicationUser.Id
            model.KeyId = appUser.Id;

            _unitOfWork
                .GenericRepository<Student>()
                .Update(model);

            _unitOfWork.Save();
        }

        // ============================================================
        // UPDATE STUDENT
        // ============================================================
        //
        // KHÔNG UPDATE:
        // - MajorId
        // - YearLevel
        //
        // Hai trường này chỉ do Admin quản lý.
        //
        // Hàm này chỉ cập nhật:
        // - FirstName
        // - LastName
        // - DOB
        // - DateOfJoin
        // - Email
        // ============================================================

        public async Task UpdateStudent(
            int id,
            CreateStudentViewModel student)
        {
            if (student == null)
            {
                throw new Exception(
                    "Thông tin Student không được để trống.");
            }

            var repository =
                _unitOfWork
                    .GenericRepository<Student>();

            // --------------------------------------------------------
            // FIND STUDENT
            // --------------------------------------------------------

            var existingStudent =
                repository.GetById(id);

            if (existingStudent == null)
            {
                throw new Exception(
                    "Không tìm thấy Student.");
            }

            // --------------------------------------------------------
            // UPDATE PERSONAL INFORMATION
            // --------------------------------------------------------

            existingStudent.FirstName =
                student.FirstName;

            existingStudent.LastName =
                student.LastName;

            existingStudent.DOB =
                student.DOB;

            existingStudent.DateOfJoin =
                student.DateOfJoin;

            // --------------------------------------------------------
            // KHÔNG ĐƯỢC UPDATE:
            //
            // existingStudent.MajorId
            // existingStudent.YearLevel
            //
            // Major và YearLevel do Admin quản lý.
            // --------------------------------------------------------

            // --------------------------------------------------------
            // FIND APPLICATION USER
            // --------------------------------------------------------

            var appUser =
                await _userManager
                    .FindByIdAsync(
                        existingStudent.KeyId);

            if (appUser != null)
            {
                bool userChanged = false;

                // ----------------------------------------------------
                // UPDATE FULL NAME
                // ----------------------------------------------------

                var fullName =
                    $"{student.FirstName} {student.LastName}";

                if (appUser.FullName != fullName)
                {
                    appUser.FullName =
                        fullName;

                    userChanged = true;
                }

                // ----------------------------------------------------
                // UPDATE EMAIL
                // ----------------------------------------------------

                if (!string.IsNullOrWhiteSpace(
                    student.Email) &&
                    appUser.Email != student.Email)
                {
                    var emailExists =
                        await _userManager
                            .FindByEmailAsync(
                                student.Email);

                    if (emailExists != null &&
                        emailExists.Id != appUser.Id)
                    {
                        throw new Exception(
                            "Email đã tồn tại.");
                    }

                    appUser.Email =
                        student.Email;

                    appUser.UserName =
                        student.Email;

                    userChanged = true;
                }

                // ----------------------------------------------------
                // SAVE APPLICATION USER
                // ----------------------------------------------------

                if (userChanged)
                {
                    var result =
                        await _userManager
                            .UpdateAsync(appUser);

                    if (!result.Succeeded)
                    {
                        var errors =
                            string.Join(
                                ", ",
                                result.Errors.Select(
                                    x => x.Description));

                        throw new Exception(errors);
                    }
                }
            }

            // --------------------------------------------------------
            // SAVE STUDENT
            // --------------------------------------------------------

            repository.Update(
                existingStudent);

            _unitOfWork.Save();
        }

        // ============================================================
        // DELETE STUDENT
        // ============================================================

        public async Task DeleteStudent(
            int id)
        {
            var repository =
                _unitOfWork
                    .GenericRepository<Student>();

            // --------------------------------------------------------
            // FIND STUDENT
            // --------------------------------------------------------

            var student =
                repository.GetById(id);

            if (student == null)
            {
                throw new Exception(
                    "Không tìm thấy Student.");
            }

            // --------------------------------------------------------
            // CHECK ENROLL
            // --------------------------------------------------------

            var hasEnroll =
                _unitOfWork
                    .GenericRepository<Enroll>()
                    .GetAll()
                    .Any(x =>
                        x.StudentId == id);

            if (hasEnroll)
            {
                throw new Exception(
                    "Không thể xóa Student vì Student đã đăng ký môn học.");
            }

            // --------------------------------------------------------
            // CHECK FEE
            // --------------------------------------------------------

            var hasFee =
                _unitOfWork
                    .GenericRepository<Fee>()
                    .GetAll()
                    .Any(x =>
                        x.StudentId == id);

            if (hasFee)
            {
                throw new Exception(
                    "Không thể xóa Student vì Student đã có học phí.");
            }

            // --------------------------------------------------------
            // GET APPLICATION USER
            // --------------------------------------------------------

            ApplicationUser? appUser = null;

            if (!string.IsNullOrWhiteSpace(
                student.KeyId))
            {
                appUser =
                    await _userManager
                        .FindByIdAsync(
                            student.KeyId);
            }

            // --------------------------------------------------------
            // DELETE STUDENT
            // --------------------------------------------------------

            repository.Delete(student);

            _unitOfWork.Save();

            // --------------------------------------------------------
            // DELETE APPLICATION USER
            // --------------------------------------------------------

            if (appUser != null)
            {
                var result =
                    await _userManager
                        .DeleteAsync(appUser);

                if (!result.Succeeded)
                {
                    var errors =
                        string.Join(
                            ", ",
                            result.Errors.Select(
                                x => x.Description));

                    throw new Exception(errors);
                }
            }
        }

        // ============================================================
        // COUNT STUDENTS
        // ============================================================

        public int GetAllStudents()
        {
            return _unitOfWork
                .GenericRepository<Student>()
                .GetAll()
                .Count();
        }
    }
}