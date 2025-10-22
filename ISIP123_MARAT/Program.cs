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
using System.Data.SqlClient; 
using System.Linq; 

namespace AutoServiceSimulator {
    class Program 
    {
        static void Main(string[] args) 
        {
            Console.WriteLine("=== АВТОСЕРВИС ==="); 

            string connectionString = @"Server=localhost;Database=AutoServiceDB;Trusted_Connection=true;"; 

            try //обработка исключений
            {
                Game game = new Game(connectionString); 
                game.Start(); 
            }
            catch (Exception ex) // Обработка всех исключений
            {
                Console.WriteLine($"Ошибка: {ex.Message}"); 
                Console.WriteLine("Проверьте подключение к базе данных SQL Server"); 
                Console.ReadLine(); 
            }
        }
    }

    public enum RepairOrderStatus //перечесление статуса заказа
    {
        Pending = 0, // ожидает обработки
        InProgress = 1, // принят в работу
        Completed = 2, // успешно завершен
        Failed = 3, // завершен с ошибкой
        Declined = 4 //отклонен
    }

    public class SparePart // запчасти
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public decimal PurchasePrice { get; set; } // Цена покупки 
        public decimal SellPrice { get; set; } // Цена продажи/ремонта 

        public SparePart(int id, string name, decimal purchasePrice, decimal sellPrice) 
        {
            Id = id; 
            Name = name;
            PurchasePrice = purchasePrice; 
            SellPrice = sellPrice; 
        }

        public override string ToString() // Переопределение метода для строкового представления
        {
            return string.Format("{0} (Покупка: {1:C}, Ремонт: {2:C})", Name, PurchasePrice, SellPrice); // Форматированная строка с информацией о запчасти
        }
    }

    public class RepairOrder //заказ на ремонт
    {
        public int Id { get; set; } 
        public string ClientName { get; set; } 
        public string CarModel { get; set; } 
        public string BrokenPartName { get; set; }
        public decimal RepairCost { get; set; } 
        public RepairOrderStatus Status { get; set; } 
        public decimal Profit { get; set; } 
        public DateTime CreatedDate { get; set; } 

        public override string ToString() 
        {
            string statusText; 
            switch (Status) 
            {
                case RepairOrderStatus.Pending:
                    statusText = "Ожидает"; 
                    break;
                case RepairOrderStatus.InProgress:
                    statusText = "В работе"; 
                    break;
                case RepairOrderStatus.Completed:
                    statusText = "Завершен"; 
                    break;
                case RepairOrderStatus.Failed:
                    statusText = "Провален"; 
                    break;
                case RepairOrderStatus.Declined:
                    statusText = "Отклонен"; 
                    break;
                default:
                    statusText = "Неизвестен"; 
                    break;
            }

            return string.Format("Заказ #{0}: {1} ({2}) - {3} - {4} ({5:C})", 
                Id, ClientName, CarModel, BrokenPartName, statusText, Profit);
        }
    }

public class SupplyOrder
{
    public int Id { get; set; }
    public decimal TotalCost { get; set; }
    public int CarsUntilDelivery { get; set; }

    public void DecrementDeliveryCounter()
    {
        if (CarsUntilDelivery > 0)
            CarsUntilDelivery--; // Уменьшение счетчика на 1
    }

    public bool IsReadyForDelivery()
    {
        return CarsUntilDelivery <= 0; // Возвращает тру, если доставка готова
    }

