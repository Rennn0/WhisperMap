namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Product"></param>
/// <param name="FileName"></param>
public record GetSignedUrlContext(long Product, string FileName) : ApiContract;