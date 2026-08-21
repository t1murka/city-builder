USE city_builder_mini;

INSERT INTO building_types (nombre, costo, beneficio_poblacion, beneficio_dinero, requisitos) VALUES 
('Casa de Madera', 100, 4, 0, 'Ninguno'),
('Edificio de Apartamentos', 500, 20, 0, 'Requiere Agua y Electricidad'),
('Tienda Local', 250, 0, 50, 'Requiere Población > 10'),
('Fábrica de Carbón', 1000, -5, 300, 'Lejos de zona residencial'),
('Parque Central', 300, 5, -10, 'Ninguno'),
('Planta Eléctrica', 800, 0, 0, 'Ninguno'),
('Planta de Agua', 800, 0, 0, 'Ninguno');

INSERT INTO events (nombre, efecto) VALUES 
('Subsidio del Gobierno', '+500 de dinero al jugador'),
('Terremoto Menor', 'Destruye un edificio aleatorio'),
('Festival de la Ciudad', '+10 de satisfacción, -100 dinero'),
('Baby Boom', '+15 de población instantánea'),
('Corte de Energía', 'Las fábricas no producen dinero este turno');
