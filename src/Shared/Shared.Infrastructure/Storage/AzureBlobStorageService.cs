using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Shared.Application.Interfaces;

namespace Shared.Infrastructure.Storage
{
    public class AzureBlobStorageService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            var connectionString = configuration
                ["Storage:Azure:ConnectionString"]!;
            _containerName = configuration
                ["Storage:Azure:ContainerName"] ?? "products";

            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken ct = default)
        {
            // Get or create container
            var containerClient = _blobServiceClient
                .GetBlobContainerClient(_containerName);

            await containerClient.CreateIfNotExistsAsync(
                PublicAccessType.Blob, cancellationToken: ct);

            // Build blob path: products/abc123_filename.jpg
            var blobName = $"{folder}/{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload
            await blobClient.UploadAsync(
                fileStream,
                new BlobHttpHeaders { ContentType = contentType },
                cancellationToken: ct);

            // Return public URL
            return blobClient.Uri.ToString();
        }

        public async Task DeleteAsync(
            string fileUrl, CancellationToken ct = default)
        {
            try
            {
                var uri = new Uri(fileUrl);

                // Extract blob name from URL path
                // URL format: https://account.blob.core.windows.net/container/folder/file.jpg
                var blobName = string.Join("/",
                    uri.AbsolutePath.TrimStart('/').Split('/').Skip(1));

                var containerClient = _blobServiceClient
                    .GetBlobContainerClient(_containerName);

                await containerClient
                    .GetBlobClient(blobName)
                    .DeleteIfExistsAsync(cancellationToken: ct);
            }
            catch
            {
                // Log but don't throw
            }
        }

        public async Task<bool> ExistsAsync(
            string fileUrl, CancellationToken ct = default)
        {
            try
            {
                var uri = new Uri(fileUrl);
                var blobName = string.Join("/",
                    uri.AbsolutePath.TrimStart('/').Split('/').Skip(1));

                var containerClient = _blobServiceClient
                    .GetBlobContainerClient(_containerName);

                var blobClient = containerClient.GetBlobClient(blobName);
                var response = await blobClient.ExistsAsync(ct);
                return response.Value;
            }
            catch
            {
                return false;
            }
        }
    }

}
