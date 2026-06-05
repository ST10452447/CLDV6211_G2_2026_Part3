using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace EventEase.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(IConfiguration configuration)
        {
            // ★ FIXED: Use "StorageConnection" instead of "AzureStorage"
            // This must match what's in your appsettings.json
            var connectionString = configuration.GetConnectionString("StorageConnection");

            // Create the blob service client
            var blobServiceClient = new BlobServiceClient(connectionString);

            // Get or create the container named "venue-images"
            _containerClient = blobServiceClient.GetBlobContainerClient("venue-images");

            // Create the container if it doesn't exist
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            // Check if file exists
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // Generate unique filename (GUID + original extension)
            // Example: "123e4567-e89b-12d3-a456-426614174000.jpg"
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

            // Get a reference to the blob
            var blobClient = _containerClient.GetBlobClient(fileName);

            // Upload the file stream to blob storage
            await using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders
                {
                    ContentType = file.ContentType  // e.g., "image/jpeg"
                });
            }

            // Return the URL where the image can be accessed
            return blobClient.Uri.ToString();
        }
    }
}