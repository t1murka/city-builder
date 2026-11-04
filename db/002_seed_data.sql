USE city_builder_mini;

INSERT INTO building_types (nombre, costo, beneficio_poblacion, beneficio_dinero, requisitos, beneficio_satisfaccion) VALUES
('Casa de Madera', 100, 4, 10, '1 de electricidad y 1 de agua disponibles', 0),
('Edificio de Apartamentos', 500, 20, 0, 'Requiere Agua y Electricidad', 0),
('Tienda Local', 250, 0, 50, 'Población > 10; 2 de electricidad y 2 de agua disponibles', 0),
('Fábrica de Carbón', 1000, -5, 300, 'Lejos de zona residencial', 0),
('Parque Central', 300, 0, -10, 'Ninguno', 10),
('Planta Eléctrica', 800, 0, 0, 'Ninguno', 0),
('Planta de Agua', 800, 0, 0, 'Ninguno', 0);

INSERT INTO events (nombre, efecto) VALUES 
('Subsidio del Gobierno', '+500 de dinero al jugador'),
('Terremoto Menor', 'Destruye un edificio aleatorio'),
('Festival de la Ciudad', '+10 de satisfacción, -100 dinero'),
('Baby Boom', '+15 de población instantánea'),
('Corte de Energía', 'Las fábricas no producen dinero este turno');
