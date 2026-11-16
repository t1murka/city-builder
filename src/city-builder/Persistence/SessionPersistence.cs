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
    private Task<bool> _writeTask = Task.FromResult(true);
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

            if (!_startTask.IsCompleted) return DatabaseStatus.Connecting;
            if (!_startTask.Result.HasValue) return DatabaseStatus.Unavailable;
            return !_writeTask.IsCompleted ? DatabaseStatus.Saving
                : _writeTask.Result ? DatabaseStatus.Ready : DatabaseStatus.Failed;
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
        _writeTask = Task.FromResult(true);
        _pendingSaves.RemoveAll(task => task.IsCompleted);
        _startTask = _repository is null ? Task.FromResult<long?>(null)
            : StartAsync(DateTime.UtcNow);
    }

    public void RecordConstruction(CellPosition position, BuildingType building)
    {
        QueueWrite((sessionId, token) => _repository!.SaveConstructionAsync(sessionId, position, building, token));
    }

    public void RecordResources(int turn, int money, int population, int electricity, int water,
        CityEvent? cityEvent)
    {
        QueueWrite((sessionId, token) =>
            _repository!.SaveResourcesAsync(sessionId, turn, money, population, electricity, water, cityEvent, token));
    }

    private void QueueWrite(Func<long, CancellationToken, Task> write)
    {
        if (_repository is null || _saveTask is not null)
        {
            return;
        }

        _writeTask = SaveWriteAsync(_startTask, _writeTask, write);
        _pendingSaves.Add(_writeTask);
    }

    public void FinishSession(CityResult result)
    {
        if (_repository is null || _saveTask is not null)
        {
            return;
        }

        _saveTask = SaveAsync(_startTask, _writeTask, DateTime.UtcNow, result);
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

    private async Task<bool> SaveWriteAsync(Task<long?> startTask, Task<bool> previousWrite,
        Func<long, CancellationToken, Task> write)
    {
        if (!await previousWrite.ConfigureAwait(false))
        {
            return false;
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            var sessionId = await startTask.WaitAsync(timeout.Token).ConfigureAwait(false);
            if (sessionId is null)
            {
                return false;
            }

            await write(sessionId.Value, timeout.Token).ConfigureAwait(false);
            return true;
        }
        catch (Exception exception) when (IsDatabaseFailure(exception))
        {
            Trace.TraceError("Session data persistence failed ({0}).", exception.GetType().Name);
            return false;
        }
    }

    private async Task<bool> SaveAsync(Task<long?> startTask, Task<bool> writes,
        DateTime endedAtUtc, CityResult result)
    {
        if (!await writes.ConfigureAwait(false))
        {
            return false;
        }

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
