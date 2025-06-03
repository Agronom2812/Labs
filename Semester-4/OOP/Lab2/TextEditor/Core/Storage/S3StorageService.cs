using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;

namespace TextEditor.Core.Storage;

public sealed class S3StorageService(IAmazonS3 s3Client, IConfiguration config) {
    private readonly string _bucketName = config["AWS:BucketName"] ?? "text-ditor-bucket";

    public async Task UploadAsync(string key, Stream content) {
        var request = new PutObjectRequest { BucketName = _bucketName, Key = key, InputStream = content };
        await s3Client.PutObjectAsync(request);
    }

    public async Task<GetObjectResponse> DownloadAsync(string bucketName, string key) {
        var request = new GetObjectRequest { BucketName = bucketName, Key = key };
        return await s3Client.GetObjectAsync(request);
    }

    public async Task DeleteAsync(string key) {
        await s3Client.DeleteObjectAsync(_bucketName, key);
    }

    public async Task<bool> ExistsAsync(string key) {
        try {
            await s3Client.GetObjectMetadataAsync(_bucketName, key);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
            return false;
        }
    }
}
