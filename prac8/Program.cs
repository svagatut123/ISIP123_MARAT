using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.Remoting.Contexts;

// Классы моделей
public class User
{
    public int Id { get; set; }
    [Required]
    [StringLength(50)]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    [StringLength(100)]
    public string Email { get; set; }
    [Required]
    [StringLength(100)]
    public string FullName { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
}

public class Category
{
    public int Id { get; set; }
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    [StringLength(500)]
    public string Description { get; set; }

    public virtual ICollection<Product> Products { get; set; }
}

public class Product
{
    public int Id { get; set; }
    [Required]
    [StringLength(200)]
    public string Name { get; set; }
    [StringLength(1000)]
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }

    public virtual Category Category { get; set; }
    public virtual ICollection<CartItem> CartItems { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}

public class PickupPoint
{
    public int Id { get; set; }
    [Required]
    [StringLength(200)]
    public string Name { get; set; }
    [Required]
    [StringLength(500)]
    public string Address { get; set; }
    [StringLength(20)]
    public string Phone { get; set; }
    [StringLength(100)]
    public string WorkingHours { get; set; }

    public virtual ICollection<Order> Orders { get; set; }
}

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime AddedDate { get; set; }

    public virtual User User { get; set; }
    public virtual Product Product { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int PickupPointId { get; set; }
    public string Status { get; set; }

    public virtual User User { get; set; }
    public virtual PickupPoint PickupPoint { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public virtual Order Order { get; set; }
    public virtual Product Product { get; set; }
}

// Контекст базы данных
public class MarketplaceContext : DbContext
{
    public MarketplaceContext() : base("name=MarketplaceConnection")
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<PickupPoint> PickupPoints { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}

// Основной класс приложения
public class MarketplaceApp
{
    private MarketplaceContext context;
    private User currentUser;

    public MarketplaceApp()
    {
        context = new MarketplaceContext();
    }

    public void Run()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== Добро пожаловать в GMWOG Маркетплейс! ===");

        while (true)
        {
            if (currentUser == null)
            {
                ShowMainMenu();
            }
            else
            {
                ShowUserMenu();
            }
        }
    }

    private void ShowMainMenu()
    {
        Console.WriteLine("\n=== ГЛАВНОЕ МЕНЮ ===");
        Console.WriteLine("1 - Просмотр товаров");
        Console.WriteLine("2 - Регистрация");
        Console.WriteLine("3 - Вход в аккаунт");
        Console.WriteLine("4 - Выход");
        Console.Write("Выберите действие: ");

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                BrowseProducts();
                break;
            case "2":
                Register();
                break;
            case "3":
                Login();
                break;
            case "4":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine("Неверный выбор!");
                break;
        }
    }

    private void ShowUserMenu()
    {
        Console.WriteLine($"\n=== ДОБРО ПОЖАЛОВАТЬ, {currentUser.FullName.ToUpper()}! ===");
        Console.WriteLine("1 - Просмотр товаров");
        Console.WriteLine("2 - Корзина");
        Console.WriteLine("3 - Мои заказы");
        Console.WriteLine("4 - Выйти из аккаунта");
        Console.Write("Выберите действие: ");

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                BrowseProducts();
                break;
            case "2":
                ShowCart();
                break;
            case "3":
                ShowOrders();
                break;
            case "4":
                currentUser = null;
                Console.WriteLine("Вы вышли из аккаунта.");
                break;
            default:
                Console.WriteLine("Неверный выбор!");
                break;
        }
    }

    private void BrowseProducts()
    {
        Console.WriteLine("\n=== КАТАЛОГ ТОВАРОВ ===");

        var products = context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.StockQuantity > 0)
            .ToList();

        if (!products.Any())
        {
            Console.WriteLine("Товары не найдены.");
            return;
        }

        foreach (var product in products)
        {
            Console.WriteLine($"{product.Id}. {product.Name}");
            Console.WriteLine($"   Категория: {product.Category.Name}");
            Console.WriteLine($"   Цена: {product.Price:C}");
            Console.WriteLine($"   В наличии: {product.StockQuantity} шт.");
            Console.WriteLine($"   Описание: {product.Description}");
            Console.WriteLine();
        }

