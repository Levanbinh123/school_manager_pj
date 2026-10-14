using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IRegistrationService
    {
        // =====================================================
        // LẤY DANH SÁCH CLASS SESSION SINH VIÊN ĐƯỢC PHÉP
        // ĐĂNG KÝ
        //
        // Student sẽ thấy:
        // - Subject
        // - Class
        // - Teacher
        // - Session
        // =====================================================

        Task<List<ClassSessionViewModel>>
            GetAvailableClassSessions(
                int studentId,
                int sessionId);


        // =====================================================
        // ĐĂNG KÝ CLASS SESSION
        //
        // Student chọn trực tiếp ClassSession
        // =====================================================

        Task<bool> RegisterClassSession(
            int studentId,
            int classSessionId);


        // =====================================================
        // LẤY DANH SÁCH CLASS SESSION
        // STUDENT ĐÃ ĐĂNG KÝ
        // =====================================================

        Task<List<EnrollViewModel>>
            GetMyEnrollments(
                int studentId,
                int sessionId);


        // =====================================================
        // HỦY ĐĂNG KÝ CLASS SESSION
        // =====================================================

        Task<bool> CancelRegistration(
            int studentId,
            int classSessionId);
    }
}