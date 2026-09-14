PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Users (
	Id INTEGER NOT NULL PRIMARY KEY,
	Name TEXT NOT NULL,
	Email TEXT,
	UpdatedAt TEXT
);

INSERT INTO Users (Id, Name, Email, UpdatedAt)
SELECT 1, '山田太郎', 'taro@example.com', datetime('now')
WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Id = 1);

CREATE TABLE IF NOT EXISTS Departments (
	Id INTEGER NOT NULL PRIMARY KEY,
	Name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Products (
	Id INTEGER NOT NULL PRIMARY KEY,
	Name TEXT NOT NULL,
	Price NUMERIC NOT NULL,
	Stock INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Orders (
	Id INTEGER NOT NULL PRIMARY KEY,
	UserId INTEGER NOT NULL,
	ProductId INTEGER NOT NULL,
	Quantity INTEGER NOT NULL,
	OrderedAt TEXT NOT NULL,
	FOREIGN KEY (UserId) REFERENCES Users (Id),
	FOREIGN KEY (ProductId) REFERENCES Products (Id)
);

INSERT INTO Departments (Id, Name)
SELECT 1, '開発部'
WHERE NOT EXISTS (SELECT 1 FROM Departments WHERE Id = 1);

INSERT INTO Departments (Id, Name)
SELECT 2, '品質保証部'
WHERE NOT EXISTS (SELECT 1 FROM Departments WHERE Id = 2);

INSERT INTO Products (Id, Name, Price, Stock)
SELECT 1, 'テスト用ノートPC', 120000, 10
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 1);

INSERT INTO Products (Id, Name, Price, Stock)
SELECT 2, 'テスト用モニター', 35000, 25
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Id = 2);

INSERT INTO Orders (Id, UserId, ProductId, Quantity, OrderedAt)
SELECT 1, 1, 1, 1, datetime('now', '-1 day')
WHERE NOT EXISTS (SELECT 1 FROM Orders WHERE Id = 1);

INSERT INTO Orders (Id, UserId, ProductId, Quantity, OrderedAt)
SELECT 2, 1, 2, 2, datetime('now')
WHERE NOT EXISTS (SELECT 1 FROM Orders WHERE Id = 2);
