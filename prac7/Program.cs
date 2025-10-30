using System;
using System.Collections.Generic;
using System.Linq;
using prac7;
// Основной класс симулятора автосервиса
class AutoServiceGame
{
    private decimal money;
    private Dictionary<string, int> warehouse;
    private List<PurchaseOrder> purchaseOrders;
    private Random random;
    private int totalCarsProcessed;

    public AutoServiceGame()
    {
        warehouse = new Dictionary<string, int>();
        purchaseOrders = new List<PurchaseOrder>();
        random = new Random();

        InitializeGame();
    }

    private void InitializeGame()
    {
        using (var context = new maratpractic7Entities3())
        {
            // Загружаем состояние игры из таблицы GameState
            var gameState = context.GameState.FirstOrDefault(g => g.Id == 1);
            if (gameState != null)
            {
                money = gameState.Balance;
                totalCarsProcessed = gameState.TotalCarsProcessed = 0;
                Console.WriteLine($"Загружена игра: {gameState.ServiceName}");
            }
            else
            {
                // Создаем новое состояние игры, если оно не существует
                money = 5000;
                totalCarsProcessed = 0;

                var newGameState = new GameState
                {
                    Balance = money,
                    TotalCarsProcessed = totalCarsProcessed,
                    ServiceName = "Мой Автосервис",
                    LastUpdated = DateTime.Now
                };
                context.GameState.Add(newGameState);
                context.SaveChanges();
                Console.WriteLine("Создана новая игра!");
            }

            // Загружаем склад из таблицы Warehouse
            var warehouseData = context.Warehouse
                .Where(w => w.Quantity > 0)
                .Join(context.SpareParts,
                      w => w.SparePartId,
                      sp => sp.Id,
                      (w, sp) => new { sp.Name, w.Quantity })
                .ToList();

            foreach (var item in warehouseData)
            {
                warehouse[item.Name] = item.Quantity;
            }

            // Загружаем ожидающие поставки из таблицы SupplyOrders
            var pendingOrders = context.SupplyOrders
                .Where(so => !so.IsDelivered)
                .Join(context.SupplyOrderItems,
                      so => so.Id,
                      soi => soi.SupplyOrderId,
                      (so, soi) => new { so, soi })
                .Join(context.SpareParts,
                      x => x.soi.SparePartId,
                      sp => sp.Id,
                      (x, sp) => new {
                          OrderId = x.so.Id,
                          PartName = sp.Name,
                          Quantity = x.soi.Quantity,
                          CarsUntilDelivery = x.so.CarsUntilDelivery
                      })
                .ToList();

            foreach (var order in pendingOrders)
            {
                purchaseOrders.Add(new PurchaseOrder(
                    order.OrderId,
                    order.PartName,
                    order.Quantity,
                    order.CarsUntilDelivery
                ));
            }
        }
    }

