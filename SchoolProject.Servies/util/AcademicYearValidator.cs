using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SchoolProject.Servies.util
{
    public static class AcademicYearValidator
    {
        public static bool TryValidate(
            string? academicYear,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(academicYear))
            {
                errorMessage = "Năm học không được để trống.";
                return false;
            }

            academicYear = academicYear.Trim();

            // Định dạng bắt buộc: YYYY-YYYY
            var match = Regex.Match(
                academicYear,
                @"^(\d{4})-(\d{4})$"
            );

            if (!match.Success)
            {
                errorMessage =
                    "Năm học phải có định dạng YYYY-YYYY. Ví dụ: 2026-2027.";
                return false;
            }

            int startYear = int.Parse(match.Groups[1].Value);
            int endYear = int.Parse(match.Groups[2].Value);

            // Năm kết thúc phải lớn hơn năm bắt đầu đúng 1
            if (endYear != startYear + 1)
            {
                errorMessage =
                    "Năm kết thúc phải lớn hơn năm bắt đầu đúng 1 năm.";
                return false;
            }

            int currentYear = DateTime.Now.Year;

            // Chặn hoàn toàn năm học đã qua
            if (endYear < currentYear)
            {
                errorMessage =
                    $"Không thể tạo năm học {academicYear} vì năm học này đã qua. " +
                    $"Năm hiện tại là {currentYear}.";
                return false;
            }

            return true;
        }
    }

}
