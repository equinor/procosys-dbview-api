namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.MappingConfig;

public class PunchItemMappingConfig : ISourceObjectMappingConfig
{
    public string TargetTable { get; } = "PunchListItem";

    public PropertyMapping PrimaryKey { get; } = new("Guid", PropertyType.Guid, "Procosys_guid", null, null, false);

    public PunchItemMappingConfig() => PropertyMappings = new List<PropertyMapping>
        {
            new ("Plant",                  PropertyType.String,      "ProjectSchema",            null,                                   null,                          true),
            new ("Id",                     PropertyType.Int,         "PunchListItem_id",         null,                                   "SEQ_PUNCHLISTITEM.NEXTVAL",   true),
            new ("Guid",                   PropertyType.Guid,        "Procosys_guid",            null,                                   null,                          true),
            new ("CheckListGuid",          PropertyType.Guid,        "TagCheck_id",              ValueConversion.GuidToTagCheckId,       null,                          true),
            new ("CreatedBy.Oid",          PropertyType.Guid,        "CreatedBy_id",             ValueConversion.OidToPersonId,          null,                          true),
            new ("CreatedAtUtc",           PropertyType.DateTime,    "CreatedAt",                null,                                   null,                          true),
            new ("Category",               PropertyType.String,      "Status_id",                ValueConversion.PunchCategoryToLibId,   null,                          false),
            new ("Description",            PropertyType.String,      "Description",              null,                                   null,                          false),
            new ("RaisedByOrgGuid",        PropertyType.Guid,        "RaisedByOrg_id",           ValueConversion.GuidToLibId,            null,                          false),
            new ("ClearingByOrgGuid",      PropertyType.Guid,        "ClearedByOrg_id",          ValueConversion.GuidToLibId,            null,                          false),
            new ("ActionBy.Oid",           PropertyType.Guid,        "ActionByPerson_id",        ValueConversion.OidToPersonId,          null,                          false),
            new ("DueTimeUtc",             PropertyType.DateTime,    "DueDate",                  null,                                   null,                          false),
            new ("Estimate",               PropertyType.Int,         "Estimate",                 null,                                   null,                          false),
            new ("PriorityGuid",           PropertyType.Guid,        "Priority_id",              ValueConversion.GuidToLibId,            null,                          false),
            new ("SortingGuid",            PropertyType.Guid,        "PunchListSorting_id",      ValueConversion.GuidToLibId,            null,                          false),
            new ("TypeGuid",               PropertyType.Guid,        "PunchListType_id",         ValueConversion.GuidToLibId,            null,                          false),
            new ("OriginalWorkOrderGuid",  PropertyType.Guid,        "OriginalWO_id",            ValueConversion.GuidToWorkOrderId,      null,                          false),
            new ("WorkOrderGuid",          PropertyType.Guid,        "WO_id",                    ValueConversion.GuidToWorkOrderId,      null,                          false),
            new ("SWCRGuid",               PropertyType.Guid,        "SWCR_id",                  ValueConversion.GuidToSWCRId,           null,                          false),
            new ("DocumentGuid",           PropertyType.Guid,        "Drawing_id",               ValueConversion.GuidToDocumentId,       null,                          false),
            new ("ExternalItemNo",         PropertyType.String,      "External_ItemNo",          null,                                   null,                          false),
            new ("MaterialRequired",       PropertyType.Bool,        "IsMaterialRequired",       null,                                   null,                          false),
            new ("MaterialETAUtc",         PropertyType.DateTime,    "Material_ETA",             null,                                   null,                          false),
            new ("MaterialExternalNo",     PropertyType.String,      "MaterialNo",               null,                                   null,                          false),
            new ("ClearedBy.Oid",          PropertyType.Guid,        "ClearedBY_id",             ValueConversion.OidToPersonId,          null,                          false),
            new ("ClearedAtUtc",           PropertyType.DateTime,    "ClearedAt",                null,                                   null,                          false),
            new ("RejectedBy.Oid",         PropertyType.Guid,        "RejectedBY_id",            ValueConversion.OidToPersonId,          null,                          false),
            new ("RejectedAtUtc",          PropertyType.DateTime,    "RejectedAt",               null,                                   null,                          false),
            new ("VerifiedBy.Oid",         PropertyType.Guid,        "VerifiedBy_id",            ValueConversion.OidToPersonId,          null,                          false),
            new ("VerifiedAtUtc",          PropertyType.DateTime,    "VerifiedAt",               null,                                   null,                          false),
            new ("ModifiedBy.Oid",         PropertyType.Guid,        "UpdatedBy_id",             ValueConversion.OidToPersonId,          null,                          false),
            new ("ModifiedAtUtc",          PropertyType.DateTime,    "UpdatedAt",                null,                                   null,                          false),
        };

    public List<PropertyMapping> PropertyMappings { get; }
}
