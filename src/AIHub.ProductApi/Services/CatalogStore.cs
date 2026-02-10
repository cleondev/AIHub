using AIHub.ProductApi.Models;

namespace AIHub.ProductApi.Services;

public sealed class CatalogStore
{
    private readonly Dictionary<Guid, ProductDetails> _products = new();
    private readonly Dictionary<Guid, OrderDetails> _orders = new();

    public CatalogStore()
    {
        var sampleProduct = new ProductDetails(
            Guid.NewGuid(),
            "Laptop AIHub",
            "electronics",
            25999000,
            20,
            DateTimeOffset.UtcNow);

        _products[sampleProduct.Id] = sampleProduct;
    }

    public IReadOnlyCollection<ProductSummary> GetProducts() =>
        _products.Values
            .Select(x => new ProductSummary(x.Id, x.Name, x.Category, x.Price))
            .ToArray();

    public ProductDetails? GetProduct(Guid id) => _products.GetValueOrDefault(id);

    public ProductDetails CreateProduct(UpsertProductRequest request)
    {
        var product = new ProductDetails(
            Guid.NewGuid(),
            request.Name,
            request.Category,
            request.Price,
            request.Stock,
            DateTimeOffset.UtcNow);

        _products[product.Id] = product;
        return product;
    }

    public ProductDetails? UpdateProduct(Guid id, UpsertProductRequest request)
    {
        if (!_products.ContainsKey(id))
        {
            return null;
        }

        var product = new ProductDetails(
            id,
            request.Name,
            request.Category,
            request.Price,
            request.Stock,
            DateTimeOffset.UtcNow);

        _products[id] = product;
        return product;
    }

    public IReadOnlyCollection<OrderSummary> GetOrders() =>
        _orders.Values
            .Select(x => new OrderSummary(x.Id, x.ProductId, x.Quantity, x.TotalAmount, x.Status))
            .ToArray();

    public OrderDetails? GetOrder(Guid id) => _orders.GetValueOrDefault(id);

    public OrderDetails? CreateOrder(UpsertOrderRequest request)
    {
        if (!_products.TryGetValue(request.ProductId, out var product))
        {
            return null;
        }

        var order = new OrderDetails(
            Guid.NewGuid(),
            request.ProductId,
            request.Quantity,
            product.Price,
            product.Price * request.Quantity,
            request.Status,
            DateTimeOffset.UtcNow);

        _orders[order.Id] = order;
        return order;
    }

    public OrderDetails? UpdateOrder(Guid id, UpsertOrderRequest request)
    {
        if (!_orders.ContainsKey(id) || !_products.TryGetValue(request.ProductId, out var product))
        {
            return null;
        }

        var order = new OrderDetails(
            id,
            request.ProductId,
            request.Quantity,
            product.Price,
            product.Price * request.Quantity,
            request.Status,
            DateTimeOffset.UtcNow);

        _orders[id] = order;
        return order;
    }
}
