using prac7;
using System;
using System.Collections.Generic;
using System.Linq;

public class Client
{
    public string CarModel { get; set; }
    public string CarBrand { get; set; }
    public DateTime CreatedDate { get; set; }
}

class AutoServiceGame
{
    private decimal money;
    private Dictionary<string, int> warehouse;
    private List<PurchaseOrder> purchaseOrders;
    private Random random;
    private int totalCarsProcessed;
    private int currentWarehouseId = 1;

    private string[] carBrands = { "Toyota", "Honda", "Ford", "BMW", "Mercedes", "Audi", "Volkswagen" };
    private string[] carModels = { "Camry", "Civic", "Focus", "X5", "C-Class", "A4", "Golf" };

    public AutoServiceGame()
    {
        warehouse = new Dictionary<string, int>();
        purchaseOrders = new List<PurchaseOrder>();
        random = new Random();
        InitializeGame();
    }

    private void InitializeGame()
    {
        var context = Core.Context;
        var warehouseData = context.WareHouse.FirstOrDefault(w => w.Id == currentWarehouseId);

        money = warehouseData?.BBalance ?? 10000;

        if (warehouseData == null)
        {
            context.WareHouse.Add(new WareHouse { Id = currentWarehouseId, BBalance = money });
            context.SaveChanges();
        }

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

            if (money <= 0)
            {
                GameOver();
                return;
            }

            var client = CreateNewClient();
            var brokenPart = GetRandomSparePart();
            var repairCost = CalculateRepairCost(brokenPart);
            bool hasPart = warehouse.ContainsKey(brokenPart.Name) && warehouse[brokenPart.Name] > 0;

            Console.WriteLine($"\nДанные клиента: {client.CarBrand} {client.CarModel}");
            Console.WriteLine($"Поломка: {brokenPart.Name}");
            Console.WriteLine($"Стоимость ремонта: {repairCost:C}");
            Console.WriteLine($"Наличие на складе: {(hasPart ? "✓ В наличии" : "✗ Нет в наличии")}");

            Console.WriteLine("\n1 - Взять заказ");
            Console.WriteLine("2 - Отказать клиенту (штраф 3000 руб.)");
            Console.WriteLine("3 - Закупить запчасти");
            Console.WriteLine("4 - Показать склад");

            switch (Console.ReadLine())
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
                    WaitForKey();
                    continue;
                default:
                    Console.WriteLine("Неверный выбор!");
                    WaitForKey();
                    continue;
            }

            totalCarsProcessed++;
            SaveGameState();

            if (money <= 0)
            {
                GameOver();
                return;
            }

