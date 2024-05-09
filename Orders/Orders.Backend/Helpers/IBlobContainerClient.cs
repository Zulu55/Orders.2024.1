using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Orders.Backend.Helpers
{
    public interface IBlobContainerClient
    {
        Task<BlobClient> GetBlobClientAsync(string name);

        Task CreateIfNotExistsAsync();

        Task SetAccessPolicyAsync(PublicAccessType accessType);
    }
}