using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Data;
using Equinor.ProCoSys.DbView.WebApi.Oracle;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    public class TagRepository : ITagRepository
    {
        private readonly ILogger<TagRepository> _logger;
        private readonly string _connectionString;

        private static readonly string s_tagNo = "TagNo";
        private static readonly string s_project = "Project";
        private static readonly string s_punchCount = "PunchCount";
        private static readonly string s_commPkgNo = "CommPkgNo";
        private static readonly string s_commPkgDesc = "CommPkgDesc";
        private static readonly string s_mcPkgNo = "McPkgNo";
        private static readonly string s_mcPkgDesc = "McPkgDesc";
        private static readonly string s_priority = "Priority";
        private static readonly string s_phase = "Phase";
        private static readonly string s_rfcc = "RFCC";
        private static readonly string s_rfoc = "RFOC";
        private static readonly string s_responsible = "Responsible";
        private static readonly string s_status = "Status";

        private static readonly string s_instCodeToken = "[INSTCODE]";
        
        private static readonly string s_tagQuery =
            $@"SELECT 
                 CT.TAGNO AS ""{s_tagNo}"",
                 CT.PROJECT AS ""{s_project}"",
                 COUNT (P.PUNCHTYPE) AS ""{s_punchCount}"",
                 CT.COMMPKGNO AS ""{s_commPkgNo}"",
                 H.DESCRIPTION AS ""{s_commPkgDesc}"",
                 MCPT.MCPKGNO AS ""{s_mcPkgNo}"",
                 MCP.DESCRIPTION AS ""{s_mcPkgDesc}"",
                 H.PRIORITY AS ""{s_priority}"",
                 H.PHASE AS ""{s_phase}"",
                 (CASE
                     WHEN H.CNT_MCPKGS_WITH_RFCC_SIGNED = 0
                     THEN
                        'Not sent'
                     WHEN     H.CNT_MCPKGS_WITH_RFCC_SIGNED > 0
                          AND H.CNT_MCPKGS_WITH_RFCC_SIGNED < H.CNT_MCPKGS
                     THEN
                        'Partly accepted'
                     WHEN H.CNT_MCPKGS_WITH_RFCC_SIGNED = H.CNT_MCPKGS
                     THEN
                        'Fully accepted'
                  END)
                    AS ""{s_rfcc}"",
                 (CASE
                     WHEN H.CNT_MCPKGS_WITH_RFOC_SIGNED = 0
                     THEN
                        'Not sent'
                     WHEN     H.CNT_MCPKGS_WITH_RFOC_SIGNED > 0
                          AND H.CNT_MCPKGS_WITH_RFOC_SIGNED < H.CNT_MCPKGS
                     THEN
                        'Partly accepted'
                     WHEN H.CNT_MCPKGS_WITH_RFOC_SIGNED = H.CNT_MCPKGS
                     THEN
                        'Fully accepted'
                  END)
                    AS ""{s_rfoc}"",
                 T.FORMRESPONSIBLE AS ""{s_responsible}"",
                 T.FORMSTATUS AS ""{s_status}""
            FROM PROCOSYS_TIE.HOLO$COMMPKG_TAG CT
                 LEFT JOIN PROCOSYS_TIE.TIME$HANDOVER H
                    ON (CT.PLANT = H.PLANT AND CT.COMMPKGNO = H.COMMPKGNO)
                 LEFT JOIN PROCOSYS_TIE.HOLO$PUNCH P
                    ON (CT.PLANT = P.PLANT AND CT.TAGNO = P.TAGNO)
                 LEFT JOIN PROCOSYS_TIE.HOLO$TAGCHECK T
                    ON (    CT.PLANT = T.PLANT
                        AND CT.PROJECT = T.PROJECT
                        AND CT.TAGNO = T.TAGNO)
                 LEFT JOIN PROCOSYS_TIE.HOLO$MCPKG_TAG MCPT
                    ON (    CT.PLANT = MCPT.PLANT
                        AND CT.PROJECT = MCPT.PROJECT
                        AND CT.TAGNO = MCPT.TAGNO)
                 LEFT JOIN PROCOSYS_TIE.HOLO$MCPKG MCP
                    ON (    CT.PLANT = MCP.PLANT
                        AND CT.PROJECT = MCP.PROJECT
                        AND MCPT.MCPKGNO = MCP.MCPKGNO)
           WHERE CT.PLANT = '{s_instCodeToken}'
        GROUP BY CT.TAGNO,
                 CT.PROJECT,
                 CT.COMMPKGNO,
                 H.DESCRIPTION,
                 H.PRIORITY,
                 H.PHASE,
                 (CASE
                     WHEN H.CNT_MCPKGS_WITH_RFCC_SIGNED = 0
                     THEN
                        'Not sent'
                     WHEN     H.CNT_MCPKGS_WITH_RFCC_SIGNED > 0
                          AND H.CNT_MCPKGS_WITH_RFCC_SIGNED < H.CNT_MCPKGS
                     THEN
                        'Partly accepted'
                     WHEN H.CNT_MCPKGS_WITH_RFCC_SIGNED = H.CNT_MCPKGS
                     THEN
                        'Fully accepted'
                  END),
                 (CASE
                     WHEN H.CNT_MCPKGS_WITH_RFOC_SIGNED = 0
                     THEN
                        'Not sent'
                     WHEN     H.CNT_MCPKGS_WITH_RFOC_SIGNED > 0
                          AND H.CNT_MCPKGS_WITH_RFOC_SIGNED < H.CNT_MCPKGS
                     THEN
                        'Partly accepted'
                     WHEN H.CNT_MCPKGS_WITH_RFOC_SIGNED = H.CNT_MCPKGS
                     THEN
                        'Fully accepted'
                  END),
                 T.FORMRESPONSIBLE,
                 T.FORMSTATUS,
                 MCPT.MCPKGNO,
                 MCP.DESCRIPTION";

        public TagRepository(
            IConfiguration configuration,
            ILogger<TagRepository> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("ProCoSys");
        }

        public TagModel GetPage(string installationCode, int currentPage, int itemsPerPage, int takeMax = 0)
        {
            var (tags, timeUsed) = GetTagPropertiesInstances(installationCode, currentPage, itemsPerPage);
            _logger.LogInformation($"Got {tags.Count()} records for 3D Ecosystems, page {currentPage}, pagesize {itemsPerPage} during {FormatTimeSpan(timeUsed)}");

            return new TagModel
            {
                TimeUsed = FormatTimeSpan(timeUsed),
                Heading = GetTagPropertiesHeading(),
                Tags = takeMax > 0 ? tags.Take(takeMax) : tags,
                Count = tags.Count()
            };
        }

        private string FormatTimeSpan(TimeSpan ts) => $"{ts.Hours:00}h {ts.Minutes:00}m {ts.Seconds:00}s";
        
        private IEnumerable<string> GetTagPropertiesHeading()
            => new List<string>
            {
                s_tagNo,
                s_project,
                s_punchCount,
                s_commPkgNo,
                s_commPkgDesc,
                s_mcPkgNo,
                s_mcPkgDesc,
                s_priority,
                s_phase,
                s_rfcc,
                s_rfoc,
                s_responsible,
                s_status
            };

        private static IEnumerable<IEnumerable<object>> GeTagPropertiesInstances(DataTable dataTable)
        {
            var instances = (from DataRow row in dataTable.Rows
                select new List<object>
            {
                    row[s_tagNo] as string,
                    row[s_project] as string,
                    Convert.ToInt32(row[s_punchCount]),
                    row[s_commPkgNo] as string,
                    row[s_commPkgDesc] as string,
                    row[s_mcPkgNo] as string,
                    row[s_mcPkgDesc] as string,
                    row[s_priority] as string,
                    row[s_phase] as string,
                    row[s_rfcc] as string,
                    row[s_rfoc] as string,
                    row[s_responsible] as string,
                    row[s_status] as string
                }
                ).ToList();
            return instances;
        }

        private (long, TimeSpan) CountMaxAvailable(string installationCode)
        {
            DataTable result = null;
            var oracleDatabase = new OracleDb(_connectionString);
            try
            {
                var strSql = $@"SELECT COUNT(*) AS COUNT_ALL FROM ({s_tagQuery.Replace(s_instCodeToken, installationCode)})";
                _logger.LogInformation($"Counting records for 3D Ecosystems for installation code {installationCode}");
                
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

        private (IEnumerable<IEnumerable<object>>, TimeSpan) GetTagPropertiesInstances(string installationCode, int currentPage, int itemsPerPage)
        {
            DataTable result = null;

            var oracleDatabase = new OracleDb(_connectionString);
            try
            {
                var strSql = s_tagQuery.Replace(s_instCodeToken, installationCode);
                var message = $"Getting {itemsPerPage} records at page {currentPage} for 3D Ecosystems for installation code {installationCode}";
                strSql += 
                    $@" ORDER BY CT.TAGNO,
                     CT.PROJECT,
                     CT.COMMPKGNO,
                     T.FORMRESPONSIBLE
                    OFFSET {currentPage * itemsPerPage} ROWS FETCH NEXT {itemsPerPage} ROWS ONLY";
                _logger.LogInformation(message);
                
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                result = oracleDatabase.QueryDataTable(strSql);
                var instances = GeTagPropertiesInstances(result);
                stopWatch.Stop();
                
                return (instances, stopWatch.Elapsed);
            }
            finally
            {
                result?.Dispose();
                oracleDatabase.Close();
            }
        }
    }
}
