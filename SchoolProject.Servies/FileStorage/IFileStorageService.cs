
using Microsoft.AspNetCore.Http;

namespace SchoolProject.Servies
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(
           IFormFile file,
           string folder);

        Task<string> UploadBytesAsync(
            byte[] data,
            string folder,
            string fileName,
            string contentType);

        Task DeleteAsync(string? objectKey);

        string? GetReadUrl(
            string? objectKey,
            int expirationMinutes = 15);
    }
}
