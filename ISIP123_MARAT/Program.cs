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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

public class SparePart
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellPrice { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class Client
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string CarModel { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class RepairOrder
{
    [Key]
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int BrokenPartId { get; set; }
    public int Status { get; set; } // 0-отказ, 1-успех, 2-ошибка, 3-в процессе, 4-доставка
    public decimal Profit { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    [ForeignKey("ClientId")]
    public virtual Client Client { get; set; }

    [ForeignKey("BrokenPartId")]
    public virtual SparePart BrokenPart { get; set; }
}

public class Warehouse
{
    [Key]
    public int Id { get; set; }
    public int SparePartId { get; set; }
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; }

    [ForeignKey("SparePartId")]
    public virtual SparePart SparePart { get; set; }
}

public class SupplyOrder
{
    [Key]
    public int Id { get; set; }
    public decimal TotalCost { get; set; }
    public int CarsUntilDelivery { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsDelivered { get; set; }

    public virtual ICollection<SupplyOrderItem> SupplyOrderItems { get; set; }
}

public class SupplyOrderItem
{
    [Key]
    public int Id { get; set; }
    public int SupplyOrderId { get; set; }
    public int SparePartId { get; set; }
    public int Quantity { get; set; }

    [ForeignKey("SupplyOrderId")]
    public virtual SupplyOrder SupplyOrder { get; set; }

    [ForeignKey("SparePartId")]
    public virtual SparePart SparePart { get; set; }
}

public class GameState
{
    [Key]
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public int TotalCarsProcessed { get; set; }
    public string ServiceName { get; set; }
    public DateTime LastUpdated { get; set; }
}

// Контекст базы данных
public class AutoServiceDbContext : DbContext
{
    public AutoServiceDbContext() : base("name=AutoServiceConnection")
    {
    }

    public DbSet<SparePart> SpareParts { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<RepairOrder> RepairOrders { get; set; }
    public DbSet<Warehouse> Warehouse { get; set; }
    public DbSet<SupplyOrder> SupplyOrders { get; set; }
    public DbSet<SupplyOrderItem> SupplyOrderItems { get; set; }
    public DbSet<GameState> GameStates { get; set; }
}

// Класс для работы с БД
public static class Core
{
    public static AutoServiceDbContext Context = new AutoServiceDbContext();
}

// Основной класс игры
public class AutoServiceGame
{
    private Random random;
    private int clientCounter;

    public AutoServiceGame()
    {
        random = new Random();
        InitializeGame();
    }

    private void InitializeGame()
    {
        // Загрузка состояния игры из БД
        var gameState = Core.Context.GameStates.Find(1);
        if (gameState == null)
        {
            throw new Exception("Игра не инициализирована в базе данных. Запустите скрипт создания БД.");
        }
        clientCounter = gameState.TotalCarsProcessed;
    }

    private decimal Balance
    {
        get { return Core.Context.GameStates.Find(1).Balance; }
        set
        {
            var gameState = Core.Context.GameStates.Find(1);
            gameState.Balance = value;
            gameState.LastUpdated = DateTime.Now;
            Core.Context.SaveChanges();
        }
    }

    public void StartGame()
    {
        Console.WriteLine($"Добро пожаловать в {Core.Context.GameStates.Find(1).ServiceName}!");
        Console.WriteLine($"Ваш баланс: {Balance:C}");
        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();

        while (true)
        {
            Console.Clear();
            ShowStatus();

            // Обработка ожидающих заказов
            ProcessPendingOrders();

            // Прибытие нового клиента
            var client = GenerateClient();
            Console.WriteLine($"\nПриехал клиент #{clientCounter + 1}: {client.Name}");
            Console.WriteLine($"Марка автомобиля: {client.CarModel}");

            var brokenPart = GetRandomSparePart();
            var repairCost = brokenPart.SellPrice;

            Console.WriteLine($"Сломана деталь: {brokenPart.Name}");
            Console.WriteLine($"Стоимость ремонта: {repairCost:C}");

            // Проверка наличия детали на складе
            var warehouseItem = Core.Context.Warehouse.FirstOrDefault(w => w.SparePartId == brokenPart.Id && w.Quantity > 0);

            Console.WriteLine("\nВаши действия:");
            Console.WriteLine("1 - Принять заказ и починить машину");
            Console.WriteLine("2 - Отказать в обслуживании");
            Console.WriteLine("3 - Перейти в меню закупки");
            Console.WriteLine("4 - Показать склад");
            Console.WriteLine("5 - Показать статистику");
            Console.WriteLine("6 - Выйти из игры");

            var choice = GetUserChoice(1, 6);

            switch (choice)
            {
                case 1:
                    AcceptOrder(client, brokenPart, repairCost, warehouseItem);
                    break;
                case 2:
                    RefuseOrder(client, brokenPart, repairCost);
                    break;
                case 3:
                    ShowPurchaseMenu();
                    break;
                case 4:
                    ShowWarehouse();
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    break;
                case 5:
                    ShowStatistics();
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    break;
                case 6:
                    Console.WriteLine("Игра сохранена. До свидания!");
                    return;
            }

            UpdateGameState();
        }
    }

    private Client GenerateClient()
    {
        var names = new[] { "Иванов", "Петров", "Сидоров", "Кузнецов", "Смирнов", "Попов", "Васильев", "Фёдоров" };
        var carModels = new[] { "Toyota Camry", "Honda Civic", "BMW X5", "Mercedes C-Class", "Lada Vesta", "Kia Rio", "Hyundai Solaris", "Volkswagen Polo" };

        var client = new Client
        {
            Name = names[random.Next(names.Length)],
            CarModel = carModels[random.Next(carModels.Length)],
            CreatedDate = DateTime.Now
        };

        Core.Context.Clients.Add(client);
        Core.Context.SaveChanges();

        return client;
    }

    private SparePart GetRandomSparePart()
    {
        var parts = Core.Context.SpareParts.ToList();
        return parts[random.Next(parts.Count)];
    }

    private void AcceptOrder(Client client, SparePart brokenPart, decimal repairCost, Warehouse warehouseItem)
    {
        var repairOrder = new RepairOrder
        {
            ClientId = client.Id,
            BrokenPartId = brokenPart.Id,
            CreatedDate = DateTime.Now,
            Status = 3 // в процессе
        };

        if (warehouseItem != null)
        {
            // Успешный ремонт
            warehouseItem.Quantity--;
            warehouseItem.LastUpdated = DateTime.Now;

            var profit = repairCost - brokenPart.PurchasePrice;
            Balance += profit;

            repairOrder.Status = 1; // успех
            repairOrder.Profit = profit;
            repairOrder.CompletedDate = DateTime.Now;

            Console.WriteLine($"\n✅ Вы успешно заменили {brokenPart.Name}!");
            Console.WriteLine($"💰 Прибыль: {profit:C}");
        }
        else
        {
            // Неудачный ремонт - замена случайной деталью
            Console.WriteLine("\n❌ На складе нет нужной детали! Производится замена случайной деталью...");

            var randomAvailableItem = Core.Context.Warehouse.Where(w => w.Quantity > 0).OrderBy(x => random.Next()).FirstOrDefault();

            if (randomAvailableItem != null)
            {
                randomAvailableItem.Quantity--;
                randomAvailableItem.LastUpdated = DateTime.Now;

                var penalty = repairCost * 1.5m; // Штраф 150%
                Balance -= penalty;

                var randomPart = Core.Context.SpareParts.Find(randomAvailableItem.SparePartId);

                repairOrder.Status = 2; // ошибка
                repairOrder.Profit = -penalty;

                Console.WriteLine($"Клиент недоволен! Вы поставили {randomPart.Name} вместо {brokenPart.Name}");
                Console.WriteLine($"Штраф: {penalty:C}");
            }
            else
            {
                Console.WriteLine("На складе совсем нет деталей! Штраф увеличен.");
                var penalty = repairCost * 2m;
                Balance -= penalty;

                repairOrder.Status = 2; // ошибка
                repairOrder.Profit = -penalty;
                Console.WriteLine($"Штраф: {penalty:C}");
            }
        }

        Core.Context.RepairOrders.Add(repairOrder);
        Core.Context.SaveChanges();
        clientCounter++;

        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }

    private void RefuseOrder(Client client, SparePart brokenPart, decimal repairCost)
    {
        var penalty = repairCost * 0.2m; // Штраф 20% за отказ
        Balance -= penalty;

        var repairOrder = new RepairOrder
        {
            ClientId = client.Id,
            BrokenPartId = brokenPart.Id,
            Status = 0, // отказ
            Profit = -penalty,
            CreatedDate = DateTime.Now,
            CompletedDate = DateTime.Now
        };

        Core.Context.RepairOrders.Add(repairOrder);
        Core.Context.SaveChanges();
        clientCounter++;

        Console.WriteLine($"\n Вы отказали в обслуживании.");
        Console.WriteLine($"Штраф за отказ: {penalty:C}");
        Console.WriteLine("Нажмите любую клавишу для продолжения");
        Console.ReadKey();
    }

    private void ShowPurchaseMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("МЕНЮ");
            Console.WriteLine($"Баланс: {Balance:C}\n");

            Console.WriteLine("Доступные детали:");
            var parts = Core.Context.SpareParts.ToList();

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                var warehouseItem = Core.Context.Warehouse.FirstOrDefault(w => w.SparePartId == part.Id);
                var currentQuantity = warehouseItem?.Quantity ?? 0;

                Console.WriteLine($"{i + 1}. {part.Name} - Цена: {part.PurchasePrice:C} (на складе: {currentQuantity} шт.)");
            }

            Console.WriteLine($"\n{parts.Count + 1}. Вернуться в главное меню");

            Console.Write("\nВыберите деталь для покупки: ");
            var choice = GetUserChoice(1, parts.Count + 1);

            if (choice == parts.Count + 1) break;

            var selectedPart = parts[choice - 1];
            PurchasePart(selectedPart);
        }
    }

    private void PurchasePart(SparePart part)
    {
        Console.Write($"Сколько {part.Name} хотите купить? ");
        if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0)
        {
            var totalCost = part.PurchasePrice * quantity;

            if (totalCost <= Balance)
            {
                Balance -= totalCost;

                var supplyOrder = new SupplyOrder
                {
                    TotalCost = totalCost,
                    CarsUntilDelivery = 2, // доставка через 2 машины
                    CreatedDate = DateTime.Now,
                    IsDelivered = false
                };

                Core.Context.SupplyOrders.Add(supplyOrder);
                Core.Context.SaveChanges();

                var supplyOrderItem = new SupplyOrderItem
                {
                    SupplyOrderId = supplyOrder.Id,
                    SparePartId = part.Id,
                    Quantity = quantity
                };

                Core.Context.SupplyOrderItems.Add(supplyOrderItem);
                Core.Context.SaveChanges();

                Console.WriteLine($"\nЗаказ на {quantity} {part.Name} оформлен!");
                Console.WriteLine($"Списано: {totalCost:C}");
                Console.WriteLine($"Доставка через 2 клиента");
            }
            else
            {
                Console.WriteLine("Недостаточно средств!");
            }
        }
        else
        {
            Console.WriteLine("Неверное количество!");
        }

        Console.WriteLine("Нажмите любую клавишу для продолжения");
        Console.ReadKey();
    }

    private void ProcessPendingOrders()
    {
        var pendingOrders = Core.Context.SupplyOrders.Where(o => !o.IsDelivered).ToList();

        foreach (var order in pendingOrders)
        {
            order.CarsUntilDelivery--;

            if (order.CarsUntilDelivery <= 0)
            {
                // Доставка заказа
                order.IsDelivered = true;
                var orderItems = Core.Context.SupplyOrderItems.Where(i => i.SupplyOrderId == order.Id).ToList();

                foreach (var item in orderItems)
                {
                    var warehouseItem = Core.Context.Warehouse.FirstOrDefault(w => w.SparePartId == item.SparePartId);
                    if (warehouseItem != null)
                    {
                        warehouseItem.Quantity += item.Quantity;
                        warehouseItem.LastUpdated = DateTime.Now;
                    }
                    else
                    {
                        warehouseItem = new Warehouse
                        {
                            SparePartId = item.SparePartId,
                            Quantity = item.Quantity,
                            LastUpdated = DateTime.Now
                        };
                        Core.Context.Warehouse.Add(warehouseItem);
                    }
                }

                Console.WriteLine($"\n🚚 Доставлен заказ #{order.Id} на сумму {order.TotalCost:C}");
            }
        }
        Core.Context.SaveChanges();
    }

    private void ShowStatus()
    {
        var gameState = Core.Context.GameStates.Find(1);
        var totalParts = Core.Context.Warehouse.Sum(w => w.Quantity);
        var pendingOrders = Core.Context.SupplyOrders.Count(o => !o.IsDelivered);

        Console.WriteLine($"=== {gameState.ServiceName.ToUpper()} ===");
        Console.WriteLine($"💰 Баланс: {Balance:C}");
        Console.WriteLine($"👥 Обслужено клиентов: {clientCounter}");
        Console.WriteLine($"📦 Деталей на складе: {totalParts}");
        Console.WriteLine($"🚚 Ожидается поставок: {pendingOrders}");
    }

    private void ShowWarehouse()
    {
        Console.WriteLine("\n=== СКЛАД ===");
        var warehouseItems = Core.Context.Warehouse.Include(w => w.SparePart).Where(w => w.Quantity > 0).ToList();

        if (warehouseItems.Any())
        {
            foreach (var item in warehouseItems)
            {
                Console.WriteLine($"{item.SparePart.Name}: {item.Quantity} шт. (цена продажи: {item.SparePart.SellPrice:C})");
            }
        }
        else
        {
            Console.WriteLine("Склад пуст!");
        }

        var pendingOrders = Core.Context.SupplyOrders.Where(o => !o.IsDelivered).ToList();
        if (pendingOrders.Count > 0)
        {
            Console.WriteLine("\n=== ОЖИДАЮТСЯ ПОСТАВКИ ===");
            foreach (var order in pendingOrders)
            {
                var items = Core.Context.SupplyOrderItems
                    .Include(i => i.SparePart)
                    .Where(i => i.SupplyOrderId == order.Id)
                    .ToList();

                Console.WriteLine($"Заказ #{order.Id}:");
                foreach (var item in items)
                {
                    Console.WriteLine($"  {item.SparePart.Name}: {item.Quantity} шт. (доставка через {order.CarsUntilDelivery} клиента)");
                }
            }
        }
    }

    private void ShowStatistics()
    {
        var successfulRepairs = Core.Context.RepairOrders.Count(r => r.Status == 1);
        var failedRepairs = Core.Context.RepairOrders.Count(r => r.Status == 2);
        var refusedOrders = Core.Context.RepairOrders.Count(r => r.Status == 0);
        var totalProfit = Core.Context.RepairOrders.Sum(r => r.Profit) == 0;

        Console.WriteLine("\n=== СТАТИСТИКА ===");
        Console.WriteLine($"Успешных ремонтов: {successfulRepairs}");
        Console.WriteLine($"Неудачных ремонтов: {failedRepairs}");
        Console.WriteLine($"Отказов в обслуживании: {refusedOrders}");
        Console.WriteLine($"Общая прибыль: {totalProfit:C}");
        Console.WriteLine($"Всего клиентов: {clientCounter}");
    }

    private void UpdateGameState()
    {
        var gameState = Core.Context.GameStates.Find(1);
        gameState.TotalCarsProcessed = clientCounter;
        gameState.LastUpdated = DateTime.Now;
        Core.Context.SaveChanges();
    }

    private int GetUserChoice(int min, int max)
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max)
            {
                return choice;
            }
            Console.Write($"Пожалуйста, введите число от {min} до {max}: ");
        }
    }
}

// Главный класс программы
class Program
{
    static void Main(string[] args)
    {
        try
        {
            var game = new AutoServiceGame();
            game.StartGame();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
            Console.WriteLine("Убедитесь, что база данных создана и содержит начальные данные.");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}