namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Tests;

public record NestedSourceTestObject(Guid? guid)
{
    public Guid? Guid { get; } = guid;
}
