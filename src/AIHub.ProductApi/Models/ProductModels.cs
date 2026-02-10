namespace AIHub.ProductApi.Models;

public sealed record ProductSummary(Guid Id, string Name, string Category, decimal Price);

public sealed record ProductDetails(Guid Id, string Name, string Category, decimal Price, int Stock, DateTimeOffset UpdatedAt);

public sealed record UpsertProductRequest(string Name, string Category, decimal Price, int Stock);
