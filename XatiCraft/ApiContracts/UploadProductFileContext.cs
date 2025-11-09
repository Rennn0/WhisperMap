namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Product"></param>
/// <param name="Stream"></param>
/// <param name="FileName"></param>
public record UploadProductFileContext(long Product, Stream Stream, string FileName) : ApiContract;