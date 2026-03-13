using Microsoft.Extensions.Configuration;
using Shared.Application.Interfaces;

namespace Shared.Infrastructure.Storage
{

    public class LocalStorageService : IStorageService
    {
        private readonly string _uploadRootPath;
        private readonly string _baseUrl;

        public LocalStorageService(IConfiguration configuration)
        {
            // Path where files will be physically saved
            // e.g. "C:/Projects/Ecommerce/uploads"
            // or relative: "uploads"  → resolves to app working directory
            _uploadRootPath = configuration["Storage:Local:UploadPath"]
                ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            // Base URL used to build the public URL returned to caller
            // e.g. "https://localhost:7000/uploads"
            _baseUrl = configuration["Storage:Local:BaseUrl"]
                ?? "https://localhost:7000/uploads";
        }

        public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken ct = default)
        {
            // Build full directory path
            var folderPath = Path.Combine(_uploadRootPath, folder);
            Directory.CreateDirectory(folderPath);

            // Generate unique filename to avoid collisions
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            // Write file to disk
            await using var output = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await fileStream.CopyToAsync(output, ct);

            // Return public URL
            // e.g. https://localhost:7000/uploads/products/abc123/guid.jpg
            return $"{_baseUrl.TrimEnd('/')}/{folder}/{uniqueFileName}";
        }

        public Task DeleteAsync(
            string fileUrl, CancellationToken ct = default)
        {
            try
            {
                var filePath = UrlToFilePath(fileUrl);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {
                // Log but never throw — file may already be gone
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(
            string fileUrl, CancellationToken ct = default)
        {
            try
            {
                var filePath = UrlToFilePath(fileUrl);
                return Task.FromResult(File.Exists(filePath));
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        // ── Private Helpers ──────────────────────────────────

        // Converts public URL back to physical file path
        // URL: https://localhost:7000/uploads/products/abc/guid.jpg
        // Path: C:/Projects/uploads/products/abc/guid.jpg
        private string UrlToFilePath(string fileUrl)
        {
            var baseUri = new Uri(_baseUrl.TrimEnd('/') + "/");
            var fileUri = new Uri(fileUrl);

            // Get relative path after base URL
            // e.g. "products/abc/guid.jpg"
            var relativePath = baseUri
                .MakeRelativeUri(fileUri)
                .ToString()
                .Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(_uploadRootPath, relativePath);
        }
    }


}
