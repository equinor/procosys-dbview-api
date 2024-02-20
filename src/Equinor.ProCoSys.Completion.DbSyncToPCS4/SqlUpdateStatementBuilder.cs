using System.Text;
using Dapper;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Build an sql statement for update of a row in the PCS 4 database, based on a sourceObject and mapping configuration
 */
public class SqlUpdateStatementBuilder(IPcs4Repository pcs4Repository)
{
    public async Task<(string sqlStatement, DynamicParameters sqlParameters)> BuildAsync(ISourceObjectMappingConfig sourceObjectMappingConfig,
                                                                                         object sourceObject,
                                                                                         string plant,
                                                                                         CancellationToken cancellationToken)
    {
        var updateStatement = new StringBuilder($"update {sourceObjectMappingConfig.TargetTable} set ");
        var sqlParameters = new DynamicParameters();

        await AddParameters(sourceObjectMappingConfig, sourceObject, plant, updateStatement, sqlParameters, cancellationToken);
        await AddWhereClause(sourceObjectMappingConfig, sourceObject, plant, updateStatement, sqlParameters, cancellationToken);

        return (updateStatement.ToString(), sqlParameters);
    }

    /**
     * Add parameters to the update statement and to the list of dynamic parameters.
     */
    private async Task AddParameters(ISourceObjectMappingConfig sourceObjectMappingConfig,
                                     object sourceObject,
                                     string plant,
                                     StringBuilder updateStatement,
                                     DynamicParameters sqlParameters,
                                     CancellationToken cancellationToken)
    {
        foreach (var propertyMapping in sourceObjectMappingConfig.PropertyMappings)
        {
            if (propertyMapping.OnlyForInsert)
            {
                continue;
            }

            var (sourcePropertyValue, sourcePropertyExists) = PropertyMapping.GetSourcePropertyValue(propertyMapping.SourcePropertyName, sourceObject);

            if (!sourcePropertyExists)
            {
                continue; //If source object does not contain a configured property, we skip it.
            }

            var targetColumnValue = await SqlParameterConversionHelper.ConvertSourceValueToTargetValueAsync(sourcePropertyValue,
                                                                                                            propertyMapping,
                                                                                                            plant,
                                                                                                            pcs4Repository,
                                                                                                            cancellationToken);

            sqlParameters.Add($":{propertyMapping.TargetColumnName}", targetColumnValue);
            updateStatement.Append($"{propertyMapping.TargetColumnName} = :{propertyMapping.TargetColumnName}");

            if (propertyMapping != sourceObjectMappingConfig.PropertyMappings.Last())
            {
                updateStatement.Append(", ");
            }

        }
    }

    /**
     * Add a where clause with the primary key, to the sql statement
     */
    private async Task AddWhereClause(ISourceObjectMappingConfig sourceObjectMappingConfig,
                                      object sourceObject,
                                      string plant,
                                      StringBuilder updateStatement,
                                      DynamicParameters sqlParameters,
                                      CancellationToken cancellationToken)
    {
        var (primaryKeyValue, primaryKeyExists) = PropertyMapping.GetSourcePropertyValue(sourceObjectMappingConfig.PrimaryKey.SourcePropertyName, sourceObject);

        if (!primaryKeyExists)
        {
            throw new Exception($"Primary key given by the property '{sourceObjectMappingConfig.PrimaryKey.SourcePropertyName}' is not found in the source object.");
        }

        var primaryKeyTargetValue = await SqlParameterConversionHelper.ConvertSourceValueToTargetValueAsync(primaryKeyValue,
                                                                                                            sourceObjectMappingConfig.PrimaryKey,
                                                                                                            plant,
                                                                                                            pcs4Repository,
                                                                                                            cancellationToken);

        if (primaryKeyTargetValue is null)
        {
            throw new Exception("Not able to build update statement. Primary key target value is null.");
        }

        var primaryKeyName = sourceObjectMappingConfig.PrimaryKey.TargetColumnName;

        sqlParameters.Add($":{primaryKeyName}", primaryKeyTargetValue);
        updateStatement.Append($" where {primaryKeyName} = :{primaryKeyName}");
    }
}
