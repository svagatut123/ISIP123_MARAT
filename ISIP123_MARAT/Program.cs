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
using System.Xml.Linq;

namespace AutoServiceSimulator
{
    public partial class AutoService
    {
        /// <summary>
        /// Принять заказ на ремонт
        /// </summary>
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

        /// <summary>
        /// Отклонить заказ на ремонт
        /// </summary>
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

        /// <summary>
        /// Показать каталог доступных деталей
        /// </summary>
        public void ShowPartsCatalog()
        {
            Console.WriteLine("\n=== КАТАЛОГ ДЕТАЛЕЙ ===");
            for (int i = 0; i < AvailablePartTypes.Count; i++)
            {
                var part = AvailablePartTypes[i];
                Console.WriteLine($"{i + 1}. {part}");
            }
        }

        /// <summary>
        /// Покупка запчастей
        /// </summary>
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

        /// <summary>
        /// Обработка доставок заказов
        /// </summary>
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

        /// <summary>
        /// Показать статус автосервиса
        /// </summary>
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

        /// <summary>
        /// Показать историю заказов
        /// </summary>
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
}