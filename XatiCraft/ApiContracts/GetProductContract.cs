namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Id"></param>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="Price"></param>
/// <param name="Resources"></param>
public record GetProductContract(
    long Id,
    string Title,
    string Description,
    decimal Price,
    IEnumerable<string>? Resources) : ApiContract;