namespace XatiCraft.ApiContracts;

/// <inheritdoc />
public record UpdateProductContext(long Id,string Title, string Description, decimal Price) : CreateProductContext(Title, Description, Price);