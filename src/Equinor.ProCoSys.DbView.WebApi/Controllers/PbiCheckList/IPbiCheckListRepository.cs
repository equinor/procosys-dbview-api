namespace Equinor.ProCoSys.DbView.WebApi.Controllers.PbiCheckList
{
    public interface IPbiCheckListRepository
    {
        PbiCheckListMaxAvailableModel GetMaxAvailable();
        PbiCheckListModel GetPage(int currentPage, int itemsPerPage, int takeMax = 0);
    }
}
