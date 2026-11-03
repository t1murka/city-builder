# Registro de cambios

## Sin publicar

### Agregado
- Registro de construcciones exitosas en el mapa y el historial de la partida en MySQL.
- Conexión configurable a MySQL y registro de jugadores, partidas y resultados finales.
- Guardado asíncrono de resultados con transacción e indicadores de estado.
- Archivo de ejemplo para configurar la conexión y el nombre del jugador.
- Parques de $300 con mantenimiento de $10 por turno y una mejora de 10 puntos de satisfacción.
- Satisfacción inicial de 50%, limitada al rango de 0 a 100%, visible durante la partida y en el resultado.
- Tienda de $250 con ingresos de $50 por turno y requisito de población mayor que 10.
- Selector compacto de casas y tiendas, detalles de construcción y vista previa del edificio elegido.
- Partidas de 20 turnos con cobro del último turno y bloqueo de acciones al finalizar.
- Pantalla de resultados con población, dinero, satisfacción y puntaje ((población × 100 + dinero) × satisfacción / 100).
- Botón para iniciar una nueva partida con el mapa y los recursos reiniciados.
- Turnos con ingresos de $10 por casa y botón para avanzar al siguiente turno.
- Indicadores de turno actual e ingresos por turno en el panel de recursos.
- Dinero inicial, costo de construcción y crecimiento de población por casa.
- Panel compacto de recursos y aviso de dinero insuficiente.
- Construcción de casas con vista previa, validación de celdas ocupadas y orden de dibujo isométrico.
- Mapa isométrico de 10×10 celdas con estado en un array bidimensional.
- Resaltado de celdas, coordenadas en el título y adaptación al tamaño de la ventana.
- Separación entre estado, geometría y representación gráfica.
- Instrucciones de ejecución.

### Modificado
- El script base de la base de datos conserva las tablas y los datos existentes.
- El balance por turno incluye ingresos y gastos; los valores negativos se muestran en rojo.
- Versiones de MonoGame y herramientas fijadas a 3.8.4.1 para obtener compilaciones consistentes.
- Uso del tile de césped existente con una huella isométrica de 64×32.

### Corregido
- Formato ICO del icono de la aplicación, con tamaños de 16 a 256 píxeles.
