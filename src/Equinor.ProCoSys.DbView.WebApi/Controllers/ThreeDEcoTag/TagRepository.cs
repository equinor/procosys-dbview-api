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
        private static readonly string s_commPkg_Id = "CommPkg_Id";
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

        // Don't use AS-statemens on ID-columns used in order by clause
        private static readonly string s_tagQuery =
            $@"SELECT t.tagno AS ""{s_tagNo}"",
                   t.tag_id,
                   pr.name AS ""{s_project}"",
                   PR.PROJECT_ID,
                   MC.MCPKGNO AS ""{s_mcPkgNo}"",
                   MC.DESCRIPTION AS ""{s_mcPkgDesc}"",
                   C.COMMPKG_ID,
                   C.COMMPKGNO AS ""{s_commPkgNo}"",
                   C.DESCRIPTION AS ""{s_commPkgDesc}"",
                   PRI.CODE AS ""{s_priority}"",
                   PH.CODE AS ""{s_phase}"",
                   RES.CODE AS ""{s_responsible}"",
                   RES.RESPONSIBLE_ID,
                   stat.CODE AS ""{s_status}"",
                   (SELECT COUNT(1)
                      FROM procosys.punchlistitem pi
                     WHERE     PI.TAGCHECK_ID = TC.TAGCHECK_ID
                           AND pi.isvoided = 'N'
                           AND((PI.CLEAREDAT IS NULL) OR(PI.REJECTEDAT IS NOT NULL)))
                      AS ""{s_punchCount}"",
                    'na' AS ""{s_rfcc}"",
                    'na' AS ""{s_rfoc}""
              FROM procosys.tag t
                   INNER JOIN procosys.project pr
                      ON pr.PROJECT_ID = t.project_id
                         AND pr.isclosed = 'N'
                         AND (pr.name LIKE '_.%')
                   INNER JOIN procosys.library fac ON FAC.LIBRARY_ID = t.installation_id
                   INNER JOIN procosys.library lib_register
                      ON LIB_REGISTER.LIBRARY_ID = T.REGISTER_ID
                         AND LIB_REGISTER.code IN ('INSTRUMENT_FIELD',
                                                   'MAIN_EQUIPMENT',
                                                   'ELECTRICAL_FIELD',
                                                   'FIRE_AND_GAS_FIELD',
                                                   'JUNCTION_BOX',
                                                   'MANUAL_VALVE',
                                                   'TELECOM_FIELD',
                                                   'SPECIAL_ITEM',
                                                   'CIVIL',
                                                   'PIPE_SUPPORT',
                                                   'DUCTING',
                                                   'PIPELINE',
                                                   'HOSE_ASSEMBLY')
                   LEFT OUTER JOIN procosys.mcpkg mc ON MC.MCPKG_ID = t.mcpkg_id
                   LEFT OUTER JOIN procosys.commpkg c ON C.COMMPKG_ID = mc.commpkg_id
                   LEFT OUTER JOIN PROCOSYS.TAGFORMULARTYPE TFT ON TFT.TAG_ID = T.TAG_ID
                   LEFT OUTER JOIN PROCOSYS.TAGCHECK TC
                      ON TC.TAGFORMULARTYPE_ID = TFT.TAGFORMULARTYPE_ID
                   LEFT OUTER JOIN procosys.responsible res
                      ON RES.RESPONSIBLE_ID = TC.RESPONSIBLE_ID
                   LEFT OUTER JOIN procosys.library pri
                      ON PRI.LIBRARY_ID = C.COMMPRIORITY_ID
                   LEFT OUTER JOIN procosys.library ph ON PH.LIBRARY_ID = C.COMMPHASE_ID
                   LEFT OUTER JOIN procosys.library stat
                      ON stat.LIBRARY_ID = TC.STATUS_ID
                WHERE FAC.CODE = '{s_instCodeToken}'
            UNION ALL
            SELECT DISTINCT
                   t.tagno AS ""{ s_tagNo}"",
                   t.tag_id,
                   pr.name AS ""{s_project}"",
                   PR.PROJECT_ID,
                   MC.MCPKGNO AS ""{s_mcPkgNo}"",
                   MC.DESCRIPTION AS ""{s_mcPkgDesc}"",
                   C.COMMPKG_ID,
                   C.COMMPKGNO AS ""{s_commPkgNo}"",
                   C.DESCRIPTION AS ""{s_commPkgDesc}"",
                   PRI.CODE AS ""{s_priority}"",
                   PH.CODE AS ""{s_phase}"",
                   RES.CODE AS ""{s_responsible}"",
                   RES.RESPONSIBLE_ID,
                   stat.CODE AS ""{s_status}"",
                   (SELECT COUNT (1)
                      FROM procosys.punchlistitem pi
                     WHERE     PI.TAGCHECK_ID = TC.TAGCHECK_ID
                           AND pi.isvoided = 'N'
                           AND ((PI.CLEAREDAT IS NULL) OR (PI.REJECTEDAT IS NOT NULL)))
                      AS ""{s_punchCount}"",
                    'na' AS ""{s_rfcc}"",
                    'na' AS ""{s_rfoc}""
              FROM procosys.tag t
                   INNER JOIN procosys.project pr
                      ON pr.PROJECT_ID = t.project_id
                         AND pr.isclosed = 'N'
                         AND (pr.name LIKE '_.%')
                   INNER JOIN procosys.library fac ON FAC.LIBRARY_ID = t.installation_id
                   INNER JOIN procosys.library lib_register
                      ON LIB_REGISTER.LIBRARY_ID = T.REGISTER_ID
                         AND LIB_REGISTER.code = 'LINE'
                   LEFT OUTER JOIN procosys.pipingspool ps ON ps.tag_id = t.tag_id
                   LEFT OUTER JOIN procosys.pipingrevision pr
                      ON PR.PIPINGREVISION_ID = PS.PIPINGREVISION_ID
                   LEFT OUTER JOIN procosys.pipetest pt
                      ON pt.PIPINGREVISION_ID = PR.PIPINGREVISION_ID
                   LEFT OUTER JOIN PROCOSYS.TAGCHECK TC
                      ON TC.TAGCHECK_ID = pt.TAGCHECK_ID
                   LEFT OUTER JOIN procosys.mcpkg mc ON MC.MCPKG_ID = pr.mcpkg_id
                   LEFT OUTER JOIN procosys.commpkg c ON C.COMMPKG_ID = mc.commpkg_id
                   LEFT OUTER JOIN procosys.responsible res
                      ON RES.RESPONSIBLE_ID = TC.RESPONSIBLE_ID
                   LEFT OUTER JOIN procosys.library pri
                      ON PRI.LIBRARY_ID = C.COMMPRIORITY_ID
                   LEFT OUTER JOIN procosys.library ph ON PH.LIBRARY_ID = C.COMMPHASE_ID
                   LEFT OUTER JOIN procosys.library stat
                      ON stat.LIBRARY_ID = TC.STATUS_ID
                WHERE FAC.CODE = '{s_instCodeToken}'";

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
                s_status,
                s_commPkg_Id
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
                    row[s_status] as string,
                    row[s_commPkg_Id] == DBNull.Value
                        ? null : Convert.ToInt32(row[s_commPkg_Id]),
                }
                ).ToList();
            return instances;
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
                    $@"
                        ORDER BY TAG_ID,
                        PROJECT_ID,
                        COMMPKG_ID,
                        RESPONSIBLE_ID
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
