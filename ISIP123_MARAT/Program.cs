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
using System.Runtime.ConstrainedExecution;

namespace AutoServiceSimulator
{
    /// <summary>
    /// Заказ на поставку запчастей
    /// </summary>
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

    /// <summary>
    /// Главный класс, управляющий всей логикой игры
    /// </summary>
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

        /// <summary>
        /// Инициализация начальными данными
        /// </summary>
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

        /// <summary>
        /// Обновление баланса с проверкой
        /// </summary>
        private void UpdateBalance(decimal amount)
        {
            Balance += amount;
            if (Balance < 0)
            {
                Balance = 0; // Баланс не может быть отрицательным
            }
        }

        /// <summary>
        /// Создание нового клиента со случайной поломкой
        /// </summary>
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
    }
}