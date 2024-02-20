using Dapper;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Build an sql statement for insertion of a new row in the PCS 4 database, based on a sourceObject and mapping configuration
 */
public class SqlInsertStatementBuilder(IPcs4Repository pcs4Repository)
{
    public async Task<(string sqlStatement, DynamicParameters sqlParameters)> BuildAsync(ISourceObjectMappingConfig sourceObjectMappingConfig,
                                                                                         object sourceObject,
                                                                                         string plant,
                                                                                         CancellationToken cancellationToken)
    {
        var columnNamesForInsert = new List<string>();
        var parameterValuesForInsert = new List<string>();
        var sqlParameters = new DynamicParameters();

        foreach (var propertyMapping in sourceObjectMappingConfig.PropertyMappings)
        {
            if (propertyMapping.TargetFixedValue is null)
            {
                //Regular column
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

                if (targetColumnValue is not null)
                {
                    sqlParameters.Add($":{propertyMapping.TargetColumnName}", targetColumnValue);
                    columnNamesForInsert.Add(propertyMapping.TargetColumnName);
                    parameterValuesForInsert.Add($":{propertyMapping.TargetColumnName}"); //We use prepared statement (with parametrized sql statement).
                }
            }
            else
            {
                //Column value will be the fixed value (can be a sequence, e.g SEQ_PUNCHLISTITEM.NEXTVAL). 
                columnNamesForInsert.Add(propertyMapping.TargetColumnName);
                parameterValuesForInsert.Add($"{propertyMapping.TargetFixedValue}"); //We use prepared statement (with parametrized sql statement).
            }
        }

        var insertStatement = $"insert into {sourceObjectMappingConfig.TargetTable} " +
            $"( {string.Join(", ", columnNamesForInsert)} ) values " +
            $"( {string.Join(", ", parameterValuesForInsert)} )";

        return (insertStatement, sqlParameters);
    }
}
