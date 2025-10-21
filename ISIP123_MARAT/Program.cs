//// Главные сущности (отвечаем на вопрос 1)
//public class AutoService { }
//public class SparePart { }
//public class Warehouse { }
//public class WarehouseItem { }
//public class Client { }
//public class Car { }
//public class RepairOrder { }
//public class SupplyOrder { }
//public class Game { }

//// Основные операции (отвечаем на вопрос 2)
//public partial class AutoService
//{
//    public void AcceptRepairOrder(RepairOrder order) { }      // ГЛАГОЛ: "принять"
//    public void DeclineRepairOrder(RepairOrder order) { }    // ГЛАГОЛ: "отклонить" 
//    public void PurchaseParts() { }                          // ГЛАГОЛ: "купить"
//    private void ProcessDeliveries() { }                     // ГЛАГОЛ: "обработать"
//}

//// Группировка данных (отвечаем на вопрос 3)
//public class WarehouseItem
//{
//    public SparePart Part { get; set; }      // Данные ВСЕГДА вместе: деталь...
//    public int Quantity { get; set; }        // ...и её количество
//}

//public class RepairOrder
//{
//    public Client Client { get; set; }       // Данные ВСЕГДА вместе: клиент...
//    public RepairOrderStatus Status { get; set; } // ...статус...
//    public decimal Profit { get; set; }      // ...и финансовый результат
//}