    public void RunGame()
    {
        Console.WriteLine("=== АВТОСЕРВИС ===");
        Console.WriteLine($"Баланс: {money:C}");
        Console.WriteLine($"Обслужено автомобилей: {totalCarsProcessed}");
        Console.WriteLine("Нажмите любую клавишу для начала обслуживания клиентов...");
        Console.ReadKey();

        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== КЛИЕНТ №{totalCarsProcessed + 1} ===");

            ProcessDeliveries();
            ShowStatus();

            // Создаем нового клиента
            var client = CreateNewClient();
            var brokenPart = GetRandomSparePart();
            var repairCost = brokenPart.SellPrice;

            Console.WriteLine($"\nКлиент: {client.Name}");
            Console.WriteLine($"Автомобиль: {client.CarModel}");
            Console.WriteLine($"Поломка: {brokenPart.Name}");
            Console.WriteLine($"Стоимость ремонта: {repairCost:C}");

            // Проверяем наличие детали на складе
            bool hasPart = warehouse.ContainsKey(brokenPart.Name) && warehouse[brokenPart.Name] > 0;

            Console.WriteLine($"Наличие на складе: {(hasPart ? " В наличии" : " Нет в наличии")}");

            Console.WriteLine("\nВаши действия:");
            Console.WriteLine("1 - Взять заказ (если есть деталь на складе)");
            Console.WriteLine("2 - Отказать клиенту (штраф 20% от стоимости ремонта)");
            Console.WriteLine("3 - Закупить запчасти");
            Console.WriteLine("4 - Показать склад");
            Console.WriteLine("5 - Показать статистику");
            Console.WriteLine("6 - Выйти из игры");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AcceptOrder(client, brokenPart, repairCost, hasPart);
                    break;
                case "2":
                    RefuseOrder(client, brokenPart, repairCost);
                    break;
                case "3":
                    ShowPurchaseMenu();
                    break;
                case "4":
                    ShowWarehouse();
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    continue;
                case "5":
                    ShowStatistics();
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    continue;
                case "6":
                    SaveGameState();
                    Console.WriteLine($"Игра завершена! Итоговый баланс: {money:C}");
                    return;
                default:
                    Console.WriteLine("Неверный выбор! Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    continue;
            }

            totalCarsProcessed++;
            SaveGameState();

            Console.WriteLine("Нажмите любую клавишу для следующего клиента...");
            Console.ReadKey();
        }
    }

    private void ProcessDeliveries()
    {
        var deliveredOrders = new List<int>();

        foreach (var order in purchaseOrders.ToList())
        {
            order.CarsUntilDelivery--;

            if (order.CarsUntilDelivery <= 0)
            {
                // Доставляем заказ
                if (warehouse.ContainsKey(order.PartName))
                    warehouse[order.PartName] += order.Quantity;
                else
                    warehouse[order.PartName] = order.Quantity;

                Console.WriteLine($"✓ Доставлены {order.Quantity} {order.PartName}");
                deliveredOrders.Add(order.OrderId);
                purchaseOrders.Remove(order);

                UpdateStockInDb(order.PartName, warehouse[order.PartName]);
            }
        }

        // Обновляем базу данных
        if (deliveredOrders.Count > 0)
        {
            using (var context = new maratpractic7Entities3())
            {
                // Помечаем заказы как доставленные
                var ordersToUpdate = context.SupplyOrders
                    .Where(so => deliveredOrders.Contains(so.Id))
                    .ToList();

                foreach (var order in ordersToUpdate)
                {
                    order.IsDelivered = true;
                }

                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обновлении заказов: {ex.Message}");
                }
            }
        }

        // Обновляем оставшиеся заказы в базе
        using (var context = new maratpractic7Entities3())
        {
            foreach (var order in purchaseOrders)
            {
                var dbOrder = context.SupplyOrders.FirstOrDefault(so => so.Id == order.OrderId);
                if (dbOrder != null)
                {
                    dbOrder.CarsUntilDelivery = order.CarsUntilDelivery;
                }
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении заказов: {ex.Message}");
            }
        }
    }

    private void ShowStatus()
    {
        Console.WriteLine($"\nБаланс: {money:C}");
        Console.WriteLine($"Обслужено автомобилей: {totalCarsProcessed}");
        Console.WriteLine("Склад:");

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
        var names = new[] { "Гуцалюк Александр Александрович(вип)", "Родионов Дензл Михалыч", "Абасов Абиль Джабасович", "Уляненко Никита Дэнисович", "Черный Михаил Джекович", "Шуриков Шурик Шурикович", "Федотов Русик Валерьевич" };
        var carModels = new[] { "Самолет", "Коррабль", "Шевроле Камаро", "Гелик", "ВАЗ 2106 m5 f90", "тук-тук", "Hyundai porter" };

        var client = new Client
        {
            Name = names[random.Next(names.Length)],
            CarModel = carModels[random.Next(carModels.Length)],
            CreatedDate = DateTime.Now
        };

        // Сохраняем клиента в базу
        using (var context = new maratpractic7Entities3())
        {
            var dbClient = new Clients
            {
                Name = client.Name,
                CarModel = client.CarModel,
                CreatedDate = client.CreatedDate
            };
            context.Clients.Add(dbClient);
            context.SaveChanges();
            client.Id = dbClient.Id;
        }

        return client;
    }

    private SparePart GetRandomSparePart()
    {
        using (var context = new maratpractic7Entities3())
        {
            var parts = context.SpareParts.ToList();

            if (parts.Count > 0)
            {
                var part = parts[random.Next(parts.Count)];
                return new SparePart
                {
                    Id = part.Id,
                    Name = part.Name,
                    PurchasePrice = part.PurchasePrice,
                    SellPrice = part.SellPrice
                };
            }
        }
        throw new Exception("В базе данных нет запчастей");
    }

    private void AcceptOrder(Client client, SparePart brokenPart, decimal repairCost, bool hasPart)
    {
        var profit = 0m;
        var status = 3; // в процессе

        if (hasPart)
        {
            // Успешный ремонт
            warehouse[brokenPart.Name]--;
            profit = repairCost - brokenPart.PurchasePrice;
            money += profit;

            status = 1; // успех
            Console.WriteLine($" Успешный ремонт! Прибыль: {profit:C}");
        }
        else
        {
            Console.WriteLine("Нужной детали нет на складе! Производится замена случайной деталью...");

            var availableParts = warehouse.Where(p => p.Value > 0).ToList();
            if (availableParts.Count > 0)
            {
                var randomPart = availableParts[random.Next(availableParts.Count)];
                warehouse[randomPart.Key]--;

                var penalty = repairCost * 1.5m; // Штраф 150%
                money -= penalty;
                profit = -penalty;

                status = 2; // ошибка

                Console.WriteLine($"Клиент недоволен! Вы поставили {randomPart.Key} вместо {brokenPart.Name}");
                Console.WriteLine($"Штраф: {penalty:C}");
            }
            else
            {
                var penalty = repairCost * 2m; // Штраф 200%
                money -= penalty;
                profit = -penalty;

                status = 2; // ошибка

                Console.WriteLine($"На складе нет деталей. Штраф: {penalty:C} 2 недели");
            }
        }

        // Сохраняем заказ на ремонт
        using (var context = new maratpractic7Entities3())
        {
            var repairOrder = new RepairOrders
            {
                ClientId = client.Id,
                BrokenPartId = brokenPart.Id,
                Status = status,
                Profit = profit,
                CreatedDate = DateTime.Now,
                CompletedDate = status != 3 ? DateTime.Now : (DateTime?)null
            };
            context.RepairOrders.Add(repairOrder);
            context.SaveChanges();
        }

        // Обновляем склад в базе
        UpdateWarehouseInDatabase();
        SaveGameState();
    }

    private void RefuseOrder(Client client, SparePart brokenPart, decimal repairCost)
    {
        var penalty = repairCost * 0.2m; // Штраф 20%
        money -= penalty;

        // Сохраняем заказ на ремонт
        using (var context = new maratpractic7Entities3())
        {
            var repairOrder = new RepairOrders
            {
                ClientId = client.Id,
                BrokenPartId = brokenPart.Id,
                Status = 0, // отказ
                Profit = -penalty,
                CreatedDate = DateTime.Now,
                CompletedDate = DateTime.Now
            };
            context.RepairOrders.Add(repairOrder);
            context.SaveChanges();
        }

        SaveGameState();
        Console.WriteLine($"Вы отказали клиенту. Штраф: {penalty:C}");
    }

    private void ShowPurchaseMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== ЗАКУПКА ЗАПЧАСТЕЙ ===");
            Console.WriteLine($"Баланс: {money:C}");
            Console.WriteLine("\nДоступные запчасти:");

            var parts = GetAvailableSpareParts();
            int i = 1;
            foreach (var part in parts)
            {
                var currentStock = warehouse.ContainsKey(part.Name) ? warehouse[part.Name] : 0;
                Console.WriteLine($"{i} - {part.Name}: {part.PurchasePrice:C}/шт. (на складе: {currentStock} шт.)");
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
                        decimal totalCost = selectedPart.PurchasePrice * quantity;

                        if (totalCost <= money)
                        {
                            money -= totalCost;
                            CreateSupplyOrder(selectedPart, quantity);
                            SaveGameState();
                            Console.WriteLine($"Заказ на {quantity} {selectedPart.Name} оформлен! Доставка через 2 клиента.");
                            Console.WriteLine($"Списано: {totalCost:C}");
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

    private List<SparePart> GetAvailableSpareParts()
    {
        var parts = new List<SparePart>();
        using (var context = new maratpractic7Entities3())
        {
            var dbParts = context.SpareParts.ToList();
            foreach (var dbPart in dbParts)
            {
                parts.Add(new SparePart
                {
                    Id = dbPart.Id,
                    Name = dbPart.Name,
                    PurchasePrice = dbPart.PurchasePrice,
                    SellPrice = dbPart.SellPrice
                });
            }
        }
        return parts;
    }

    private void CreateSupplyOrder(SparePart part, int quantity)
    {
        using (var context = new maratpractic7Entities3())
        {
            // Создаем заказ на поставку
            var supplyOrder = new SupplyOrders
            {
                TotalCost = part.PurchasePrice * quantity,
                CarsUntilDelivery = 2,
                CreatedDate = DateTime.Now,
                IsDelivered = false
            };
            context.SupplyOrders.Add(supplyOrder);
            context.SaveChanges();

            // Добавляем позиции заказа
            var orderItem = new SupplyOrderItems
            {
                SupplyOrderId = supplyOrder.Id,
                SparePartId = part.Id,
                Quantity = quantity
            };
            context.SupplyOrderItems.Add(orderItem);
            context.SaveChanges();

            // Добавляем в локальный список
            purchaseOrders.Add(new PurchaseOrder(supplyOrder.Id, part.Name, quantity, 2));
        }
    }

    private void ShowWarehouse()
    {
        Console.Clear();
        Console.WriteLine("=== СКЛАД ===");

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

    private void ShowStatistics()
    {
        Console.Clear();
        Console.WriteLine("=== СТАТИСТИКА ===");

        using (var context = new maratpractic7Entities3())
        {
            // Статистика по заказам
            var stats = context.RepairOrders
                .GroupBy(r => 1)
                .Select(g => new
                {
                    TotalOrders = g.Count(),
                    Successful = g.Count(r => r.Status == 1),
                    Failed = g.Count(r => r.Status == 2),
                    Refused = g.Count(r => r.Status == 0),
                    TotalProfit = g.Sum(r => r.Profit)
                })
                .FirstOrDefault();

            if (stats != null)
            {
                Console.WriteLine($"Всего заказов: {stats.TotalOrders}");
                Console.WriteLine($"Успешных ремонтов: {stats.Successful}");
                Console.WriteLine($"Неудачных ремонтов: {stats.Failed}");
                Console.WriteLine($"Отказов: {stats.Refused}");
                Console.WriteLine($"Общая прибыль: {stats.TotalProfit:C}");
            }

            Console.WriteLine($"\nТекущий баланс: {money:C}");
            Console.WriteLine($"Обслужено автомобилей: {totalCarsProcessed}");
        }
    }

    private void ShowPendingOrders()
    {
        if (purchaseOrders.Count > 0)
        {
            Console.WriteLine("\nОжидаются поставки:");
            foreach (var order in purchaseOrders)
            {
                Console.WriteLine($"  {order.PartName}: {order.Quantity} шт. (через {order.CarsUntilDelivery} клиентов)");
            }
        }
    }

    private void SaveGameState()
    {
        using (var context = new maratpractic7Entities3())
        {
            var gameState = context.GameState.FirstOrDefault(g => g.Id == 1);
            if (gameState != null)
            {
                gameState.Balance = money;
                gameState.TotalCarsProcessed = totalCarsProcessed;
                gameState.LastUpdated = DateTime.Now;
                context.SaveChanges();
            }

            // Обновляем склад в базе
            UpdateWarehouseInDatabase();
        }
    }

    private void UpdateWarehouseInDatabase()
    {
        using (var context = new maratpractic7Entities3())
        {
            foreach (var part in warehouse)
            {
                var partId = GetSparePartId(part.Key);
                var warehouseItem = context.Warehouse.FirstOrDefault(w => w.SparePartId == partId);

                if (warehouseItem != null)
                {
                    warehouseItem.Quantity = part.Value;
                    warehouseItem.LastUpdated = DateTime.Now;
                }
                else
                {
                    context.Warehouse.Add(new Warehouse
                    {
                        SparePartId = partId,
                        Quantity = part.Value,
                        LastUpdated = DateTime.Now
                    });
                }
            }
            context.SaveChanges();
        }
    }

    private void UpdateStockInDb(string partName, int amount)
    {
        using (var context = new maratpractic7Entities3())
        {
            var partId = GetSparePartId(partName);
            var warehouseItem = context.Warehouse.FirstOrDefault(w => w.SparePartId == partId);

            if (warehouseItem != null)
            {
                warehouseItem.Quantity = amount;
                warehouseItem.LastUpdated = DateTime.Now;
            }
            else
            {
                context.Warehouse.Add(new Warehouse
                {
                    SparePartId = partId,
                    Quantity = amount,
                    LastUpdated = DateTime.Now
                });
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении склада: {ex.Message}");
            }
        }
    }

    private int GetSparePartId(string partName)
    {
        using (var context = new maratpractic7Entities3())
        {
            var part = context.SpareParts.FirstOrDefault(sp => sp.Name == partName);
            return part?.Id ?? 0;
        }
    }
}

// Классы для хранения данных
class PurchaseOrder
{
    public int OrderId { get; set; }
    public string PartName { get; set; }
    public int Quantity { get; set; }
    public int CarsUntilDelivery { get; set; }

    public PurchaseOrder(int orderId, string partName, int quantity, int carsUntilDelivery)
    {
        OrderId = orderId;
        PartName = partName;
        Quantity = quantity;
        CarsUntilDelivery = carsUntilDelivery;
    }
}

class Client
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CarModel { get; set; }
    public DateTime CreatedDate { get; set; }
}

class SparePart
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellPrice { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Добро пожаловать в автосервис!");
            AutoServiceGame game = new AutoServiceGame();
            game.RunGame();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.WriteLine("Проверьте подключение к базе данных и наличие начальных данных");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}