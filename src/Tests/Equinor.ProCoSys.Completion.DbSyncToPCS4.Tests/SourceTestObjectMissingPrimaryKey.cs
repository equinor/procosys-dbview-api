#nullable enable
namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public record SourceTestObjectMissingPrimaryKey(
    string? TestOnlyForInsert,
    string? TestString,
    DateTime? TestDate,
    DateTime? TestDate2,
    bool TestBool,
    int? TestInt,
    NestedSourceTestObject NestedObject,
    Guid? WoGuid,
    Guid? SwcrGuid,
    Guid? PersonOid,
    Guid? DocumentGuid)
{
    public string? TestOnlyForInsert { get; } = TestOnlyForInsert;
    public string? TestString { get; } = TestString;
    public DateTime? TestDate { get; } = TestDate;
    public DateTime? TestDate2 { get; } = TestDate2;
    public bool TestBool { get; } = TestBool;
    public int? TestInt { get; } = TestInt;
    public NestedSourceTestObject NestedObject { get; } = NestedObject;
    public Guid? WoGuid { get; } = WoGuid;
    public Guid? SwcrGuid { get; } = SwcrGuid;
    public Guid? PersonOid { get; } = PersonOid;
    public Guid? DocumentGuid { get; } = DocumentGuid;
}
