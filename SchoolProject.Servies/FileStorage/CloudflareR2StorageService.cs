
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SchoolProject.Utilities;

namespace SchoolProject.Servies
{
    public class CloudflareR2StorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3;
        private readonly CloudflareR2Options _options;

        public CloudflareR2StorageService(
            IAmazonS3 s3,
            IOptions<CloudflareR2Options> options)
        {
            _s3 = s3;
            _options = options.Value;
        }

        // =========================================
        // Upload file từ IFormFile
        // =========================================
        public async Task<string> UploadAsync(
            IFormFile file,
            string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            var extension = Path.GetExtension(file.FileName);

            var objectKey =
                $"{folder}/{Guid.NewGuid()}{extension}";

            await using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                    ? "application/octet-stream"
                    : file.ContentType,
              

                // Cloudflare R2 compatibility
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true
            };

            await _s3.PutObjectAsync(request);

            return objectKey;
        }

        // =========================================
        // Upload dữ liệu byte[] (chữ ký PNG...)
        // =========================================
        public async Task<string> UploadBytesAsync(
            byte[] data,
            string folder,
            string fileName,
            string contentType)
        {
            if (data == null || data.Length == 0)
                throw new ArgumentException("Dữ liệu upload không hợp lệ.");

            var objectKey =
                $"{folder}/{Guid.NewGuid()}_{Path.GetFileName(fileName)}";

            using var stream = new MemoryStream(data);

            var request = new PutObjectRequest
            {
                BucketName = _options.BucketName,
                Key = objectKey,
                InputStream = stream,
                ContentType = contentType,
               

                // Cloudflare R2 compatibility
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true
            };

            await _s3.PutObjectAsync(request);

            return objectKey;
        }

        // =========================================
        // Xóa file
        // =========================================
        public async Task DeleteAsync(string? objectKey)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
                return;

            await _s3.DeleteObjectAsync(
                new DeleteObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = objectKey
                });
        }

        // =========================================
        // Tạo URL đọc file có thời hạn
        // =========================================
        public string? GetReadUrl(
            string? objectKey,
            int expirationMinutes = 15)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
                return null;

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = objectKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };

            return _s3.GetPreSignedURL(request);
        }
    }
}
