using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text.Json;

[Serializable]
public class User
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool IsDeleted { get; set; } // Флаг, указывающий на удаленного пользователя
    public string Role { get; set; } // Роль пользователя

    public User(string username, string password, string role = "user")
    {
        Username = username;
        Password = password;
        IsDeleted = false;
        Role = role; // Роль по умолчанию - "user"
    }
}

public class Product
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public Product(string name, int quantity, decimal price)
    {
        Name = name;
        Quantity = quantity;
        Price = price;
    }
}
public class Sale
{
    public string ProductName { get; set; }
    public int QuantitySold { get; set; }
    public decimal Price { get; set; }
    public decimal Profit { get; set; }
    public DateTime SaleDate { get; set; }

    public Sale(string productName, int quantitySold, decimal price)
    {
        ProductName = productName;
        QuantitySold = quantitySold;
        Price = price;
        Profit = quantitySold * price; // Прибыль от продажи товара
        SaleDate = DateTime.Now;
    }
}


public class UserManager
{
    private const string FilePath = "users.json";
    private const string AdminUsername = "admin";
    private const string AdminPassword = "admin123"; // Пароль для администратора
    private const string HrUsername = "hr"; // Логин кадровика
    private const string HrPassword = "123"; // Пароль для кадровика
    private const string Sklad = "Sklad"; // Логин для склада
    private const string SkladPas = "pas"; // Пароль для склада
    private const string LogFilePath = "user_activity_log.txt"; // Путь к файлу журнала
    private const string ProductFilePath = "products.json"; // Путь к файлу с товарами
    private const string SellerU = "Sel";
    private const string SellerP = "Selpas";

    public List<User> Users { get; private set; }
    public List<Product> Products { get; private set; }
    public List<Sale> Sales { get; private set; }


    public UserManager()
    {
        Users = LoadUsersFromFile();
        WarehouseData warehouseData = LoadWarehouseDataFromFile();
        Products = warehouseData.Products;
        Sales = LoadSalesFromFile();

    }
    private List<Sale> LoadSalesFromFile()
    {
        if (File.Exists("sales.json"))
        {
            string jsonString = File.ReadAllText("sales.json");
            return JsonSerializer.Deserialize<List<Sale>>(jsonString) ?? new List<Sale>();
        }
        else
        {
            return new List<Sale>();
        }
    }
    public class WarehouseData
    {

        public List<Product> Products { get; set; }
        public string WarehouseInfo { get; set; } // Можете добавить информацию о складе, например, его название или описание

        public WarehouseData()
        {
            Products = new List<Product>();
            WarehouseInfo = "Main warehouse";
        }
    }

    private WarehouseData LoadWarehouseDataFromFile()
    {
        if (File.Exists("store_data.json"))
        {
            string jsonString = File.ReadAllText("store_data.json");
            return JsonSerializer.Deserialize<WarehouseData>(jsonString) ?? new WarehouseData();
        }
        else
        {
            return new WarehouseData();
        }
    }

