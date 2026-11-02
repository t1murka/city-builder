using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using city_builder.World;

namespace city_builder.Persistence;

public sealed class SessionPersistence
{
    private readonly MySqlSessionRepository? _repository;
    private readonly string _playerName;
    private readonly bool _invalidConfiguration;
    private readonly List<Task<bool>> _pendingSaves = new();
    private Task<long?> _startTask = Task.FromResult<long?>(null);
    private Task<bool>? _saveTask;

    public SessionPersistence(MySqlSessionRepository? repository, string playerName = "Jugador",
        bool invalidConfiguration = false)
    {
        _repository = repository;
        _playerName = playerName;
        _invalidConfiguration = invalidConfiguration;
    }

    public DatabaseStatus Status
    {
        get
        {
            if (_invalidConfiguration) return DatabaseStatus.InvalidConfiguration;
            if (_repository is null) return DatabaseStatus.Disabled;
            if (_saveTask is { } save)
            {
                return !save.IsCompleted ? DatabaseStatus.Saving
                    : save.Result ? DatabaseStatus.Saved : DatabaseStatus.Failed;
            }

            return !_startTask.IsCompleted ? DatabaseStatus.Connecting
                : _startTask.Result.HasValue ? DatabaseStatus.Ready : DatabaseStatus.Unavailable;
        }
    }

    public static SessionPersistence FromFile(string path)
    {
        try
        {
            var settings = DatabaseSettings.Load(path);
            return settings is null ? new SessionPersistence(null)
                : new SessionPersistence(new MySqlSessionRepository(settings.BuildConnectionString()), settings.PlayerName);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException
            or JsonException or ArgumentException)
        {
            Trace.TraceError("Invalid database configuration ({0}).", exception.GetType().Name);
            return new SessionPersistence(null, invalidConfiguration: true);
        }
    }

    public void StartNewSession()
    {
        _saveTask = null;
        _pendingSaves.RemoveAll(task => task.IsCompleted);
        _startTask = _repository is null ? Task.FromResult<long?>(null)
            : StartAsync(DateTime.UtcNow);
    }

    public void FinishSession(CityResult result)
    {
        if (_repository is null || _saveTask is not null)
        {
            return;
        }

        _saveTask = SaveAsync(_startTask, DateTime.UtcNow, result);
        _pendingSaves.Add(_saveTask);
    }

    public void WaitForPendingSaves() => Task.WhenAll(_pendingSaves).GetAwaiter().GetResult();

    private async Task<long?> StartAsync(DateTime startedAtUtc)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            return await _repository!.StartSessionAsync(_playerName, startedAtUtc, timeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (IsDatabaseFailure(exception))
        {
            Trace.TraceError("Session registration failed ({0}).", exception.GetType().Name);
            return null;
        }
    }

    private async Task<bool> SaveAsync(Task<long?> startTask, DateTime endedAtUtc, CityResult result)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            var sessionId = await startTask.WaitAsync(timeout.Token).ConfigureAwait(false);
            if (sessionId is null)
            {
                return false;
            }

            await _repository!.SaveResultAsync(sessionId.Value, endedAtUtc, result, timeout.Token).ConfigureAwait(false);
            return true;
        }
        catch (Exception exception) when (IsDatabaseFailure(exception))
        {
            Trace.TraceError("Result persistence failed ({0}).", exception.GetType().Name);
            return false;
        }
    }

    private static bool IsDatabaseFailure(Exception exception) =>
        exception is DbException or OperationCanceledException or TimeoutException or IOException or InvalidOperationException;
}
