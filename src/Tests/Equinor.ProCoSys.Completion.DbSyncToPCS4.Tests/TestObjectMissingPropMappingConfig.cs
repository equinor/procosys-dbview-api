namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

/**
 * This class is used to test missing property in source object
 */
public class TestObjectMissingPropMappingConfig : ISourceObjectMappingConfig
{
    public string TargetTable { get; } = "TestTargetObject";

    public PropertyMapping PrimaryKey { get; } = new("TestGuid", PropertyType.Guid, "TestGuid", null, null, false);

    public TestObjectMissingPropMappingConfig() => PropertyMappings =
        [
            new PropertyMapping("PropMissing", PropertyType.String, "PropMissing", null, null, false)
        ];

    public List<PropertyMapping> PropertyMappings { get; }
}
