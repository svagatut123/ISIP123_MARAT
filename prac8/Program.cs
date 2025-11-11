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
                default: Console.WriteLine("Неверный выбор!"); break;
            }
        }

        private void ShowUserMenu()
        {
            Console.WriteLine($"\n--- Личный кабинет ({currentUser.Username}) ---");
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
                default: Console.WriteLine("Неверный выбор!"); break;
            }
        }

        private void Register()
        {
            Console.WriteLine("\n--- Регистрация ---");

            Console.Write("Логин: ");
            string login = Console.ReadLine();

            if (Core.Context.Users.Any(u => u.Username == login))
            {
                Console.WriteLine("Этот логин уже занят!");
                return;
            }

            Console.Write("Email: ");
            string email = Console.ReadLine();

            if (Core.Context.Users.Any(u => u.Email == email))
            {
                Console.WriteLine("Этот email уже используется!");
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
                    Console.WriteLine("Пароли не совпадают! Попробуйте еще раз.");
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

            Console.WriteLine("Регистрация успешна!");
        }
    private void Login()
    {
        Console.WriteLine("\n--- Вход ---");

        Console.Write("Логин: ");
        string login = Console.ReadLine();

        Console.Write("Пароль: ");
        string password = Console.ReadLine();

        Users user = Core.Context.Users.FirstOrDefault(u => u.Username == login);

        if (user != null && password == user.PasswordHash)
        {
            currentUser = user;
            Console.WriteLine($"Добро пожаловать, {user.FullName}!");
        }
        else
        {
            Console.WriteLine("Неверный логин или пароль!");
        }
    }

    private void ShowProducts(Users user = null)
    {
        Console.WriteLine("\n--- Товары ---");

        var products = Core.Context.Products.ToList();

        if (products.Count == 0)
        {
            Console.WriteLine("Товаров нет");
            return;
        }

        foreach (var product in products)
        {
            var category = Core.Context.Categories.Find(product.CategoryId);
            Console.WriteLine($"{product.ProductId}. {product.ProductName} - {product.Price} руб.");
            Console.WriteLine($"   {product.Description}");
            Console.WriteLine($"   Категория: {category?.CategoryName}, Осталось: {product.StockQuantity} шт.");
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
            Console.WriteLine("Для покупки необходимо войти в систему.");
            Console.ReadKey();
        }
    }

    private void AddToCart(Users user)
    {
        Console.Write("ID товара: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Ошибка ввода!");
            return;
        }

        Products product = Core.Context.Products.Find(productId);
        if (product == null)
        {
            Console.WriteLine("Товар не найден!");
            return;
        }

        Console.Write("Количество: ");
        if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
        {
            Console.WriteLine("Неверное количество!");
            return;
        }

        if (quantity > product.StockQuantity)
        {
            Console.WriteLine("Недостаточно товара на складе!");
            return;
        }

        Cart cartItem = Core.Context.Cart.FirstOrDefault(c => c.UserId == user.UserId && c.ProductId == productId);

        if (cartItem != null)
        {
            if (cartItem.Quantity + quantity > product.StockQuantity)
            {
                Console.WriteLine("Недостаточно товара на складе!");
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
        Console.WriteLine("Товар добавлен в корзину!");
    }

    private void BuyProduct(Users user)
    {
        Console.Write("ID товара: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            Console.WriteLine("Ошибка ввода!");
            return;
        }

        Products product = Core.Context.Products.Find(productId);
        if (product == null)
        {
            Console.WriteLine("Товар не найден!");
            return;
        }

        Console.Write("Количество: ");
        if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
        {
            Console.WriteLine("Неверное количество!");
            return;
        }

        if (quantity > product.StockQuantity)
        {
            Console.WriteLine("Недостаточно товара на складе!");
            return;
        }

        var points = Core.Context.PickupPoints.Where(p => p.IsActive == true).ToList();

        if (points.Count == 0)
        {
            Console.WriteLine("Нет доступных пунктов выдачи!");
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
            Console.WriteLine("Ошибка ввода!");
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

        Console.WriteLine($"\nЗаказ №{order.OrderId} успешно оформлен!");
        Console.WriteLine($"Товар: {product.ProductName} x{quantity}");
        Console.WriteLine($"Сумма: {total} руб.");
        Console.WriteLine($"Статус: {order.Status}");
    }

