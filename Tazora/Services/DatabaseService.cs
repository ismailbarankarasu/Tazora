using SQLite;
using Tazora.Models;
using System.Security.Cryptography;

namespace Tazora.Services;

public class DatabaseService
{
    private const string DatabaseFileName = "tazora.db";
    private const int PasswordIterationCount = 100_000;
    private const int PasswordSaltSize = 16;
    private const int PasswordHashSize = 32;
    private SQLiteAsyncConnection? _database;
    private bool _isInitialized;

    private async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        var databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            DatabaseFileName);
        System.Diagnostics.Debug.WriteLine( $"TAZORA DATABASE PATH: {databasePath}");
        await CopyDatabaseAsync(databasePath);

        _database = new SQLiteAsyncConnection(
            databasePath,
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache);

        await _database.ExecuteAsync(
            "PRAGMA foreign_keys = ON;");

        await _database.CreateTableAsync<Category>();
        await _database.CreateTableAsync<Product>();
        await _database.CreateTableAsync<Discount>();
        await _database.CreateTableAsync<BasketItem>();
        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<CustomerOrder>();
        await _database.CreateTableAsync<OrderItem>();
        await _database.CreateTableAsync<Coupon>();


        await SeedDataAsync();

        _isInitialized = true;
    }
    private async Task SeedDataAsync()
    {
        if (_database is null)
            throw new InvalidOperationException(
                "Seed işlemi için veritabanı bağlantısı bulunamadı.");

        var couponCount = await _database.Table<Coupon>().CountAsync();
        if (couponCount == 0)
        {
            var defaultCoupons = new List<Coupon>
    {
        new Coupon { Code = "TAZORA10", DiscountRate = 10, MinimumBasketAmount = 0, IsActive = true },
        new Coupon { Code = "SPIN25", DiscountAmount = 25, MinimumBasketAmount = 50, IsActive = true },
        new Coupon { Code = "SPIN50", DiscountAmount = 50, MinimumBasketAmount = 100, IsActive = true },
        new Coupon { Code = "FIRSAT100", DiscountAmount = 100, MinimumBasketAmount = 200, IsActive = true },
        new Coupon { Code = "FIRSAT250", DiscountAmount = 250, MinimumBasketAmount = 500, IsActive = true }
    };

            await _database.InsertAllAsync(defaultCoupons);
        }


        var categoryCount = await _database
            .Table<Category>()
            .CountAsync();

        if (categoryCount > 0)
            return;
       
        var categories = new List<Category>
    {
        new()
        {
            Name = "Meyve & Sebze",
            ImageName = "category_fruit_vegetable.jpg",
            IconCode = "\ue2e7",
            DisplayOrder = 1
        },
        new()
        {
            Name = "Süt & Kahvaltı",
            ImageName = "category_dairy.jpg",
            IconCode = "\ueb47",
            DisplayOrder = 2
        },
        new()
        {
            Name = "Et & Tavuk",
            ImageName = "category_meat.jpg",
            IconCode = "\ue56c",
            DisplayOrder = 3
        },
        new()
        {
            Name = "İçecek",
            ImageName = "category_drinks.jpg",
            IconCode = "\ue540",
            DisplayOrder = 4
        },
        new()
        {
            Name = "Atıştırmalık",
            ImageName = "category_snacks.jpg",
            IconCode = "\uea69",
            DisplayOrder = 5
        },
        new()
        {
            Name = "Temel Gıda",
            ImageName = "category_grocery.jpg",
            IconCode = "\ue8cc",
            DisplayOrder = 6
        }

    };

        foreach (var category in categories)
        {
            await _database.InsertAsync(category);
        }

        var products = new List<Product>
    {
        new()
        {
            CategoryId = categories[0].Id,
            Name = "Amasya Elması",
            Description = "Taze ve özenle seçilmiş Amasya elması.",
            Unit = "1 kg",
            Price = 25.50m,
            ImageName = "product_apple.jpg",
            IsPopular = true
        },
        new()
        {
            CategoryId = categories[0].Id,
            Name = "İthal Muz",
            Description = "Olgun ve tatlı ithal muz.",
            Unit = "1 kg",
            Price = 49.90m,
            ImageName = "product_banana.jpg",
            IsPopular = true
        },
        new()
        {
            CategoryId = categories[0].Id,
            Name = "Salkım Domates",
            Description = "Günlük ve taze salkım domates.",
            Unit = "1 kg",
            Price = 34.50m,
            ImageName = "product_tomato.jpg",
            IsPopular = true
        },
        new()
        {
            CategoryId = categories[0].Id,
            Name = "Organik Ispanak",
            Description = "Taze organik ıspanak.",
            Unit = "500 g",
            Price = 18.90m,
            ImageName = "product_spinach.jpg",
            IsPopular = true
        },
        new()
        {
            CategoryId = categories[1].Id,
            Name = "Taze Kaşar Peyniri",
            Description = "Kahvaltılar için taze kaşar peyniri.",
            Unit = "600 g",
            Price = 170.00m,
            ImageName = "product_cheese.jpg",
            IsPopular = true
        },
        new()
        {
            CategoryId = categories[1].Id,
            Name = "Organik Yumurta",
            Description = "Doğal ortamda yetiştirilen tavuklardan organik yumurta.",
            Unit = "10'lu paket",
            Price = 85.00m,
            ImageName = "product_eggs.jpg",
            IsPopular = true
        },
        new()
        {
            CategoryId = categories[2].Id,
            Name = "Tavuk Göğsü",
            Description = "Taze ve paketlenmiş tavuk göğsü.",
            Unit = "1 kg",
            Price = 189.90m,
            ImageName = "product_chicken.jpg"
        },
        new()
        {
            CategoryId = categories[3].Id,
            Name = "Doğal Maden Suyu",
            Description = "Doğal mineralli maden suyu.",
            Unit = "6 x 200 ml",
            Price = 54.90m,
            ImageName = "product_mineral_water.jpg"
        },
        new()
        {
            CategoryId = categories[4].Id,
            Name = "Patates Cipsi",
            Description = "Çıtır ve klasik lezzetli patates cipsi.",
            Unit = "150 g",
            Price = 44.90m,
            ImageName = "product_chips.jpg"
        },
        new()
        {
            CategoryId = categories[5].Id,
            Name = "Ekşi Mayalı Köy Ekmeği",
            Description = "Geleneksel yöntemlerle hazırlanan ekşi mayalı ekmek.",
            Unit = "1 adet",
            Price = 32.50m,
            ImageName = "product_bread.jpg",
            IsPopular = true
        },
        new()
        {
            CategoryId = categories[5].Id,
            Name = "Soğuk Sıkım Zeytinyağı",
            Description = "Doğal soğuk sıkım zeytinyağı.",
            Unit = "750 ml",
            Price = 145.00m,
            ImageName = "product_olive_oil.jpg"
        }
    };

        foreach (var product in products)
        {
            await _database.InsertAsync(product);
        }

        var currentDate = DateTime.UtcNow;

        var discounts = new List<Discount>
    {
        new()
        {
            Title = "İlk Siparişine Özel",
            Description = "İlk siparişinde seçili ürünlerde %30 indirim.",
            ImageName = "campaign_first_order.jpg",
            DiscountRate = 30,
            StartDate = currentDate.AddDays(-1),
            EndDate = currentDate.AddDays(30)
        },
        new()
        {
            ProductId = products[1].Id,
            Title = "İthal Muz Fırsatı",
            Description = "İthal muzda kaçırılmayacak indirim.",
            ImageName = "campaign_banana.jpg",
            DiscountRate = 15,
            StartDate = currentDate.AddDays(-1),
            EndDate = currentDate.AddDays(7)
        },
        new()
        {
            ProductId = products[4].Id,
            Title = "Kahvaltılık Fırsatı",
            Description = "Taze kaşar peynirinde özel indirim.",
            ImageName = "campaign_breakfast.jpg",
            DiscountRate = 20,
            StartDate = currentDate.AddDays(-1),
            EndDate = currentDate.AddDays(7)
        }
    };

        foreach (var discount in discounts)
        {
            await _database.InsertAsync(discount);
        }
    }
    private async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        await InitializeAsync();

        return _database
            ?? throw new InvalidOperationException(
                "Veritabanı bağlantısı oluşturulamadı.");
    }
    public async Task<Coupon?> GetCouponByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var cleanCode = code.Trim().ToUpperInvariant();
        var allActiveCoupons = await _database.Table<Coupon>().Where(c => c.IsActive).ToListAsync();

        return allActiveCoupons.FirstOrDefault(c => c.Code.Equals(cleanCode, StringComparison.OrdinalIgnoreCase));
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

    public async Task<int> AddProductToBasketAsync(int productId, int quantityToAdd = 1)
    {
        quantityToAdd = Math.Clamp(quantityToAdd, 1, 99);
        var database = await GetDatabaseAsync();

        var product = await database
            .Table<Product>()
            .Where(item =>
                item.Id == productId &&
                item.IsActive)
            .FirstOrDefaultAsync();

        if (product is null)
        {
            throw new InvalidOperationException(
                "Sepete eklenecek ürün bulunamadı.");
        }

        var basketItem = await database
            .Table<BasketItem>()
            .Where(item => item.ProductId == productId)
            .FirstOrDefaultAsync();

        if (basketItem is null)
        {
            basketItem = new BasketItem
            {
                ProductId = productId,
                Quantity = quantityToAdd,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await database.InsertAsync(basketItem);

            return basketItem.Quantity;
        }

        if (basketItem.Quantity >= 99)
        {
            return basketItem.Quantity;
        }

        basketItem.Quantity = Math.Min(basketItem.Quantity + quantityToAdd, 99);
        basketItem.UpdatedAt = DateTime.UtcNow;

        await database.UpdateAsync(basketItem);

        return basketItem.Quantity;
    }

    public async Task<List<BasketItem>> GetBasketItemsAsync()
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<BasketItem>()
            .Where(item => item.Quantity > 0)
            .OrderByDescending(item => item.UpdatedAt)
            .ToListAsync();
    }

    public async Task<int> UpdateBasketQuantityAsync(
        int productId,
        int quantity)
    {
        var database = await GetDatabaseAsync();

        var basketItem = await database
            .Table<BasketItem>()
            .Where(item => item.ProductId == productId)
            .FirstOrDefaultAsync();

        if (basketItem is null)
            return 0;

        if (quantity <= 0)
        {
            await database.DeleteAsync(basketItem);
            return 0;
        }

        basketItem.Quantity = Math.Min(quantity, 99);
        basketItem.UpdatedAt = DateTime.UtcNow;

        await database.UpdateAsync(basketItem);

        return basketItem.Quantity;
    }

    public async Task RemoveProductFromBasketAsync(
        int productId)
    {
        var database = await GetDatabaseAsync();

        var basketItem = await database
            .Table<BasketItem>()
            .Where(item => item.ProductId == productId)
            .FirstOrDefaultAsync();

        if (basketItem is not null)
        {
            await database.DeleteAsync(basketItem);
        }
    }

    public async Task ClearBasketAsync()
    {
        var database = await GetDatabaseAsync();

        await database.DeleteAllAsync<BasketItem>();
    }
    public async Task<bool> IsEmailRegisteredAsync(string email)
    {
        var database = await GetDatabaseAsync();
        var normalizedEmail = NormalizeEmail(email);

        var userCount = await database
            .Table<User>()
            .Where(user => user.Email == normalizedEmail)
            .CountAsync();

        return userCount > 0;
    }

    public async Task<int> RegisterUserAsync(
    string fullName,
    string email,
    string? phoneNumber,
    string password)
    {
        var database = await GetDatabaseAsync();
        var normalizedEmail = NormalizeEmail(email);

        var isRegistered = await IsEmailRegisteredAsync(normalizedEmail);

        if (isRegistered)
        {
            throw new InvalidOperationException(
                "Bu e-posta adresi zaten kullanılıyor.");
        }

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            PhoneNumber = phoneNumber?.Trim(),
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        await database.InsertAsync(user);

        return user.Id;
    }

    public async Task<User?> LoginAsync(
    string email,
    string password)
    {
        var database = await GetDatabaseAsync();
        var normalizedEmail = NormalizeEmail(email);

        var user = await database
            .Table<User>()
            .Where(user => user.Email == normalizedEmail)
            .FirstOrDefaultAsync();

        if (user is null)
            return null;

        var isPasswordValid = VerifyPassword(
            password,
            user.PasswordHash);

        return isPasswordValid ? user : null;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(
            PasswordSaltSize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            PasswordIterationCount,
            HashAlgorithmName.SHA256,
            PasswordHashSize);

        return string.Join(
            ".",
            PasswordIterationCount,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    private static bool VerifyPassword(
        string password,
        string storedPasswordHash)
    {
        try
        {
            var parts = storedPasswordHash.Split('.');

            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out var iterationCount))
                return false;

            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterationCount,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static async Task CopyDatabaseAsync(
    string destinationPath)
    {
        if (File.Exists(destinationPath))
            return;

        await using var sourceStream =
            await FileSystem.OpenAppPackageFileAsync(
                DatabaseFileName);

        await using var destinationStream =
            File.Create(destinationPath);

        await sourceStream.CopyToAsync(destinationStream);
    }

    public async Task<int> CreateOrderFromBasketAsync(
    int userId)
    {
        if (userId <= 0)
        {
            throw new InvalidOperationException(
                "Sipariş oluşturmak için kullanıcı girişi gereklidir.");
        }

        var database = await GetDatabaseAsync();
        var createdOrderId = 0;

        await database.RunInTransactionAsync(connection =>
        {
            var basketItems = connection
                .Table<BasketItem>()
                .Where(item => item.Quantity > 0)
                .ToList();

            if (basketItems.Count == 0)
            {
                throw new InvalidOperationException(
                    "Sepette sipariş oluşturulacak ürün bulunmuyor.");
            }

            var currentDate = DateTime.UtcNow;

            var activeDiscounts = connection
                .Table<Discount>()
                .Where(discount =>
                    discount.IsActive &&
                    discount.StartDate <= currentDate &&
                    discount.EndDate >= currentDate)
                .ToList();

            var discountsByProductId = activeDiscounts
                .Where(discount => discount.ProductId.HasValue)
                .GroupBy(discount => discount.ProductId!.Value)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(item => item.DiscountRate)
                        .First());

            var orderLines = new List<OrderItem>();

            decimal subtotal = 0;
            decimal discountAmount = 0;

            foreach (var basketItem in basketItems)
            {
                var product = connection
                    .Table<Product>()
                    .FirstOrDefault(item =>
                        item.Id == basketItem.ProductId &&
                        item.IsActive);

                if (product is null)
                    continue;

                var originalLineTotal =
                    product.Price * basketItem.Quantity;

                var discountRate =
                    discountsByProductId.TryGetValue(
                        product.Id,
                        out var discount)
                        ? discount.DiscountRate
                        : 0;

                var unitPrice = discountRate > 0
                    ? Math.Round(
                        product.Price *
                        (100 - discountRate) / 100,
                        2)
                    : product.Price;

                var lineTotal =
                    unitPrice * basketItem.Quantity;

                subtotal += originalLineTotal;
                discountAmount +=
                    originalLineTotal - lineTotal;

                orderLines.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = unitPrice,
                    Quantity = basketItem.Quantity,
                    TotalPrice = lineTotal
                });
            }

            if (orderLines.Count == 0)
            {
                throw new InvalidOperationException(
                    "Sipariş oluşturulabilecek aktif ürün bulunamadı.");
            }

            var discountedSubtotal =
                subtotal - discountAmount;

            var deliveryFee =
                discountedSubtotal >= 300m
                    ? 0
                    : 19.90m;

            var order = new CustomerOrder
            {
                UserId = userId,
                OrderDate = currentDate,
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                DeliveryFee = deliveryFee,
                TotalAmount =
                    discountedSubtotal + deliveryFee,
                Status = OrderStatus.Preparing
            };

            connection.Insert(order);

            foreach (var orderLine in orderLines)
            {
                orderLine.OrderId = order.Id;
                connection.Insert(orderLine);
            }

            connection.DeleteAll<BasketItem>();

            createdOrderId = order.Id;
        });

        return createdOrderId;
    }

    public async Task<List<CustomerOrder>> GetOrdersByUserAsync(
    int userId)
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<CustomerOrder>()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();
    }

    public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(
        int orderId)
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<OrderItem>()
            .Where(item => item.OrderId == orderId)
            .OrderBy(item => item.Id)
            .ToListAsync();
    }

    public async Task<CustomerOrder?> GetOrderByIdAsync(
    int orderId,
    int userId)
    {
        var database = await GetDatabaseAsync();

        return await database
            .Table<CustomerOrder>()
            .Where(order =>
                order.Id == orderId &&
                order.UserId == userId)
            .FirstOrDefaultAsync();
    }
}