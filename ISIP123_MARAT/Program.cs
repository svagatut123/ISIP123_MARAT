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
    /// <summary>
    /// Управляет складом запчастей
    /// </summary>
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

    /// <summary>
    /// Представляет клиента и его автомобиль
    /// </summary>
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

    /// <summary>
    /// Заказ на ремонт
    /// </summary>
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
}