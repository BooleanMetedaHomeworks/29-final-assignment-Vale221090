-- Tabelle principali
CREATE TABLE Categories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE Dishes (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Price DECIMAL(10,2) NOT NULL,
    CategoryId INT NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

CREATE TABLE Menus (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);

-- Tabella di collegamento many-to-many tra Dishes e Menus
CREATE TABLE MenuDishes (
    MenuId INT NOT NULL,
    DishId INT NOT NULL,
    PRIMARY KEY (MenuId, DishId),
    FOREIGN KEY (MenuId) REFERENCES Menus(Id),
    FOREIGN KEY (DishId) REFERENCES Dishes(Id)
);
