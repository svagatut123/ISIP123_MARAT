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

namespace AutoServiceSimulator
{
    /// <summary>
    /// Управляет игровым процессом
    /// </summary>
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