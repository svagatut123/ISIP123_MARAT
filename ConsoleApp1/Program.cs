using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.Linq;

// Класс для хранения информации о клиенте
public class Client
{
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

    private string[] carBrands = { "ваз", "марк", "сааб", "ауди", "чери тиго", "субару", "мопед" };

   
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
        var warehouseData = context.WareHouse.FirstOrDefault(w => w.Id == currentWarehouseId); //склад по ID

        money = warehouseData?.BBalance ?? 10000;  

        if (warehouseData == null)
        {
            context.WareHouse.Add(new WareHouse { Id = currentWarehouseId, BBalance = money });  
            context.SaveChanges();  
        }

        
        var warehouseParts = context.WarehouseParts
            .Include("Parts")  // Подгружаем связанные данные о деталях
            .Where(wp => wp.WarehouseID == currentWarehouseId && wp.Count > 0)  // Фильтруем по складу и ненулевому количеству
            .ToList();  // Преобразуем в список

        // Переносим данные из БД в локальный словарь склада
        foreach (var wp in warehouseParts)
        {
            warehouse[wp.Parts.Name] = wp.Count;  // Добавляем деталь в склад
        }

        Console.WriteLine($"начальный баланс: {money}");  
    }

    public void RunGame()
    {
        Console.WriteLine("автосервис");  

        while (money > 0)
        {
            Console.Clear();  
            Console.WriteLine($"клиент №{totalCarsProcessed + 1}");  

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

            Console.WriteLine($"\nданные клиента: {client.CarBrand}");
            Console.WriteLine($"поломка: {brokenPart.Name}");
            Console.WriteLine($"стоимость ремонта: {repairCost}");
            Console.WriteLine($"наличие на складе: {(hasPart ? "В наличии" : "Нет в наличии")}");

            Console.WriteLine("\n1 - взять заказ");
            Console.WriteLine("2 - отказать клиенту (штраф 2000 руб.)");
            Console.WriteLine("3 - закупить запчасти");
            Console.WriteLine("4 - показать склад");

            switch (Console.ReadLine())
            {
                case "1":
                    AcceptOrder(client, brokenPart, repairCost, hasPart);  
                    break;
                case "2":
                    RefuseOrder(client);  // Отказ клиенту
                    break;
                case "3":
                    ShowPurchaseMenu();  //меню закупок
                    break;
                case "4":
                    ShowWarehouse();  //склад
                    break;
                default:
                    Console.WriteLine("неверный выбор"); 
                    break;
            }

            totalCarsProcessed++;  
            SaveGameState();  

           
            if (money <= 0)
            {
                GameOver();  
                return;  
            }
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

                Console.WriteLine($"доставлены {order.Quantity} {order.PartName}");  
                purchaseOrders.Remove(order);  // Удаляем заказ
            }
        }
    }

    private void ShowStatus()
    {
        Console.WriteLine($"\nбаланс: {money}");  
        Console.WriteLine($"обслужено: {totalCarsProcessed}");  
        Console.WriteLine("склад:");  

        
        if (warehouse.Count == 0)
            Console.WriteLine("  (пусто)");
        else
            
            foreach (var part in warehouse.Where(p => p.Value > 0))
                Console.WriteLine($"  {part.Key}: {part.Value} шт.");
    }

    
    private Client CreateNewClient() => new Client
    {
        CarBrand = carBrands[random.Next(carBrands.Length)],  
        CreatedDate = DateTime.Now  
    };

   
    private Parts GetRandomSparePart()
    {
        var parts = Core.Context.Parts.ToList();  
       
        return parts.Count > 0 ? parts[random.Next(parts.Count)] :
            new Parts { ID = 5, Name = "бампер", Price = 1200 };
    }

   
    private decimal CalculateRepairCost(Parts part)
    {
        return part.Price + random.Next(1000, 2001);  
    }

    
    private void AcceptOrder(Client client, Parts brokenPart, decimal repairCost, bool hasPart)
    {
        
        if (hasPart)
        {
            warehouse[brokenPart.Name]--;  
            money += repairCost;  
            Console.WriteLine($"прибыль: {repairCost}");  
        }
        else
        {
            Console.WriteLine("\nнужной детали нет на складе");  
            var availableParts = warehouse.Where(p => p.Value > 0).ToList();  

            
            if (availableParts.Count > 0)
            {
                var randomPart = availableParts[random.Next(availableParts.Count)];  
                warehouse[randomPart.Key]--;  
                money -= repairCost + 5000;  
                Console.WriteLine($"вы поставили {randomPart.Key} вместо {brokenPart.Name}");  
            }
            else
            {
                money -= repairCost + 10000;  
                Console.WriteLine($"на складе нет деталей для замены");  
            }
        }
        SaveGameState();  
    }

    private void RefuseOrder(Client client)
    {
        money -= 2000;  
        SaveGameState();  
        Console.WriteLine($"\nотказ клиенту,  штраф: 2000"); 
    }

    private void ShowPurchaseMenu()
    {
        while (true)
        {
            Console.Clear();  
            Console.WriteLine("закупка запчастей");  
            Console.WriteLine($"баланс: {money}\n");  

            var parts = Core.Context.Parts.ToList();  
            int i = 1;  

            foreach (var part in parts)
            {
                var stock = warehouse.ContainsKey(part.Name) ? warehouse[part.Name] : 0;  // Текущее количество на складе
                Console.WriteLine($"{i} - {part.Name}: {part.Price}/шт. (на складе: {stock} шт.)");  // Информация о детали
                i++;  
            }
            Console.WriteLine($"{i} - врнуться к клиенту");  
          
            if (int.TryParse(Console.ReadLine(), out int choice) && choice == i) break;  

            if (choice >= 1 && choice <= parts.Count)
            {
                var part = parts[choice - 1]; 
                Console.Write($"введите кол-во {part.Name} "); 

                if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0)
                {
                    decimal totalCost = part.Price * quantity; 

                    if (totalCost <= money)
                    {
                        money -= totalCost;  
                        purchaseOrders.Add(new PurchaseOrder(part.Name, quantity, 2)); 
                        SaveGameState(); 
                        Console.WriteLine("заказ на оформлен");  
                    }
                    else Console.WriteLine("нет денег");  
                }
                else Console.WriteLine("неверно");  
            }
            else Console.WriteLine("неверно"); 
        }
    }

    private void ShowWarehouse()
    {
        Console.Clear();  
        Console.WriteLine("склад");  
        
        if (warehouse.Count == 0) Console.WriteLine("склад пуст");
        else
            // вывод всех деталей с количеством больше 0
            foreach (var part in warehouse.Where(p => p.Value > 0))
                Console.WriteLine($"{part.Key}: {part.Value} шт.");
    }

    private void SaveGameState()
    {
        var context = Core.Context; 

        var warehouseData = context.WareHouse.FirstOrDefault(w => w.Id == currentWarehouseId); 
        if (warehouseData != null)
        {
            warehouseData.BBalance = money; 
        }
        else
        {
            context.WareHouse.Add(new WareHouse { Id = currentWarehouseId, BBalance = money });  // создаем новый склад
        }

        foreach (var partEntry in warehouse)
        {
            var part = context.Parts.FirstOrDefault(p => p.Name == partEntry.Key);  
            if (part == null) continue;  // Пропускаем если деталь не найдена

            // Ищем запись о детали на складе
            var warehousePart = context.WarehouseParts
                .FirstOrDefault(wp => wp.WarehouseID == currentWarehouseId && wp.PartsID == part.ID);

            // Если запись найдена - обновляем количество, иначе создаем новую
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

        context.SaveChanges(); 
    }

    private void GameOver()
    {
        Console.Clear();  
        Console.WriteLine("-игра окончена-");  
        Console.WriteLine($"баланс: {money}"); 
        Console.WriteLine($"обслужено клиентов: {totalCarsProcessed}"); 
        Environment.Exit(0); 
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
            Console.WriteLine("автосервис");  
            new AutoServiceGame().RunGame();  
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ошибка: {ex.Message}");  
            Console.ReadKey();
    }
}



//if (parts.Count > 0)
//{
//    int randomIndex = random.Next(parts.Count);  // Случайный индекс
//    return parts[randomIndex];                   // Вернуть случайную деталь
//}
//else
//{
//    return new Parts { ID = 5, Name = "бампер", Price = 1200 };  // Запасная деталь
//}