using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MultiModuleApp.Infrastructure.Data;

public interface IOracleRepository
{
    Task<DataTable> ExecuteQueryAsync(string query, params OracleParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(string query, params OracleParameter[] parameters);
    Task<object?> ExecuteScalarAsync(string query, params OracleParameter[] parameters);
}

public class OracleRepository : IOracleRepository, IDisposable
{
    private readonly string _connectionString;
    private OracleConnection? _connection;

    public OracleRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private OracleConnection GetConnection()
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            _connection = new OracleConnection(_connectionString);
            _connection.Open();
        }
        return _connection;
    }

    public async Task<DataTable> ExecuteQueryAsync(string query, params OracleParameter[] parameters)
    {
        using var command = GetConnection().CreateCommand();
        command.CommandText = query;
        command.Parameters.AddRange(parameters);

        using var adapter = new OracleDataAdapter(command);
        var table = new DataTable();
        await Task.Run(() => adapter.Fill(table));
        return table;
    }

    public async Task<int> ExecuteNonQueryAsync(string query, params OracleParameter[] parameters)
    {
        using var command = GetConnection().CreateCommand();
        command.CommandText = query;
        command.Parameters.AddRange(parameters);

        return await command.ExecuteNonQueryAsync();
    }

    public async Task<object?> ExecuteScalarAsync(string query, params OracleParameter[] parameters)
    {
        using var command = GetConnection().CreateCommand();
        command.CommandText = query;
        command.Parameters.AddRange(parameters);

        return await command.ExecuteScalarAsync();
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
