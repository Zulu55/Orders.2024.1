using Azure.Storage.Blobs.Models;

namespace Orders.Backend.Helpers
{
    public class FileStorage : IFileStorage
    {
        private readonly string _connectionString;
        private readonly IBlobContainerClientFactory _blobContainerClientFactory;

        public FileStorage(IConfiguration configuration, IBlobContainerClientFactory blobContainerClientFactory)
        {
            _connectionString = configuration["ConnectionStrings:AzureStorage"] ?? throw new InvalidOperationException("Connection string 'AzureStorage' not found.");
            _blobContainerClientFactory = blobContainerClientFactory;
        }

        public async Task RemoveFileAsync(string path, string containerName)
        {
            var client = _blobContainerClientFactory.CreateBlobContainerClient(_connectionString, containerName);
            await client.CreateIfNotExistsAsync();
            var fileName = Path.GetFileName(path);
            var blob = await client.GetBlobClientAsync(fileName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<string> SaveFileAsync(byte[] content, string extension, string containerName)
        {
            var client = _blobContainerClientFactory.CreateBlobContainerClient(_connectionString, containerName);
            await client.CreateIfNotExistsAsync();
            await client.SetAccessPolicyAsync(PublicAccessType.Blob);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var blob = await client.GetBlobClientAsync(fileName);

            using (var ms = new MemoryStream(content))
            {
                await blob.UploadAsync(ms);
            }

            return blob.Uri.ToString();
        }
    }
}