    private void SaveWarehouseDataToFile()
    {
        WarehouseData warehouseData = new WarehouseData
        {
            Products = Products, // Сохраняем список товаров
            WarehouseInfo = "Main warehouse" // Можете изменить на нужную информацию о складе
        };

        string jsonString = JsonSerializer.Serialize(warehouseData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("store_data.json", jsonString);
    }


    private void SaveSalesToFile()
    {
        string jsonString = JsonSerializer.Serialize(Sales, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("sales.json", jsonString);
    }

    // Метод для добавления продажи и подсчета прибыли
    public void AddSale(string productName, int quantitySold)
    {
        var product = Products.FirstOrDefault(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));
        if (product != null && product.Quantity >= quantitySold)
        {
            product.Quantity -= quantitySold; // Уменьшаем количество товара на складе
            decimal saleProfit = product.Price * quantitySold; // Рассчитываем прибыль от продажи

            // Добавляем продажу в журнал
            Sale sale = new Sale(productName, quantitySold, product.Price);
            Sales.Add(sale);

            SaveSalesToFile(); // Сохраняем журнал в файл
            SaveProductsToFile(); // Обновляем файл с товарами

            Console.WriteLine($"Продажа: {productName}, Количество: {quantitySold}, Прибыль: {saleProfit:C}");
        }
        else
        {
            Console.WriteLine("Недостаточно товара на складе или товар не найден.");
        }
    }
    public decimal GetTotalProfit()
    {
        return Sales.Sum(sale => sale.Profit);
    }


    // Загрузка пользователей из JSON файла
    private List<User> LoadUsersFromFile()
    {
        if (File.Exists(FilePath))
        {
            string jsonString = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<User>>(jsonString) ?? new List<User>();
        }
        else
        {
            return new List<User>();
        }
    }
    private List<Product> LoadProductsFromFile()
    {
        if (!File.Exists(ProductFilePath))
        {
            string jsonString = File.ReadAllText(ProductFilePath);
            return System.Text.Json.JsonSerializer.Deserialize<List<Product>>(jsonString) ?? new List<Product>();
        }
        else
        {
            return new List<Product>();
        }
    }
    // Сохранение пользователей в JSON файл
    private void SaveUsersToFile()
    {
        string jsonString = JsonSerializer.Serialize(Users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, jsonString);
    }

    // Загрузка товаров из JSON файла



    // Сохранение товаров в JSON файл
    private void SaveProductsToFile()
    {
        string jsonString = JsonSerializer.Serialize(Products, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ProductFilePath, jsonString);
    }

    // Проверка логина и пароля администратора
    public bool IsAdmin(string username, string password)
    {
        return username == AdminUsername && password == AdminPassword;
    }

    // Проверка логина и пароля кадровика
    public bool IsHR(string username, string password)
    {
        return username == HrUsername && password == HrPassword;
    }

    // Проверка логина и пароля склада
    public bool IsWarehouse(string username, string password)
    {
        return username == Sklad && password == SkladPas;
    }
    public bool IsSeller(string username, string password)
    {
        return username == SellerU && password == SellerP;
    }
    // Запись в журнал
    public void LogActivity(string action)
    {
        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {action}";
        File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
    }
    // Создание нового пользователя (для HR)
    public void CreateUserForHR(string username, string password, string role = "user")
    {
        if (Users.Any(u => u.Username == username))
        {
            Console.WriteLine("Пользователь с таким именем уже существует.");
            return;
        }

        Users.Add(new User(username, password, role));
        SaveUsersToFile();
        LogActivity($"Создан новый пользователь: {username} с ролью {role}");
        Console.WriteLine("Пользователь успешно создан.");
    }
    public void DeleteUser()
    {
        Console.Clear();
        Console.WriteLine("Введите логин пользователя для удаления:");
        string login = Console.ReadLine();

        // Находим пользователя по логину
        var user = Users.FirstOrDefault(u => u.Username == login && !u.IsDeleted);

        if (user != null)
        {
            // Устанавливаем флаг IsDeleted в true, чтобы пометить пользователя как удаленного
            user.IsDeleted = true;

            // Сохраняем обновленный список пользователей в файл
            SaveUsersToFile();

            // Записываем в журнал действия
            LogActivity($"Пользователь уволен (удален): {login}");

            Console.WriteLine("Пользователь успешно удален!");
        }
        else
        {
            Console.WriteLine("Пользователь не найден или уже удален!");
        }

        Console.ReadKey();
    }

    // Изменение данных пользователя
    public void ModifyUser(string oldUsername, string newUsername, string newPassword, string newRole)
    {
        User user = Users.FirstOrDefault(u => u.Username == oldUsername && !u.IsDeleted);

        if (user != null)
        {
            user.Username = newUsername;
            user.Password = newPassword;
            user.Role = newRole;
            SaveUsersToFile();
            LogActivity($"Изменены данные пользователя: {oldUsername} -> {newUsername} с должности {newRole}");
            Console.WriteLine("Данные пользователя успешно изменены.");
        }
        else
        {
            Console.WriteLine("Пользователь не найден или удален.");
        }
    }

    // Увольнение (удаление) пользователя
    public void FireUser(string username)
    {
        User user = Users.FirstOrDefault(u => u.Username == username && !u.IsDeleted);
        if (user != null)
        {
            user.IsDeleted = true;
            SaveUsersToFile();
            LogActivity($"Пользователь уволен: {username}");
            Console.WriteLine("Пользователь успешно уволен.");
        }
        else
        {
            Console.WriteLine("Пользователь не найден или уже уволен.");
        }
    }

    // Присвоение должности (изменение роли)
    public void AssignRole(string username, string newRole)
    {
        User user = Users.FirstOrDefault(u => u.Username == username && !u.IsDeleted);

        if (user != null)
        {
            user.Role = newRole;
            SaveUsersToFile();
            LogActivity($"Должность пользователя {username} изменена на {newRole}");
            Console.WriteLine($"Роль пользователя {username} успешно изменена на {newRole}.");
        }
        else
        {
            Console.WriteLine("Пользователь не найден или удален.");
        }
    }

    // Отображение всех пользователей
    public void DisplayUsers()
    {
        var activeUsers = Users.Where(u => !u.IsDeleted).ToList();
        Console.WriteLine("Список активных пользователей:");
        foreach (var user in activeUsers)
        {
            Console.WriteLine($"Логин: {user.Username}, Должность: {user.Role}");
        }
    }

    // Просмотр журнала
    public void ViewLog()
    {
        if (File.Exists(LogFilePath))
        {
            string[] logEntries = File.ReadAllLines(LogFilePath);
            Console.WriteLine("Журнал действий:");
            foreach (var logEntry in logEntries)
            {
                Console.WriteLine(logEntry);
            }
        }
        else
        {
            Console.WriteLine("Журнал пуст.");
        }
    }

    // Добавление товара на склад
    public void AddProduct(string name, int quantity, decimal price)
    {
        var product = new Product(name, quantity, price);
        Products.Add(product);
        SaveWarehouseDataToFile();
        LogActivity($"Добавлен товар: {name}, Количество: {quantity}, Цена: {price}С");
        Console.WriteLine("Товар успешно добавлен.");
    }

    // Отображение всех товаров
    public void RemoveProduct(string name)
    {
        var product = Products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (product != null)
        {
            Products.Remove(product);
            SaveProductsToFile();
            Console.WriteLine("Товар успешно удален.");
        }
        else
        {
            Console.WriteLine("Товар не найден.");
        }
    }

    // Изменение количества товара
    public void UpdateProductQuantity(string name, int newQuantity)
    {
        var product = Products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (product != null)
        {
            product.Quantity = newQuantity;
            SaveProductsToFile();
            Console.WriteLine($"Количество товара {name} успешно изменено на {newQuantity}.");
        }
        else
        {
            Console.WriteLine("Товар не найден.");
        }
    }

    // Изменение цены товара
    public void UpdateProductPrice(string name, decimal newPrice)
    {
        var product = Products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (product != null)
        {
            product.Price = newPrice;
            SaveProductsToFile();
            Console.WriteLine($"Цена товара {name} успешно изменена на {newPrice}.");
        }
        else
        {
            Console.WriteLine("Товар не найден.");
        }
    }

    // Показать все товары на складе
    public void DisplayProducts()
    {
        Console.WriteLine("Список всех товаров:");
        if (Products.Any())
        {
            foreach (var product in Products)
            {
                Console.WriteLine($"Название: {product.Name}, Количество: {product.Quantity}, Цена: {product.Price}");
            }
        }
        else
        {
            Console.WriteLine("Нет товаров на складе.");
        }
    }
}
public class Program
{
    public static void Main()
    {
        UserManager userManager = new UserManager();

        // Начальный выбор роли
        Console.WriteLine("Выберите роль для входа:");
        Console.WriteLine("1. Администратор");
        Console.WriteLine("2. Кадровик");
        Console.WriteLine("3. Склад");
        Console.WriteLine("4. Продваец");
        Console.Write("Выберите роль:");
        string roleChoice = Console.ReadLine();

        string username = "";
        string password = "";

        if (roleChoice == "1") // Администратор
        {
            Console.WriteLine("Введите логин администратора:");
            username = Console.ReadLine();
            Console.WriteLine("Введите пароль администратора:");
            password = Console.ReadLine();
            if (username != "admin" || password != "admin123")
            {
                Console.WriteLine("Неверный логин или пароль администратора.");
                return;
            }
        }
        else if (roleChoice == "2") // Кадровик
        {
            Console.WriteLine("Введите логин кадровика:");
            username = Console.ReadLine();
            Console.WriteLine("Введите пароль кадровика:");
            password = Console.ReadLine();
            if (username != "hr" || password != "123")
            {
                Console.WriteLine("Неверный логин или пароль кадровика.");
                return;
            }
        }
        else if (roleChoice == "3") // Склад
        {
            Console.WriteLine("Введите логин склада:");
            username = Console.ReadLine();
            Console.WriteLine("Введите пароль склада:");
            password = Console.ReadLine();
            if (username != "Sklad" || password != "pas")
            {
                Console.WriteLine("Неверный логин или пароль склада.");
                return;
            }
        }
        else if (roleChoice == "4")
        {
            if (roleChoice == "4") // Продавец
            {
                Console.WriteLine("Введите логин продавца:");
                username = Console.ReadLine();
                Console.WriteLine("Введите пароль продавца:");
                password = Console.ReadLine();
                if (!userManager.IsSeller(username, password))
                {
                    Console.WriteLine("Неверный логин или пароль продавца.");
                    return;
                }

            }
        }
        // После успешного входа для выбранной роли
        while (true)
        {
            Console.Clear();
            if (roleChoice == "1") // Меню для администратора
            {
                Console.WriteLine("Меню администратора:");
                Console.WriteLine("1. Создать пользователя");
                Console.WriteLine("2. Изменить пользователя");
                Console.WriteLine("3. Удалить пользователя");
                Console.WriteLine("4. Показать всех пользователей");
                Console.WriteLine("5. Просмотр журнала");
                Console.WriteLine("6. Выйти");
            }
            else if (roleChoice == "2") // Меню для кадровика
            {
                Console.WriteLine("Меню кадровика:");
                Console.WriteLine("1. Создать пользователя");
                Console.WriteLine("2. Уволить пользователя");
                Console.WriteLine("3. Присвоить должность");
                Console.WriteLine("4. Показать всех пользователей");
                Console.WriteLine("5. Выйти");
            }
            else if (roleChoice == "3") // Меню для склада
            {
                Console.WriteLine("Меню склада:");
                Console.WriteLine("1. Добавить товар");
                Console.WriteLine("2. Удалить товар");
                Console.WriteLine("3. Изменить количество товара");
                Console.WriteLine("4. Изменить цену товара");
                Console.WriteLine("5. Показать все товары");
                Console.WriteLine("6. Выйти");
            }
            else if (roleChoice == "4") // Меню для продавца
            {
                Console.WriteLine("Меню продавца:");
                Console.WriteLine("1. Показать все товары");
                Console.WriteLine("2. Изменить количество товара");
                Console.WriteLine("3. Сделать продажу");
                Console.WriteLine("4. Показать прибыль от продаж");
                Console.WriteLine("5. Выйти");
            }

            Console.Write("Выберите действие: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    if (roleChoice == "1") // Администратор
                    {
                        Console.Write("Введите логин нового пользователя: ");
                        string newUsername = Console.ReadLine();
                        Console.Write("Введите пароль нового пользователя: ");
                        string newPassword = Console.ReadLine();
                        Console.Write("Введите должность нового пользователя: ");
                        string newRole = Console.ReadLine();
                        userManager.CreateUserForHR(newUsername, newPassword, newRole);
                    }
                    else if (roleChoice == "2") // Кадровик
                    {
                        Console.Write("Введите логин нового пользователя: ");
                        string newUsername = Console.ReadLine();
                        Console.Write("Введите пароль нового пользователя: ");
                        string newPassword = Console.ReadLine();
                        Console.Write("Введите должность нового пользователя: ");
                        string newRole = Console.ReadLine();
                        userManager.CreateUserForHR(newUsername, newPassword, newRole);
                    }


                    else if (roleChoice == "3") // Склад
                    {
                        Console.Write("Введите название товара: ");
                        string productName = Console.ReadLine();
                        Console.Write("Введите количество товара: ");
                        int quantity = int.Parse(Console.ReadLine());
                        Console.Write("Введите цену товара: ");
                        decimal price = decimal.Parse(Console.ReadLine());
                        userManager.AddProduct(productName, quantity, price);
                    }
                    else if (roleChoice == "4") // Продавец
                    {
                        userManager.DisplayProducts(); // Показать все товары
                    }
                    break;



                case "2":
                    if (roleChoice == "1") // Администратор
                    {
                        Console.Write("Введите логин пользователя, которого хотите изменить: ");
                        string oldUsername = Console.ReadLine();
                        Console.Write("Введите новый логин: ");
                        string modifiedUsername = Console.ReadLine();
                        Console.Write("Введите новый пароль: ");
                        string modifiedPassword = Console.ReadLine();
                        Console.Write("Введите специальность: ");
                        string modifiedRole = Console.ReadLine();
                        userManager.ModifyUser(oldUsername, modifiedUsername, modifiedPassword, modifiedRole);
                    }
                    else if (roleChoice == "2") // Кадровик
                    {
                        Console.Write("Введите логин пользователя, которого хотите уволить: ");
                        string fireUsername = Console.ReadLine();
                        userManager.FireUser(fireUsername);
                    }

                    else if (roleChoice == "3") // Склад
                    {
                        Console.Write("Введите название товара для удаления: ");
                        string productName = Console.ReadLine();
                        userManager.RemoveProduct(productName);
                    }
                    else if (roleChoice == "4") // Продавец
                    {
                        Console.Write("Введите название товара для изменения количества: ");
                        string productName = Console.ReadLine();
                        Console.Write("Введите новое количество товара: ");
                        int newQuantity = int.Parse(Console.ReadLine());
                        userManager.UpdateProductQuantity(productName, newQuantity); // Изменить количество товара
                    }
                    break;

                case "3":
                    if (roleChoice == "1") // Администратор
                    {
                        userManager.DeleteUser(); // Показать всех пользователей
                    }
                    else if (roleChoice == "2") // Кадровик
                    {
                        Console.Write("Введите логин пользователя, которому хотите присвоить должность: ");
                        string assignUsername = Console.ReadLine();
                        Console.Write("Введите новую роль: ");
                        string newRoleForUser = Console.ReadLine();
                        userManager.AssignRole(assignUsername, newRoleForUser); // Присвоение роли
                    }
                    else if (roleChoice == "3") // Склад
                    {
                        Console.Write("Введите название товара для изменения количества: ");
                        string productName = Console.ReadLine();
                        Console.Write("Введите новое количество товара: ");
                        int newQuantity = int.Parse(Console.ReadLine());
                        userManager.UpdateProductQuantity(productName, newQuantity);
                    }
                    else if (roleChoice == "4") // Продавец
                    {
                        Console.Write("Введите название товара для продажи: ");
                        string productName = Console.ReadLine();
                        Console.Write("Введите количество товара для продажи: ");
                        int quantitySold = int.Parse(Console.ReadLine());
                        userManager.AddSale(productName, quantitySold); // Добавить продажу и вычислить прибыль
                    }
                    break;

                case "4":
                    if (roleChoice == "1") // Администратор
                    {
                        userManager.DisplayUsers();

                    }
                    else if (roleChoice == "2") // Кадровик
                    {
                        userManager.DisplayUsers(); // Показать всех пользователей
                    }
                    else if (roleChoice == "3") // Склад
                    {
                        Console.Write("Введите название товара для изменения цены: ");
                        string productName = Console.ReadLine();
                        Console.Write("Введите новую цену товара: ");
                        decimal newPrice = decimal.Parse(Console.ReadLine());
                        userManager.UpdateProductPrice(productName, newPrice);
                    }
                    else if (roleChoice == "4") // Продавец
                    {
                        decimal totalProfit = userManager.GetTotalProfit(); // Показать общую прибыль
                        Console.WriteLine($"Общая прибыль от продаж: {totalProfit:C}");
                    }
                    break;

                case "5":
                    if (roleChoice == "2") // Кадровик
                    {
                        return; // Выход
                    }
                    else if (roleChoice == "1")
                    {
                        userManager.ViewLog(); // Просмотр журнала
                    }
                    else if (roleChoice == "3") // Склад
                    {
                        userManager.DisplayProducts(); // Показать все товары
                    }
                    break;

                case "6":
                    if (roleChoice == "2") // Кадровик
                    {
                        return; // Выход
                    }
                    else
                    {
                        return;
                    }// Выход
                default:
                    Console.WriteLine("Неверный выбор.");
                    break;
            }

            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}
