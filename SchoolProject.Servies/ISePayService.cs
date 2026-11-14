using System.Threading.Tasks;

namespace SchoolProject.Servies
{
    public interface ISePayService
    {
        // Tạo giao dịch thanh toán và lưu trạng thái Pending
        Task<string> CreatePayment(
            string userId,
            int feeId);

        // Tạo HTML form POST sang SePay Checkout
        Task<string> GenerateCheckoutHtml(
            string orderInvoiceNumber);

        // Xử lý Return từ SePay
        // Chỉ đọc trạng thái, không tự xác nhận đã thanh toán
        Task<(bool Success, int FeeId, string Message)>
            ProcessReturn(string orderInvoiceNumber);

        // Xử lý IPN từ SePay
        Task<(bool Success, string Message)>
            ProcessIpn(string requestBody, string? secretKey);
    }
}