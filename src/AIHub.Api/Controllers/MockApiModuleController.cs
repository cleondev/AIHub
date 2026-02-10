using AIHub.Api.Models;
using AIHub.Modules.MockApi;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("module/mock-api")]
public sealed class MockApiModuleController : ControllerBase
{
    private readonly IMockApiService _mockApiService;

    public MockApiModuleController(IMockApiService mockApiService)
    {
        _mockApiService = mockApiService;
    }

    [HttpGet("products")]
    public ActionResult<ApiResponse<IEnumerable<ProductItem>>> ListProducts([FromQuery] string? keyword)
    {
        return Ok(ApiResponse.From(_mockApiService.ListProducts(keyword), TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("purchase-requests")]
    public ActionResult<ApiResponse<PurchaseRequest>> CreatePurchaseRequest([FromBody] PurchaseCreateRequest request)
    {
        try
        {
            var created = _mockApiService.CreatePurchaseRequest(request.ProductId, request.Quantity);
            return Ok(ApiResponse.From(created, TraceIdProvider.GetFromHttpContext(HttpContext)));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.From(new ErrorResponse("product_not_found", "Không tìm thấy sản phẩm."), TraceIdProvider.GetFromHttpContext(HttpContext)));
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(ApiResponse.From(new ErrorResponse("invalid_quantity", ex.Message), TraceIdProvider.GetFromHttpContext(HttpContext)));
        }
    }
}
