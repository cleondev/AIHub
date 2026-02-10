namespace AIHub.ProductApi.Models;

public sealed record OrderSummary(Guid Id, Guid ProductId, int Quantity, decimal TotalAmount, string Status);

public sealed record OrderDetails(Guid Id, Guid ProductId, int Quantity, decimal UnitPrice, decimal TotalAmount, string Status, DateTimeOffset UpdatedAt);

public sealed record UpsertOrderRequest(Guid ProductId, int Quantity, string Status);
