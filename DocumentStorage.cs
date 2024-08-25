using Amazon.S3;
using Amazon.S3.Model;
using AWS_Model;
using Owner = AWS_Model.Owner;

namespace AWS_Exportel;

public interface IDocumentStorage
{
    Task<List<S3Object>> ListExportBucketFiles();
    Task<byte[]> DownloadObjectFromBucketAsync(string objectName);
}

public class DocumentStorage: IDocumentStorage
{
    private readonly string _bucketName = "bucketName ";
    private readonly string _prefix = "prefix";
    // Create an Amazon S3 client object. The constructor uses the
    // default user installed on the system. To work with Amazon S3
    // features in a different AWS Region, pass the AWS Region as a
    // parameter to the client constructor.
    private readonly  IAmazonS3 _s3Client = new AmazonS3Client();
    // public async Task<ExportBucketResult> ListExportBucketFiles()
    // {
    //     ExportBucketResult bucketResult = new ExportBucketResult(){ExportBucketCollection = new List<ExportBucketCollection>()};
    //     var request = new ListObjectsV2Request()
    //     {
    //         BucketName = _bucketName,
    //         Prefix = $"{_prefix}/",
    //         MaxKeys = 1000
    //     };
    //     ListObjectsV2Response response;
    //
    //     do
    //     {
    //         response = await _s3Client.ListObjectsV2Async(request);
    //
    //         foreach (var s3Files in response.S3Objects)
    //         {
    //             if(s3Files==null) {continue;}
    //             bucketResult.ExportBucketCollection.Add(new ExportBucketCollection()
    //             {
    //                 Key = s3Files.Key,
    //                 FileName = Path.GetFileName(s3Files.Key),
    //                 LastModified =  s3Files.LastModified,
    //                 Size = s3Files.Size,
    //                 Owner = new Owner()
    //                 {
    //                     DisplayName = s3Files.Owner?.DisplayName,
    //                     Id = s3Files.Owner?.Id
    //                 }
    //             });
    //         }
    //         // If the response is truncated, set the request ContinuationToken
    //         // from the NextContinuationToken property of the response.
    //         request.ContinuationToken = response.NextContinuationToken;
    //     }
    //     while (response.IsTruncated);
    //
    //     return bucketResult;
    // }
    public async Task<List<S3Object>> ListExportBucketFiles()
    {
        ExportBucketResult bucketResult = new ExportBucketResult(){ExportBucketCollection = new List<ExportBucketCollection>()};
        List<S3Object> s3Objects = new List<S3Object>();
        var request = new ListObjectsV2Request()
        {
            BucketName = _bucketName,
            Prefix = $"{_prefix}/",
            MaxKeys = 1000
        };
        ListObjectsV2Response response;
        do
        {
            response = await _s3Client.ListObjectsV2Async(request);
            s3Objects.AddRange(response.S3Objects);
            // If the response is truncated, set the request ContinuationToken
            // from the NextContinuationToken property of the response.
            request.ContinuationToken = response.NextContinuationToken;
        }
        while (response.IsTruncated);
    
        return s3Objects;
    }
    /// <summary>
    /// Shows how to download an object from an Amazon S3 bucket to the
    /// local computer.
    /// </summary>
    /// <param name="objectName">The name of the object to download.</param>
    /// <returns>A boolean value indicating the success or failure of the
    /// download process.</returns>
    public async Task<byte[]> DownloadObjectFromBucketAsync(
        string objectName)
    {
        // Create a GetObject request
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = objectName,
        };

        // Issue request and remember to dispose of the response
        using GetObjectResponse response = await _s3Client.GetObjectAsync(request);
        try
        {
            using MemoryStream stream=new MemoryStream();
            await response.ResponseStream.CopyToAsync(stream);
            byte[] bytes=stream.ToArray();
            return bytes;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error get {objectName}: {ex.Message}");
            return null;
        }
    }
}