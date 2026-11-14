
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        // ============================================================

        public PagedResult<StudentViewModel> GetAll(
            int pageNumber,
            int pageSize,
            string? search = null)
        {
            // 1. Lấy Student và Include Major
            var students = _unitOfWork
                .GenericRepository<Student>()
                .GetAll(
                filter: s=>!s.IsDeleted,
                    include: query => query.Include(s => s.Major)
                );

            // 2. Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                students = students.Where(s =>
                    s.FirstName.Contains(search) ||
                    s.LastName.Contains(search) ||
                    (s.FirstName + " " + s.LastName).Contains(search)
                );
            }

            // 3. Đếm tổng số Student
            var totalItems = students.Count();

            // 4. Phân trang
            var pagedStudents = students
                .OrderBy(s => s.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 5. Chuyển sang ViewModel
            var result = pagedStudents
                .Select(s => new StudentViewModel(s))
                .ToList();

            return new PagedResult<StudentViewModel>
            {
                Data = result,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // ============================================================
        // GET STUDENT BY ID
        // ============================================================

        public async Task<StudentViewModel?> GetById(int id)
        {
            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(
                    filter: s => s.Id == id&& !s.IsDeleted,
                    include: query => query.Include(s => s.Major)
                );

            if (student == null)
                return null;

            return new StudentViewModel(student);
        }

        // ============================================================
        // ADD STUDENT
        // ============================================================

        public async Task AddStudent(CreateStudentViewModel student)
        {
            if (student == null)
                throw new Exception("Thông tin Student không được để trống.");


            if (student.MajorId <= 0)
                throw new Exception("MajorId không hợp lệ.");

            if (string.IsNullOrWhiteSpace(student.Email))
                throw new Exception("Email không được để trống.");

            if (string.IsNullOrWhiteSpace(student.Password))
                throw new Exception("Password không được để trống.");

            // 1. KIỂM TRA MAJOR
            var major = _unitOfWork
                .GenericRepository<Major>()
                .GetById(student.MajorId);

            if (student.YearLevel < 1 ||
               student.YearLevel > major.StudyDurationYears)
            {
                throw new Exception(
                    $"YearLevel phải từ 1 đến {major.StudyDurationYears}.");
            }
            if (major == null)
                throw new Exception("Không tìm thấy Major.");

            // 2. KIỂM TRA EMAIL
            var existingUser = await _userManager
                .FindByEmailAsync(student.Email);

            if (existingUser != null)
                throw new Exception("Email đã tồn tại.");

            // 3. BẮT ĐẦU TRANSACTION
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 4. TẠO STUDENT
                var model = student.ConvertModel();
                model.Status = StudentStatus.Studying;
                model.IsDeleted = false;

                // Gán MajorId cho Student
                model.MajorId = student.MajorId;

                await _unitOfWork
                    .GenericRepository<Student>()
                    .AddAsync(model);

                await _unitOfWork.SaveAsync();

                if (model.Id <= 0)
                    throw new Exception("Không thể tạo Student.");

                // 5. TẠO APPLICATION USER
                var appUser = new ApplicationUser
                {
                    UserName = student.Email,
                    Email = student.Email,
                    FullName = $"{student.FirstName} {student.LastName}",
                    StudentId = model.Id
                };

                var userResult = await _userManager
                    .CreateAsync(appUser, student.Password);

                if (!userResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        userResult.Errors.Select(x => x.Description));

                    throw new Exception(
                        $"Tạo tài khoản thất bại: {errors}");
                }

                // 6. TẠO ROLE STUDENT NẾU CHƯA CÓ
                if (!await _roleManager.RoleExistsAsync("Student"))
                {
                    var roleResult = await _roleManager
                        .CreateAsync(new IdentityRole("Student"));

                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(
                            ", ",
                            roleResult.Errors.Select(x => x.Description));

                        throw new Exception(
                            $"Tạo role Student thất bại: {errors}");
                    }
                }

                // 7. GÁN ROLE CHO USER
                var roleAddResult = await _userManager
                    .AddToRoleAsync(appUser, "Student");

                if (!roleAddResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleAddResult.Errors.Select(x => x.Description));

                    throw new Exception(
                        $"Gán role Student thất bại: {errors}");
                }

                // 8. GÁN KEYID
                // KeyId lưu ApplicationUser.Id
                model.KeyId = appUser.Id;

                _unitOfWork
                    .GenericRepository<Student>()
                    .Update(model);

                await _unitOfWork.SaveAsync();

                // 9. COMMIT TRANSACTION
                await transaction.CommitAsync();
            }
            catch
            {
                // Có lỗi: hoàn tác tất cả thao tác trong transaction
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ============================================================
        // UPDATE STUDENT
        // Không cập nhật MajorId và YearLevel
        // ============================================================

        public async Task UpdateStudent(
            int id,
            CreateStudentViewModel student)
        {
            if (student == null)
                throw new Exception("Thông tin Student không được để trống.");

            var repository = _unitOfWork
                .GenericRepository<Student>();

            var existingStudent = repository.GetAll(filter: s => s.Id == id && !s.IsDeleted)
    .FirstOrDefault();

            if (existingStudent == null)
                throw new Exception("Không tìm thấy Student.");

            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. CẬP NHẬT THÔNG TIN STUDENT
                existingStudent.FirstName = student.FirstName;
                existingStudent.LastName = student.LastName;
                existingStudent.DOB = student.DOB;
                existingStudent.DateOfJoin = student.DateOfJoin;

                // Không cập nhật:
                // existingStudent.MajorId
                // existingStudent.YearLevel

                // 2. TÌM APPLICATION USER
                var appUser = await _userManager
                    .FindByIdAsync(existingStudent.KeyId);

                if (appUser != null)
                {
                    // 3. CẬP NHẬT FULL NAME
                    appUser.FullName =
                        $"{student.FirstName} {student.LastName}";

                    // 4. CẬP NHẬT EMAIL VÀ USERNAME
                    if (!string.IsNullOrWhiteSpace(student.Email) &&
                        appUser.Email != student.Email)
                    {
                        var emailExists = await _userManager
                            .FindByEmailAsync(student.Email);

                        if (emailExists != null &&
                            emailExists.Id != appUser.Id)
                        {
                            throw new Exception("Email đã tồn tại.");
                        }

                        var emailResult = await _userManager
                            .SetEmailAsync(appUser, student.Email);

                        if (!emailResult.Succeeded)
                        {
                            var errors = string.Join(
                                ", ",
                                emailResult.Errors.Select(x => x.Description));

                            throw new Exception(
                                $"Cập nhật Email thất bại: {errors}");
                        }

                        var userNameResult = await _userManager
                            .SetUserNameAsync(appUser, student.Email);

                        if (!userNameResult.Succeeded)
                        {
                            var errors = string.Join(
                                ", ",
                                userNameResult.Errors.Select(x => x.Description));

                            throw new Exception(
                                $"Cập nhật Username thất bại: {errors}");
                        }
                    }

                    // 5. CẬP NHẬT FULL NAME
                    var updateResult = await _userManager
                        .UpdateAsync(appUser);

                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(
                            ", ",
                            updateResult.Errors.Select(x => x.Description));

                        throw new Exception(
                            $"Cập nhật tài khoản thất bại: {errors}");
                    }
                }

                // 6. LƯU STUDENT
                repository.Update(existingStudent);

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ============================================================
        // DELETE STUDENT
        // ============================================================

        public async Task DeleteStudent(int id)
        {
            var repository = _unitOfWork
                .GenericRepository<Student>();

            // 1. Tìm Student chưa bị xóa
            var student = repository.GetById(id);

            if (student == null || student.IsDeleted)
                throw new Exception("Không tìm thấy Student.");

            // 2. Không cho xóa sinh viên đang bảo lưu
            if (student.Status == StudentStatus.OnLeave)
                throw new Exception(
                    "Không thể xóa sinh viên đang trong thời gian bảo lưu.");

            // 3. Kiểm tra Enroll
            var hasEnroll = _unitOfWork
                .GenericRepository<Enroll>()
                .GetAll()
                .Any(x => x.StudentId == id);

            if (hasEnroll)
                throw new Exception(
                    "Không thể xóa Student vì Student đã đăng ký môn học.");

            // 4. Kiểm tra Fee
            var hasFee = _unitOfWork
                .GenericRepository<Fee>()
                .GetAll()
                .Any(x => x.StudentId == id);

            if (hasFee)
                throw new Exception(
                    "Không thể xóa Student vì Student đã có học phí.");

            // 5. Soft Delete
            student.IsDeleted = true;

            repository.Update(student);

            await _unitOfWork.SaveAsync();
        }

        // ============================================================
        // COUNT STUDENTS
        // ============================================================

        public int GetAllStudents()
        {
            return _unitOfWork
                .GenericRepository<Student>()
                .GetAll(
                filter:s=>!s.IsDeleted)
                .Count();
        }
    }
}
