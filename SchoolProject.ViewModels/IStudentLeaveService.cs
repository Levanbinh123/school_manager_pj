using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.ViewModels
{
    public interface IStudentLeaveService
    {
        Task<StudentLeaveViewModel> CreateLeave(
            int studentId,
            CreateStudentLeaveViewModel model);

        Task<List<StudentLeaveViewModel>> GetAll();

        Task<StudentLeaveViewModel?> GetById(int id);

        Task Approve(
            int id,
            string? approvedBy,
            StudentLeaveDecisionViewModel model);

        Task Reject(
            int id,
            StudentLeaveDecisionViewModel model);

        Task ReturnToStudy(int id);
    }
}
