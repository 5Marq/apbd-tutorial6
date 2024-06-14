using Microsoft.AspNetCore.Mvc;
using tutorial6.Models;
using tutorial6.Services;

namespace tutorial6.Controllers;

[Route("api/warehouse")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private IProductWarehouseService _productWarehouseService;

    public WarehouseController(IProductWarehouseService productWarehouseService)
    {
        _productWarehouseService = productWarehouseService;
    }

    [HttpPost]
    public IActionResult CreateProductWarehouseRepository(ProductWarehouse productWarehouse)
    {
        var affectedCount = _productWarehouseService.CreateProductWarehouse(productWarehouse);
        return StatusCode(StatusCodes.Status201Created);
    }
}