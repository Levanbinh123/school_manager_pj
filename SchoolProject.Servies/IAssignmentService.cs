using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IAssignmentService
    {
        // =====================================================
        // TEACHER - ASSIGNMENT
        // =====================================================

        Task AddAssignment(
            string userId,
            CreateAssignmentViewModel vm);

        Task<List<AssignmentViewModel>> GetTeacherAssignments(
            string userId);

        Task<AssignmentViewModel?> GetTeacherAssignmentById(
            string userId,
            int id);

        Task UpdateAssignment(
            string userId,
            int id,
            CreateAssignmentViewModel vm);

        Task DeleteAssignment(
            string userId,
            int id);


        // =====================================================
        // TEACHER - CLASS SESSION
        // =====================================================

        Task<List<TeacherClassSessionViewModel>>
            GetTeacherClassSessions(
                string userId);

        Task<List<TeacherStudentViewModel>>
            GetStudentsByClassSession(
                string userId,
                int classSessionId);


        // =====================================================
        // STUDENT
        // =====================================================

        Task<List<AssignmentViewModel>> GetStudentAssignments(
            string userId);

        Task<AssignmentViewModel?> GetStudentAssignmentById(
            string userId,
            int id);

        Task SubmitAssignment(
            string userId,
            int assignmentId,
            SubmitAssignmentRequest request);


        // =====================================================
        // TEACHER - SUBMISSIONS
        // =====================================================

        Task<List<SubmissionViewModel>> GetSubmissions(
            string userId,
            int assignmentId);

        Task GradeSubmission(
            string userId,
            int submissionId,
            GradeSubmissionRequest request);


        // =====================================================
        // TEACHER - ASSIGNMENT MANAGEMENT
        // =====================================================

        Task<AssignmentSummaryViewModel>
            GetAssignmentSummary(
                string userId,
                int assignmentId);

        Task<List<StudentNotSubmittedViewModel>>
            GetNotSubmittedStudents(
                string userId,
                int assignmentId);

        Task<List<SubmissionNotGradedViewModel>>
            GetNotGradedSubmissions(
                string userId,
                int assignmentId);
    }
}