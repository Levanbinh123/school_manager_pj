using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Utilities
{
    public class SePaySettings
    {
        // Merchant ID được cấp từ SePay Sandbox
        public string MerchantId { get; set; } = string.Empty;

        // Secret Key dùng để ký yêu cầu thanh toán
        public string SecretKey { get; set; } = string.Empty;

        // Địa chỉ Checkout Sandbox
        public string CheckoutUrl { get; set; } = string.Empty;

        // Backend nhận kết quả Return
        public string ReturnUrl { get; set; } = string.Empty;

        // Backend nhận IPN
        public string IpnUrl { get; set; } = string.Empty;

        // Chỉ cấu hình nếu phương thức xác thực IPN
        // của tài khoản SePay yêu cầu Secret Key riêng
        public string IpnSecretKey { get; set; } = string.Empty;

        // Frontend nhận kết quả sau khi quay về
        public string FrontendReturnUrl { get; set; } = string.Empty;
    }
}