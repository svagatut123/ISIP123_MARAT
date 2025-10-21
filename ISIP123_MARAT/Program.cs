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

            var autoService = new AutoService("Мой Автосервис", 10000m);
            autoService.InitializeStartingParts();

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
}