    public override string ToString()
    {
        return string.Format("Заказ #{0}: доставка через {1} машин", Id, CarsUntilDelivery);
    }
}
    public class DatabaseService 
    {
        private string _connectionString; 

        public DatabaseService(string connectionString) 
        {
            _connectionString = connectionString; 
        }

        public Tuple<decimal, int, string> GetGameState()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) // Создание подключения к БД
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand( // Создание SQL команды
                    "SELECT Balance, TotalCarsProcessed, ServiceName FROM GameState WHERE Id = 1", // SQL запрос
                    connection);

                using (SqlDataReader reader = command.ExecuteReader()) 
                {
                    if (reader.Read()) // Чтение первой строки результата
                    {
                        return new Tuple<decimal, int, string>( // Возврат строки с данными
                            reader.GetDecimal(0),
                            reader.GetInt32(1), 
                            reader.GetString(2) 
                        );
                    }
                }
            }

            return new Tuple<decimal, int, string>(10000m, 0, "Мой Автосервис"); // Возврат значений по умолчанию
        }

        public void UpdateGameState(decimal balance, int totalCars)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open();

                SqlCommand command = new SqlCommand( 
                    "UPDATE GameState SET Balance = @Balance, TotalCarsProcessed = @TotalCars, LastUpdated = GETDATE() WHERE Id = 1", 
                    connection);
                command.Parameters.AddWithValue("@Balance", balance); 
                command.Parameters.AddWithValue("@TotalCars", totalCars); 

                command.ExecuteNonQuery(); 
            }
        }

        
        public List<SparePart> GetAllSpareParts()
        {
            List<SparePart> parts = new List<SparePart>(); 

            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand("SELECT Id, Name, PurchasePrice, SellPrice FROM SpareParts", connection); 

                using (SqlDataReader reader = command.ExecuteReader()) 
                {
                    while (reader.Read()) 
                    {
                        parts.Add(new SparePart( 
                            reader.GetInt32(0), 
                            reader.GetString(1), 
                            reader.GetDecimal(2), 
                            reader.GetDecimal(3) 
                        ));
                    }
                }
            }

            return parts; 
        }

        
        public Dictionary<int, int> GetWarehouseStock()
        {
            Dictionary<int, int> stock = new Dictionary<int, int>(); // Создание словаря для хранения запасов

            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand( 
                    "SELECT w.SparePartId, w.Quantity " + 
                    "FROM Warehouse w " + 
                    "INNER JOIN SpareParts s ON w.SparePartId = s.Id " + 
                    "WHERE w.Quantity > 0", 
                    connection);

                using (SqlDataReader reader = command.ExecuteReader()) 
                {
                    while (reader.Read()) // Чтение всех строк результата
                    {
                        stock[reader.GetInt32(0)] = reader.GetInt32(1); // Добавление в словарь
                    }
                }
            }

            return stock; 
        }

        public void UpdateWarehouseQuantity(int sparePartId, int quantity)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand( 
                    "UPDATE Warehouse SET Quantity = @Quantity, LastUpdated = GETDATE() WHERE SparePartId = @SparePartId", 
                    connection);
                command.Parameters.AddWithValue("@SparePartId", sparePartId); 
                command.Parameters.AddWithValue("@Quantity", quantity); 

                command.ExecuteNonQuery(); 
            }
        }

        public int CreateClient(string name, string carModel)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand( 
                    "INSERT INTO Clients (Name, CarModel) OUTPUT INSERTED.Id VALUES (@Name, @CarModel)", 
                    connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@CarModel", carModel);

                return (int)command.ExecuteScalar();
            }
        }

        public int CreateRepairOrder(int clientId, int brokenPartId, int status, decimal profit)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand( 
                    "INSERT INTO RepairOrders (ClientId, BrokenPartId, Status, Profit) OUTPUT INSERTED.Id VALUES (@ClientId, @BrokenPartId, @Status, @Profit)", // SQL запрос
                    connection);
                command.Parameters.AddWithValue("@ClientId", clientId);
                command.Parameters.AddWithValue("@BrokenPartId", brokenPartId); 
                command.Parameters.AddWithValue("@Status", status); 
                command.Parameters.AddWithValue("@Profit", profit);

                return (int)command.ExecuteScalar(); 
            }
        }

        public List<RepairOrder> GetRecentRepairOrders(int count = 10)
        {
            List<RepairOrder> orders = new List<RepairOrder>(); 

            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open();

                SqlCommand command = new SqlCommand( 
                    "SELECT TOP (@Count) ro.Id, c.Name, c.CarModel, sp.Name, sp.SellPrice, ro.Status, ro.Profit, ro.CreatedDate " + // Выбор полей
                    "FROM RepairOrders ro " + 
                    "INNER JOIN Clients c ON ro.ClientId = c.Id " +
                    "INNER JOIN SpareParts sp ON ro.BrokenPartId = sp.Id " + 
                    "ORDER BY ro.CreatedDate DESC", 
                    connection);
                command.Parameters.AddWithValue("@Count", count); 

                using (SqlDataReader reader = command.ExecuteReader()) 
                {
                    while (reader.Read()) 
                    {
                        RepairOrder order = new RepairOrder 
                        {
                            Id = reader.GetInt32(0), 
                            ClientName = reader.GetString(1), 
                            CarModel = reader.GetString(2), 
                            BrokenPartName = reader.GetString(3), 
                            RepairCost = reader.GetDecimal(4), 
                            Status = (RepairOrderStatus)reader.GetInt32(5), 
                            Profit = reader.GetDecimal(6), 
                            CreatedDate = reader.GetDateTime(7) 
                        };
                        orders.Add(order); 
                    }
                }
            }

            return orders; 
        }

        public int CreateSupplyOrder(decimal totalCost)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand(
                    "INSERT INTO SupplyOrders (TotalCost, CarsUntilDelivery) OUTPUT INSERTED.Id VALUES (@TotalCost, 2)", 
                    connection);
                command.Parameters.AddWithValue("@TotalCost", totalCost);

                return (int)command.ExecuteScalar(); 
            }
        }

        public void AddSupplyOrderItem(int supplyOrderId, int sparePartId, int quantity)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand( 
                    "INSERT INTO SupplyOrderItems (SupplyOrderId, SparePartId, Quantity) VALUES (@SupplyOrderId, @SparePartId, @Quantity)",
                    connection);
                command.Parameters.AddWithValue("@SupplyOrderId", supplyOrderId); 
                command.Parameters.AddWithValue("@SparePartId", sparePartId); 
                command.Parameters.AddWithValue("@Quantity", quantity); 

                command.ExecuteNonQuery(); 
            }
        }

        public List<SupplyOrder> GetPendingSupplyOrders()
        {
            List<SupplyOrder> orders = new List<SupplyOrder>(); 

            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand( 
                    "SELECT Id, TotalCost, CarsUntilDelivery FROM SupplyOrders WHERE IsDelivered = 0", 
                    connection);

                using (SqlDataReader reader = command.ExecuteReader()) 
                {
                    while (reader.Read())
                    {
                        SupplyOrder order = new SupplyOrder 
                        {
                            Id = reader.GetInt32(0), 
                            TotalCost = reader.GetDecimal(1), 
                            CarsUntilDelivery = reader.GetInt32(2) 
                        };
                        orders.Add(order); 
                    }
                }
            }

            return orders; 
        }

        
        public void UpdateSupplyOrderDelivery(int supplyOrderId, int carsUntilDelivery)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand( 
                    "UPDATE SupplyOrders SET CarsUntilDelivery = @CarsUntilDelivery WHERE Id = @SupplyOrderId", 
                    connection);
                command.Parameters.AddWithValue("@SupplyOrderId", supplyOrderId); 
                command.Parameters.AddWithValue("@CarsUntilDelivery", carsUntilDelivery); 

                command.ExecuteNonQuery(); 
            }
        }

        public void MarkSupplyOrderAsDelivered(int supplyOrderId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand(
                    "UPDATE SupplyOrders SET IsDelivered = 1 WHERE Id = @SupplyOrderId",
                    connection);
                command.Parameters.AddWithValue("@SupplyOrderId", supplyOrderId); 

                command.ExecuteNonQuery(); 
            }
        }

        public Tuple<int, int, int, int, decimal> GetRepairOrderStats()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand( 
                    "SELECT COUNT(*), " +
                    "SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END), " + 
                    "SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END), " + 
                    "SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END), " + 
                    "SUM(Profit) FROM RepairOrders", 
                    connection);

                using (SqlDataReader reader = command.ExecuteReader()) 
                {
                    if (reader.Read()) 
                    {
                        decimal totalProfit = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4); 
                        return new Tuple<int, int, int, int, decimal>( 
                            reader.GetInt32(0),
                            reader.GetInt32(1), 
                            reader.GetInt32(2), 
                            reader.GetInt32(3), 
                            totalProfit 
                        );
                    }
                }
            }

            return new Tuple<int, int, int, int, decimal>(0, 0, 0, 0, 0); 
        }

        public SparePart GetSparePartById(int partId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 

                SqlCommand command = new SqlCommand(
                    "SELECT Id, Name, PurchasePrice, SellPrice FROM SpareParts WHERE Id = @Id", 
                    connection);
                command.Parameters.AddWithValue("@Id", partId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read()) 
                    {
                        return new SparePart( 
                            reader.GetInt32(0), 
                            reader.GetString(1), 
                            reader.GetDecimal(2), 
                            reader.GetDecimal(3) 
                        );
                    }
                }
            }

            return null; 
        }

        
        public Dictionary<int, int> GetSupplyOrderItems(int supplyOrderId)
        {
            Dictionary<int, int> items = new Dictionary<int, int>();

            using (SqlConnection connection = new SqlConnection(_connectionString)) 
            {
                connection.Open(); 
                SqlCommand command = new SqlCommand( 
                    "SELECT SparePartId, Quantity FROM SupplyOrderItems WHERE SupplyOrderId = @SupplyOrderId", 
                    connection);
                command.Parameters.AddWithValue("@SupplyOrderId", supplyOrderId); 

                using (SqlDataReader reader = command.ExecuteReader()) 
                {
                    while (reader.Read()) 
                    {
                        items[reader.GetInt32(0)] = reader.GetInt32(1); 
                    }
                }
            }

            return items; 
        }
    }

    public class AutoService 
    {
        public string Name { get; set; }
        public decimal Balance { get; private set; }
        public int TotalCarsProcessed { get; set; } 

        private DatabaseService _db;
        private Random _random; 
        private List<SparePart> _allParts; 

        public AutoService(string connectionString) 
        {
            _db = new DatabaseService(connectionString);
            _random = new Random();

           
            LoadGameState(); 
            _allParts = _db.GetAllSpareParts();
        }

        private void LoadGameState() 
        {
            Tuple<decimal, int, string> state = _db.GetGameState(); 
            Balance = state.Item1; 
            TotalCarsProcessed = state.Item2; 
            Name = state.Item3; 
        }

        private void SaveGameState() 
        {
            _db.UpdateGameState(Balance, TotalCarsProcessed);
        }

        public void UpdateBalance(decimal amount) 
        {
            Balance += amount; 
            if (Balance < 0) Balance = 0; 
            SaveGameState(); 
        }


        public Tuple<string, string, SparePart> GenerateRandomClient()
        {
            string[] carModels = { "Toyota Camry", "Honda Civic", "BMW X5", "Ford Focus", "Lada Vesta" };
            string[] clientNames = { "Иван Петров", "Мария Сидорова", "Алексей Козлов", "Ольга Орлова" };

            string randomModel = carModels[_random.Next(carModels.Length)];
            SparePart randomPart = _allParts[_random.Next(_allParts.Count)];
            string randomName = clientNames[_random.Next(clientNames.Length)];

            return new Tuple<string, string, SparePart>(randomName, randomModel, randomPart);
        }

        
        public bool IsPartAvailable(int sparePartId)
        {
            Dictionary<int, int> stock = _db.GetWarehouseStock(); // Получение текущих запасов
            return stock.ContainsKey(sparePartId) && stock[sparePartId] > 0; // Проверка наличия и количества
        }

        
        public void AcceptRepairOrder(string clientName, string carModel, SparePart brokenPart)
        {
            Dictionary<int, int> stock = _db.GetWarehouseStock(); 
            int currentQuantity = stock.ContainsKey(brokenPart.Id) ? stock[brokenPart.Id] : 0; 

            if (currentQuantity > 0) 
            {
                
                _db.UpdateWarehouseQuantity(brokenPart.Id, currentQuantity - 1);
                int clientId = _db.CreateClient(clientName, carModel); 
                _db.CreateRepairOrder(clientId, brokenPart.Id, (int)RepairOrderStatus.Completed, brokenPart.SellPrice); 
                UpdateBalance(brokenPart.SellPrice); 
                Console.WriteLine("Ремонт завершен успешно! Заработано: {0:C}", brokenPart.SellPrice); 
            }
            else 
            {
                
                List<KeyValuePair<int, int>> availableParts = stock.Where(s => s.Value > 0).ToList();
                if (availableParts.Count > 0)
                {
                    int randomPartId = availableParts[_random.Next(availableParts.Count)].Key;
                    SparePart wrongPart = _db.GetSparePartById(randomPartId);
                    _db.UpdateWarehouseQuantity(randomPartId, stock[randomPartId] - 1);
                    int clientId = _db.CreateClient(clientName, carModel);

                    decimal penalty = -brokenPart.SellPrice * 2 - wrongPart.PurchasePrice;
                    _db.CreateRepairOrder(clientId, brokenPart.Id, (int)RepairOrderStatus.Failed, penalty);

                    UpdateBalance(penalty);
                    Console.WriteLine("ОШИБКА! Установлена не та деталь. Убыток: {0:C}", penalty);
                }
                else
                {

                    int clientId = _db.CreateClient(clientName, carModel);
                    _db.CreateRepairOrder(clientId, brokenPart.Id, (int)RepairOrderStatus.Declined, -100);

                    UpdateBalance(-100);
                    Console.WriteLine("Нет деталей. Заказ отклонен. Штраф: 100 руб");
                }
            }

            TotalCarsProcessed++; 
            SaveGameState(); 
            ProcessDeliveries(); 
        }

        // Метод отклонения заказа
        public void DeclineRepairOrder(string clientName, string carModel, SparePart brokenPart)
        {
            int clientId = _db.CreateClient(clientName, carModel); // Создание клиента в БД
            _db.CreateRepairOrder(clientId, brokenPart.Id, (int)RepairOrderStatus.Declined, -100); // Создание отклоненного заказа

            UpdateBalance(-100); 
            TotalCarsProcessed++; 
            SaveGameState(); 
            Console.WriteLine(" Заказ отклонен. Штраф: 100 руб"); 

            ProcessDeliveries(); 
        }

        public void ShowPartsCatalog()
        {
            Console.WriteLine("\n=== КАТАЛОГ ДЕТАЛЕЙ ==="); 
            foreach (SparePart part in _allParts) // Перебор всех запчастей
            {
                Console.WriteLine("{0}. {1}", part.Id, part); 
            }
        }

        
        public void PurchaseParts()
        {
            ShowPartsCatalog(); // Показ каталога
            Console.WriteLine("\nБаланс: {0:C}", Balance); // Показ текущего баланса

            decimal totalCost = 0; // Общая стоимость заказа
            Dictionary<int, int> orderItems = new Dictionary<int, int>(); // Словарь для хранения заказанных деталей

            while (true) 
            {
                Console.Write("\nВведите ID детали (0 - завершить): "); 
                string input = Console.ReadLine(); 
                int partId;
                if (!int.TryParse(input, out partId) || partId < 0 || partId > _allParts.Count) 
                {
                    Console.WriteLine("Неверный ID!"); 
                    continue; 
                }

                if (partId == 0) break; 

                SparePart part = _allParts.FirstOrDefault(p => p.Id == partId); 
                if (part == null) 
                {
                    Console.WriteLine("Деталь не найдена!"); 
                    continue; 
                }

                Console.Write("Количество (макс: {0}): ", (int)((Balance - totalCost) / part.PurchasePrice)); 
                input = Console.ReadLine();
                int quantity; 
                if (!int.TryParse(input, out quantity) || quantity <= 0) 
                {
                    Console.WriteLine("Неверное количество!"); 
                    continue; 
                }

                decimal cost = part.PurchasePrice * quantity; 
                if (totalCost + cost > Balance) 
                {
                    Console.WriteLine("Недостаточно средств!"); 
                    continue; 
                }

                if (orderItems.ContainsKey(partId)) 
                {
                    orderItems[partId] += quantity;  
                }
                else 
                {
                    orderItems[partId] = quantity; 
                }

                totalCost += cost; 
                Console.WriteLine("Добавлено: {0} x{1} = {2:C}", part.Name, quantity, cost); 
            }

            if (orderItems.Count > 0) 
            {
                int orderId = _db.CreateSupplyOrder(totalCost); 
                foreach (KeyValuePair<int, int> item in orderItems) // Перебор всех заказанных деталей
                {
                    _db.AddSupplyOrderItem(orderId, item.Key, item.Value); // Добавление детали в заказ
                }

                UpdateBalance(-totalCost); 
                Console.WriteLine("\n Заказ #{0} создан! Доставка через 2 машины.", orderId); // Сообщение об успехе
            }
        }

        
        private void ProcessDeliveries()
        {
            List<SupplyOrder> orders = _db.GetPendingSupplyOrders(); // Получение ожидающих заказов
            foreach (SupplyOrder order in orders) // Перебор всех заказов
            {
                order.DecrementDeliveryCounter(); // Уменьшение счетчика доставки
                _db.UpdateSupplyOrderDelivery(order.Id, order.CarsUntilDelivery); // Обновление в БД

                if (order.IsReadyForDelivery()) // Если заказ готов к доставке
                {
                    
                    Dictionary<int, int> orderItems = _db.GetSupplyOrderItems(order.Id); // Получение деталей заказа
                    foreach (KeyValuePair<int, int> item in orderItems) // Перебор всех деталей
                    {
                        Dictionary<int, int> stock = _db.GetWarehouseStock(); // Получение текущих запасов
                        int currentQty = stock.ContainsKey(item.Key) ? stock[item.Key] : 0; // Получение текущего количества
                        _db.UpdateWarehouseQuantity(item.Key, currentQty + item.Value); // Увеличение количества на складе
                    }

                    _db.MarkSupplyOrderAsDelivered(order.Id); // Отметка заказа как доставленного
                    Console.WriteLine("\n Доставлен заказ #{0}!", order.Id); 
                }
            }
        }

        
        public void ShowStatus()
        {
            Console.WriteLine("\n=== {0} ===", Name.ToUpper()); 
            Console.WriteLine("Баланс: {0:C}", Balance); 
            Console.WriteLine("Обработано машин: {0}", TotalCarsProcessed); // Показ количества машин

            Dictionary<int, int> stock = _db.GetWarehouseStock(); // Получение запасов
            Console.WriteLine("\n=== СКЛАД ==="); // Заголовок склада
            if (stock.Count == 0) 
            {
                Console.WriteLine("Склад пуст"); 
            }
            else 
            {
                foreach (KeyValuePair<int, int> item in stock) // Перебор всех деталей
                {
                    SparePart part = _allParts.First(p => p.Id == item.Key); // Поиск информации о детали
                    Console.WriteLine("- {0}: {1} шт.", part.Name, item.Value); 
                }
            }

            List<SupplyOrder> pendingOrders = _db.GetPendingSupplyOrders(); // Получение ожидающих поставок
            if (pendingOrders.Count > 0) // Если есть ожидающие поставки
            {
                Console.WriteLine("\n=== ЗАКАЗЫ НА ПОСТАВКУ ==="); 
                foreach (SupplyOrder order in pendingOrders) // Перебор всех поставок
                {
                    Console.WriteLine("- {0}", order); 
                }
            }
        }

        
        public void ShowOrderHistory()
        {
            List<RepairOrder> orders = _db.GetRecentRepairOrders(10); // Получение последних заказов
            Tuple<int, int, int, int, decimal> stats = _db.GetRepairOrderStats(); // Получение статистики

            Console.WriteLine("\n=== ИСТОРИЯ ЗАКАЗОВ ==="); 
            foreach (RepairOrder order in orders) 
            {
                Console.WriteLine("- {0}", order); 
            }

            Console.WriteLine("\nВсего: {0} заказов", stats.Item1); 
            Console.WriteLine("Успешных: {0}, Неудачных: {1}, Отклоненных: {2}", stats.Item2, stats.Item3, stats.Item4); 
            Console.WriteLine("Общая прибыль: {0:C}", stats.Item5); 
        }
    }

    public class Game 
    {
        private AutoService _service; 
        private string _connectionString; 

        public Game(string connectionString) 
        {
            _connectionString = connectionString; 
        }

        public void Start() 
        {
            _service = new AutoService(_connectionString); 

            Console.WriteLine("Добро пожаловать в {0}!", _service.Name); 
            Console.WriteLine("Ваша задача - ремонтировать автомобили и зарабатывать деньги."); 
            Console.WriteLine("Будьте осторожны: неправильный ремонт приведет к убыткам!\n"); 

            while (_service.Balance > 0) 
            {
                ProcessClient(); 
                if (_service.Balance <= 0) break; 

                Console.WriteLine("\nНажмите Enter для следующего клиента..."); 
                Console.ReadLine(); 
            }

            Console.WriteLine("\nБАНКРОТ! Игра окончена."); 
            _service.ShowOrderHistory(); 
            Console.ReadLine(); 
        }

        private void ProcessClient() 
        {
            Console.Clear(); 
            _service.ShowStatus(); 

            Tuple<string, string, SparePart> clientInfo = _service.GenerateRandomClient(); 
            string clientName = clientInfo.Item1; 
            string carModel = clientInfo.Item2; 
            SparePart brokenPart = clientInfo.Item3; 

            Console.WriteLine("\n=== НОВЫЙ КЛИЕНТ ==="); 
            Console.WriteLine("Клиент: {0}", clientName); 
            Console.WriteLine("Автомобиль: {0}", carModel); 
            Console.WriteLine("Поломка: {0}", brokenPart.Name); 
            Console.WriteLine("Стоимость ремонта: {0:C}", brokenPart.SellPrice); 
            Console.WriteLine("Деталь на складе: {0}", (_service.IsPartAvailable(brokenPart.Id) ? "ЕСТЬ" : "НЕТ")); 

            Console.WriteLine("\n1. Принять заказ"); 
            Console.WriteLine("2. Отклонить заказ (штраф 100 руб)"); 
            Console.WriteLine("3. Купить запчасти"); 
            Console.WriteLine("4. История заказов"); 

            int choice = GetChoice(1, 4); 
            ProcessChoice(choice, clientName, carModel, brokenPart); 
        }

        private int GetChoice(int min, int max) 
        {
            while (true) 
            {
                Console.Write("Выбор ({0}-{1}): ", min, max); 
                string input = Console.ReadLine(); 
                int choice; 
                if (int.TryParse(input, out choice) && choice >= min && choice <= max) 
                    return choice; 
                Console.WriteLine("Неверный выбор!"); 
            }
        }

        private void ProcessChoice(int choice, string clientName, string carModel, SparePart brokenPart) 
        {
            switch (choice) 
            {
                case 1:
                    _service.AcceptRepairOrder(clientName, carModel, brokenPart); // Принятие заказа
                    break;
                case 2:
                    _service.DeclineRepairOrder(clientName, carModel, brokenPart); // Отклонение заказа
                    break;
                case 3:
                    _service.PurchaseParts(); // Покупка запчастей
                    break;
                case 4:
                    _service.ShowOrderHistory(); // Просмотр истории
                    Console.WriteLine("\nНажмите Enter чтобы продолжить..."); 
                    Console.ReadLine(); 
                    ProcessClient(); 
                    break;
            }
        }
    }
}

