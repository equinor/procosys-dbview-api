using System;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.PbiCheckList
{
    public interface IPbiCheckListRepository
    {
        PbiCheckListMaxAvailableModel GetMaxAvailable(DateTime? cutoffDate);
        PbiCheckListModel GetPage(DateTime? cutoffDate, int currentPage, int itemsPerPage, int takeMax = 0);
    }
}
