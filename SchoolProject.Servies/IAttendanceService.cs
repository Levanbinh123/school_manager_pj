using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IAttendanceService
    {
        Task AddAttendance(
            string userId,
            CreateAttendanceViewModel vm);

        Task<List<AttendanceViewModel>> GetBySchedule(
            string userId,
            int scheduleId);

        Task<List<AttendanceViewModel>> GetStudentAttendance(
            string userId);

        Task UpdateAttendance(
            string userId,
            int id,
            CreateAttendanceViewModel vm);
        Task<BulkAttendanceResultViewModel> BulkAttendance(
           string userId,
           BulkAttendanceViewModel vm);
        Task<List<AttendanceStudentViewModel>> GetStudentsForAttendance(
    string userId,
    int scheduleId,
    DateTime date);
    }
}