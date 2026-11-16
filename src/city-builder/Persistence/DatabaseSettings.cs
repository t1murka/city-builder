using System;
using System.IO;
using System.Text.Json;
using MySqlConnector;

namespace city_builder.Persistence;

public sealed record DatabaseSettings
{
    public string Server { get; init; } = "localhost";
    public uint Port { get; init; } = 3306;
    public string Database { get; init; } = "city_builder_mini";
    public string User { get; init; } = "root";
    public string Password { get; init; } = "";
    public string PlayerName { get; init; } = "Jugador";

    public static DatabaseSettings? Load(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var settings = JsonSerializer.Deserialize<DatabaseSettings>(File.ReadAllText(path))
            ?? throw new InvalidDataException("Database settings are empty.");
        if (string.IsNullOrWhiteSpace(settings.Server) || string.IsNullOrWhiteSpace(settings.Database)
            || string.IsNullOrWhiteSpace(settings.User) || settings.Password is null
            || string.IsNullOrWhiteSpace(settings.PlayerName) || settings.PlayerName.Trim().Length > 50
            || settings.Port is 0 or > 65535)
        {
            throw new InvalidDataException("Database settings are invalid.");
        }

        return settings with { PlayerName = settings.PlayerName.Trim() };
    }

    public string BuildConnectionString() => new MySqlConnectionStringBuilder
    {
        Server = Server,
        Port = Port,
        Database = Database,
        UserID = User,
        Password = Password,
        ConnectionTimeout = 3,
        DefaultCommandTimeout = 5,
        CancellationTimeout = 1
    }.ConnectionString;
}
