using System;
using System.Collections.Generic;
using System.Linq;
using prac8;


    class MarketplaceGame
    {
        private Users currentUser;
        private Random random;

        public MarketplaceGame()
        {
            random = new Random();
        }

        public void RunGame()
        {
            

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
            Console.WriteLine("\nГлавное меню:");
            Console.WriteLine("1 - Регистрация");
            Console.WriteLine("2 - Вход");
            Console.WriteLine("3 - Просмотр товаров");
            Console.WriteLine("4 - Выход");
            Console.Write("Ваш выбор: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": Register(); break;
                case "2": Login(); break;
                case "3": ShowProducts(); break;
                case "4": Environment.Exit(0); break;
                default: Console.WriteLine("Неверный выбор"); break;
            }
        }

        private void ShowUserMenu()
        {
            Console.WriteLine($"\nЛичный кабинет ({currentUser.Username})");
            Console.WriteLine("1 - Товары");
            Console.WriteLine("2 - Корзина");
            Console.WriteLine("3 - Мои заказы");
            Console.WriteLine("4 - Выйти");
            Console.Write("Ваш выбор: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ShowProducts(currentUser); break;
                case "2": ShowCart(currentUser); break;
                case "3": ShowOrders(currentUser); break;
                case "4": currentUser = null; break;
                default: Console.WriteLine("Неверный выбор"); break;
            }
        }

        private void Register()
        {
            Console.WriteLine("\nРегистрация");

            Console.Write("Логин: ");
            string login = Console.ReadLine();

            if (Core.Context.Users.Any(u => u.Username == login))
            {
                Console.WriteLine("Этот логин уже занят");
                return;
            }

            Console.Write("Email: ");
            string email = Console.ReadLine();

            if (Core.Context.Users.Any(u => u.Email == email))
            {
                Console.WriteLine("Этот email уже используется");
                return;
            }

            Console.Write("Полное имя: ");
            string fullName = Console.ReadLine();

            Console.Write("Телефон: ");
            string phone = Console.ReadLine();

            string password;
            while (true)
            {
                Console.Write("Пароль: ");
                password = Console.ReadLine();

                Console.Write("Повторите пароль: ");
                string password2 = Console.ReadLine();

                if (password == password2)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Пароли не совпадают");
                }
            }

            Users newUser = new Users
            {
                Username = login,
                Email = email,
                PasswordHash = password,
                PhoneNumber = phone,
                FullName = fullName,
                CreatedDate = DateTime.Now
            };

            Core.Context.Users.Add(newUser);
            Core.Context.SaveChanges();

            Console.WriteLine("Регистрация успешна");
        }
    private void Login()
    {
        Console.WriteLine("\nВход");

        Console.Write("Логин: ");
        string login = Console.ReadLine();

        Console.Write("Пароль: ");
        string password = Console.ReadLine();

        Users user = Core.Context.Users.FirstOrDefault(u => u.Username == login);

        if (user != null && password == user.PasswordHash)
        {
            currentUser = user;
            Console.WriteLine($"Добро пожаловать, {user.FullName}");
        }
        else
        {
            Console.WriteLine("Неверный логин или пароль");
        }
    }

    private void ShowProducts(Users user = null)
    {
        Console.WriteLine("\nТовары");

        var products = Core.Context.Products.ToList();

        if (products.Count == 0)
        {
            Console.WriteLine("Товаров нет");
            return;
        }

        foreach (var product in products)
        {
            var category = Core.Context.Categories.Find(product.CategoryId);
            Console.WriteLine($"{product.ProductId}. {product.ProductName} - {product.Price} руб");
            Console.WriteLine($"   {product.Description}");
            Console.WriteLine($"   Категория: {category?.CategoryName}, Осталось: {product.StockQuantity} шт");
            Console.WriteLine();
        }

        if (user != null)
        {
            Console.Write("1 - Добавить в корзину\n2 - Купить сразу\n3 - Назад\nВаш выбор: ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                AddToCart(user);
            }
            else if (choice == "2")
            {
                BuyProduct(user);
            }
        }
        else
        {
            Console.WriteLine("Для покупки необходимо войти в систему");
            Console.ReadKey();
        }
    }

    private void AddToCart(Users user)
    {
        Console.Write("ID товара: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Ошибка ввода");
            return;
        }

        Products product = Core.Context.Products.Find(productId);
        if (product == null)
        {
            Console.WriteLine("Товар не найден");
            return;
        }

        Console.Write("Количество: ");
        if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
        {
            Console.WriteLine("Неверное количество");
            return;
        }

        if (quantity > product.StockQuantity)
        {
            Console.WriteLine("Недостаточно товара на складе");
            return;
        }

        Cart cartItem = Core.Context.Cart.FirstOrDefault(c => c.UserId == user.UserId && c.ProductId == productId);

        if (cartItem != null)
        {
            if (cartItem.Quantity + quantity > product.StockQuantity)
            {
                Console.WriteLine("Недостаточно товара на складе");
                return;
            }
            cartItem.Quantity += quantity;
        }
        else
        {
            cartItem = new Cart
            {
                UserId = user.UserId,
                ProductId = productId,
                Quantity = quantity,
                AddedDate = DateTime.Now
            };
            Core.Context.Cart.Add(cartItem);
        }

        Core.Context.SaveChanges();
        Console.WriteLine("Товар добавлен в корзину");
    }

    private void BuyProduct(Users user)
    {
        Console.Write("ID товара: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Ошибка ввода");
            return;
        }

        Products product = Core.Context.Products.Find(productId);
        if (product == null)
        {
            Console.WriteLine("Товар не найден");
            return;
        }

        Console.Write("Количество: ");
        if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
        {
            Console.WriteLine("Неверное количество");
            return;
        }

        if (quantity > product.StockQuantity)
        {
            Console.WriteLine("Недостаточно товара на складе");
            return;
        }

        var points = Core.Context.PickupPoints.Where(p => p.IsActive == true).ToList();

        if (points.Count == 0)
        {
            Console.WriteLine("Нет доступных пунктов выдачи");
            return;
        }

        Console.WriteLine("\nДоступные пункты выдачи:");
        foreach (var point in points)
        {
            Console.WriteLine($"{point.PickupPointId}. {point.PointName} - {point.Address} ({point.PhoneNumber})");
        }

        Console.Write("Выберите пункт выдачи: ");
        if (!int.TryParse(Console.ReadLine(), out int pointId))
        {
            Console.WriteLine("Ошибка ввода");
            return;
        }

        decimal total = quantity * product.Price;

        Orders order = new Orders
        {
            UserId = user.UserId,
            PickupPointId = pointId,
            OrderDate = DateTime.Now,
            TotalAmount = total,
            Status = "Pending"
        };

        Core.Context.Orders.Add(order);
        Core.Context.SaveChanges();

        OrderItems orderItem = new OrderItems
        {
            OrderId = order.OrderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = product.Price
        };

        Core.Context.OrderItems.Add(orderItem);
        product.StockQuantity -= quantity;

        Core.Context.SaveChanges();

        Console.WriteLine($"\nЗаказ №{order.OrderId} оформлен");
        Console.WriteLine($"Товар: {product.ProductName} x{quantity}");
        Console.WriteLine($"Сумма: {total} руб");
        Console.WriteLine($"Статус: {order.Status}");
    }

    private void ShowCart(Users user)
    {
        Console.WriteLine("\nКорзина");

        var cartItems = from c in Core.Context.Cart
                        join p in Core.Context.Products on c.ProductId equals p.ProductId
                        where c.UserId == user.UserId
                        select new { Cart = c, Product = p };

        // Явное преобразование в список и проверка
        var cartItemsList = cartItems.ToList();
        if (!cartItemsList.Any())
        {
            Console.WriteLine("Корзина пуста");
            return;
        }

        decimal total = 0;

        Console.WriteLine("Товары в корзине:");
        foreach (var item in cartItemsList)
        {
            decimal itemTotal = item.Cart.Quantity * item.Product.Price;
            total += itemTotal;

            Console.WriteLine($"{item.Product.ProductName} x{item.Cart.Quantity} = {itemTotal} руб");
        }

        Console.WriteLine($"\nИтого: {total} руб");

        Console.Write("\n1 - Оформить заказ\n2 - Удалить товар\n3 - Назад\nВаш выбор: ");
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            CreateOrderFromCart(user);
        }
        else if (choice == "2")
        {
            RemoveFromCart(user);
        }
    }

    private void RemoveFromCart(Users user)
    {
        Console.Write("ID товара для удаления: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Ошибка ввода");
            return;
        }

        Cart item = Core.Context.Cart.FirstOrDefault(c => c.UserId == user.UserId && c.ProductId == productId);

        if (item != null)
        {
            Core.Context.Cart.Remove(item);
            Core.Context.SaveChanges();
            Console.WriteLine("Товар удален из корзины");
        }
        else
        {
            Console.WriteLine("Товар не найден в корзине");
        }
    }

    private void CreateOrderFromCart(Users user)
    {
        Console.WriteLine("\nОформление заказа из корзины");

        var cartItems = from c in Core.Context.Cart
                        join p in Core.Context.Products on c.ProductId equals p.ProductId
                        where c.UserId == user.UserId
                        select new { Cart = c, Product = p };

        // Явное преобразование и проверка
        var cartItemsList = cartItems.ToList();
        if (cartItemsList.Count == 0)
        {
            Console.WriteLine("Корзина пуста");
            return;
        }

        // Проверка наличия товаров
        bool hasInsufficientStock = false;
        foreach (var item in cartItemsList)
        {
            if (item.Cart.Quantity > item.Product.StockQuantity)
            {
                Console.WriteLine($"Недостаточно товара '{item.Product.ProductName}' на складе");
                hasInsufficientStock = true;
            }
        }

        if (hasInsufficientStock)
            return;

        var points = Core.Context.PickupPoints.Where(p => p.IsActive == true).ToList();

        if (points.Count == 0)
        {
            Console.WriteLine("Нет доступных пунктов выдачи");
            return;
        }

        Console.WriteLine("\nДоступные пункты выдачи:");
        foreach (var point in points)
        {
            Console.WriteLine($"{point.PickupPointId}. {point.PointName} - {point.Address} ({point.PhoneNumber})");
        }

        Console.Write("Выберите пункт выдачи: ");
        if (!int.TryParse(Console.ReadLine(), out int pointId))
        {
            Console.WriteLine("Ошибка ввода");
            return;
        }

        decimal total = cartItemsList.Sum(item => item.Cart.Quantity * item.Product.Price);

        Orders order = new Orders
        {
            UserId = user.UserId,
            PickupPointId = pointId,
            OrderDate = DateTime.Now,
            TotalAmount = total,
            Status = "ожидается"
        };

        Core.Context.Orders.Add(order);
        Core.Context.SaveChanges();

        foreach (var item in cartItemsList)
        {
            OrderItems orderItem = new OrderItems
            {
                OrderId = order.OrderId,
                ProductId = item.Product.ProductId,
                Quantity = item.Cart.Quantity,
                UnitPrice = item.Product.Price
            };

            Core.Context.OrderItems.Add(orderItem);
            item.Product.StockQuantity -= item.Cart.Quantity;
        }

        var userCart = Core.Context.Cart.Where(c => c.UserId == user.UserId).ToList();
        Core.Context.Cart.RemoveRange(userCart);

        Core.Context.SaveChanges();

        Console.WriteLine($"\nЗаказ №{order.OrderId} успешно оформлен");
        Console.WriteLine($"Сумма: {total} руб");
        Console.WriteLine($"Статус: {order.Status}");
    }

    private void ShowOrders(Users user)
    {
        Console.WriteLine("\nМои заказы");

        var orders = from o in Core.Context.Orders
                     join p in Core.Context.PickupPoints on o.PickupPointId equals p.PickupPointId
                     where o.UserId == user.UserId
                     orderby o.OrderDate descending
                     select new { Order = o, Point = p };

        // Явное преобразование
        var ordersList = orders.ToList();
        if (ordersList.Count == 0)
        {
            Console.WriteLine("Заказов нет");
            return;
        }

        foreach (var orderInfo in ordersList)
        {
            Console.WriteLine($"\nЗаказ №{orderInfo.Order.OrderId} от {orderInfo.Order.OrderDate:dd.MM.yyyy HH:mm}");
            Console.WriteLine($"Сумма: {orderInfo.Order.TotalAmount} руб");
            Console.WriteLine($"Пункт выдачи: {orderInfo.Point.PointName}");
            Console.WriteLine($"Адрес: {orderInfo.Point.Address}");
            Console.WriteLine($"Статус: {orderInfo.Order.Status}");

            var items = from oi in Core.Context.OrderItems
                        join p in Core.Context.Products on oi.ProductId equals p.ProductId
                        where oi.OrderId == orderInfo.Order.OrderId
                        select new { Item = oi, Product = p };

            // Явное преобразование
            var itemsList = items.ToList();
            Console.WriteLine("Состав заказа:");
            foreach (var item in itemsList)
            {
                Console.WriteLine($"  - {item.Product.ProductName} x{item.Item.Quantity} - {item.Item.UnitPrice} руб./шт");
            }
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Маркетплейс GMWOG");
            new MarketplaceGame().RunGame();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.ReadKey();
        }
    }
}


