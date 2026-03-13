using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Shared.Application.Interfaces;

namespace Shared.Infrastructure.Storage
{
    public class CloudflareR2StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _publicBaseUrl;

        public CloudflareR2StorageService(IConfiguration configuration)
        {
            _bucketName = configuration["Storage:R2:BucketName"]!;
            _publicBaseUrl = configuration["Storage:R2:PublicBaseUrl"]!;

            var accountId = configuration["Storage:R2:AccountId"]!;
            var accessKey = configuration["Storage:R2:AccessKey"]!;
            var secretKey = configuration["Storage:R2:SecretKey"]!;

            _s3Client = new AmazonS3Client(
                accessKey,
                secretKey,
                new AmazonS3Config
                {
                    ServiceURL =
                        $"https://{accountId}.r2.cloudflarestorage.com",
                    ForcePathStyle = true
                });
        }

        public async Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken ct = default)
        {
            var key = $"{folder}/{Guid.NewGuid()}_{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request, ct);

            // Return public CDN URL
            return $"{_publicBaseUrl}/{key}";
        }

        public async Task DeleteAsync(
            string fileUrl, CancellationToken ct = default)
        {
            try
            {
                var key = new Uri(fileUrl).AbsolutePath.TrimStart('/');

                await _s3Client.DeleteObjectAsync(
                    _bucketName, key, ct);
            }
            catch { }
        }

        public async Task<bool> ExistsAsync(
            string fileUrl, CancellationToken ct = default)
        {
            try
            {
                var key = new Uri(fileUrl).AbsolutePath.TrimStart('/');

                await _s3Client.GetObjectMetadataAsync(
                    _bucketName, key, ct);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

}
