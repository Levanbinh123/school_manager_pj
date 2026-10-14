using SchoolProject.ViewModels;

namespace SchoolProject.Servies
{
    public interface IFeeService
    {
        // =========================================================
        // ADMIN
        // =========================================================

        Task<List<FeeViewModel>> GetAllFees();

        Task<FeeViewModel?> GetFeeById(
            int id);

        Task<FeeViewModel> CreateFee(
            CreateFeeViewModel vm);

        Task<FeeViewModel> UpdateFee(
            int id,
            UpdateFeeViewModel vm);

        Task<bool> DeleteFee(
            int id);


        // =========================================================
        // STUDENT
        // =========================================================

        Task<List<FeeViewModel>> GetMyFees(
            string userId);

        Task<FeeViewModel?> GetMyFee(
            string userId,
            int feeId);
    }
}