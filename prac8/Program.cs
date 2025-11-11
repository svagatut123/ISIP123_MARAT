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