//// Повторяющаяся логика (отвечаем на вопрос 5)
//public class Warehouse
//{
//    public bool IsPartAvailable(SparePart part) { }  // ПОВТОРЯЕТСЯ: проверка наличия
//    public void AddPart(SparePart part, int quantity) { } // ПОВТОРЯЕТСЯ: добавление
//    public bool RemovePart(SparePart part) { }       // ПОВТОРЯЕТСЯ: удаление
//}
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoServiceSimulator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== АВТОСЕРВИС ===");

            // Создаем автосервис с начальным балансом
            var autoService = new AutoService("Мой Автосервис", 10000m);
            autoService.InitializeStartingParts();

            // Запускаем игровой цикл
            var game = new Game(autoService);
            game.Start();
        }
    }

    public enum RepairOrderStatus
    {
        Pending,    // Заказ создан, ожидает решения
        InProgress, // Принят в работу
        Completed,  // Успешно завершен
        Failed,     // Завершен неудачно (поставили не ту деталь)
        Declined    // Отклонен
    }

    public class SparePart
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellPrice { get; set; }

        public SparePart(int id, string name, decimal purchasePrice, decimal marginPercent = 50)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название детали не может быть пустым");
            if (purchasePrice <= 0)
                throw new ArgumentException("Цена покупки должна быть положительной");
            if (marginPercent < 0)
                throw new ArgumentException("Наценка не может быть отрицательной");

            Id = id;
            Name = name;
            PurchasePrice = purchasePrice;
            SellPrice = purchasePrice * (1 + marginPercent / 100);
        }

        public override string ToString()
        {
            return $"{Name} (Покупка: {PurchasePrice:C}, Ремонт: {SellPrice:C})";
        }
    }

    public class WarehouseItem
    {
        public SparePart Part { get; set; }
        public int Quantity { get; set; }

        public WarehouseItem(SparePart part, int quantity)
        {
            Part = part ?? throw new ArgumentNullException(nameof(part));
            if (quantity < 0)
                throw new ArgumentException("Количество не может быть отрицательным");

            Part = part;
            Quantity = quantity;
        }

        public override string ToString()
        {
            return $"{Part.Name}: {Quantity} шт.";
        }
    }

    public class Warehouse
    {
        public List<WarehouseItem> Items { get; set; }

        public Warehouse()
        {
            Items = new List<WarehouseItem>();
        }

        public bool IsPartAvailable(SparePart part)
        {
            var item = FindItem(part);
            return item != null && item.Quantity > 0;
        }

        public WarehouseItem FindItem(SparePart part)
        {
            return Items.FirstOrDefault(i => i.Part.Id == part.Id);
        }

        public void AddPart(SparePart part, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Количество должно быть положительным");

            var existingItem = FindItem(part);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                Items.Add(new WarehouseItem(part, quantity));
            }
        }

        public bool RemovePart(SparePart part)
        {
            var item = FindItem(part);
            if (item != null && item.Quantity > 0)
            {
                item.Quantity--;
                return true;
            }
            return false;
        }

        public SparePart GetRandomAvailablePart()
        {
            var availableParts = Items.Where(i => i.Quantity > 0).ToList();
            if (availableParts.Count == 0)
                return null;

            var random = new Random();
            return availableParts[random.Next(availableParts.Count)].Part;
        }

        public void DisplayStock()
        {
            Console.WriteLine("\n=== СКЛАД ===");
            if (Items.Count == 0 || Items.All(i => i.Quantity == 0))
            {
                Console.WriteLine("Склад пуст");
                return;
            }

            foreach (var item in Items.Where(i => i.Quantity > 0))
            {
                Console.WriteLine($"- {item.Part.Name}: {item.Quantity} шт.");
            }
        }
    }

    public class Client
    {
        private static int _nextId = 1;
        public int Id { get; set; }
        public string Name { get; set; }
        public Car Car { get; set; }

        public Client(string name, Car car)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя клиента не может быть пустым");

            Id = _nextId++;
            Name = name;
            Car = car ?? throw new ArgumentNullException(nameof(car));
        }

        public override string ToString()
        {
            return $"{Name} ({Car.Model})";
        }
    }

    public class Car
    {
        public string Model { get; set; }
        public SparePart BrokenPart { get; set; }

        public Car(string model, SparePart brokenPart)
        {
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Модель автомобиля не может быть пустой");

            Model = model;
            BrokenPart = brokenPart ?? throw new ArgumentNullException(nameof(brokenPart));
        }

        public override string ToString()
        {
            return $"{Model} (сломано: {BrokenPart.Name})";
        }
    }

    public class RepairOrder
    {
        private static int _nextId = 1;
        public int Id { get; set; }
        public Client Client { get; set; }
        public RepairOrderStatus Status { get; set; }
        public decimal Profit { get; set; }

        public RepairOrder(Client client)
        {
            Id = _nextId++;
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Status = RepairOrderStatus.Pending;
            Profit = 0;
        }

        public decimal CalculateRepairCost()
        {
            return Client.Car.BrokenPart.SellPrice;
        }

        public void CompleteSuccessfully()
        {
            Status = RepairOrderStatus.Completed;
            Profit = CalculateRepairCost();
        }

        public void CompleteWithFailure(SparePart wrongPartUsed)
        {
            Status = RepairOrderStatus.Failed;
            // Штраф: стоимость ремонта + компенсация + стоимость неправильно использованной детали
            Profit = -CalculateRepairCost() * 2 - wrongPartUsed.PurchasePrice;
        }

        public void Decline()
        {
            Status = RepairOrderStatus.Declined;
            Profit = -100; // Штраф за отказ
        }

        public override string ToString()
        {
            var statusText = Status switch
            {
                RepairOrderStatus.Pending => "Ожидает",
                RepairOrderStatus.InProgress => "В работе",
                RepairOrderStatus.Completed => "Завершен",
                RepairOrderStatus.Failed => "Провален",
                RepairOrderStatus.Declined => "Отклонен",
                _ => "Неизвестен"
            };

            return $"Заказ #{Id}: {Client} - {statusText} ({Profit:C})";
        }
    }
    public class SupplyOrder
    {
        private static int _nextId = 1;
        public int Id { get; set; }
        public Dictionary<SparePart, int> OrderedParts { get; set; }
        public decimal TotalCost { get; set; }
        public int CarsUntilDelivery { get; set; }

        public SupplyOrder(Dictionary<SparePart, int> orderedParts, decimal totalCost)
        {
            if (orderedParts == null || orderedParts.Count == 0)
                throw new ArgumentException("Заказ должен содержать детали");
            if (totalCost <= 0)
                throw new ArgumentException("Стоимость заказа должна быть положительной");

            Id = _nextId++;
            OrderedParts = orderedParts;
            TotalCost = totalCost;
            CarsUntilDelivery = 2;
        }

        public void DecrementDeliveryCounter()
        {
            if (CarsUntilDelivery > 0)
                CarsUntilDelivery--;
        }

        public bool IsReadyForDelivery()
        {
            return CarsUntilDelivery <= 0;
        }

        public override string ToString()
        {
            var parts = string.Join(", ", OrderedParts.Select(p => $"{p.Key.Name} x{p.Value}"));
            return $"Заказ #{Id}: {parts} - доставка через {CarsUntilDelivery} машин";
        }
    }

    public class AutoService
    {
        public string Name { get; set; }
        public decimal Balance { get; private set; }
        public Warehouse Warehouse { get; set; }
        public List<RepairOrder> RepairOrders { get; set; }
        public List<SupplyOrder> SupplyOrders { get; set; }
        public List<SparePart> AvailablePartTypes { get; set; }
        public int TotalCarsProcessed { get; set; }
        private Random _random;

        public AutoService(string name, decimal initialBalance)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название сервиса не может быть пустым");
            if (initialBalance < 0)
                throw new ArgumentException("Начальный баланс не может быть отрицательным");

            Name = name;
            Balance = initialBalance;
            Warehouse = new Warehouse();
            RepairOrders = new List<RepairOrder>();
            SupplyOrders = new List<SupplyOrder>();
            AvailablePartTypes = new List<SparePart>();
            TotalCarsProcessed = 0;
            _random = new Random();
        }

        public void InitializeStartingParts()
        {
            // Создаем каталог доступных деталей
            AvailablePartTypes.AddRange(new[]
            {
                new SparePart(1, "Тормозные колодки", 2000, 60),
                new SparePart(2, "Масляный фильтр", 500, 50),
                new SparePart(3, "Воздушный фильтр", 800, 50),
                new SparePart(4, "Свечи зажигания", 1200, 55),
                new SparePart(5, "Аккумулятор", 5000, 40),
                new SparePart(6, "Шины", 4000, 35),
                new SparePart(7, "Тормозные диски", 3500, 45),
                new SparePart(8, "Амортизаторы", 6000, 50)
            });

            // Начальный склад
            Warehouse.AddPart(AvailablePartTypes[0], 2); // Тормозные колодки
            Warehouse.AddPart(AvailablePartTypes[1], 3); // Масляный фильтр
            Warehouse.AddPart(AvailablePartTypes[2], 2); // Воздушный фильтр
        }

        private void UpdateBalance(decimal amount)
        {
            Balance += amount;
            if (Balance < 0)
            {
                Balance = 0; // Баланс не может быть отрицательным
            }
        }

        public Client GenerateRandomClient()
        {
            var carModels = new[]
            {
                "Toyota Camry", "Honda Civic", "BMW X5", "Mercedes C-Class",
                "Ford Focus", "Hyundai Solaris", "Kia Rio", "Lada Vesta"
            };

            var randomModel = carModels[_random.Next(carModels.Length)];
            var randomPart = AvailablePartTypes[_random.Next(AvailablePartTypes.Count)];
            var car = new Car(randomModel, randomPart);

            var clientNames = new[]
            {
                "Иван Петров", "Мария Сидорова", "Алексей Козлов", "Екатерина Новикова",
                "Дмитрий Волков", "Ольга Орлова", "Сергей Морозов", "Анна Павлова"
            };

            var randomName = clientNames[_random.Next(clientNames.Length)];

            return new Client(randomName, car);
        }

        public bool AcceptRepairOrder(RepairOrder order)
        {
            if (order.Status != RepairOrderStatus.Pending)
                return false;

            order.Status = RepairOrderStatus.InProgress;
            var brokenPart = order.Client.Car.BrokenPart;

            if (Warehouse.IsPartAvailable(brokenPart))
            {
                // Есть нужная деталь - успешный ремонт
                Warehouse.RemovePart(brokenPart);
                order.CompleteSuccessfully();
                UpdateBalance(order.Profit);
                Console.WriteLine($"✅ Ремонт завершен успешно! Заработано: {order.Profit:C}");
            }
            else
            {
                // Нет нужной детали - ставим случайную (неудачный ремонт)
                var wrongPart = Warehouse.GetRandomAvailablePart();
                if (wrongPart != null)
                {
                    Warehouse.RemovePart(wrongPart);
                    order.CompleteWithFailure(wrongPart);
                    UpdateBalance(order.Profit);
                    Console.WriteLine($"❌ КРИТИЧЕСКАЯ ОШИБКА! Установлена не та деталь.");
                    Console.WriteLine($"Убыток: {order.Profit:C}");
                }
                else
                {
                    // Нет вообще никаких деталей - автоматический отказ
                    order.Decline();
                    UpdateBalance(order.Profit);
                    Console.WriteLine($"⚠️ Нет деталей для ремонта. Заказ отклонен. Штраф: {-order.Profit:C}");
                }
            }

            RepairOrders.Add(order);
            TotalCarsProcessed++;
            ProcessDeliveries();
            return true;
        }

        public bool DeclineRepairOrder(RepairOrder order)
        {
            if (order.Status != RepairOrderStatus.Pending)
                return false;

            order.Decline();
            UpdateBalance(order.Profit);
            RepairOrders.Add(order);
            TotalCarsProcessed++;
            Console.WriteLine($"⚠️ Заказ отклонен. Штраф: {-order.Profit:C}");

            ProcessDeliveries();
            return true;
        }

        public void ShowPartsCatalog()
        {
            Console.WriteLine("\n=== КАТАЛОГ ДЕТАЛЕЙ ===");
            for (int i = 0; i < AvailablePartTypes.Count; i++)
            {
                var part = AvailablePartTypes[i];
                Console.WriteLine($"{i + 1}. {part}");
            }
        }

        public void PurchaseParts()
        {
            ShowPartsCatalog();
            Console.WriteLine($"\nВаш баланс: {Balance:C}");

            var orderedParts = new Dictionary<SparePart, int>();
            decimal totalCost = 0;

            while (true)
            {
                Console.Write("\nВведите номер детали для заказа (0 - завершить): ");
                if (!int.TryParse(Console.ReadLine(), out int partIndex) || partIndex < 0 || partIndex > AvailablePartTypes.Count)
                {
                    Console.WriteLine("Неверный номер детали!");
                    continue;
                }

                if (partIndex == 0)
                    break;

                var selectedPart = AvailablePartTypes[partIndex - 1];

                Console.Write($"Введите количество (доступно средств: {(Balance - totalCost):C}): ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("Неверное количество!");
                    continue;
                }

                var cost = selectedPart.PurchasePrice * quantity;
                if (totalCost + cost > Balance)
                {
                    Console.WriteLine("Недостаточно средств!");
                    continue;
                }

                if (orderedParts.ContainsKey(selectedPart))
                {
                    orderedParts[selectedPart] += quantity;
                }
                else
                {
                    orderedParts[selectedPart] = quantity;
                }

                totalCost += cost;
                Console.WriteLine($"Добавлено: {selectedPart.Name} x{quantity} = {cost:C}");
                Console.WriteLine($"Общая стоимость заказа: {totalCost:C}");
            }

            if (orderedParts.Count > 0)
            {
                var supplyOrder = new SupplyOrder(orderedParts, totalCost);
                SupplyOrders.Add(supplyOrder);
                UpdateBalance(-totalCost);
                Console.WriteLine($"\n✅ Заказ #{supplyOrder.Id} создан! Доставка через 2 машины.");
                Console.WriteLine($"Списано: {totalCost:C}, Баланс: {Balance:C}");
            }
            else
            {
                Console.WriteLine("Заказ не создан.");
            }
        }

        private void ProcessDeliveries()
        {
            foreach (var order in SupplyOrders.ToList())
            {
                order.DecrementDeliveryCounter();

                if (order.IsReadyForDelivery())
                {
                    // Доставляем детали на склад
                    foreach (var (part, quantity) in order.OrderedParts)
                    {
                        Warehouse.AddPart(part, quantity);
                    }

                    SupplyOrders.Remove(order);
                    Console.WriteLine($"\n📦 Доставлен заказ #{order.Id}!");
                    foreach (var (part, quantity) in order.OrderedParts)
                    {
                        Console.WriteLine($"   + {part.Name} x{quantity}");
                    }
                }
            }
        }

        public void ShowStatus()
        {
            Console.WriteLine($"\n=== {Name.ToUpper()} ===");
            Console.WriteLine($"Баланс: {Balance:C}");
            Console.WriteLine($"Обработано машин: {TotalCarsProcessed}");
            Console.WriteLine($"Активных заказов на поставку: {SupplyOrders.Count}");

            Warehouse.DisplayStock();

            if (SupplyOrders.Count > 0)
            {
                Console.WriteLine("\n=== ЗАКАЗЫ НА ПОСТАВКУ ===");
                foreach (var order in SupplyOrders)
                {
                    Console.WriteLine($"- {order}");
                }
            }
        }

        public void ShowOrderHistory()
        {
            Console.WriteLine("\n=== ИСТОРИЯ ЗАКАЗОВ ===");
            if (RepairOrders.Count == 0)
            {
                Console.WriteLine("Заказов пока нет");
                return;
            }

            foreach (var order in RepairOrders.TakeLast(10)) // Последние 10 заказов
            {
                Console.WriteLine($"- {order}");
            }

            var totalProfit = RepairOrders.Sum(o => o.Profit);
            var successfulOrders = RepairOrders.Count(o => o.Status == RepairOrderStatus.Completed);
            var failedOrders = RepairOrders.Count(o => o.Status == RepairOrderStatus.Failed);

            Console.WriteLine($"\nИтого: {RepairOrders.Count} заказов");
            Console.WriteLine($"Успешных: {successfulOrders}, Неудачных: {failedOrders}");
            Console.WriteLine($"Общая прибыль: {totalProfit:C}");
        }
    }

    public class Game
    {
        private AutoService _autoService;
        private bool _isRunning;

        public Game(AutoService autoService)
        {
            _autoService = autoService;
            _isRunning = true;
        }

        public void Start()
        {
            Console.WriteLine($"Добро пожаловать в {_autoService.Name}!");
            Console.WriteLine("Ваша задача - ремонтировать автомобили и зарабатывать деньги.");
            Console.WriteLine("Будьте осторожны: неправильный ремонт приведет к убыткам!\n");

            while (_isRunning && _autoService.Balance > 0)
            {
                ProcessNextClient();

                if (_autoService.Balance <= 0)
                {
                    Console.WriteLine("\n💸 ВЫ БАНКРОТ! Игра окончена.");
                    break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }

            ShowFinalStats();
        }

        private void ProcessNextClient()
        {
            Console.Clear();
            _autoService.ShowStatus();

            // Создаем нового клиента
            var client = _autoService.GenerateRandomClient();
            var order = new RepairOrder(client);

            Console.WriteLine($"\n=== НОВЫЙ КЛИЕНТ ===");
            Console.WriteLine($"Клиент: {client}");
            Console.WriteLine($"Поломка: {client.Car.BrokenPart.Name}");
            Console.WriteLine($"Стоимость ремонта: {order.CalculateRepairCost():C}");

            // Проверяем наличие детали
            var isPartAvailable = _autoService.Warehouse.IsPartAvailable(client.Car.BrokenPart);
            Console.WriteLine($"Деталь на складе: {(isPartAvailable ? "✅ ЕСТЬ" : "❌ НЕТ")}");

            ShowMenu(isPartAvailable);

            var choice = GetUserChoice(1, 4);
            ProcessMenuChoice(choice, order);
        }

        private void ShowMenu(bool isPartAvailable)
        {
            Console.WriteLine("\n=== ВАШИ ДЕЙСТВИЯ ===");
            Console.WriteLine("1. Принять заказ и выполнить ремонт");

            if (isPartAvailable)
            {
                Console.WriteLine("2. Отклонить заказ (штраф 100 руб)");
            }
            else
            {
                Console.WriteLine("2. Отклонить заказ (штраф 100 руб) - РЕКОМЕНДУЕТСЯ!");
            }

            Console.WriteLine("3. Заказать запчасти");
            Console.WriteLine("4. Показать историю заказов");
        }

        private int GetUserChoice(int min, int max)
        {
            while (true)
            {
                Console.Write($"\nВыберите действие ({min}-{max}): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max)
                {
                    return choice;
                }
                Console.WriteLine("Неверный выбор! Попробуйте снова.");
            }
        }

        private void ProcessMenuChoice(int choice, RepairOrder order)
        {
            switch (choice)
            {
                case 1:
                    _autoService.AcceptRepairOrder(order);
                    break;

                case 2:
                    _autoService.DeclineRepairOrder(order);
                    break;

                case 3:
                    _autoService.PurchaseParts();
                    break;

                case 4:
                    _autoService.ShowOrderHistory();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    ProcessNextClient(); // Возвращаемся к тому же клиенту
                    break;
            }
        }

        private void ShowFinalStats()
        {
            Console.WriteLine("\n=== ИТОГИ ИГРЫ ===");
            Console.WriteLine($"Обработано машин: {_autoService.TotalCarsProcessed}");

            var totalProfit = _autoService.RepairOrders.Sum(o => o.Profit);
            Console.WriteLine($"Общая прибыль: {totalProfit:C}");

            var successfulOrders = _autoService.RepairOrders.Count(o => o.Status == RepairOrderStatus.Completed);
            var failedOrders = _autoService.RepairOrders.Count(o => o.Status == RepairOrderStatus.Failed);
            var declinedOrders = _autoService.RepairOrders.Count(o => o.Status == RepairOrderStatus.Declined);

            Console.WriteLine($"Успешных ремонтов: {successfulOrders}");
            Console.WriteLine($"Неудачных ремонтов: {failedOrders}");
            Console.WriteLine($"Отклоненных заказов: {declinedOrders}");

            if (successfulOrders > 0)
            {
                var averageProfit = totalProfit / _autoService.RepairOrders.Count;
                Console.WriteLine($"Средняя прибыль на заказ: {averageProfit:C}");
            }
        }
    }
}


//--Создание базы данных
//CREATE DATABASE AutoServiceDB;
//GO

//USE AutoServiceDB;
//GO

//-- Таблица запчастей
//CREATE TABLE SpareParts (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    Name NVARCHAR(100) NOT NULL,
//    PurchasePrice DECIMAL(10,2) NOT NULL,
//    SellPrice DECIMAL(10,2) NOT NULL,
//    CreatedDate DATETIME2 DEFAULT GETDATE()
//);

//--Таблица клиентов
//CREATE TABLE Clients (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    Name NVARCHAR(100) NOT NULL,
//    CarModel NVARCHAR(100) NOT NULL,
//    CreatedDate DATETIME2 DEFAULT GETDATE()
//);

//--Таблица заказов на ремонт
//CREATE TABLE RepairOrders (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    ClientId INT NOT NULL,
//    BrokenPartId INT NOT NULL,
//    Status INT NOT NULL, -- 0=Pending, 1=InProgress, 2=Completed, 3=Failed, 4=Declined
//    Profit DECIMAL(10,2) NOT NULL,
//    CreatedDate DATETIME2 DEFAULT GETDATE(),
//    FOREIGN KEY (ClientId) REFERENCES Clients(Id),
//    FOREIGN KEY (BrokenPartId) REFERENCES SpareParts(Id)
//);

//--Таблица склада
//CREATE TABLE Warehouse (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    SparePartId INT NOT NULL,
//    Quantity INT NOT NULL,
//    LastUpdated DATETIME2 DEFAULT GETDATE(),
//    FOREIGN KEY (SparePartId) REFERENCES SpareParts(Id)
//);

//--Таблица заказов на поставку
//CREATE TABLE SupplyOrders (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    TotalCost DECIMAL(10,2) NOT NULL,
//    CarsUntilDelivery INT NOT NULL,
//    CreatedDate DATETIME2 DEFAULT GETDATE()
//);

//--Таблица деталей в заказах на поставку
//CREATE TABLE SupplyOrderItems (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    SupplyOrderId INT NOT NULL,
//    SparePartId INT NOT NULL,
//    Quantity INT NOT NULL,
//    FOREIGN KEY (SupplyOrderId) REFERENCES SupplyOrders(Id),
//    FOREIGN KEY (SparePartId) REFERENCES SpareParts(Id)
//);

//--Вставка начальных данных
//INSERT INTO SpareParts (Name, PurchasePrice, SellPrice) VALUES
//('Тормозные колодки', 2000.00, 3200.00),
//('Масляный фильтр', 500.00, 750.00),
//('Воздушный фильтр', 800.00, 1200.00),
//('Свечи зажигания', 1200.00, 1860.00),
//('Аккумулятор', 5000.00, 7000.00),
//('Шины', 4000.00, 5400.00),
//('Тормозные диски', 3500.00, 5075.00),
//('Амортизаторы', 6000.00, 9000.00);

//INSERT INTO Warehouse (SparePartId, Quantity) VALUES
//(1, 2), (2, 3), (3, 2);

//namespace AutoServiceSimulator
//{
//    /// <summary>
//    /// Класс для работы с базой данных
//    /// </summary>
//    public class DatabaseService
//    {
//        private string _connectionString;

//        public DatabaseService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        // Методы для работы с базой данных будут здесь
//        // В реальном приложении нужно реализовать:
//        // - Сохранение состояния игры
//        // - Загрузка состояния игры
//        // - Логирование всех операций
//    }
//}