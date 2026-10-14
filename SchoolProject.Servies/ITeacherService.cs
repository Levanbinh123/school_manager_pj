using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface ITeacherService
    {
        Task<int> AddSessionWithTeacher(
    int sessionId,
    List<int> teacherIdsList);

        Task AddTeacher(
            CreateTeacherViewModel vm);

        PagedResult<TeacherViewModel> GetAllTeacher(
            int pageNumber,
            int pageSize);

        int GetAllTeacher();
    }
}
