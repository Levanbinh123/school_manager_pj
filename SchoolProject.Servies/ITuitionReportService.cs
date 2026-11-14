
using SchoolProject.ViewModels;
using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface ITuitionReportService
    {
        // Báo cáo tổng quan học phí
        Task<TuitionReportViewModel> GetTuitionOverview();

        // Báo cáo học phí theo học kỳ
        Task<TuitionSessionReportViewModel> GetTuitionBySession(
            int sessionId
        );
    }
}
