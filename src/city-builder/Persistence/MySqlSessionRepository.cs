using System;
using System.Threading;
using System.Threading.Tasks;
using city_builder.World;
using MySqlConnector;

namespace city_builder.Persistence;

public sealed class MySqlSessionRepository(string connectionString)
{
    public async Task<long> StartSessionAsync(string playerName, DateTime startedAtUtc,
        CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        using var playerSelect = new MySqlCommand("SELECT id FROM players WHERE username = @name;", connection);
        playerSelect.Parameters.AddWithValue("@name", playerName);
        var playerId = Convert.ToInt64(await playerSelect.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        if (playerId == 0)
        {
            using var playerInsert = new MySqlCommand("INSERT INTO players (username) VALUES (@name);", connection);
            playerInsert.Parameters.AddWithValue("@name", playerName);
            try
            {
                await playerInsert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                playerId = playerInsert.LastInsertedId;
            }
            catch (MySqlException exception) when (exception.Number == 1062)
            {
                // Another game may have created this player after the SELECT.
                playerId = Convert.ToInt64(await playerSelect.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
            }
        }

        using var sessionInsert = new MySqlCommand("""
            INSERT INTO sessions (player_id, inicio) VALUES (@player, @started);
            """, connection);
        sessionInsert.Parameters.AddWithValue("@player", playerId);
        sessionInsert.Parameters.AddWithValue("@started", startedAtUtc);
        await sessionInsert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        var sessionId = sessionInsert.LastInsertedId;
        if (playerId <= 0 || sessionId <= 0)
        {
            throw new InvalidOperationException("Session registration did not return valid identifiers.");
        }

        return sessionId;
    }

    public async Task SaveConstructionAsync(long sessionId, CellPosition position, BuildingType building,
        CancellationToken cancellationToken)
    {
        var buildingName = building switch
        {
            BuildingType.House => "Casa de Madera",
            BuildingType.Shop => "Tienda Local",
            BuildingType.Park => "Parque Central",
            BuildingType.PowerPlant => "Planta Eléctrica",
            BuildingType.WaterPlant => "Planta de Agua",
            BuildingType.Factory => "Fábrica de Carbón",
            _ => throw new ArgumentOutOfRangeException(nameof(building))
        };

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        using var buildingSelect = new MySqlCommand("SELECT id FROM building_types WHERE nombre = @name;",
            connection, transaction);
        buildingSelect.Parameters.AddWithValue("@name", buildingName);
        var buildingId = Convert.ToInt64(await buildingSelect.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false));
        if (buildingId <= 0)
        {
            throw new InvalidOperationException("The building type does not exist in the database.");
        }

        using var mapInsert = new MySqlCommand("""
            INSERT INTO map_grid (session_id, x, y, building_id)
            VALUES (@session, @x, @y, @building);
            """, connection, transaction);
        mapInsert.Parameters.AddWithValue("@session", sessionId);
        mapInsert.Parameters.AddWithValue("@x", position.X);
        mapInsert.Parameters.AddWithValue("@y", position.Y);
        mapInsert.Parameters.AddWithValue("@building", buildingId);
        await mapInsert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        using var logInsert = new MySqlCommand("""
            INSERT INTO construction_logs (session_id, x, y, building_id)
            VALUES (@session, @x, @y, @building);
            """, connection, transaction);
        logInsert.Parameters.AddWithValue("@session", sessionId);
        logInsert.Parameters.AddWithValue("@x", position.X);
        logInsert.Parameters.AddWithValue("@y", position.Y);
        logInsert.Parameters.AddWithValue("@building", buildingId);
        await logInsert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveResourcesAsync(long sessionId, int turn, int money, int population, int electricity, int water,
        CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        using var resourceInsert = new MySqlCommand("""
            INSERT INTO resource_logs (session_id, turno, dinero, poblacion, electricidad, agua)
            VALUES (@session, @turn, @money, @population, @electricity, @water);
            """, connection);
        resourceInsert.Parameters.AddWithValue("@session", sessionId);
        resourceInsert.Parameters.AddWithValue("@turn", turn);
        resourceInsert.Parameters.AddWithValue("@money", money);
        resourceInsert.Parameters.AddWithValue("@population", population);
        resourceInsert.Parameters.AddWithValue("@electricity", electricity);
        resourceInsert.Parameters.AddWithValue("@water", water);
        await resourceInsert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveResultAsync(long sessionId, DateTime endedAtUtc, CityResult result,
        CancellationToken cancellationToken)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        using var sessionUpdate = new MySqlCommand("""
            UPDATE sessions
            SET fin = @ended, poblacion_final = @population, dinero_final = @money
            WHERE id = @session;
            """, connection, transaction);
        sessionUpdate.Parameters.AddWithValue("@ended", endedAtUtc);
        sessionUpdate.Parameters.AddWithValue("@population", result.Population);
        sessionUpdate.Parameters.AddWithValue("@money", result.Money);
        sessionUpdate.Parameters.AddWithValue("@session", sessionId);
        if (await sessionUpdate.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) != 1)
        {
            throw new InvalidOperationException("The session to complete does not exist.");
        }

        using var resultInsert = new MySqlCommand("""
            INSERT INTO session_results (session_id, poblacion, dinero, satisfaccion, puntaje)
            VALUES (@session, @population, @money, @satisfaction, @score);
            """, connection, transaction);
        resultInsert.Parameters.AddWithValue("@session", sessionId);
        resultInsert.Parameters.AddWithValue("@population", result.Population);
        resultInsert.Parameters.AddWithValue("@money", result.Money);
        resultInsert.Parameters.AddWithValue("@satisfaction", result.Satisfaction);
        resultInsert.Parameters.AddWithValue("@score", result.Score);
        await resultInsert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
