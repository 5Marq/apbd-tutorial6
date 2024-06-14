using tutorial6.Models;

namespace tutorial6.Repos;

public interface IProductWarehouseRepository
{
    int CreateProductWarehouse(ProductWarehouse productWarehouse);
}