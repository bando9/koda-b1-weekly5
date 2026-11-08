
// namespace Ewallet.Mvc.Repositories;


// public class SqlUserRepository : IUserRepository
// {
//     private readonly string _connectionString;
    

//     public SqlUserRepository(IConfiguration configuration)
//     {
//         _connectionString=configuration.GetConnectionString("ewalletdb") ?? throw new InvalidOperationException("Connection string tidak ditemukan di appsettings.");
        
//     }

//     public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
//     {
//         const string sql = """
//             SELECT Id, CategoryId, ProductCode, Name, Price, Stock, IsActive
//             FROM dbo.Product
//         """;

//         var products = new List<Product>();

//         await using var connection = new SqlConnection(_connectionString);

//         await connection.OpenAsync(cancellationToken);

//         await using var command = new SqlCommand(sql, connection);

//         await using var reader = await command.ExecuteReaderAsync(cancellationToken);

//         while(await reader.ReadAsync(cancellationToken))
//         {
//             products.Add(ReadProduct(reader));
//         }
//         return products;
//     }

//     public async Task<Product?> GetByCodeAsync(string productCode, CancellationToken cancellationToken)
//     {
//         const string sql = """
//         SELECT Id, CategoryId, ProductCode, Name, Price, Stock, IsActive FROM dbo.Product WHERE ProductCode = @productCode;
//         """;

//         await using var connection = new SqlConnection(_connectionString);

//         await connection.OpenAsync(cancellationToken);

//         await using var command = new SqlCommand(sql, connection);

//         AddProductCode(command, productCode);

//         await using var reader = await command.ExecuteReaderAsync(cancellationToken);

//         if (!await reader.ReadAsync(cancellationToken))
//         {
//             return null;
//         }
//         return ReadProduct(reader);
//     }

//     public async Task<bool> TryAddAsync(Product product, CancellationToken cancellationToken)
//     {
//         const string sql = """
//             INSERT INTO dbo.Product (CategoryId, ProductCode, Name, Price, Stock)
//             VALUES (@categoryId, @productCode, @name, @price, @stock);
//         """;

//         await using var connection = new SqlConnection(_connectionString);

//         await connection.OpenAsync(cancellationToken);

//         await using var command = new SqlCommand(sql, connection);

//         AddProductParameters(command, product);

//         try
//         {
//             var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

//             return affectedRows == 1;
//         }
//         catch (SqlException e) when (e.Number is 2601 or 2627)
//         {
//             return false;
//         }
//         catch (SqlException e)
//         {
//             System.Console.WriteLine($"SQL Error tidak terduga saat menambah produk: {e.Number}", e.Number);
//             return false;
//         }
//     }

//     public async Task<bool> TryDeleteAsync(string productCode, CancellationToken cancellationToken)
//     {
//         const string sql = """
//             DELETE FROM dbo.Product WHERE ProductCode = @productCode;
//         """;

//         await using var connection = new SqlConnection(_connectionString);

//         await connection.OpenAsync(cancellationToken);

//         await using var command = new SqlCommand(sql, connection);

//         AddProductCode(command, productCode);
        
//         var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

//         return affectedRows == 1;


//     }

//     public async Task<bool> TryUpdateAsync(Product product, CancellationToken cancellationToken)
//     {
//         const string sql = """
//             UPDATE dbo.Product SET
//                 CategoryId = @categoryId,
//                 Name = @name,
//                 Price = @price,
//                 Stock = @stock
//             WHERE ProductCode = @productCode;
//             """;

//         await using var connection = new SqlConnection(_connectionString);

//         await connection.OpenAsync(cancellationToken);

//         await using var command = new SqlCommand(sql, connection);

//         AddProductParameters(command, product);
        
//         var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);

//         return affectedRows == 1;

//     }


//     // * ==================== METHOD ====================


//     private void AddProductParameters(SqlCommand command, Product product)
//     {
//         AddProductCode(command, product.ProductCode);

//         command.Parameters.Add("@categoryId", SqlDbType.Int).Value = product.CategoryId;

//         command.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = product.Name;

//         command.Parameters.Add("@stock", SqlDbType.Int).Value = product.Stock;

//         var price = command.Parameters.Add("@price", SqlDbType.Decimal);
//         price.Precision = 18;
//         price.Scale = 2;
//         price.Value = product.Price;
//     }

//     private static void AddProductCode(SqlCommand command, string productCode)
//     {
//         command.Parameters.Add("@productCode", SqlDbType.NVarChar, 30).Value = productCode.Trim().ToUpperInvariant();
//     }

//     private static Product ReadProduct(SqlDataReader reader)
//     {
//         return new Product(
//             reader.GetInt32(1),
//             reader.GetString(2),
//             reader.GetString(3),
//             reader.GetDecimal(4),
//             reader.GetInt32(5)
//         );
        
//     }
// }