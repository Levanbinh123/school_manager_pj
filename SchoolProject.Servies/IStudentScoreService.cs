using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IStudentScoreService
    {
        // Giáo viên xem điểm tổng kết
        Task<List<StudentScoreViewModel>> GetTeacherScores(string userId);

        // Tổng kết điểm cuối môn
        Task AddScore(
            string userId,
            CreateStudentScoreViewModel vm);

        // Cập nhật điểm tổng kết
        Task UpdateScore(
            string userId,
            int id,
            CreateStudentScoreViewModel vm);

        // Sinh viên xem điểm tổng kết
        Task<List<StudentScoreViewModel>> GetStudentScores(
            string userId);

        // Sinh viên xem điểm tổng kết theo môn
        Task<List<StudentScoreViewModel>> GetStudentScoresBySubject(
            string userId,
            int subjectId);
    }
}