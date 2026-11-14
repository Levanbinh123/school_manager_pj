using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SchoolProject.Utilities
{
    public static class SePaySignature
    {
        public static string CreateSignature(
            Dictionary<string, string> fields,
            string secretKey)
        {
            string[] signedFields =
            {
                "order_amount",
                "merchant",
                "currency",
                "operation",
                "order_description",
                "order_invoice_number",
                "customer_id",
                "payment_method",
                "success_url",
                "error_url",
                "cancel_url"
            };

            var signedData = new List<string>();

            foreach (var field in signedFields)
            {
                if (!fields.TryGetValue(field, out var value))
                    continue;

                // SePay bỏ qua giá trị rỗng, 0 và "0".
                if (string.IsNullOrEmpty(value) || value == "0")
                    continue;

                signedData.Add($"{field}={value}");
            }

            string rawData = string.Join(",", signedData);

            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] dataBytes = Encoding.UTF8.GetBytes(rawData);

            using var hmac = new HMACSHA256(keyBytes);

            byte[] hash = hmac.ComputeHash(dataBytes);

            return Convert.ToBase64String(hash);
        }
    }
}
