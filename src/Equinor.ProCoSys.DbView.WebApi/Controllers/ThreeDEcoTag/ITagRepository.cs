namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    public interface ITagRepository
    {
        TagMaxAvailableModel GetMaxAvailable(string installationCode);
        TagModel GetPage(string installationCode, int currentPage, int itemsPerPage, int takeMax = 0);
    }
}
