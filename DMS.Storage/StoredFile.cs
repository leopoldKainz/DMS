namespace DMS.Storage;

public record StoredFile(Stream Content, string ContentType, long Size);
