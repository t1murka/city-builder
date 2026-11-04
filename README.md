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

`resource_logs` registra dinero y población al finalizar cada turno, después de cobrar el balance.
Electricidad y agua se guardan como 0 hasta implementar su mecánica.
