

//USE GMWOG1_Marketplace;
//GO

//-- Таблица пользователей
//CREATE TABLE Users (
//    UserID INT IDENTITY(1,1) PRIMARY KEY,
//    Username NVARCHAR(50) UNIQUE NOT NULL,
//    PasswordHash NVARCHAR(255) NOT NULL,
//    Email NVARCHAR(100) UNIQUE NOT NULL,
//    FullName NVARCHAR(100) NOT NULL,
//    PhoneNumber NVARCHAR(20),
//    RegistrationDate DATETIME NOT NULL DEFAULT GETDATE(),
//    LastLoginDate DATETIME NULL
//);

//--Таблица товаров
//CREATE TABLE Products (
//    ProductID INT IDENTITY(1,1) PRIMARY KEY,
//    ProductName NVARCHAR(200) NOT NULL,
//    Description NVARCHAR(1000) NULL,
//    Price DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
//    StockQuantity INT NOT NULL DEFAULT 0 CHECK (StockQuantity >= 0),
//    Category NVARCHAR(100) NOT NULL,
//    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
//    IsActive BIT NOT NULL DEFAULT 1
//);

//--Таблица ПВЗ(пунктов выдачи заказов)
//CREATE TABLE PickupPoints (
//    PickupPointID INT IDENTITY(1,1) PRIMARY KEY,
//    PointName NVARCHAR(200) NOT NULL,
//    Address NVARCHAR(500) NOT NULL,
//    PhoneNumber NVARCHAR(20),
//    WorkingHours NVARCHAR(100),
//    City NVARCHAR(100) NOT NULL,
//    IsActive BIT NOT NULL DEFAULT 1
//);

//--Таблица корзины
//CREATE TABLE Cart (
//    CartID INT IDENTITY(1,1) PRIMARY KEY,
//    UserID INT NOT NULL,
//    ProductID INT NOT NULL,
//    Quantity INT NOT NULL CHECK (Quantity > 0),
//    AddedDate DATETIME NOT NULL DEFAULT GETDATE(),
//    FOREIGN KEY (UserID) REFERENCES Users(UserID),
//    FOREIGN KEY (ProductID) REFERENCES Products(ProductID),
//    UNIQUE (UserID, ProductID)
//);

//--Таблица заказов
//CREATE TABLE Orders (
//    OrderID INT IDENTITY(1,1) PRIMARY KEY,
//    UserID INT NOT NULL,
//    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
//    TotalAmount DECIMAL(10,2) NOT NULL CHECK (TotalAmount >= 0),
//    PickupPointID INT NOT NULL,
//    Status NVARCHAR(50) NOT NULL DEFAULT 'Обрабатывается',
//    FOREIGN KEY (UserID) REFERENCES Users(UserID),
//    FOREIGN KEY (PickupPointID) REFERENCES PickupPoints(PickupPointID)
//);

//--Таблица элементов заказа
//CREATE TABLE OrderItems (
//    OrderItemID INT IDENTITY(1,1) PRIMARY KEY,
//    OrderID INT NOT NULL,
//    ProductID INT NOT NULL,
//    Quantity INT NOT NULL CHECK (Quantity > 0),
//    UnitPrice DECIMAL(10,2) NOT NULL CHECK (UnitPrice >= 0),
//    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
//    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
//);

