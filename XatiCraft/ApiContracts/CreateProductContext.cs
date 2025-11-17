namespace XatiCraft.ApiContracts;

/// <summary>
/// </summary>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="Price"></param>
public record CreateProductContext(string Title, string Description, decimal Price) : ApiContext;