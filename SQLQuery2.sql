-- Inserimento Categories
INSERT INTO Categories (Name) VALUES 
('Antipasti'),
('Primi Piatti'),
('Secondi Piatti'),
('Contorni'),
('Dolci'),
('Bevande');

-- Inserimento Dishes
INSERT INTO Dishes (Name, Description, Price, CategoryId) VALUES
-- Antipasti
('Bruschetta al Pomodoro', 'Pane tostato con pomodorini, aglio e basilico', 6.50, 1),
('Carpaccio di Manzo', 'Fettine di manzo crudo con rucola e parmigiano', 12.00, 1),
('Caprese', 'Mozzarella di bufala con pomodoro e basilico', 9.00, 1),

-- Primi Piatti
('Spaghetti alla Carbonara', 'Pasta con uovo, guanciale, pecorino e pepe', 13.00, 2),
('Risotto ai Funghi Porcini', 'Riso carnaroli con funghi porcini', 14.50, 2),
('Lasagna alla Bolognese', 'Pasta al forno con ragù e besciamella', 12.50, 2),

-- Secondi Piatti
('Filetto al Pepe Verde', 'Filetto di manzo con salsa al pepe verde', 24.00, 3),
('Branzino al Forno', 'Branzino con patate e pomodorini', 22.00, 3),
('Scaloppine al Limone', 'Scaloppine di vitello al limone', 18.00, 3),

-- Contorni
('Patate al Forno', 'Patate arrosto con rosmarino', 5.00, 4),
('Verdure Grigliate', 'Melanzane, zucchine e peperoni alla griglia', 6.00, 4),
('Insalata Mista', 'Insalata verde mista di stagione', 4.50, 4),

-- Dolci
('Tiramisù', 'Classico tiramisù con caffè e mascarpone', 7.00, 5),
('Panna Cotta', 'Panna cotta con salsa ai frutti di bosco', 6.50, 5),
('Cannolo Siciliano', 'Cannolo ripieno di ricotta', 6.00, 5),

-- Bevande
('Vino della Casa (Rosso)', 'Vino rosso della casa (0.75l)', 18.00, 6),
('Vino della Casa (Bianco)', 'Vino bianco della casa (0.75l)', 18.00, 6),
('Acqua Minerale', 'Acqua minerale (1l)', 3.00, 6);

-- Inserimento Menus
INSERT INTO Menus (Name) VALUES
('Menu del Giorno'),
('Menu Degustazione'),
('Menu Vegetariano');

-- Collegamento MenuDishes
-- Menu del Giorno
INSERT INTO MenuDishes (MenuId, DishId) VALUES
(1, 1),  -- Bruschetta
(1, 4),  -- Carbonara
(1, 8),  -- Branzino
(1, 11), -- Verdure Grigliate
(1, 13); -- Tiramisù

-- Menu Degustazione
INSERT INTO MenuDishes (MenuId, DishId) VALUES
(2, 2),  -- Carpaccio
(2, 5),  -- Risotto
(2, 7),  -- Filetto
(2, 10), -- Patate
(2, 14); -- Panna Cotta

-- Menu Vegetariano
INSERT INTO MenuDishes (MenuId, DishId) VALUES
(3, 3),  -- Caprese
(3, 5),  -- Risotto ai Funghi
(3, 11), -- Verdure Grigliate
(3, 12), -- Insalata
(3, 13); -- Tiramisù
