namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    public interface ITagRepository
    {
        TagModel GetPage(string installationCode, int currentPage, int itemsPerPage, int takeMax = 0);
    }
}
