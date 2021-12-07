namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    public interface ITagRepository
    {
        TagMaxAvailableModel GetMaxAvailable(string plantName);
        TagModel GetPage(string plantName, int currentPage, int itemsPerPage, int takeMax = 0);
    }
}
