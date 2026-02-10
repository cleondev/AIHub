using AIHub.ProductApi.Models;
using AIHub.ProductApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.ProductApi.Controllers;

[ApiController]
[Route("api/orders")]
public sealed class OrdersController(CatalogStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<OrderSummary>> GetList()
    {
        return Ok(store.GetOrders());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<OrderDetails> Get(Guid id)
    {
        var order = store.GetOrder(id);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost]
    public ActionResult<OrderDetails> Post([FromBody] UpsertOrderRequest request)
    {
        var order = store.CreateOrder(request);
        if (order is null)
        {
            return BadRequest("ProductId không tồn tại.");
        }

        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<OrderDetails> Put(Guid id, [FromBody] UpsertOrderRequest request)
    {
        var order = store.UpdateOrder(id, request);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}
