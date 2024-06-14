using tutorial6.Models;

namespace tutorial6.Services;

public interface IProductWarehouseService
{
    Task<int> CreateProductWarehouse(ProductWarehouse productWarehouse);
}