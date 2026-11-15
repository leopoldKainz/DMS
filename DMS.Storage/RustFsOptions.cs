namespace DMS.Storage;

public class RustFsOptions
{
    public const string SectionName = "RustFS";

    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = "documents";
    public bool UseSsl { get; set; }
    public string Region { get; set; } = "us-east-1";
}
