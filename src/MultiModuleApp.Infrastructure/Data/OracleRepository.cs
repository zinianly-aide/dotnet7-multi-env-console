using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace MultiModuleApp.Infrastructure.Data;

public interface IOracleRepository
{
    Task<DataTable> ExecuteQueryAsync(string query, params OracleParameter[] parameters);
    Task<int> ExecuteNonQueryAsync(string query, params OracleParameter[] parameters);
    Task<object?> ExecuteScalarAsync(string query, params OracleParameter[] parameters);
}

public class OracleRepository : IOracleRepository
{
    private readonly string _connectionString;

    public OracleRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private OracleConnection CreateConnection()
    {
        var connection = new OracleConnection(_connectionString);
        connection.Open();
        return connection;
    }

    public async Task<DataTable> ExecuteQueryAsync(string query, params OracleParameter[] parameters)
    {
        await using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.AddRange(parameters);

        using var adapter = new OracleDataAdapter(command);
        var table = new DataTable();
        await Task.Run(() => adapter.Fill(table));
        return table;
    }

    public async Task<int> ExecuteNonQueryAsync(string query, params OracleParameter[] parameters)
    {
        await using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.AddRange(parameters);

        return await command.ExecuteNonQueryAsync();
    }

    public async Task<object?> ExecuteScalarAsync(string query, params OracleParameter[] parameters)
    {
        await using var connection = CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.AddRange(parameters);

        return await command.ExecuteScalarAsync();
    }
}
