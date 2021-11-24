using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data;
using Equinor.ProCoSys.DbView.WebApi.Oracle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.PbiCheckList
{
    public class PbiCheckListRepository : IPbiCheckListRepository
    {
        private readonly ILogger<PbiCheckListRepository> _logger;
        private readonly string _connectionString;
        private readonly string _checkListUrlFormat;

        private const string UpdatedDateColumn = "UPDATED_AT_DATE";
        private static readonly string s_checkListQuery =
            $@"SELECT DISTINCT CHECKLIST_ID,
                            PROJECTSCHEMA,
                            PROJECT,
                            TAGNO,
                            TAG_CATEGORY,
                            FORMULARTYPE,
                            FORMULARGROUP,
                            FORMULARDISCIPLINE,
                            RESPONSIBLE,
                            STATUS,
                            CREATED_AT_DATE,
                            {UpdatedDateColumn},
                            FACILITY,
                            COMMPKGNO,
                            MCPKGNO,
                            PONO,
                            CALLOFFNO,
                            SIGNED_AT_DATE,
                            VERIFIED_AT_DATE,
                            FAT_PLANNED_AT_DATE
                            FROM PROCOSYS_TIE.PBI$CHECKLIST";

        public PbiCheckListRepository(
            IConfiguration configuration,
            IOptionsMonitor<ApiOptions> options,
            ILogger<PbiCheckListRepository> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("ProCoSys");
            var mainUrl = options.CurrentValue.ProCoSysMainUrl.TrimEnd('/');
            _checkListUrlFormat = $"{mainUrl}/{{PROJECTSCHEMA}}/Completion/TagCheck/Form/Main/Index?id={{TAGCHECK_ID}}";
        }

        public PbiCheckListMaxAvailableModel GetMaxAvailable(DateTime? cutoffDate)
        {
            var (maxAvailable, timeUsed) = CountMaxAvailable(cutoffDate);

            _logger.LogInformation($"Found {maxAvailable} records in PBI$CHECKLIST during {FormatTimeSpan(timeUsed)}");
            return new PbiCheckListMaxAvailableModel
            {
                TimeUsed = FormatTimeSpan(timeUsed),
                MaxAvailable = maxAvailable
            };
        }

        public PbiCheckListModel GetPage(int currentPage, int itemsPerPage, DateTime? cutoffDate, int takeMax = 0)
        {
            var (checkLists, timeUsed) = GetCheckListInstances(currentPage, itemsPerPage, cutoffDate);
            _logger.LogInformation($"Got {checkLists.Count} records from PBI$CHECKLIST, page {currentPage}, pagesize {itemsPerPage} during {FormatTimeSpan(timeUsed)}");

            return new PbiCheckListModel
            {
                CheckListUrlFormat = _checkListUrlFormat,
                TimeUsed = FormatTimeSpan(timeUsed),
                CheckLists = takeMax > 0 ? checkLists.Take(takeMax) : checkLists,
                Count = checkLists.Count
            };
        }

        private string FormatTimeSpan(TimeSpan ts) => $"{ts.Hours:00}h {ts.Minutes:00}m {ts.Seconds:00}s";
        
        private static List<CheckListInstance> GetCheckListInstances(DataTable dataTable)
        {
            var checkListInstances = (from DataRow row in dataTable.Rows
                select new CheckListInstance
                {
                    CheckList_Id = (long) row["CHECKLIST_ID"],
                    Projectschema = row["PROJECTSCHEMA"] as string,
                    Project = row["PROJECT"] as string,
                    TagNo = row["TAGNO"] as string,
                    Tag_Category = row["TAG_CATEGORY"] as string,
                    FormularType = row["FORMULARTYPE"] as string,
                    FormularGroup = row["FORMULARGROUP"] as string,
                    FormularDiscipline = row["FORMULARDISCIPLINE"] as string,
                    Responsible = row["RESPONSIBLE"] as string,
                    Status = row["STATUS"] as string,
                    Created_At_Date = (DateTime) row["CREATED_AT_DATE"],
                    Updated_At_Date = (DateTime) row["UPDATED_AT_DATE"],
                    Facility = row["FACILITY"] as string,
                    CommPkgNo = row["COMMPKGNO"] as string,
                    McPkgNo = row["MCPKGNO"] as string,
                    PoNo = row["PONO"] as string,
                    CallOffNo = row["CALLOFFNO"] as string,
                    // ReSharper disable once MergeConditionalExpression
                    Signed_At_Date = row["SIGNED_AT_DATE"] == DBNull.Value
                        ? null
                        : (DateTime?) Convert.ToDateTime(row["SIGNED_AT_DATE"]),
                    // ReSharper disable once MergeConditionalExpression
                    Verified_At_Date = row["VERIFIED_AT_DATE"] == DBNull.Value
                        ? null
                        : (DateTime?) Convert.ToDateTime(row["VERIFIED_AT_DATE"]),
                    // ReSharper disable once MergeConditionalExpression
                    Fat_Planned_At_Date = row["FAT_PLANNED_AT_DATE"] == DBNull.Value
                        ? null
                        : (DateTime?) Convert.ToDateTime(row["FAT_PLANNED_AT_DATE"])
                }).ToList();
            return checkListInstances;
        }

        private (long, TimeSpan) CountMaxAvailable(DateTime? cutoffDate)
        {
            DataTable result = null;
            var oracleDatabase = new OracleDb(_connectionString);
            try
            {
                var strSql = $@"SELECT COUNT(*) AS COUNT_ALL FROM ({s_checkListQuery})";
                var message = "Counting records in PBI$CHECKLIST";
                if (cutoffDate.HasValue)
                {
                    message += $", using cutoff date={cutoffDate.Value:s}";
                    strSql = AddFilterToSql(strSql, cutoffDate.Value);
                }
                _logger.LogInformation(message);
                
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                result = oracleDatabase.QueryDataTable(strSql);
                stopWatch.Stop();
                
                var count = Convert.ToInt64(result.Rows[0]["COUNT_ALL"]);
                return (count, stopWatch.Elapsed);
            }
            finally
            {
                result?.Dispose();
                oracleDatabase.Close();
            }
        }

        private (List<CheckListInstance>, TimeSpan) GetCheckListInstances(int currentPage, int itemsPerPage, DateTime? cutoffDate)
        {
            DataTable result = null;
            // After bugfix Oct 02 2021 in PBI$CHECKLIST view, checklist id is unique pr row: ORDER BY CHECKLIST_ID should be sufficient for pagination

            var oracleDatabase = new OracleDb(_connectionString);
            try
            {
                var strSql = s_checkListQuery;
                var message = $"Getting {itemsPerPage} records at page {currentPage} from PBI$CHECKLIST";
                if (cutoffDate.HasValue)
                {
                    message += $", using cutoff date={cutoffDate.Value:s}";
                    strSql = AddFilterToSql(strSql, cutoffDate.Value);
                }
                strSql += 
                    $@" ORDER BY CHECKLIST_ID
                    OFFSET {currentPage * itemsPerPage} ROWS FETCH NEXT {itemsPerPage} ROWS ONLY";
                _logger.LogInformation(message);
                
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                result = oracleDatabase.QueryDataTable(strSql);
                var checkListInstances = GetCheckListInstances(result);
                stopWatch.Stop();
                
                return (checkListInstances, stopWatch.Elapsed);
            }
            finally
            {
                result?.Dispose();
                oracleDatabase.Close();
            }
        }

        private string AddFilterToSql(string baseSql, DateTime cutoffDate)
        {
            var cutoffDateStr = cutoffDate.ToString("yyyy-MM-dd HH:mm:ss");
            return baseSql + $" WHERE {UpdatedDateColumn} > to_date('{cutoffDateStr}','YYYY-MM-DD HH24:MI:SS')";
        }
    }
}
