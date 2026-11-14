using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using SchoolProject.Repositories;
using Microsoft.EntityFrameworkCore;

namespace SchoolProject.ViewModels
{
    public class StudentLeaveService : IStudentLeaveService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentLeaveService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // =================================================
        // CREATE LEAVE REQUEST
        // =================================================
        public async Task<StudentLeaveViewModel> CreateLeave(
            int studentId,
            CreateStudentLeaveViewModel model)
        {
            if (model.ExpectedReturnDate.Date <= model.StartDate.Date)
            {
                throw new Exception(
                    "Ngày dự kiến quay lại phải sau ngày bắt đầu bảo lưu.");
            }

            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(
                    filter: s => s.Id == studentId && !s.IsDeleted
                );

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên.");

            if (student.Status != StudentStatus.Studying)
            {
                throw new Exception(
                    "Chỉ sinh viên đang học mới được đăng ký bảo lưu.");
            }

            var existingLeave = await _unitOfWork
                .GenericRepository<StudentLeave>()
                .GetByIdAsync(
                    filter: x =>
                        x.StudentId == studentId &&
                        (x.Status == StudentLeaveStatus.Pending ||
                         x.Status == StudentLeaveStatus.Approved)
                );

            if (existingLeave != null)
            {
                throw new Exception(
                    "Sinh viên đã có đơn bảo lưu đang chờ duyệt hoặc đang được bảo lưu.");
            }

            var leave = new StudentLeave
            {
                StudentId = studentId,
                StartDate = model.StartDate,
                ExpectedReturnDate = model.ExpectedReturnDate,
                Reason = model.Reason.Trim(),
                Status = StudentLeaveStatus.Pending,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork
                .GenericRepository<StudentLeave>()
                .AddAsync(leave);

            await _unitOfWork.SaveAsync();

            return await GetById(leave.Id)
                ?? throw new Exception("Không thể lấy thông tin đơn bảo lưu.");
        }

        // =================================================
        // GET ALL
        // =================================================
        public async Task<List<StudentLeaveViewModel>> GetAll()
        {
            var leaves = await _unitOfWork
                .GenericRepository<StudentLeave>()
                .GetAllAsync(
                    include: query => query.Include(x => x.Student)
                );

            return leaves
                .OrderByDescending(x => x.CreatedAt)
                .Select(MapToViewModel)
                .ToList();
        }

        // =================================================
        // GET BY ID
        // =================================================
        public async Task<StudentLeaveViewModel?> GetById(int id)
        {
            var leave = await _unitOfWork
                .GenericRepository<StudentLeave>()
                .GetByIdAsync(
                    filter: x => x.Id == id,
                    include: query => query.Include(x => x.Student)
                );

            if (leave == null)
                return null;

            return MapToViewModel(leave);
        }

        // =================================================
        // APPROVE
        // =================================================
        public async Task Approve(
            int id,
            string? approvedBy,
            StudentLeaveDecisionViewModel model)
        {
            var leave = await _unitOfWork
                .GenericRepository<StudentLeave>()
                .GetByIdAsync(
                    filter: x => x.Id == id,
                    disabledTracking: false
                );

            if (leave == null)
                throw new Exception("Không tìm thấy đơn bảo lưu.");

            if (leave.Status != StudentLeaveStatus.Pending)
            {
                throw new Exception(
                    "Chỉ đơn đang chờ duyệt mới được phê duyệt.");
            }

            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(
                    filter: s => s.Id == leave.StudentId && !s.IsDeleted,
                    disabledTracking: false
                );

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên.");

            if (student.Status != StudentStatus.Studying)
            {
                throw new Exception(
                    "Sinh viên không ở trạng thái đang học.");
            }

            leave.Status = StudentLeaveStatus.Approved;
            leave.ApprovedBy = approvedBy;
            leave.ApprovedAt = DateTime.Now;
            leave.AdminNote = model.AdminNote;

            student.Status = StudentStatus.OnLeave;

            _unitOfWork.GenericRepository<StudentLeave>()
                .Update(leave);

            _unitOfWork.GenericRepository<Student>()
                .Update(student);

            await _unitOfWork.SaveAsync();
        }

        // =================================================
        // REJECT
        // =================================================
        public async Task Reject(
            int id,
            StudentLeaveDecisionViewModel model)
        {
            var leave = await _unitOfWork
                .GenericRepository<StudentLeave>()
                .GetByIdAsync(
                    filter: x => x.Id == id,
                    disabledTracking: false
                );

            if (leave == null)
                throw new Exception("Không tìm thấy đơn bảo lưu.");

            if (leave.Status != StudentLeaveStatus.Pending)
            {
                throw new Exception(
                    "Chỉ đơn đang chờ duyệt mới được từ chối.");
            }

            leave.Status = StudentLeaveStatus.Rejected;
            leave.AdminNote = model.AdminNote;

            _unitOfWork.GenericRepository<StudentLeave>()
                .Update(leave);

            await _unitOfWork.SaveAsync();
        }

        // =================================================
        // RETURN TO STUDY
        // =================================================
        public async Task ReturnToStudy(int id)
        {
            var leave = await _unitOfWork
                .GenericRepository<StudentLeave>()
                .GetByIdAsync(
                    filter: x => x.Id == id,
                    disabledTracking: false
                );

            if (leave == null)
                throw new Exception("Không tìm thấy đơn bảo lưu.");

            if (leave.Status != StudentLeaveStatus.Approved)
            {
                throw new Exception(
                    "Đơn bảo lưu chưa được duyệt hoặc đã kết thúc.");
            }

            var student = await _unitOfWork
                .GenericRepository<Student>()
                .GetByIdAsync(
                    filter: s => s.Id == leave.StudentId && !s.IsDeleted,
                    disabledTracking: false
                );

            if (student == null)
                throw new Exception("Không tìm thấy sinh viên.");

            if (student.Status != StudentStatus.OnLeave)
            {
                throw new Exception(
                    "Sinh viên không ở trạng thái bảo lưu.");
            }

            leave.Status = StudentLeaveStatus.Completed;
            leave.ActualReturnDate = DateTime.Now;

            student.Status = StudentStatus.Studying;

            _unitOfWork.GenericRepository<StudentLeave>()
                .Update(leave);

            _unitOfWork.GenericRepository<Student>()
                .Update(student);

            await _unitOfWork.SaveAsync();
        }

        // =================================================
        // MAPPING
        // =================================================
        private static StudentLeaveViewModel MapToViewModel(
            StudentLeave leave)
        {
            return new StudentLeaveViewModel
            {
                Id = leave.Id,
                StudentId = leave.StudentId,
                StudentName = leave.Student != null
                    ? leave.Student.FirstName + " " + leave.Student.LastName
                    : string.Empty,
                StartDate = leave.StartDate,
                ExpectedReturnDate = leave.ExpectedReturnDate,
                ActualReturnDate = leave.ActualReturnDate,
                Reason = leave.Reason,
                Status = leave.Status,
                ApprovedBy = leave.ApprovedBy,
                ApprovedAt = leave.ApprovedAt,
                AdminNote = leave.AdminNote,
                CreatedAt = leave.CreatedAt
            };
        }
    }
}
