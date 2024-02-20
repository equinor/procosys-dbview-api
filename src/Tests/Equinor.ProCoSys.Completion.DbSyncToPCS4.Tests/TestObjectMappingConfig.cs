namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public class TestObjectMappingConfig : ISourceObjectMappingConfig
{
    public string TargetTable { get; } = "TestTargetTable";

    public PropertyMapping PrimaryKey { get; } = new("TestGuid", PropertyType.Guid, "TestGuid", null, null, false);

    public TestObjectMappingConfig() => PropertyMappings =
        [
            new("TestOnlyForInsert", PropertyType.String, "TestOnlyForInsert", null, null, true),
            new("TestFixedValue", PropertyType.String, "TestFixedValue", null, "'Fixed value'", true),
            new("TestGuid", PropertyType.Guid, "TestGuid", null, null, true),
            new("TestString", PropertyType.String, "TestString", null, null, false),
            new("TestDate", PropertyType.DateTime, "TestDateWithTime", null, null, false),
            new("TestDate2", PropertyType.DateTime, "TestDate", null, null, false),
            new("TestBool", PropertyType.Bool, "TestBool", null, null, false),
            new("TestInt", PropertyType.Int, "TestInt", null, null, false),
            new("NestedObject.Guid", PropertyType.Guid, "TestLibId", ValueConversion.GuidToLibId, null, false),
            new("WoGuid", PropertyType.Guid, "WoGuidLibId", ValueConversion.GuidToWorkOrderId, null, false),
            new("SwcrGuid", PropertyType.Guid, "SwcrLibId", ValueConversion.GuidToSWCRId, null, false),
            new("PersonOID", PropertyType.Guid, "PersonOid", ValueConversion.OidToPersonId, null, false),
            new("DocumentGuid", PropertyType.Guid, "DocumentId", ValueConversion.GuidToDocumentId, null, false),
        ];

    public List<PropertyMapping> PropertyMappings { get; }
}
