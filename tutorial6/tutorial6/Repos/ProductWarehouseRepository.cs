using System;
using System.Data.SqlClient;
using tutorial6.Models;

namespace tutorial6.Repos
{
    public class ProductWarehouseRepository : IProductWarehouseRepository
    {
        private readonly IConfiguration _configuration;

        public ProductWarehouseRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int CreateProductWarehouse(ProductWarehouse productWarehouse)
        {
            using var con = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            con.Open();
            using var transaction = con.BeginTransaction();

            try
            {
                // Sprawdzenie, czy produkt istnieje
                var cmd = new SqlCommand("SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct", con, transaction);
                cmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                var productExists = (int)cmd.ExecuteScalar() > 0;
                if (!productExists)
                {
                    throw new Exception("Product not found."); //działa
                }

                // Sprawdzenie, czy magazyn istnieje
                cmd.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                var warehouseExists = (int)cmd.ExecuteScalar() > 0;
                if (!warehouseExists)
                {
                    throw new Exception("Warehouse not found.");
                }
                
                // Sprawdzenie, czy istnieje odpowiednie zamówienie
                cmd.CommandText = @"
                    SELECT TOP 1 IdOrder, CreatedAt 
                    FROM [Order] 
                    WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt AND FulfilledAt IS NULL";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                cmd.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);

                int orderId;
                DateTime orderCreatedAt;

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new Exception("No matching order found.");
                    }
                    reader.Read();
                    orderId = (int)reader["IdOrder"];
                    orderCreatedAt = (DateTime)reader["CreatedAt"];
                }

                // Aktualizacja kolumny FulfilledAt zamówienia
                cmd.CommandText = "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
                cmd.Parameters.AddWithValue("@IdOrder", orderId);
                cmd.ExecuteNonQuery();

                // Wstawienie rekordu do tabeli Product_Warehouse
                cmd.CommandText = @"
                    INSERT INTO Product_Warehouse(IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                    OUTPUT INSERTED.IdProductWarehouse
                    VALUES(@IdWarehouse, @IdProduct, @IdOrder, @Amount, 
                           (SELECT Price FROM Product WHERE IdProduct = @IdProduct) * @Amount, @CreatedAt)"; //swieci sie na czerwono ale dziala ...
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                cmd.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                cmd.Parameters.AddWithValue("@IdOrder", orderId);
                cmd.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                var productWarehouseId = (int)cmd.ExecuteScalar();

                // Zatwierdzenie transakcji
                transaction.Commit();

                return productWarehouseId;
            }
            catch (Exception ex)
            {
                // Cofnięcie transakcji w przypadku błędu
                transaction.Rollback();
                throw new Exception("An error occurred while creating the product warehouse: " + ex.Message);
            }
        }
    }
}
