using Dapper;
using Microsoft.Data.SqlClient;
using ShopEZ.ProductService.Data;
using ShopEZ.ProductService.DTOs;
using ShopEZ.ProductService.Models;
using ShopEZ.ProductService.Repositories.Interfaces;

namespace ShopEZ.ProductService.Repositories
{
    /// <summary>
    /// Pure Dapper repository — zero Entity Framework.
    /// Every query uses explicit column lists (no SELECT *).
    /// Every input is parameterised — no SQL injection risk.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly ISqlConnectionFactory _factory;

        public ProductRepository(ISqlConnectionFactory factory)
        {
            _factory = factory;
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET ALL
        // NOLOCK hint: acceptable for a read-heavy product catalogue.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            const string sql = @"
                SELECT  ProductId,
                        Name,
                        Description,
                        Price,
                        ImageUrl,
                        Stock
                FROM    Products WITH (NOLOCK)
                ORDER BY ProductId ASC;";

            using SqlConnection conn = _factory.CreateConnection();
            return await conn.QueryAsync<Product>(sql);
        }

        // ─────────────────────────────────────────────────────────────────────
        // GET BY ID
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Product?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT  ProductId,
                        Name,
                        Description,
                        Price,
                        ImageUrl,
                        Stock
                FROM    Products WITH (NOLOCK)
                WHERE   ProductId = @Id;";

            using SqlConnection conn = _factory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Product>(
                sql, new { Id = id });
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE
        // OUTPUT INSERTED eliminates a second round-trip to retrieve the PK.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Product> CreateAsync(Product product)
        {
            const string sql = @"
                INSERT INTO Products (Name, Description, Price, ImageUrl, Stock)
                OUTPUT  INSERTED.ProductId,
                        INSERTED.Name,
                        INSERTED.Description,
                        INSERTED.Price,
                        INSERTED.ImageUrl,
                        INSERTED.Stock
                VALUES  (@Name, @Description, @Price, @ImageUrl, @Stock);";

            using SqlConnection conn = _factory.CreateConnection();
            return await conn.QuerySingleAsync<Product>(sql, new
            {
                product.Name,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.Stock
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE
        // Single round-trip: UPDATE + OUTPUT INSERTED.
        // Returns null when no row matched the PK.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<Product?> UpdateAsync(int id, Product product)
        {
            const string sql = @"
                UPDATE  Products
                SET     Name        = @Name,
                        Description = @Description,
                        Price       = @Price,
                        ImageUrl    = @ImageUrl,
                        Stock       = @Stock
                OUTPUT  INSERTED.ProductId,
                        INSERTED.Name,
                        INSERTED.Description,
                        INSERTED.Price,
                        INSERTED.ImageUrl,
                        INSERTED.Stock
                WHERE   ProductId = @Id;";

            using SqlConnection conn = _factory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Product>(sql, new
            {
                Id = id,
                product.Name,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.Stock
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"
                DELETE FROM Products
                WHERE   ProductId = @Id;";

            using SqlConnection conn = _factory.CreateConnection();
            int rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // EXISTS
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> ExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM   Products WITH (NOLOCK)
                WHERE  ProductId = @Id;";

            using SqlConnection conn = _factory.CreateConnection();
            int count = await conn.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // SEARCH + FILTER + PAGINATION
        //
        // Dynamic WHERE built from ProductSearchDTO.
        // Single round-trip via QueryMultipleAsync:
        //   Result 1: paged data rows
        //   Result 2: total count (for HasNext / TotalPages)
        // OFFSET / FETCH requires SQL Server 2012+.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<(IEnumerable<Product> Items, int TotalCount)> SearchAsync(
            ProductSearchDTO dto)
        {
            var parameters = new DynamicParameters();
            string where = BuildWhereClause(dto, parameters);

            int page = dto.Page < 1 ? 1 : dto.Page;
            int pageSize = dto.PageSize < 1 ? 10 :
                           dto.PageSize > 100 ? 100 : dto.PageSize;
            int offset = (page - 1) * pageSize;

            parameters.Add("Offset", offset);
            parameters.Add("PageSize", pageSize);

            string sql = $@"
                -- ── Data page ────────────────────────────────────────────────
                SELECT  ProductId,
                        Name,
                        Description,
                        Price,
                        ImageUrl,
                        Stock
                FROM    Products WITH (NOLOCK)
                {where}
                ORDER BY ProductId ASC
                OFFSET   @Offset   ROWS
                FETCH NEXT @PageSize ROWS ONLY;

                -- ── Total count ───────────────────────────────────────────────
                SELECT  COUNT(1)
                FROM    Products WITH (NOLOCK)
                {where};";

            using SqlConnection conn = _factory.CreateConnection();
            using var multi = await conn.QueryMultipleAsync(sql, parameters);

            IEnumerable<Product> items = await multi.ReadAsync<Product>();
            int total = await multi.ReadFirstAsync<int>();

            return (items, total);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DEDUCT STOCK
        // Atomic check + decrement in one UPDATE.
        // Returns false when stock is insufficient.
        // ─────────────────────────────────────────────────────────────────────
        public async Task<bool> DeductStockAsync(int productId, int quantity)
        {
            const string sql = @"
                UPDATE  Products
                SET     Stock = Stock - @Quantity
                WHERE   ProductId = @ProductId
                  AND   Stock    >= @Quantity;";

            using SqlConnection conn = _factory.CreateConnection();
            int rows = await conn.ExecuteAsync(sql,
                new { ProductId = productId, Quantity = quantity });

            return rows > 0;
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE — build parameterised WHERE clause
        // ─────────────────────────────────────────────────────────────────────
        private static string BuildWhereClause(
            ProductSearchDTO dto,
            DynamicParameters parameters)
        {
            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(dto.Keyword))
            {
                conditions.Add(
                    "(Name LIKE @Keyword OR Description LIKE @Keyword)");
                parameters.Add("Keyword", $"%{dto.Keyword.Trim()}%");
            }

            if (dto.MinPrice.HasValue)
            {
                conditions.Add("Price >= @MinPrice");
                parameters.Add("MinPrice", dto.MinPrice.Value);
            }

            if (dto.MaxPrice.HasValue)
            {
                conditions.Add("Price <= @MaxPrice");
                parameters.Add("MaxPrice", dto.MaxPrice.Value);
            }

            if (dto.MinStock.HasValue)
            {
                conditions.Add("Stock >= @MinStock");
                parameters.Add("MinStock", dto.MinStock.Value);
            }

            return conditions.Count > 0
                ? "WHERE " + string.Join(" AND ", conditions)
                : string.Empty;
        }
    }
}