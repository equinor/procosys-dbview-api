using System.Data;
using Dapper;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Contains methods to execute queries and updates on the Oracle database for PCS 4. 
 */
public class Pcs4Repository : IPcs4Repository
{
    private readonly string _dbConnStr;

    public Pcs4Repository(IOptionsMonitor<SyncToPCS4Options> options)
    {
        _dbConnStr = options.CurrentValue.ConnectionString;

        if (_dbConnStr is null)
        {
            throw new Exception("DB connection string for Oracle is not set.");
        }
    }

    /**
     * This method will execute an sqlStatement to insert, update or delete a row in the Pcs4 database.
     */
    public async Task ExecuteSingleRowOperationAsync(string sqlStatement, DynamicParameters sqlParameters, CancellationToken cancellationToken)
    {
        await using var connection = new OracleConnection(_dbConnStr);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

        try
        {
            var rowsAffected = await connection.ExecuteAsync(sqlStatement, sqlParameters, transaction);

            if (rowsAffected != 1)
            {
                throw new Exception($"Expected to affect one and only one row but got {rowsAffected} numbers of affected rows.");
            }
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            var paramsStr = BuildStringWithParamsForLogging(sqlParameters);

            throw new Exception($"Error occured when trying to execute the following sql statement: {sqlStatement}. " +
                $"Parameters: {paramsStr} A rollback on the transaction is performed.", ex);
        }
    }

    /**
     * This method will be used to fetch a number value from the Pcs4 database. If the value is not found, an exception will be thrown. 
     */
    public async Task<long> ValueLookupNumberAsync(string sqlQuery, DynamicParameters sqlParameters, CancellationToken cancellationToken)
    {
        await using var connection = new OracleConnection(_dbConnStr);
        await connection.OpenAsync(cancellationToken);

        try
        {
            var result = await connection.ExecuteScalarAsync(sqlQuery, sqlParameters);

            if (result is long l)
            {
                return l;
            }

            throw new Exception($"Value lookup failed. Result was {result}.");
        }
        catch (Exception ex)
        {
            var paramsStr = BuildStringWithParamsForLogging(sqlParameters);
            throw new Exception($"Error occured when trying to look up a value in PCS4 database. Sql query: {sqlQuery}. Parameters: {paramsStr}.", ex);
        }
    }

    private static string BuildStringWithParamsForLogging(DynamicParameters sqlParameters)
    {
        var paramList = new List<string>();

        foreach (var name in sqlParameters.ParameterNames)
        {
            var value = sqlParameters.Get<object>(name);
            paramList.Add($"{name}={value}");
        }
        var paramsStr = string.Join(", ", paramList);
        return paramsStr;
    }

}
