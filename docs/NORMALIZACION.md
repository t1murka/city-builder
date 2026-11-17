# Normalización de la base de datos — City Builder

La normalización organiza los datos para reducir repeticiones y evitar anomalías de inserción, modificación y eliminación. En City Builder se separan los jugadores, las partidas, los catálogos y los registros de lo ocurrido durante el juego.

## Organización de las tablas

| Tablas | Información que representan |
| --- | --- |
| `players`, `sessions` | Jugadores y sus partidas. |
| `building_types`, `events` | Catálogos de edificios y eventos. |
| `map_grid` | Edificio situado en cada celda de una partida. |
| `construction_logs`, `event_logs` | Historial de construcciones y eventos. |
| `resource_logs` | Recursos al finalizar cada turno. |
| `session_results` | Resultado final de cada partida. |

## Primera forma normal — 1FN

Cada fila se identifica mediante una clave primaria y cada campo contiene un valor del dominio correspondiente. No hay grupos de columnas repetidas como `edificio_1`, `edificio_2` o listas de identificadores en una celda.

Por ejemplo, `map_grid` guarda cada posición en una fila con `session_id`, `x`, `y` y `building_id`. Los campos `requisitos` y `efecto` son descripciones de texto; las relaciones entre entidades se representan mediante claves foráneas.

## Segunda forma normal — 2FN

Además de cumplir 1FN, los atributos que no forman parte de una clave deben depender de la clave completa, sin dependencias parciales.

Todas las tablas tienen una clave primaria simple, `id`. También existen claves candidatas compuestas:

- En `map_grid`, `UNIQUE(session_id, x, y)` identifica una celda. Su edificio depende de la combinación completa: las mismas coordenadas pueden tener edificios distintos en otras partidas.
- En `resource_logs`, `UNIQUE(session_id, turno)` identifica un registro. Dinero, población, electricidad y agua corresponden a esa partida y ese turno, no solamente a uno de ellos.

El nombre y el costo del tipo de edificio se conservan en `building_types`, no se repiten en cada celda o construcción.

## Tercera forma normal — 3FN

Además de cumplir 2FN, deben eliminarse las dependencias transitivas entre atributos que no son parte de una clave. Por ejemplo:

- `sessions` guarda `player_id`; el nombre del jugador se obtiene desde `players`.
- `map_grid` y `construction_logs` guardan `building_id`; el nombre y el costo del edificio pertenecen a `building_types`.
- `event_logs` guarda `event_id`; el nombre y la descripción del evento pertenecen a `events`.

Así, una modificación del nombre de un tipo de edificio se realiza en su catálogo, sin actualizar todas las filas que lo referencian. Los datos relacionados pueden recuperarse mediante `JOIN`.

## Redundancias que conserva la implementación

La implementación actual no debe presentarse como una 3FN estricta en todas sus tablas:

- `session_results.puntaje` se calcula a partir de población, dinero y satisfacción. Guardarlo es una desnormalización controlada; para una 3FN estricta podría calcularse al consultar los resultados.
- `sessions` y `session_results` repiten población y dinero finales. Es una redundancia entre tablas que obliga a mantener ambos valores consistentes; el código los guarda en una misma transacción. Podrían conservarse únicamente en `session_results` para eliminar esa duplicación.

El mapa actual y el historial de construcciones, en cambio, tienen propósitos diferentes: representar el estado de las celdas y registrar las acciones realizadas.

Las restricciones `PRIMARY KEY`, `FOREIGN KEY`, `UNIQUE`, `NOT NULL` y `CHECK` complementan el diseño con integridad de datos, pero no sustituyen la justificación de las formas normales.
