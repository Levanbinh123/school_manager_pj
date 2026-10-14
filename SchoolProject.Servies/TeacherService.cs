using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public class TeacherService : ITeacherService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TeacherService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // =========================================================
        // ADD TEACHER
        // =========================================================
        public async Task AddTeacher(CreateTeacherViewModel vm)
        {
            if (vm == null)
            {
                throw new Exception(
                    "Thông tin Teacher không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(vm.Email))
            {
                throw new Exception(
                    "Email không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                throw new Exception(
                    "Password không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(vm.FirstName))
            {
                throw new Exception(
                    "FirstName không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(vm.LastName))
            {
                throw new Exception(
                    "LastName không được để trống.");
            }

            // -----------------------------------------------------
            // Kiểm tra email ApplicationUser
            // -----------------------------------------------------
            var existingUser =
                await _userManager.FindByEmailAsync(
                    vm.Email.Trim());

            if (existingUser != null)
            {
                throw new Exception(
                    "Email này đã được sử dụng.");
            }

            // -----------------------------------------------------
            // Tạo ApplicationUser
            // -----------------------------------------------------
            var appUser = new ApplicationUser
            {
                UserName = vm.Email.Trim(),
                Email = vm.Email.Trim(),
                FullName =
                    $"{vm.FirstName.Trim()} {vm.LastName.Trim()}"
            };

            var result =
                await _userManager.CreateAsync(
                    appUser,
                    vm.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    result.Errors.Select(
                        x => x.Description));

                throw new Exception(errors);
            }

            // -----------------------------------------------------
            // Kiểm tra Role Teacher
            // -----------------------------------------------------
            if (!await _roleManager.RoleExistsAsync("Teacher"))
            {
                var roleResult =
                    await _roleManager.CreateAsync(
                        new IdentityRole("Teacher"));

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(
                            x => x.Description));

                    throw new Exception(errors);
                }
            }

            // -----------------------------------------------------
            // Gán Role Teacher
            // -----------------------------------------------------
            var roleResultAdd =
                await _userManager.AddToRoleAsync(
                    appUser,
                    "Teacher");

            if (!roleResultAdd.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    roleResultAdd.Errors.Select(
                        x => x.Description));

                throw new Exception(errors);
            }

            // -----------------------------------------------------
            // ApplicationUser.Id
            // → Teacher.KeyId
            // -----------------------------------------------------
            vm.KeyId = appUser.Id;

            var teacher =
                vm.ConvertModel(vm);

            // -----------------------------------------------------
            // Tạo Teacher
            // -----------------------------------------------------
            await _unitOfWork
                .GenericRepository<Teacher>()
                .AddAsync(teacher);

            _unitOfWork.Save();
        }

        // =========================================================
        // ADD TEACHERS TO SESSION
        // =========================================================
        public async Task<int> AddSessionWithTeacher(
            int sessionId,
            List<int> teacherIdsList)
        {
            if (teacherIdsList == null ||
                teacherIdsList.Count == 0)
            {
                throw new Exception(
                    "Danh sách Teacher không được để trống.");
            }

            // -----------------------------------------------------
            // Kiểm tra Session
            // -----------------------------------------------------
            var session =
                await _unitOfWork
                    .GenericRepository<Session>()
                    .GetByIdAsync(
                        x => x.Id == sessionId,
                        include: x =>
                            x.Include(y =>
                                y.TeacherSessions));

            if (session == null)
            {
                throw new Exception(
                    $"Không tìm thấy Session với Id = {sessionId}.");
            }

            int count = 0;

            // -----------------------------------------------------
            // Loại bỏ Teacher ID trùng
            // -----------------------------------------------------
            var teacherIds =
                teacherIdsList
                    .Distinct()
                    .ToList();

            foreach (var teacherId in teacherIds)
            {
                // -------------------------------------------------
                // Kiểm tra Teacher
                // -------------------------------------------------
                var teacher =
                    await _unitOfWork
                        .GenericRepository<Teacher>()
                        .GetByIdAsync(
                            x => x.Id == teacherId);

                if (teacher == null)
                {
                    throw new Exception(
                        $"Không tìm thấy Teacher với Id = {teacherId}.");
                }

                // -------------------------------------------------
                // Kiểm tra Teacher đã thuộc Session chưa
                // -------------------------------------------------
                var exists =
                    _unitOfWork
                        .GenericRepository<TeacherSession>()
                        .Exists(
                            x =>
                                x.SessionId == sessionId &&
                                x.TeacherId == teacherId);

                if (exists)
                {
                    continue;
                }

                // -------------------------------------------------
                // Tạo TeacherSession
                // -------------------------------------------------
                session.TeacherSessions.Add(
                    new TeacherSession
                    {
                        SessionId = sessionId,
                        TeacherId = teacherId
                    });

                count++;
            }

            // -----------------------------------------------------
            // Không có Teacher mới
            // -----------------------------------------------------
            if (count == 0)
            {
                return 0;
            }

            // -----------------------------------------------------
            // Update Session
            // -----------------------------------------------------
            _unitOfWork
                .GenericRepository<Session>()
                .Update(session);

            _unitOfWork.Save();

            return count;
        }

        // =========================================================
        // GET ALL TEACHER - PAGINATION
        // =========================================================
        public PagedResult<TeacherViewModel> GetAllTeacher(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            int excludeRecords =
                (pageNumber - 1) * pageSize;

            var repository =
                _unitOfWork
                    .GenericRepository<Teacher>();

            var modelList =
                repository
                    .GetAll()
                    .OrderBy(x => x.Id)
                    .Skip(excludeRecords)
                    .Take(pageSize)
                    .ToList();

            int totalCount =
                repository
                    .GetAll()
                    .Count();

            var vmList =
                ConvertModelToViewModel(
                    modelList);

            return new PagedResult<TeacherViewModel>
            {
                Data = vmList,
                TotalItems = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // =========================================================
        // GET TEACHER COUNT
        // =========================================================
        public int GetAllTeacher()
        {
            return _unitOfWork
                .GenericRepository<Teacher>()
                .GetAll()
                .Count();
        }

        // =========================================================
        // CONVERT MODEL -> VIEW MODEL
        // =========================================================
        private List<TeacherViewModel>
            ConvertModelToViewModel(
                List<Teacher> modelList)
        {
            return modelList
                .Select(
                    x => new TeacherViewModel(x))
                .ToList();
        }
    }
}