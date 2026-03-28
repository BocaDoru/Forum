using commons.MessageBase;

namespace imagesServiceClient;

public partial class UploadRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileName) || Data.IsEmpty)
            return "Can not upload an empty file";
        return null;
    }
}

public partial class DownloadRequest : IRequestBase
{
    public string? Validate()
    {
        if (string.IsNullOrEmpty(FileId))
            return "Unknown file.";
        return null;
    }
}
