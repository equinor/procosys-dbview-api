namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public interface ISourceObjectMappingConfig
{
    string TargetTable { get; }
    PropertyMapping PrimaryKey { get; }
    List<PropertyMapping> PropertyMappings { get; }
}
