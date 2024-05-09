namespace Orders.Backend.Helpers
{
    public class BlobContainerClientFactory : IBlobContainerClientFactory
    {
        public IBlobContainerClient CreateBlobContainerClient(string connectionString, string containerName) => new BlobContainerClientWrapper(connectionString, containerName);
    }
}