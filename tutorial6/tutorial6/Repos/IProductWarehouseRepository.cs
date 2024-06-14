using tutorial6.Models;

namespace tutorial6.Repos;

public interface IProductWarehouseRepository
{
    Task <int> CreateProductWarehouse(ProductWarehouse productWarehouse);
}