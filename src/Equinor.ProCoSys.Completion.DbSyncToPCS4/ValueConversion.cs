namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Value conversion methods needed to convert different source properties to target sql parameter values.
 * The implementations of these methods is found in SqlParameterConversionHelper
 */
public enum ValueConversion
{
    GuidToLibId,
    OidToPersonId,
    GuidToWorkOrderId,
    GuidToSWCRId,
    GuidToDocumentId, 
    GuidToTagCheckId,
    PunchCategoryToLibId
}
