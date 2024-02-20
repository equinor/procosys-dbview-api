namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Holds configuration of mapping from a source property to a target column. 
 */
public class PropertyMapping(
    string sourcePropertyName,
    PropertyType sourceType,
    string targetColumnName,
    ValueConversion? valueConversionMethod,
    string? targetFixedValue,
    bool onlyForInsert)
{
    //Name of a property in the source object
    public string SourcePropertyName { get; } = sourcePropertyName;

    //The type of the property in the source object
    public PropertyType SourceType { get; } = sourceType;

    //The corresponding column in the target table
    public string TargetColumnName { get; } = targetColumnName;

    //A method that must be used to convert the source value to the target value. This might involve looking up a value in the target database. 
    public ValueConversion? ValueConversion { get; } = valueConversionMethod;

    //Target value is a fixed value. Can be used to specify a sequence-generation, e.g 'SEQ_PUNCHLISTITEM.NEXTVAL'
    public string? TargetFixedValue { get; } = targetFixedValue;

    //Set to true if this mapping is only to be be used when inserting new rows. This will typically be properties that cannot be modified, like plant and id.  
    public bool OnlyForInsert { get; } = onlyForInsert;

    /**
    * Helper method to find the value of a configured property in an object. 
    * This value might be in a nested property (e.g ActionBy.Oid)
    * Return value of the property, and a bool telling if the property exists in the source object (the property can exist even if the value is null).
    */
    public static (object?, bool) GetSourcePropertyValue(string configuredPropertyName, object sourceObject)
    {
        var sourcePropertyNameParts = configuredPropertyName.Split('.');
        if (sourcePropertyNameParts.Length > 2)
        {
            throw new Exception($"Only one nested level is supported for entities, so {configuredPropertyName} is not supported.");
        }

        var sourcePropertyName = sourcePropertyNameParts[0];
        var sourceProperty = sourceObject.GetType().GetProperty(sourcePropertyName);

        if (sourceProperty is null)
        {
            return (null, false);
        }

        var sourcePropertyValue = sourceProperty.GetValue(sourceObject);

        if (sourcePropertyNameParts.Length > 1)
        {
            //We must find the nested property
            if (sourcePropertyValue is null)
            {
                return (null, false);
            }

            sourceProperty = sourcePropertyValue.GetType().GetProperty(sourcePropertyNameParts[1]);

            if (sourceProperty is null)
            {
                return (null, false);
            }

            sourcePropertyValue = sourceProperty.GetValue(sourcePropertyValue);
        }

        return (sourcePropertyValue, true);
    }
}
