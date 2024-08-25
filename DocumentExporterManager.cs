using Amazon.S3.Model;
using AWS_Model;
using Owner = AWS_Model.Owner;

namespace AWS_Exportel;

public interface IDocumentExporterManager
{
    Task<List<ExportBucketCollection>> GetExportBucket();
    Task<byte[]> DownloadFile(string fileName);
}

public class DocumentExporterManager:IDocumentExporterManager
{
    private readonly IDocumentStorage _documentStorage;

    public DocumentExporterManager(IDocumentStorage documentStorage)
    {
        _documentStorage = documentStorage;
    }
    public async Task<List<ExportBucketCollection>> GetExportBucket()
    {
        List<ExportBucketCollection> exportBucketCollection = new List<ExportBucketCollection>();
        var s3List= await _documentStorage.ListExportBucketFiles();
        foreach (var s3Files in s3List)
        {
            if(s3Files is null) {continue;}
            exportBucketCollection.Add(new ExportBucketCollection()
            {
                Key = s3Files.Key,
                FileName = Path.GetFileName(s3Files.Key),
                LastModified =  s3Files.LastModified,
                Size = s3Files.Size,
                Owner = new Owner()
                {
                    DisplayName = s3Files.Owner?.DisplayName,
                    Id = s3Files.Owner?.Id
                }
            });
        }
        return exportBucketCollection;
    }
    public async Task<byte[]> DownloadFile(string fileName)
    {
        return await _documentStorage.DownloadObjectFromBucketAsync(fileName);
    }
}