//--Создание базы данных
//CREATE DATABASE AutoServiceDB;
//GO

//USE AutoServiceDB;
//GO

//-- Таблица запчастей
//CREATE TABLE SpareParts (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    Name NVARCHAR(100) NOT NULL,
//    PurchasePrice DECIMAL(10,2) NOT NULL,
//    SellPrice DECIMAL(10,2) NOT NULL,
//    CreatedDate DATETIME2 DEFAULT GETDATE()
//);

//--Таблица клиентов
//CREATE TABLE Clients (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    Name NVARCHAR(100) NOT NULL,
//    CarModel NVARCHAR(100) NOT NULL,
//    CreatedDate DATETIME2 DEFAULT GETDATE()
//);

//--Таблица заказов на ремонт
//CREATE TABLE RepairOrders (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    ClientId INT NOT NULL,
//    BrokenPartId INT NOT NULL,
//    Status INT NOT NULL, -- 0=Pending, 1=InProgress, 2=Completed, 3=Failed, 4=Declined
//    Profit DECIMAL(10,2) NOT NULL,
//    CreatedDate DATETIME2 DEFAULT GETDATE(),
//    FOREIGN KEY (ClientId) REFERENCES Clients(Id),
//    FOREIGN KEY (BrokenPartId) REFERENCES SpareParts(Id)
//);

