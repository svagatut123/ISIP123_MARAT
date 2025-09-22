using System;
using System.Collections.Generic;
using System.Linq;

public enum Category
{
    Электроника,
    Одежда,
    Продукты,
    Книги,
    Спорт
}

public class Product
{
    public string Code { get; set; }          
    public string Name { get; set; }          
    public decimal Price { get; set; }        
    public int Quantity { get; set; }         
    public bool InStock { get; set; }         
    public Category Category { get; set; }    

    public Product(string code, string name, decimal price, int quantity, Category category)
    {
        Code = code;
        Name = name;
        Price = price;
        Quantity = quantity;
        InStock = quantity > 0; 
        Category = category;
    }

    public void PrintInfo()
    {
        Console.WriteLine($"\nid товара: {Code}");
        Console.WriteLine($"название: {Name}");
        Console.WriteLine($"цена: {Price:C}");
        Console.WriteLine($"количество: {Quantity}");
        Console.WriteLine($"наличие: {(InStock ? "В наличии" : "Нет в наличии")}");
        Console.WriteLine($"категория: {Category}");
        Console.WriteLine($"общая стоимость: {Price * Quantity:C}");
    }

    public void UpdateStockStatus()
    {
        InStock = Quantity > 0;
    }
}

class Program
{
    static List<Product> products = new List<Product>();
    static int productCounter = 1; 

    static void Main(string[] args)
    {
        bool running = true;

        while (running)
        {
            Console.WriteLine("\nds,thbnt ltqcndbt");
            Console.WriteLine("1. Добавить товар");
            Console.WriteLine("2. Удалить товар");
            Console.WriteLine("3. Заказать поставку товара");
            Console.WriteLine("4. Продать товар");
            Console.WriteLine("5. Поиск товаров");
            Console.WriteLine("6. Показать все товары");
            Console.WriteLine("7. Выход");
            Console.Write("Выберите действие: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AddProduct();
                    break;
                case "2":
                    RemoveProduct();
                    break;
                case "3":
                    SupplyProduct();
                    break;
                case "4":
                    SellProduct();
                    break;
                case "5":
                    SearchProducts();
                    break;
                case "6":
                    ShowAllProducts();
                    break;
                case "7":
                    running = false;
                    Console.WriteLine("gg");
                    break;
                default:
                    Console.WriteLine("Неверный выбор");
                    break;
            }
        }
    }
    static string GenerateProductCode()
    {
        return "1" + productCounter++.ToString("D3");  
    }

    static void AddProduct()
    {
        try
        {
            Console.WriteLine("\nдобавление товара");

            string code = GenerateProductCode();
            Console.WriteLine($"код: {code}");

            Console.Write("Введите название товара: ");
            string name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Название не может быть пустым");
                return;
            }

            Console.Write("Введите цену товара: ");
            decimal price = decimal.Parse(Console.ReadLine());

            if (price <= 0)
            {
                Console.WriteLine("Цена должна быть больше 0");
                return;
            }

            Console.Write("Введите количество товара: ");
            int quantity = int.Parse(Console.ReadLine());

            if (quantity < 0)
            {
                Console.WriteLine("Количество не может быть отрицательным");
                return;
            }

            Console.WriteLine("\nдоступные категории:");
            foreach (var category in Enum.GetValues(typeof(Category)))
            {
                Console.WriteLine($"{(int)category}. {category}");
            }

            Console.Write("Выберите категорию(номер): ");
            int categoryIndex = int.Parse(Console.ReadLine());

            if (!Enum.IsDefined(typeof(Category), categoryIndex))
            {
                Console.WriteLine("Неверный номер категории");
                return;
            }

            Category selectedCategory = (Category)categoryIndex;

            Product newProduct = new Product(code, name, price, quantity, selectedCategory);
            products.Add(newProduct);

            Console.WriteLine($"товар успешно добавлен с кодом {code}!");
        }
        catch (FormatException)
        {
            Console.WriteLine("oшибка ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
    }

    static void RemoveProduct()
    {
        if (products.Count == 0)
        {
            Console.WriteLine(" список товаров пуст");
            return;
        }

        try
        {
            Console.WriteLine("\nудаление товара");
            Console.Write("введите id товара для удаления: ");
            int id = int.Parse(Console.ReadLine());

            Product productToRemove = products.FirstOrDefault(p => p.ProductID == id);

            if (productToRemove != null)
            {
                products.Remove(productToRemove);
                Console.WriteLine("товар успешно удален");
            }
            else
            {
                Console.WriteLine("товар с таким id не найден");
            }
        }
        catch (FormatException)
        {
            Console.WriteLine("неверный формат id");
        }
    }

    static void ShowAllProducts()
    {
        if (products.Count == 0)
        {
            Console.WriteLine("список пуст");
            return;
        }

        Console.WriteLine("\nсписок всех товаров");
        foreach (var product in products)
        {
            product.PrintInfo();
        }

        Console.WriteLine($"\nвсего товаров: {products.Count}");
        Console.WriteLine($"общ. стоимость всех товаров: {products.Sum(p => p.Price * p.Quantity)}");
    }

}

