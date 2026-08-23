using SQLite;
using Tazora.Models;

namespace Tazora.Services;

public class DatabaseService
{
    private const string DatabaseFileName = @"C:\Users\ismai\OneDrive\Masaüstü\tazora.db";

    private SQLiteAsyncConnection? _database;
    private bool _isInitialized;

    private async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        var databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            DatabaseFileName);

        _database = new SQLiteAsyncConnection(
            databasePath,
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache);

        await _database.CreateTableAsync<Category>();
        await _database.CreateTableAsync<Product>();
        await _database.CreateTableAsync<Discount>();
        await _database.CreateTableAsync<BasketItem>();
        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<CustomerOrder>();
        await _database.CreateTableAsync<OrderItem>();

        _isInitialized = true;
    }

    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        await InitializeAsync();

        return _database
            ?? throw new InvalidOperationException(
                "Veritabanı bağlantısı oluşturulamadı.");
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<Category>()
            .Where(category => category.IsActive)
            .OrderBy(category => category.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int categoryId)
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<Category>()
            .Where(category => category.Id == categoryId)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<Product>()
            .Where(product => product.IsActive)
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(
        int categoryId)
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<Product>()
            .Where(product =>
                product.CategoryId == categoryId &&
                product.IsActive)
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<List<Product>> GetPopularProductsAsync()
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<Product>()
            .Where(product =>
                product.IsPopular &&
                product.IsActive)
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int productId)
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<Product>()
            .Where(product => product.Id == productId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetProductCountAsync()
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<Product>()
            .Where(product => product.IsActive)
            .CountAsync();
    }

    public async Task<int> GetProductCountByCategoryAsync(int categoryId)
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<Product>()
            .Where(product =>
                product.CategoryId == categoryId &&
                product.IsActive)
            .CountAsync();
    }

    public async Task<List<Discount>> GetActiveDiscountsAsync()
    {
        var database = await GetDatabaseAsync();
        var currentDate = DateTime.UtcNow;

        return await database
            .Table<Discount>()
            .Where(discount =>
                discount.IsActive &&
                discount.StartDate <= currentDate &&
                discount.EndDate >= currentDate)
            .OrderByDescending(discount => discount.DiscountRate)
            .ToListAsync();
    }

    public async Task<Discount?> GetDiscountByProductIdAsync(
        int productId)
    {
        var database = await GetDatabaseAsync();
        var currentDate = DateTime.UtcNow;

        return await database
            .Table<Discount>()
            .Where(discount =>
                discount.ProductId == productId &&
                discount.IsActive &&
                discount.StartDate <= currentDate &&
                discount.EndDate >= currentDate)
            .OrderByDescending(discount => discount.DiscountRate)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetActiveDiscountCountAsync()
    {
        var database = await GetDatabaseAsync();
        var currentDate = DateTime.UtcNow;

        return await database
            .Table<Discount>()
            .Where(discount =>
                discount.IsActive &&
                discount.StartDate <= currentDate &&
                discount.EndDate >= currentDate)
            .CountAsync();
    }

}