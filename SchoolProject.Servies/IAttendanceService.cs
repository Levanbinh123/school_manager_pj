using SchoolProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public interface IAttendanceService
    {
        Task AddAttendance(
            string userId,
            CreateAttendanceViewModel vm);

        Task<List<AttendanceViewModel>>
            GetByClassSession(
                string userId,
                int classSessionId);

        Task<List<AttendanceViewModel>>
            GetStudentAttendance(
                string userId);

        Task UpdateAttendance(
            string userId,
            int id,
            CreateAttendanceViewModel vm);
    }
}
