
using Microsoft.EntityFrameworkCore;
using SchoolProject.Models;
using SchoolProject.Repositories;
using SchoolProject.ViewModels;
using SchoolProject.ViewModels.AcademicYear;
using SchoolProject.ViewModels.AcademicYear.SchoolProject.ViewModels;
using AcademicYearModel = SchoolProject.Models.AcademicYear;

namespace SchoolProject.Servies.Academiyear
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;

        public AcademicYearService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorage)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        // =========================================
        // 1. CREATE ACADEMIC YEAR
        // =========================================

        public async Task<AcademicYearViewModel> Create(
            CreateAcademicYearViewModel vm)
        {
            ValidateDates(vm.StartDate, vm.EndDate);

            var startDate = vm.StartDate.Date;
            var endDate = vm.EndDate.Date;

            bool isOverlapping = _unitOfWork
                .GenericRepository<AcademicYearModel>()
                .Exists(x =>
                    startDate <= x.EndDate.Date &&
                    endDate >= x.StartDate.Date);

            if (isOverlapping)
            {
                throw new InvalidOperationException(
                    "Khoảng thời gian năm học bị trùng với năm học khác.");
            }

            var academicYear = new AcademicYearModel
            {
                StartDate = startDate,
                EndDate = endDate,
                IsCurrent = false,
                Status = AcademicYearStatus.Planned
            };

            await _unitOfWork
                .GenericRepository<AcademicYearModel>()
                .AddAsync(academicYear);

            await _unitOfWork.SaveAsync();

            return new AcademicYearViewModel(academicYear);
        }

        // =========================================
        // 2. UPDATE ACADEMIC YEAR
        // =========================================

        public async Task<AcademicYearViewModel> Update(
            int id,
            UpdateAcademicYearViewModel vm)
        {
            ValidateDates(vm.StartDate, vm.EndDate);

            var repository =
                _unitOfWork.GenericRepository<AcademicYearModel>();

            var academicYear = await repository.GetByIdAsync(
                x => x.Id == id,
                include: q => q.Include(x => x.Sessions),
                disabledTracking: false);

            if (academicYear == null)
                throw new KeyNotFoundException("Không tìm thấy năm học.");

            if (academicYear.Status != AcademicYearStatus.Planned)
            {
                throw new InvalidOperationException(
                    "Chỉ được chỉnh sửa năm học ở trạng thái Planned.");
            }

            var startDate = vm.StartDate.Date;
            var endDate = vm.EndDate.Date;

            bool isOverlapping = repository.Exists(x =>
                x.Id != id &&
                startDate <= x.EndDate.Date &&
                endDate >= x.StartDate.Date);

            if (isOverlapping)
            {
                throw new InvalidOperationException(
                    "Khoảng thời gian năm học bị trùng với năm học khác.");
            }

            bool hasInvalidSession = academicYear.Sessions.Any(
                x => x.Start.Date < startDate ||
                     x.End.Date > endDate);

            if (hasInvalidSession)
            {
                throw new InvalidOperationException(
                    "Không thể thay đổi ngày năm học vì có học kỳ nằm ngoài khoảng thời gian mới.");
            }

            academicYear.StartDate = startDate;
            academicYear.EndDate = endDate;

            repository.Update(academicYear);

            await _unitOfWork.SaveAsync();

            return new AcademicYearViewModel(academicYear);
        }

        // =========================================
        // 3. GET ALL
        // =========================================

        public IEnumerable<AcademicYearViewModel> GetAll()
        {
            var academicYears = _unitOfWork
                .GenericRepository<AcademicYearModel>()
                .GetAll(
                    orderBy: q => q.OrderByDescending(x => x.StartDate),
                    include: q => q.Include(x => x.Sessions));

            return academicYears.Select(
                x => CreateViewModelWithSignatureUrl(x));
        }

        // =========================================
        // 4. GET BY ID
        // =========================================

        public async Task<AcademicYearViewModel?> GetById(int id)
        {
            var academicYear = await _unitOfWork
                .GenericRepository<AcademicYearModel>()
                .GetByIdAsync(
                    x => x.Id == id,
                    include: q => q.Include(x => x.Sessions));

            if (academicYear == null)
                return null;

            return CreateViewModelWithSignatureUrl(academicYear);
        }

        // =========================================
        // 5. GET CURRENT ACADEMIC YEAR
        // =========================================

        public async Task<AcademicYearViewModel?> GetCurrent()
        {
            var academicYear = await _unitOfWork
                .GenericRepository<AcademicYearModel>()
                .GetByIdAsync(
                    x => x.IsCurrent &&
                         x.Status == AcademicYearStatus.Active,
                    include: q => q.Include(x => x.Sessions));

            if (academicYear == null)
                return null;

            return CreateViewModelWithSignatureUrl(academicYear);
        }

        // =========================================
        // 6. ACTIVATE ACADEMIC YEAR
        // =========================================

        public async Task Activate(int id)
        {
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                var repository =
                    _unitOfWork.GenericRepository<AcademicYearModel>();

                var academicYear = await repository.GetByIdAsync(
                    x => x.Id == id,
                    disabledTracking: false);

                if (academicYear == null)
                    throw new KeyNotFoundException("Không tìm thấy năm học.");

                if (academicYear.Status != AcademicYearStatus.Planned)
                {
                    throw new InvalidOperationException(
                        "Chỉ có thể kích hoạt năm học ở trạng thái Planned.");
                }

                if (DateTime.Today < academicYear.StartDate.Date)
                {
                    throw new InvalidOperationException(
                        "Chưa đến ngày bắt đầu năm học.");
                }

                if (DateTime.Today > academicYear.EndDate.Date)
                {
                    throw new InvalidOperationException(
                        "Năm học đã qua ngày kết thúc, không thể kích hoạt.");
                }

                bool hasCurrentYear = repository.Exists(x =>
                    x.Id != id &&
                    x.IsCurrent &&
                    x.Status == AcademicYearStatus.Active);

                if (hasCurrentYear)
                {
                    throw new InvalidOperationException(
                        "Đã có một năm học khác đang hoạt động. Hãy hoàn thành năm học đó trước.");
                }

                academicYear.Status = AcademicYearStatus.Active;
                academicYear.IsCurrent = true;

                repository.Update(academicYear);

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // =========================================
        // 7. COMPLETE ACADEMIC YEAR
        // =========================================

        public async Task Complete(int id)
        {
            var repository =
                _unitOfWork.GenericRepository<AcademicYearModel>();

            var academicYear = await repository.GetByIdAsync(
                x => x.Id == id,
                include: q => q.Include(x => x.Sessions),
                disabledTracking: false);

            if (academicYear == null)
                throw new KeyNotFoundException("Không tìm thấy năm học.");

            if (academicYear.Status != AcademicYearStatus.Active)
            {
                throw new InvalidOperationException(
                    "Chỉ có thể hoàn thành năm học đang Active.");
            }

            if (DateTime.Today <= academicYear.EndDate.Date)
            {
                throw new InvalidOperationException(
                    "Chưa đến thời điểm hoàn thành năm học.");
            }

            if (!academicYear.Sessions.Any())
            {
                throw new InvalidOperationException(
                    "Năm học chưa có học kỳ nào.");
            }

            bool hasUncompletedSession = academicYear.Sessions.Any(
                x => x.Status != SessionStatus.Completed);

            if (hasUncompletedSession)
            {
                throw new InvalidOperationException(
                    "Không thể hoàn thành năm học vì vẫn còn học kỳ chưa hoàn thành.");
            }

            academicYear.Status = AcademicYearStatus.Completed;
            academicYear.IsCurrent = false;

            repository.Update(academicYear);

            await _unitOfWork.SaveAsync();
        }

        // =========================================
        // 8. CLOSE ACADEMIC YEAR
        // =========================================

        public async Task Close(
            int id,
            CloseAcademicYearViewModel vm,
            string adminUserId)
        {
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            string? uploadedSignatureKey = null;

            try
            {
                var repository =
                    _unitOfWork.GenericRepository<AcademicYearModel>();

                var academicYear = await repository.GetByIdAsync(
                    x => x.Id == id,
                    include: q => q.Include(x => x.Sessions),
                    disabledTracking: false);

                if (academicYear == null)
                    throw new KeyNotFoundException("Không tìm thấy năm học.");

                if (academicYear.Status != AcademicYearStatus.Completed)
                {
                    throw new InvalidOperationException(
                        "Chỉ có thể đóng năm học đã Completed.");
                }

                if (string.IsNullOrWhiteSpace(adminUserId))
                {
                    throw new InvalidOperationException(
                        "Không xác định được tài khoản Admin.");
                }

                if (vm == null || vm.ConfirmReviewed != true)
                {
                    throw new InvalidOperationException(
                        "Admin phải xác nhận đã kiểm tra dữ liệu.");
                }

                if (string.IsNullOrWhiteSpace(vm.Confirmation) ||
                    vm.Confirmation.Trim().Length < 20)
                {
                    throw new InvalidOperationException(
                        "Nội dung xác nhận phải có ít nhất 20 ký tự.");
                }

                const string signaturePrefix = "data:image/png;base64,";

                if (string.IsNullOrWhiteSpace(vm.SignatureData) ||
                    !vm.SignatureData.StartsWith(
                        signaturePrefix,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Chữ ký không hợp lệ.");
                }

                var signatureBase64 =
                    vm.SignatureData[signaturePrefix.Length..];

                byte[] signatureBytes;

                try
                {
                    signatureBytes =
                        Convert.FromBase64String(signatureBase64);
                }
                catch (FormatException)
                {
                    throw new InvalidOperationException(
                        "Dữ liệu chữ ký không đúng định dạng.");
                }

                byte[] pngHeader =
                {
                    137, 80, 78, 71, 13, 10, 26, 10
                };

                if (signatureBytes.Length < pngHeader.Length ||
                    !signatureBytes.Take(pngHeader.Length)
                        .SequenceEqual(pngHeader))
                {
                    throw new InvalidOperationException(
                        "Chữ ký không phải ảnh PNG hợp lệ.");
                }

                if (signatureBytes.Length > 1024 * 1024)
                {
                    throw new InvalidOperationException(
                        "Ảnh chữ ký không được vượt quá 1 MB.");
                }

                if (!academicYear.Sessions.Any())
                {
                    throw new InvalidOperationException(
                        "Năm học chưa có học kỳ nào.");
                }

                bool hasUncompletedSession =
                    academicYear.Sessions.Any(
                        x => x.Status != SessionStatus.Completed);

                if (hasUncompletedSession)
                {
                    throw new InvalidOperationException(
                        "Vẫn còn học kỳ chưa hoàn thành.");
                }

                // Upload chữ ký lên Cloudflare R2.
                uploadedSignatureKey =
                    await _fileStorage.UploadBytesAsync(
                        signatureBytes,
                        "academic-year-signatures",
                        $"academic-year-{id}.png",
                        "image/png");

                // Lưu object key vào database, không lưu base64.
                academicYear.ClosedByUserId = adminUserId;
                academicYear.ClosedAt = DateTime.UtcNow;
                academicYear.CloseConfirmation = vm.Confirmation.Trim();
                academicYear.CloseSignature = uploadedSignatureKey;

                academicYear.Status = AcademicYearStatus.Closed;
                academicYear.IsCurrent = false;

                repository.Update(academicYear);

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                // Database thất bại thì xóa chữ ký vừa upload.
                if (!string.IsNullOrWhiteSpace(uploadedSignatureKey))
                {
                    await _fileStorage.DeleteAsync(uploadedSignatureKey);
                }

                throw;
            }
        }

        // =========================================
        // PRIVATE - CREATE VIEW MODEL WITH SIGNATURE URL
        // =========================================

        private AcademicYearViewModel CreateViewModelWithSignatureUrl(
            AcademicYearModel academicYear)
        {
            var vm = new AcademicYearViewModel(academicYear);

            // CloseSignature trong model là object key.
            // ViewModel trả về URL ký số có thời hạn.
            vm.CloseSignature = _fileStorage.GetReadUrl(
                academicYear.CloseSignature);

            return vm;
        }

        // =========================================
        // 9. VALIDATE DATES
        // =========================================

        private static void ValidateDates(
            DateTime startDate,
            DateTime endDate)
        {
            if (startDate == default || endDate == default)
            {
                throw new ArgumentException(
                    "Ngày bắt đầu và ngày kết thúc không hợp lệ.");
            }

            if (endDate.Date <= startDate.Date)
            {
                throw new ArgumentException(
                    "Ngày kết thúc phải sau ngày bắt đầu.");
            }
        }
    }
}
