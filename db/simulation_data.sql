USE city_builder_mini;

INSERT INTO players (username) VALUES 
('DevTeam_Alpha'),
('Profesor_Testing'),
('Gamer_Pro_67');

INSERT INTO sessions (player_id, inicio, fin, poblacion_final, dinero_final) VALUES 
(1, '2023-10-01 14:00:00', '2023-10-01 14:30:00', 45, 1200),
(2, '2023-10-02 10:00:00', '2023-10-02 10:45:00', 80, 5000);

INSERT INTO map_grid (session_id, x, y, building_id) VALUES 
(1, 0, 0, 1), -- Casa de Madera en 0,0
(1, 0, 1, 1), -- Casa de Madera en 0,1
(1, 1, 0, 3), -- Tienda Local en 1,0
(1, 2, 2, 5), -- Parque Central en 2,2
(1, 3, 3, 1); -- Casa de Madera en 3,3

INSERT INTO construction_logs (session_id, timestamp, x, y, building_id) VALUES 
(1, '2023-10-01 14:01:00', 0, 0, 1),
(1, '2023-10-01 14:05:00', 0, 1, 1),
(1, '2023-10-01 14:10:00', 1, 0, 3),
(1, '2023-10-01 14:15:00', 2, 2, 5),
(1, '2023-10-01 14:20:00', 3, 3, 1);

INSERT INTO resource_logs (session_id, turno, dinero, poblacion, electricidad, agua) VALUES 
(1, 1, 1000, 0, 10, 10),
(1, 2, 900, 4, 9, 9),    -- Construyó la 1ra casa
(1, 3, 800, 8, 8, 8),    -- Construyó la 2da casa
(1, 4, 600, 8, 7, 8),    -- Construyó la tienda
(1, 5, 800, 13, 7, 7);   -- Cobra dinero de la tienda, construye parque y casa

INSERT INTO event_logs (session_id, turno, event_id) VALUES 
(1, 3, 1), -- En el turno 3 le tocó un Subsidio del Gobierno
(2, 5, 3); -- Al jugador 2 le tocó un Festival de la Ciudad

INSERT INTO session_results (session_id, poblacion, dinero, satisfaccion, puntaje) VALUES 
(1, 45, 1200, 85, 3450),
(2, 80, 5000, 95, 8900);
