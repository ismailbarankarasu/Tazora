using SQLite;
using Tazora.Models;

namespace Tazora.Services;

public class DatabaseService
{
    private const string DatabaseFileName = "tazora.db3";

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
}