        if (currentUser != null)
        {
            Console.Write("Введите ID товара для добавления в корзину (0 - назад): ");
            if (int.TryParse(Console.ReadLine(), out int productId) && productId > 0)
            {
                AddToCart(productId);
            }
        }
        else
        {
            Console.WriteLine("Для добавления товаров в корзину необходимо войти в аккаунт.");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }

    private void AddToCart(int productId)
    {
        var product = context.Products.Find(productId);
        if (product == null)
        {
            Console.WriteLine("Товар не найден!");
            return;
        }

        if (product.StockQuantity <= 0)
        {
            Console.WriteLine("Товара нет в наличии!");
            return;
        }

        Console.Write($"Введите количество (максимум {product.StockQuantity}): ");
        if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0 || quantity > product.StockQuantity)
        {
            Console.WriteLine("Неверное количество!");
            return;
        }

        var existingCartItem = context.CartItems
            .FirstOrDefault(ci => ci.UserId == currentUser.Id && ci.ProductId == productId);

        if (existingCartItem != null)
        {
            existingCartItem.Quantity += quantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                UserId = currentUser.Id,
                ProductId = productId,
                Quantity = quantity,
                AddedDate = DateTime.Now
            };
            context.CartItems.Add(cartItem);
        }

        context.SaveChanges();
        Console.WriteLine("Товар добавлен в корзину!");
    }

    private void ShowCart()
    {
        Console.WriteLine("\n=== КОРЗИНА ===");

        var cartItems = context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == currentUser.Id)
            .ToList();

        if (!cartItems.Any())
        {
            Console.WriteLine("Корзина пуста.");
            return;
        }

        decimal total = 0;
        foreach (var item in cartItems)
        {
            var itemTotal = item.Quantity * item.Product.Price;
            total += itemTotal;
            Console.WriteLine($"{item.Product.Name}");
            Console.WriteLine($"   Количество: {item.Quantity}");
            Console.WriteLine($"   Цена за шт: {item.Product.Price:C}");
            Console.WriteLine($"   Итого: {itemTotal:C}");
            Console.WriteLine();
        }
        Console.WriteLine($"Общая сумма: {total:C}");

        Console.WriteLine("\n1 - Оформить заказ");
        Console.WriteLine("2 - Удалить товар из корзины");
        Console.WriteLine("3 - Очистить корзину");
        Console.WriteLine("4 - Назад");
        Console.Write("Выберите действие: ");

        var choice = Console.ReadLine();
        switch (choice)
        {
            case "1":
                Checkout();
                break;
            case "2":
                RemoveFromCart();
                break;
            case "3":
                ClearCart();
                break;
            case "4":
                break;
            default:
                Console.WriteLine("Неверный выбор!");
                break;
        }
    }

    private void Checkout()
    {
        var cartItems = context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == currentUser.Id)
            .ToList();

        if (!cartItems.Any())
        {
            Console.WriteLine("Корзина пуста!");
            return;
        }

        // Проверка наличия товаров
        foreach (var item in cartItems)
        {
            if (item.Product.StockQuantity < item.Quantity)
            {
                Console.WriteLine($"Недостаточно товара '{item.Product.Name}' на складе!");
                return;
            }
        }

        // Выбор ПВЗ
        var pickupPoints = context.PickupPoints.ToList();
        Console.WriteLine("\n=== ВЫБЕРИТЕ ПУНКТ ВЫДАЧИ ===");
        foreach (var point in pickupPoints)
        {
            Console.WriteLine($"{point.Id}. {point.Name}");
            Console.WriteLine($"   Адрес: {point.Address}");
            Console.WriteLine($"   Телефон: {point.Phone}");
            Console.WriteLine($"   Часы работы: {point.WorkingHours}");
            Console.WriteLine();
        }

        Console.Write("Введите ID пункта выдачи: ");
        if (!int.TryParse(Console.ReadLine(), out int pointId) || !pickupPoints.Any(p => p.Id == pointId))
        {
            Console.WriteLine("Неверный ID пункта выдачи!");
            return;
        }

        // Расчет общей суммы
        decimal totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);

        // Создание заказа
        var order = new Order
        {
            UserId = currentUser.Id,
            OrderDate = DateTime.Now,
            TotalAmount = totalAmount,
            PickupPointId = pointId,
            Status = "Обрабатывается"
        };
        context.Orders.Add(order);
        context.SaveChanges();

        // Добавление товаров в заказ
        foreach (var cartItem in cartItems)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.Product.Price
            };
            context.OrderItems.Add(orderItem);

            // Обновление количества на складе
            cartItem.Product.StockQuantity -= cartItem.Quantity;
        }

        // Очистка корзины
        context.CartItems.RemoveRange(cartItems);
        context.SaveChanges();

        Console.WriteLine($"Заказ №{order.Id} успешно оформлен!");
        Console.WriteLine($"Общая сумма: {totalAmount:C}");
        Console.WriteLine($"Статус: {order.Status}");
    }

    private void RemoveFromCart()
    {
        Console.Write("Введите ID товара для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Неверный ID!");
            return;
        }

        var cartItem = context.CartItems
            .FirstOrDefault(ci => ci.UserId == currentUser.Id && ci.ProductId == productId);

        if (cartItem != null)
        {
            context.CartItems.Remove(cartItem);
            context.SaveChanges();
            Console.WriteLine("Товар удален из корзины!");
        }
        else
        {
            Console.WriteLine("Товар не найден в корзине!");
        }
    }

    private void ClearCart()
    {
        var cartItems = context.CartItems.Where(ci => ci.UserId == currentUser.Id);
        context.CartItems.RemoveRange(cartItems);
        context.SaveChanges();
        Console.WriteLine("Корзина очищена!");
    }

    private void ShowOrders()
    {
        Console.WriteLine("\n=== МОИ ЗАКАЗЫ ===");

        var orders = context.Orders
            .Include(o => o.PickupPoint)
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == currentUser.Id)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        if (!orders.Any())
        {
            Console.WriteLine("Заказы не найдены.");
            return;
        }

        foreach (var order in orders)
        {
            Console.WriteLine($"Заказ №{order.Id} от {order.OrderDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Общая сумма: {order.TotalAmount:C}");
            Console.WriteLine($"ПВЗ: {order.PickupPoint.Name}");
            Console.WriteLine($"Статус: {order.Status}");
            Console.WriteLine("Товары:");

            foreach (var item in order.OrderItems)
            {
                var product = context.Products.Find(item.ProductId);
                Console.WriteLine($"   {product.Name} - {item.Quantity} шт. x {item.UnitPrice:C}");
            }
            Console.WriteLine();
        }
    }

    private void Register()
    {
        Console.WriteLine("\n=== РЕГИСТРАЦИЯ ===");

        Console.Write("Имя пользователя: ");
        var username = Console.ReadLine();

        if (context.Users.Any(u => u.Username == username))
        {
            Console.WriteLine("Пользователь с таким именем уже существует!");
            return;
        }

        Console.Write("Пароль: ");
        var password = Console.ReadLine();

        Console.Write("Подтвердите пароль: ");
        var confirmPassword = Console.ReadLine();

        if (password != confirmPassword)
        {
            Console.WriteLine("Пароли не совпадают!");
            return;
        }

        Console.Write("Email: ");
        var email = Console.ReadLine();

        Console.Write("Полное имя: ");
        var fullName = Console.ReadLine();

        var user = new User
        {
            Username = username,
            Password = HashPassword(password),
            Email = email,
            FullName = fullName,
            CreatedDate = DateTime.Now
        };

        context.Users.Add(user);
        context.SaveChanges();

        Console.WriteLine("Регистрация успешно завершена! Теперь вы можете войти в аккаунт.");
    }

    private void Login()
    {
        Console.WriteLine("\n=== ВХОД В АККАУНТ ===");

        Console.Write("Имя пользователя: ");
        var username = Console.ReadLine();

        Console.Write("Пароль: ");
        var password = Console.ReadLine();

        var hashedPassword = HashPassword(password);
        var user = context.Users.FirstOrDefault(u => u.Username == username && u.Password == hashedPassword);

        if (user != null)
        {
            currentUser = user;
            Console.WriteLine($"Добро пожаловать, {user.FullName}!");
        }
        else
        {
            Console.WriteLine("Неверное имя пользователя или пароль!");
        }
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

// Точка входа
class Program
{
    static void Main(string[] args)
    {
        try
        {
            var app = new MarketplaceApp();
            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}