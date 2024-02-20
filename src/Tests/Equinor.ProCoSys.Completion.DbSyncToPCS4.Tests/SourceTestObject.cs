#nullable enable
namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public record SourceTestObject(
    string? TestOnlyForInsert,
    Guid TestGuid,
    string? TestString,
    DateTime? TestDate,
    DateTime? TestDate2,
    bool TestBool,
    int? TestInt,
    NestedSourceTestObject NestedObject,
    Guid? WoGuid,
    Guid? SwcrGuid,
    Guid? PersonOID,
    Guid? DocumentGuid)
{
    public string? TestOnlyForInsert { get; } = TestOnlyForInsert;
    public Guid TestGuid { get; } = TestGuid;
    public string? TestString { get; } = TestString;
    public DateTime? TestDate { get; } = TestDate;
    public DateTime? TestDate2 { get; } = TestDate2;
    public bool TestBool { get; } = TestBool;
    public int? TestInt { get; } = TestInt;
    public NestedSourceTestObject NestedObject { get; } = NestedObject;
    public Guid? WoGuid { get; } = WoGuid;
    public Guid? SwcrGuid { get; } = SwcrGuid;
    public Guid? PersonOID { get; } = PersonOID;
    public Guid? DocumentGuid { get; } = DocumentGuid;
}
