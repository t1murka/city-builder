DROP DATABASE IF EXISTS city_builder_mini;
CREATE DATABASE city_builder_mini;
USE city_builder_mini;

CREATE TABLE building_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE,
    costo INT NOT NULL CHECK (costo >= 0),
    beneficio_poblacion INT NOT NULL,
    beneficio_dinero INT NOT NULL,
    requisitos VARCHAR(255)
);

CREATE TABLE players (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE sessions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    player_id INT NOT NULL,
    inicio DATETIME NOT NULL,
    fin DATETIME NULL,
    poblacion_final INT DEFAULT 0 CHECK (poblacion_final >= 0),
    dinero_final INT DEFAULT 0,
    FOREIGN KEY (player_id) REFERENCES players(id) ON DELETE CASCADE
);

CREATE TABLE map_grid (
    id INT AUTO_INCREMENT PRIMARY KEY,
    session_id INT NOT NULL,
    x INT NOT NULL,
    y INT NOT NULL,
    building_id INT NOT NULL,
    UNIQUE (session_id, x, y),
    FOREIGN KEY (session_id) REFERENCES sessions(id) ON DELETE CASCADE,
    FOREIGN KEY (building_id) REFERENCES building_types(id) ON DELETE RESTRICT
);

CREATE TABLE construction_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    session_id INT NOT NULL,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    x INT NOT NULL,
    y INT NOT NULL,
    building_id INT NOT NULL,
    FOREIGN KEY (session_id) REFERENCES sessions(id) ON DELETE CASCADE,
    FOREIGN KEY (building_id) REFERENCES building_types(id) ON DELETE RESTRICT
);

CREATE TABLE resource_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    session_id INT NOT NULL,
    turno INT NOT NULL CHECK (turno >= 1),
    dinero INT NOT NULL,
    poblacion INT DEFAULT 0 CHECK (poblacion >= 0),
    electricidad INT NOT NULL,
    agua INT NOT NULL,
    UNIQUE (session_id, turno),
    FOREIGN KEY (session_id) REFERENCES sessions(id) ON DELETE CASCADE
);

CREATE TABLE events (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    efecto VARCHAR(255) NOT NULL
);

CREATE TABLE event_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    session_id INT NOT NULL,
    turno INT NOT NULL,
    event_id INT NOT NULL,
    FOREIGN KEY (session_id) REFERENCES sessions(id) ON DELETE CASCADE,
    FOREIGN KEY (event_id) REFERENCES events(id) ON DELETE RESTRICT
);

CREATE TABLE session_results (
    id INT AUTO_INCREMENT PRIMARY KEY,
    session_id INT NOT NULL UNIQUE,
    poblacion INT DEFAULT 0 CHECK (poblacion >= 0),
    dinero INT NOT NULL,
    satisfaccion INT NOT NULL CHECK (satisfaccion BETWEEN 0 AND 100),
    puntaje INT NOT NULL,
    FOREIGN KEY (session_id) REFERENCES sessions(id) ON DELETE CASCADE
);
