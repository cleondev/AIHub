namespace AIHub.Modules.MockApi;

public enum MockRequestStatus
{
    Pending,
    Approved
}

public sealed record MockRequest(Guid Id, string Name, MockRequestStatus Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public sealed record ProductItem(Guid Id, string Sku, string Name, decimal UnitPrice, int StockQuantity);

public enum PurchaseRequestStatus
{
    Created,
    RejectedOutOfStock
}

public sealed record PurchaseRequest(
    Guid Id,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    int Quantity,
    PurchaseRequestStatus Status,
    string Message,
    DateTimeOffset CreatedAt);

public interface IMockApiService
{
    // Legacy contract dùng cho ToolGateway trong MVP cũ.
    IEnumerable<MockRequest> Query(string? keyword);
    MockRequest Create(string name);
    MockRequest? Approve(Guid id);

    IEnumerable<ProductItem> ListProducts(string? keyword);
    PurchaseRequest CreatePurchaseRequest(Guid productId, int quantity);
    IEnumerable<PurchaseRequest> ListPurchaseRequests();
}

public sealed class MockApiService : IMockApiService
{
    private readonly Dictionary<Guid, MockRequest> _requests = [];

    private readonly Dictionary<Guid, ProductItem> _products = new();

    private readonly List<PurchaseRequest> _purchaseRequests = [];

    public MockApiService()
    {
        SeedProducts();
    }

    public IEnumerable<MockRequest> Query(string? keyword)
    {
        var normalized = keyword?.Trim();
        var records = _requests.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            records = records.Where(item => item.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return records.OrderBy(item => item.CreatedAt);
    }

    public MockRequest Create(string name)
    {
        var item = new MockRequest(Guid.NewGuid(), name.Trim(), MockRequestStatus.Pending, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        _requests[item.Id] = item;
        return item;
    }

    public MockRequest? Approve(Guid id)
    {
        if (!_requests.TryGetValue(id, out var current))
        {
            return null;
        }

        var approved = current with
        {
            Status = MockRequestStatus.Approved,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _requests[id] = approved;
        return approved;
    }

    public IEnumerable<ProductItem> ListProducts(string? keyword)
    {
        var normalized = keyword?.Trim();
        var records = _products.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            records = records.Where(item =>
                item.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase)
                || item.Sku.Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return records.OrderBy(item => item.Name);
    }

    public PurchaseRequest CreatePurchaseRequest(Guid productId, int quantity)
    {
        if (!_products.TryGetValue(productId, out var product))
        {
            throw new KeyNotFoundException("Không tìm thấy sản phẩm.");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Số lượng mua phải lớn hơn 0.");
        }

        if (quantity > product.StockQuantity)
        {
            var rejected = new PurchaseRequest(
                Guid.NewGuid(),
                product.Id,
                product.Sku,
                product.Name,
                quantity,
                PurchaseRequestStatus.RejectedOutOfStock,
                $"Sản phẩm '{product.Name}' đã hết hoặc không đủ tồn kho. Tồn hiện tại: {product.StockQuantity}.",
                DateTimeOffset.UtcNow);

            _purchaseRequests.Add(rejected);
            return rejected;
        }

        _products[product.Id] = product with { StockQuantity = product.StockQuantity - quantity };

        var created = new PurchaseRequest(
            Guid.NewGuid(),
            product.Id,
            product.Sku,
            product.Name,
            quantity,
            PurchaseRequestStatus.Created,
            $"Đã tạo request mua hàng thành công cho '{product.Name}' với số lượng {quantity}.",
            DateTimeOffset.UtcNow);

        _purchaseRequests.Add(created);
        return created;
    }

    public IEnumerable<PurchaseRequest> ListPurchaseRequests()
    {
        return _purchaseRequests.OrderByDescending(item => item.CreatedAt);
    }

    private void SeedProducts()
    {
        var seed = new[]
        {
            new ProductItem(Guid.Parse("0de8aca6-722f-4da9-bf5c-1a6884bc8476"), "SP-001", "Bàn phím cơ", 1290000m, 8),
            new ProductItem(Guid.Parse("562cf6cf-4a09-4e37-95cc-bf4cbccf8f84"), "SP-002", "Chuột gaming", 790000m, 3),
            new ProductItem(Guid.Parse("9e4ab5e8-6fca-4830-9ea1-d2c746ae8f3c"), "SP-003", "Tai nghe bluetooth", 990000m, 0)
        };

        foreach (var item in seed)
        {
            _products[item.Id] = item;
        }
    }
}
