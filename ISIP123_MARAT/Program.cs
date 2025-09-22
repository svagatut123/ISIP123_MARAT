using System;
using System.Collections.Generic;
using System.Linq;

public class Product
{
    public int ProductID { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public Product(int productId, string name, decimal price, int quantity)
    {
        ProductID = productId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }
    public void PrintInfo()
    {
        Console.WriteLine($"\n id товара: {ProductID}");
        Console.WriteLine($"название: {Name}");
        Console.WriteLine($"цена: {Price}");
        Console.WriteLine($"количество: {Quantity}");
        Console.WriteLine($"oбщ стоимость: {Price * Quantity}");
    }
}

class Program
{
    static List<Product> products = new List<Product>();

    static void Main(string[] args)
    {
        bool running = true;

        while (running)
        {
            Console.WriteLine("\nуправление товарами");
            Console.WriteLine("1. добавить товар");
            Console.WriteLine("2. удалить товар");
            Console.WriteLine("3. показать все товары");
            Console.WriteLine("4. выход");
            Console.Write("выберите действие: ");
        

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
                    ShowAllProducts();
                    break;
                case "4":
                    running = false;
                    Console.WriteLine("gg");
                    break;
                default:
                    Console.WriteLine("не то");
                    break;
            }
        }
    }

    static void AddProduct()
    {
        try
        {
            Console.WriteLine("\nдобавление нового товара");

            Console.Write("введите ID товара: ");
            int id = int.Parse(Console.ReadLine());

            if (products.Any(p => p.ProductID == id))
            {
                Console.WriteLine("товар с таким ID уже существует");
                return;
            }

            Console.Write("введите название товара: ");
            string name = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("название не может быть пустым");
                return;
            }

            Console.Write("введите цену товара: ");
            decimal price = decimal.Parse(Console.ReadLine());

            if (price <= 0)
            {
                Console.WriteLine("цена должна быть больше 0");
                return;
            }

            Console.Write("введите количество товара: ");
            int quantity = int.Parse(Console.ReadLine());

            if (quantity < 0)
            {
                Console.WriteLine("количество не может быть отрицательным");
                return;
            }

            Product newProduct = new Product(id, name, price, quantity);
            products.Add(newProduct);

            Console.WriteLine(" товар успешно добавлен");
        }
        catch (FormatException)
        {
            Console.WriteLine(" ошибка ввода проверьте правильность введенных данных.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" произошла ошибка: {ex.Message}");
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

