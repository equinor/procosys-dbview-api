using Equinor.ProCoSys.Completion.DbSyncToPCS4.MappingConfig;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * This class provide an interface to the operations for synchronizing data to ProCoSys 4. 
 * The class should only be used as a singleton (through dependency injection). 
 */
public class SyncToPCS4Service : ISyncToPCS4Service
{
    private readonly IPcs4Repository _pcs4Repository;
    private readonly IOptionsMonitor<SyncToPCS4Options> _options;

    public SyncToPCS4Service(IPcs4Repository pcs4Repository, IOptionsMonitor<SyncToPCS4Options> options)
    {
        _pcs4Repository = pcs4Repository;
        _options = options;
    }

    public const string PunchItem = "PunchItem";

    /**
     * Insert a new row in the PCS 4 database based on the sourceObject.
     * Note: The GetMappingConfigurationForSourceObject method in this class must have support for the given sourceObjectName.
     */
    public async Task SyncNewObjectAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }
        var sourceObjectMappingConfig = GetMappingConfigurationForSourceObject(sourceObjectName);

        var sqlInsertStatementBuilder = new SqlInsertStatementBuilder(_pcs4Repository);

        var (sqlUpdateStatement, sqlParameters) = await sqlInsertStatementBuilder.BuildAsync(sourceObjectMappingConfig,
                                                                                             sourceObject,
                                                                                             plant,
                                                                                             cancellationToken);

        await _pcs4Repository.ExecuteSingleRowOperationAsync(sqlUpdateStatement, sqlParameters, cancellationToken);
    }

    /**
     * Updates the PCS 4 database with changes provided by the sourceObject. 
     * Note: The GetMappingConfigurationForSourceObject method in this class must have support for the given sourceObjectName.
     */
    public async Task SyncObjectUpdateAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }
        var sourceObjectMappingConfig = GetMappingConfigurationForSourceObject(sourceObjectName);

        var sqlUpdateStatementBuilder = new SqlUpdateStatementBuilder(_pcs4Repository);

        var (sqlUpdateStatement, sqlParameters) = await sqlUpdateStatementBuilder.BuildAsync(sourceObjectMappingConfig,
                                                                                             sourceObject,
                                                                                             plant,
                                                                                             cancellationToken);

        await _pcs4Repository.ExecuteSingleRowOperationAsync(sqlUpdateStatement, sqlParameters, cancellationToken);
    }

    /**
     * Deletes the row in the PCS 4 database that correspond to the given source object. 
     * Note: The GetMappingConfigurationForSourceObject method in this class must have support for the given sourceObjectName.
     */
    public async Task SyncObjectDeletionAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }
        var sourceObjectMappingConfig = GetMappingConfigurationForSourceObject(sourceObjectName);

        var sqlDeleteStatementBuilder = new SqlDeleteStatementBuilder(_pcs4Repository);

        var (sqlDeleteStatement, sqlParameters) = await sqlDeleteStatementBuilder.BuildAsync(sourceObjectMappingConfig,
                                                                                             sourceObject,
                                                                                             plant,
                                                                                             cancellationToken);

        await _pcs4Repository.ExecuteSingleRowOperationAsync(sqlDeleteStatement, sqlParameters, cancellationToken);
    }

    /**
     * Will return the mapping configuration for the given source object
     */
    private static ISourceObjectMappingConfig GetMappingConfigurationForSourceObject(string sourceObjectName)
        => sourceObjectName switch
        {
            PunchItem => new PunchItemMappingConfig(),
            _ => throw new NotImplementedException(
                $"Mapping is not implemented for source object with name '{sourceObjectName}'."),
        };
}


