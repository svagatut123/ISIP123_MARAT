using prac7;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

// Классы моделей для вашей БД
public class Client
{
    public string CarModel { get; set; }
    public string CarBrand { get; set; }
    public DateTime CreatedDate { get; set; }
}

// Основной класс автосервиса
class AutoServiceGame
{
    private decimal money;
    private Dictionary<string, int> warehouse;
    private List<PurchaseOrder> purchaseOrders;
    private Random random;
    private int totalCarsProcessed;
    private int currentWarehouseId = 1; // Используем склад с ID = 1

    // Списки для генерации случайных автомобилей
    private string[] carBrands = new[]
    {
        "Toyota", "Honda", "Ford", "BMW", "Mercedes", "Audi", "Volkswagen",
        "Hyundai", "Kia", "Nissan", "Mazda", "Subaru", "Lexus", "Chevrolet"
    };

    private string[] carModels = new[]
    {
        "Camry", "Civic", "Focus", "X5", "C-Class", "A4", "Golf",
        "Elantra", "Rio", "Altima", "CX-5", "Outback", "RX", "Cruze",
        "Corolla", "Accord", "Fusion", "3 Series", "E-Class", "Q5"
    };

    public AutoServiceGame()
    {
        warehouse = new Dictionary<string, int>();
        purchaseOrders = new List<PurchaseOrder>();
        random = new Random();

        InitializeGame();
    }

    private void InitializeGame()
    {
        // Используем существующий контекст из Core
        var context = Core.Context;

        // Загружаем баланс из WareHouse
        var warehouseData = context.WareHouse.FirstOrDefault(w => w.Id == currentWarehouseId);
        if (warehouseData != null)
        {
            money = warehouseData.BBalance;
        }
        else
        {
            // Создаем новый склад, если не существует
            money = 10000; // Начальный баланс как в БД
            var newWarehouse = new WareHouse
            {
                Id = currentWarehouseId,
                BBalance = money
            };
            context.WareHouse.Add(newWarehouse);
            context.SaveChanges();
        }

        // Загружаем детали со склада
        var warehouseParts = context.WarehouseParts
            .Include("Parts")
            .Where(wp => wp.WarehouseID == currentWarehouseId && wp.Count > 0)
            .ToList();

        foreach (var wp in warehouseParts)
        {
            warehouse[wp.Parts.Name] = wp.Count;
        }

        Console.WriteLine("Автосервис инициализирован!");
        Console.WriteLine($"Начальный баланс: {money:C}");
    }

    public void RunGame()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("=== АВТОСЕРВИС GMWOG ===");

        while (money > 0)
        {
            Console.Clear();
            Console.WriteLine($"=== ОБСЛУЖИВАНИЕ КЛИЕНТА №{totalCarsProcessed + 1} ===");

            ProcessDeliveries();
            ShowStatus();

            // Проверяем баланс после доставок
            if (money <= 0)
            {
                GameOver();
                return;
            }

            // Создаем нового клиента (автоматически)
            var client = CreateNewClient();
            var brokenPart = GetRandomSparePart();
            var repairCost = CalculateRepairCost(brokenPart);

            Console.WriteLine($"\nДанные клиента:");
            Console.WriteLine($"Марка автомобиля: {client.CarBrand}");
            Console.WriteLine($"Модель: {client.CarModel}");
            Console.WriteLine($"\nПоломка: {brokenPart.Name}");
            Console.WriteLine($"Стоимость ремонта: {repairCost:C}");

            // Проверяем наличие детали на складе
            bool hasPart = warehouse.ContainsKey(brokenPart.Name) && warehouse[brokenPart.Name] > 0;
            Console.WriteLine($"Наличие на складе: {(hasPart ? "✓ В наличии" : "✗ Нет в наличии")}");

            Console.WriteLine("\nВаши действия:");
            Console.WriteLine("1 - Взять заказ (если есть деталь на складе)");
            Console.WriteLine("2 - Отказать клиенту (штраф 3000 руб.)");
            Console.WriteLine("3 - Закупить запчасти");
            Console.WriteLine("4 - Показать склад");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AcceptOrder(client, brokenPart, repairCost, hasPart);
                    break;
                case "2":
                    RefuseOrder(client);
                    break;
                case "3":
                    ShowPurchaseMenu();
                    break;
                case "4":
                    ShowWarehouse();
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    continue;
                default:
                    Console.WriteLine("Неверный выбор! Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    continue;
            }

            totalCarsProcessed++;
            SaveGameState();

            // Проверяем баланс после обработки заказа
            if (money <= 0)
            {
                GameOver();
                return;
            }

            Console.WriteLine("\nНажмите любую клавишу для следующего клиента...");
            Console.ReadKey();
        }