//--Таблица склада
//CREATE TABLE Warehouse (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    SparePartId INT NOT NULL,
//    Quantity INT NOT NULL,
//    LastUpdated DATETIME2 DEFAULT GETDATE(),
//    FOREIGN KEY (SparePartId) REFERENCES SpareParts(Id)
//);

//--Таблица заказов на поставку
//CREATE TABLE SupplyOrders (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    TotalCost DECIMAL(10,2) NOT NULL,
//    CarsUntilDelivery INT NOT NULL,
//    CreatedDate DATETIME2 DEFAULT GETDATE()
//);

//--Таблица деталей в заказах на поставку
//CREATE TABLE SupplyOrderItems (
//    Id INT PRIMARY KEY IDENTITY(1,1),
//    SupplyOrderId INT NOT NULL,
//    SparePartId INT NOT NULL,
//    Quantity INT NOT NULL,
//    FOREIGN KEY (SupplyOrderId) REFERENCES SupplyOrders(Id),
//    FOREIGN KEY (SparePartId) REFERENCES SpareParts(Id)
//);

//--Вставка начальных данных
//INSERT INTO SpareParts (Name, PurchasePrice, SellPrice) VALUES
//('Тормозные колодки', 2000.00, 3200.00),
//('Масляный фильтр', 500.00, 750.00),
//('Воздушный фильтр', 800.00, 1200.00),
//('Свечи зажигания', 1200.00, 1860.00),
//('Аккумулятор', 5000.00, 7000.00),
//('Шины', 4000.00, 5400.00),
//('Тормозные диски', 3500.00, 5075.00),
//('Амортизаторы', 6000.00, 9000.00);

//INSERT INTO Warehouse (SparePartId, Quantity) VALUES
//(1, 2), (2, 3), (3, 2);

//namespace AutoServiceSimulator
//{
//    /// <summary>
//    /// Класс для работы с базой данных
//    /// </summary>
//    public class DatabaseService
//    {
//        private string _connectionString;

//        public DatabaseService(string connectionString)
//        {
//            _connectionString = connectionString;
//        }

//        // Методы для работы с базой данных будут здесь
//        // В реальном приложении нужно реализовать:
//        // - Сохранение состояния игры
//        // - Загрузка состояния игры
//        // - Логирование всех операций
//    }
//}
