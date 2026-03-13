namespace Shared.Application.Interfaces
{
    public interface IStorageService
    {
        /// <summary>
        /// Uploads a file and returns the public URL
        /// </summary>
        Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken ct = default);

        /// <summary>
        /// Deletes a file by its URL
        /// </summary>
        Task DeleteAsync(string fileUrl, CancellationToken ct = default);

        /// <summary>
        /// Checks if a file exists
        /// </summary>
        Task<bool> ExistsAsync(string fileUrl, CancellationToken ct = default);
    }

}
