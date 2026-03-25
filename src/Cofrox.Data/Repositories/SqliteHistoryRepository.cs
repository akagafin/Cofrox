using Cofrox.Domain.Entities;
using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Windows.Storage;

namespace Cofrox.Data.Repositories;

public sealed class SqliteHistoryRepository : IHistoryRepository
{
    private readonly string _connectionString;

    public SqliteHistoryRepository()
    {
        var databasePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "cofrox-history.db");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS History (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FileName TEXT NOT NULL,
                SourceFormat TEXT NOT NULL,
                TargetFormat TEXT NOT NULL,
                ConvertedAt TEXT NOT NULL,
                Status INTEGER NOT NULL,
                OutputPath TEXT NULL,
                Message TEXT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<HistoryEntry>> GetRecentAsync(int count, CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        var results = new List<HistoryEntry>();
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, FileName, SourceFormat, TargetFormat, ConvertedAt, Status, OutputPath, Message
            FROM History
            ORDER BY datetime(ConvertedAt) DESC
            LIMIT $count;
            """;
        command.Parameters.AddWithValue("$count", count);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
        {
            results.Add(new HistoryEntry
            {
                Id = reader.GetInt64(0),
                FileName = reader.GetString(1),
                SourceFormat = reader.GetString(2),
                TargetFormat = reader.GetString(3),
                ConvertedAt = DateTimeOffset.Parse(reader.GetString(4)),
                Status = (HistoryEntryStatus)reader.GetInt32(5),
                OutputPath = reader.IsDBNull(6) ? null : reader.GetString(6),
                Message = reader.IsDBNull(7) ? null : reader.GetString(7),
            });
        }

        return results;
    }

    public async Task AddAsync(HistoryEntry entry, CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO History (FileName, SourceFormat, TargetFormat, ConvertedAt, Status, OutputPath, Message)
            VALUES ($fileName, $sourceFormat, $targetFormat, $convertedAt, $status, $outputPath, $message);
            """;
        command.Parameters.AddWithValue("$fileName", entry.FileName);
        command.Parameters.AddWithValue("$sourceFormat", entry.SourceFormat);
        command.Parameters.AddWithValue("$targetFormat", entry.TargetFormat);
        command.Parameters.AddWithValue("$convertedAt", entry.ConvertedAt.ToString("O"));
        command.Parameters.AddWithValue("$status", (int)entry.Status);
        command.Parameters.AddWithValue("$outputPath", (object?)entry.OutputPath ?? DBNull.Value);
        command.Parameters.AddWithValue("$message", (object?)entry.Message ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM History;";
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