            WaitForKey();
        }
        GameOver();
    }

    private void ProcessDeliveries()
    {
        foreach (var order in purchaseOrders.ToList())
        {
            order.DaysUntilDelivery--;
            if (order.DaysUntilDelivery <= 0)
            {
                if (warehouse.ContainsKey(order.PartName))
                    warehouse[order.PartName] += order.Quantity;
                else
                    warehouse[order.PartName] = order.Quantity;

                Console.WriteLine($"✓ Доставлены {order.Quantity} {order.PartName}");
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
            Console.WriteLine("  (пусто)");
        else
            foreach (var part in warehouse.Where(p => p.Value > 0))
                Console.WriteLine($"  {part.Key}: {part.Value} шт.");

        ShowPendingOrders();
    }

    private Client CreateNewClient() => new Client
    {
        CarBrand = carBrands[random.Next(carBrands.Length)],
        CarModel = carModels[random.Next(carModels.Length)],
        CreatedDate = DateTime.Now
    };

    private Parts GetRandomSparePart()
    {
        var parts = Core.Context.Parts.ToList();
        return parts.Count > 0 ? parts[random.Next(parts.Count)] :
            new Parts { ID = 5, Name = "бампер", Price = 1200 };
    }

    private decimal CalculateRepairCost(Parts part) => part.Price + random.Next(1000, 3001);

    private void AcceptOrder(Client client, Parts brokenPart, decimal repairCost, bool hasPart)
    {
        if (hasPart)
        {
            warehouse[brokenPart.Name]--;
            money += repairCost;
            UpdateStockInDb(brokenPart.Name, warehouse[brokenPart.Name]);
            Console.WriteLine($"✓ Успешный ремонт! Прибыль: {repairCost:C}");
        }
        else
        {
            Console.WriteLine("\n✗ Нужной детали нет на складе!");
            var availableParts = warehouse.Where(p => p.Value > 0).ToList();

            if (availableParts.Count > 0)
            {
                var randomPart = availableParts[random.Next(availableParts.Count)];
                warehouse[randomPart.Key]--;
                money -= repairCost + 5000;
                UpdateStockInDb(randomPart.Key, warehouse[randomPart.Key]);
                Console.WriteLine($"Клиент недоволен! Вы поставили {randomPart.Key} вместо {brokenPart.Name}");
            }
            else
            {
                money -= repairCost + 10000;
                Console.WriteLine($"На складе нет деталей для замены!");
            }
        }
        SaveGameState();
    }

    private void RefuseOrder(Client client)
    {
        money -= 3000;
        SaveGameState();
        Console.WriteLine($"\n✗ Вы отказали клиенту. Штраф: 3000:C");
    }

    private void ShowPurchaseMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== ЗАКУПКА ЗАПЧАСТЕЙ ===");
            Console.WriteLine($"Баланс: {money:C}\n");

            var parts = Core.Context.Parts.ToList();
            int i = 1;
            foreach (var part in parts)
            {
                var stock = warehouse.ContainsKey(part.Name) ? warehouse[part.Name] : 0;
                Console.WriteLine($"{i} - {part.Name}: {part.Price:C}/шт. (на складе: {stock} шт.)");
                i++;
            }
            Console.WriteLine($"{i} - Вернуться к клиенту");

            if (int.TryParse(Console.ReadLine(), out int choice) && choice == i) break;
            if (choice >= 1 && choice <= parts.Count)
            {
                var part = parts[choice - 1];
                Console.Write($"Сколько {part.Name} закупить? ");

                if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0)
                {
                    decimal totalCost = part.Price * quantity;
                    if (totalCost <= money)
                    {
                        money -= totalCost;
                        purchaseOrders.Add(new PurchaseOrder(part.Name, quantity, 2));
                        SaveGameState();
                        Console.WriteLine($"✓ Заказ на {quantity} {part.Name} оформлен!");
                    }
                    else Console.WriteLine("Недостаточно денег!");
                }
                else Console.WriteLine("Неверное количество!");
            }
            else Console.WriteLine("Неверный выбор!");

            WaitForKey();
        }
    }

    private void ShowWarehouse()
    {
        Console.Clear();
        Console.WriteLine("=== СКЛАД ЗАПЧАСТЕЙ ===");
        if (warehouse.Count == 0) Console.WriteLine("Склад пуст");
        else foreach (var part in warehouse.Where(p => p.Value > 0))
                Console.WriteLine($"{part.Key}: {part.Value} шт.");
        ShowPendingOrders();
    }

    private void ShowPendingOrders()
    {
        if (purchaseOrders.Count > 0)
        {
            Console.WriteLine("\nОжидаются поставки:");
            foreach (var order in purchaseOrders)
                Console.WriteLine($"  {order.PartName}: {order.Quantity} шт. (через {order.DaysUntilDelivery} дней)");
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
                    warehousePart.Count = partEntry.Value;
                else
                    context.WarehouseParts.Add(new WarehouseParts
                    {
                        WarehouseID = currentWarehouseId,
                        PartsID = part.ID,
                        Count = partEntry.Value
                    });
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
                warehousePart.Count = amount;
            else
                context.WarehouseParts.Add(new WarehouseParts
                {
                    WarehouseID = currentWarehouseId,
                    PartsID = part.ID,
                    Count = amount
                });
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
        WaitForKey();
        Environment.Exit(0);
    }

    private void WaitForKey()
    {
        Console.WriteLine("Нажмите любую клавишу для продолжения...");
        Console.ReadKey();
    }
}

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

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Добро пожаловать в автосервис GMWOG!");
            new AutoServiceGame().RunGame();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.WriteLine("Проверьте подключение к базе данных");
            Console.ReadKey();
        }
    }
}