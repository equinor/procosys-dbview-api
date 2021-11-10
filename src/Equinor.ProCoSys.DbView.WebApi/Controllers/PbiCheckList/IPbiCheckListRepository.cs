using System;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.PbiCheckList
{
    public interface IPbiCheckListRepository
    {
        PbiCheckListMaxAvailableModel GetMaxAvailable(DateTime? cutoffDate);
        PbiCheckListModel GetPage(int currentPage, int itemsPerPage, DateTime? cutoffDate, int takeMax = 0);
    }
}
