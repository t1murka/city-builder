# city builder

Videojuego 2D de construcción y gestión de una ciudad, desarrollado en C# con MonoGame.

## Ejecución

Requiere .NET 8 SDK. Desde la raíz del repositorio:

```sh
dotnet run --project src/city-builder/city-builder.csproj
```

## MySQL

1. En MySQL 8 o posterior, ejecuta `db/001_base.sql` y después `db/002_seed_data.sql`.
2. Copia `database.example.json` a `database.json` junto al ejecutable, o en `src/city-builder/` si ejecutas desde el código fuente.
3. Configura servidor, puerto, usuario, contraseña y `PlayerName`. La base es `city_builder_mini`.

Sin `database.json`, el guardado en MySQL está desactivado.

`resource_logs` registra dinero, población, electricidad y agua disponibles al finalizar cada turno.

La ciudad empieza con 10 de luz y 10 de agua. Cada casa utiliza 1 de cada recurso y cada tienda, 2.
Las centrales eléctricas y plantas de agua cuestan $800 y añaden 20 de su recurso.
Son capacidades disponibles: no se gastan de nuevo en cada turno.
