using AIHub.ProductApi.Models;
using AIHub.ProductApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.ProductApi.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(CatalogStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyCollection<ProductSummary>> GetList()
    {
        return Ok(store.GetProducts());
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ProductDetails> Get(Guid id)
    {
        var product = store.GetProduct(id);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpPost]
    public ActionResult<ProductDetails> Post([FromBody] UpsertProductRequest request)
    {
        var product = store.CreateProduct(request);
        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<ProductDetails> Put(Guid id, [FromBody] UpsertProductRequest request)
    {
        var product = store.UpdateProduct(id, request);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}