        GameOver();
    }

    private void ProcessDeliveries()
    {
        var deliveredOrders = new List<PurchaseOrder>();

        foreach (var order in purchaseOrders.ToList())
        {
            order.DaysUntilDelivery--;

            if (order.DaysUntilDelivery <= 0)
            {
                // Доставляем заказ
                if (warehouse.ContainsKey(order.PartName))
                    warehouse[order.PartName] += order.Quantity;
                else
                    warehouse[order.PartName] = order.Quantity;

                Console.WriteLine($"✓ Доставлены {order.Quantity} {order.PartName}");
                deliveredOrders.Add(order);
                purchaseOrders.Remove(order);

                UpdateStockInDb(order.PartName, warehouse[order.PartName]);
            }
        }
    }

    private void ShowStatus()
    {
        Console.WriteLine($"\nБаланс: {money:C}");
        Console.WriteLine($"Обслужено автомобилей: {totalCarsProcessed}");
        Console.WriteLine("Склад запчастей:");

        if (warehouse.Count == 0)
        {
            Console.WriteLine("  (пусто)");
        }
        else
        {
            foreach (var part in warehouse.Where(p => p.Value > 0))
            {
                Console.WriteLine($"  {part.Key}: {part.Value} шт.");
            }
        }

        ShowPendingOrders();
    }

    private Client CreateNewClient()
    {
        // Автоматически генерируем случайный автомобиль
        var client = new Client
        {
            CarBrand = carBrands[random.Next(carBrands.Length)],
            CarModel = carModels[random.Next(carModels.Length)],
            CreatedDate = DateTime.Now
        };

        return client;
    }

    private Parts GetRandomSparePart()
    {
        var context = Core.Context;
        var parts = context.Parts.ToList();

        if (parts.Count > 0)
        {
            return parts[random.Next(parts.Count)];
        }
        // Fallback если нет деталей в БД
        return new Parts { ID = 5, Name = "бампер", Price = 1200 };
    }

    private decimal CalculateRepairCost(Parts part)
    {
        // Стоимость ремонта = цена детали + работа (1000-3000 руб)
        return part.Price + random.Next(1000, 3001);
    }

    private void AcceptOrder(Client client, Parts brokenPart, decimal repairCost, bool hasPart)
    {
        if (hasPart)
        {
            // Успешный ремонт
            warehouse[brokenPart.Name]--;
            money += repairCost;

            UpdateStockInDb(brokenPart.Name, warehouse[brokenPart.Name]);
            SaveGameState();

            Console.WriteLine($"\n✓ Успешный ремонт! Прибыль: {repairCost:C}");
            Console.WriteLine($"Деталь '{brokenPart.Name}' использована со склада");
            Console.WriteLine($"Автомобиль {client.CarBrand} {client.CarModel} отремонтирован!");
        }
        else
        {
            Console.WriteLine("\n✗ Нужной детали нет на складе!");

            var availableParts = warehouse.Where(p => p.Value > 0).ToList();
            if (availableParts.Count > 0)
            {
                Console.WriteLine("Производится замена случайной деталью...");

                var randomPart = availableParts[random.Next(availableParts.Count)];
                warehouse[randomPart.Key]--;

                var penalty = repairCost + 5000; // Штраф 5000 руб
                money -= penalty;

                UpdateStockInDb(randomPart.Key, warehouse[randomPart.Key]);
                SaveGameState();

                Console.WriteLine($"Клиент недоволен! Вы поставили {randomPart.Key} вместо {brokenPart.Name}");
                Console.WriteLine($"Штраф: {penalty:C}");
                Console.WriteLine($"Автомобиль {client.CarBrand} {client.CarModel} - клиент недоволен!");
            }
            else
            {
                var penalty = repairCost + 10000; // Большой штраф
                money -= penalty;
                SaveGameState();

                Console.WriteLine($"На складе нет деталей для замены!");
                Console.WriteLine($"Штраф за простой: {penalty:C}");
                Console.WriteLine($"Автомобиль {client.CarBrand} {client.CarModel} - ремонт невозможен!");
            }
        }
    }

    private void RefuseOrder(Client client)
    {
        int fine = 3000;
        money -= fine;
        SaveGameState();

        Console.WriteLine($"\n✗ Вы отказали клиенту с автомобилем {client.CarBrand} {client.CarModel}");
        Console.WriteLine($"Штраф: {fine:C}");
    }

    private void ShowPurchaseMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== ЗАКУПКА ЗАПЧАСТЕЙ ===");
            Console.WriteLine($"Баланс: {money:C}");
            Console.WriteLine("\nДоступные запчасти:");

            var parts = GetAvailableParts();
            int i = 1;
            foreach (var part in parts)
            {
                var currentStock = warehouse.ContainsKey(part.Name) ? warehouse[part.Name] : 0;
                Console.WriteLine($"{i} - {part.Name}: {part.Price:C}/шт. (на складе: {currentStock} шт.)");
                i++;
            }
            Console.WriteLine($"{i} - Вернуться к клиенту");

            Console.Write("\nВыберите деталь для заказа: ");
            string choice = Console.ReadLine();

            if (int.TryParse(choice, out int partIndex))
            {
                if (partIndex == i)
                    break;

                if (partIndex >= 1 && partIndex <= parts.Count)
                {
                    var selectedPart = parts[partIndex - 1];
                    Console.Write($"Сколько {selectedPart.Name} закупить? ");

                    if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0)
                    {
                        decimal totalCost = selectedPart.Price * quantity;

                        if (totalCost <= money)
                        {
                            money -= totalCost;
                            CreateSupplyOrder(selectedPart, quantity);
                            SaveGameState();
                            Console.WriteLine($"\n✓ Заказ на {quantity} {selectedPart.Name} оформлен!");
                            Console.WriteLine($"Доставка через 2 дня. Списано: {totalCost:C}");
                        }
                        else
                        {
                            Console.WriteLine("Недостаточно денег!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неверное количество!");
                    }
                }
                else
                {
                    Console.WriteLine("Неверный выбор!");
                }
            }
            else
            {
                Console.WriteLine("Неверный ввод!");
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }

    private List<Parts> GetAvailableParts()
    {
        var context = Core.Context;
        return context.Parts.ToList();
    }

    private void CreateSupplyOrder(Parts part, int quantity)
    {
        // Добавляем в локальный список заказов (доставка через 2 дня)
        purchaseOrders.Add(new PurchaseOrder(part.Name, quantity, 2));
    }

    private void ShowWarehouse()
    {
        Console.Clear();
        Console.WriteLine("=== СКЛАД ЗАПЧАСТЕЙ ===");

        if (warehouse.Count == 0)
        {
            Console.WriteLine("Склад пуст");
        }
        else
        {
            foreach (var part in warehouse.Where(p => p.Value > 0))
            {
                Console.WriteLine($"{part.Key}: {part.Value} шт.");
            }
        }

        ShowPendingOrders();
    }

    private void ShowPendingOrders()
    {
        if (purchaseOrders.Count > 0)
        {
            Console.WriteLine("\nОжидаются поставки:");
            foreach (var order in purchaseOrders)
            {
                Console.WriteLine($"  {order.PartName}: {order.Quantity} шт. (через {order.DaysUntilDelivery} дней)");
            }
        }
    }

    private void SaveGameState()
    {
        var context = Core.Context;
        var warehouseData = context.WareHouse.FirstOrDefault(w => w.Id == currentWarehouseId);
        if (warehouseData != null)
        {
            warehouseData.BBalance = money;
            context.SaveChanges();
        }

        // Обновляем склад в базе
        UpdateWarehouseInDatabase();
    }

    private void UpdateWarehouseInDatabase()
    {
        var context = Core.Context;
        foreach (var partEntry in warehouse)
        {
            var part = context.Parts.FirstOrDefault(p => p.Name == partEntry.Key);
            if (part != null)
            {
                var warehousePart = context.WarehouseParts
                    .FirstOrDefault(wp => wp.WarehouseID == currentWarehouseId && wp.PartsID == part.ID);

                if (warehousePart != null)
                {
                    warehousePart.Count = partEntry.Value;
                }
                else
                {
                    context.WarehouseParts.Add(new WarehouseParts
                    {
                        WarehouseID = currentWarehouseId,
                        PartsID = part.ID,
                        Count = partEntry.Value
                    });
                }
            }
        }
        context.SaveChanges();
    }

    private void UpdateStockInDb(string partName, int amount)
    {
        var context = Core.Context;
        var part = context.Parts.FirstOrDefault(p => p.Name == partName);
        if (part != null)
        {
            var warehousePart = context.WarehouseParts
                .FirstOrDefault(wp => wp.WarehouseID == currentWarehouseId && wp.PartsID == part.ID);

            if (warehousePart != null)
            {
                warehousePart.Count = amount;
            }
            else
            {
                context.WarehouseParts.Add(new WarehouseParts
                {
                    WarehouseID = currentWarehouseId,
                    PartsID = part.ID,
                    Count = amount
                });
            }
            context.SaveChanges();
        }
    }

    private void GameOver()
    {
        Console.Clear();
        Console.WriteLine("=== ИГРА ОКОНЧЕНА ===");
        Console.WriteLine($"Ваш баланс: {money:C}");
        Console.WriteLine($"Всего обслужено автомобилей: {totalCarsProcessed}");
        Console.WriteLine("\nСпасибо за игру!");
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
        Environment.Exit(0);
    }
}

// Вспомогательные классы
class PurchaseOrder
{
    public string PartName { get; set; }
    public int Quantity { get; set; }
    public int DaysUntilDelivery { get; set; }

    public PurchaseOrder(string partName, int quantity, int daysUntilDelivery)
    {
        PartName = partName;
        Quantity = quantity;
        DaysUntilDelivery = daysUntilDelivery;
    }
}

// Точка входа
class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Добро пожаловать в автосервис GMWOG!");

            AutoServiceGame game = new AutoServiceGame();
            game.RunGame();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.WriteLine("Проверьте подключение к базе данных");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}