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
                      AS ""{s_punchCount}""
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
                      AS ""{s_punchCount}""
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

        private static readonly string s_mcPkgCount = "McPkgCount";
        private static readonly string s_mcPkgsSentToCommissioning = "McPkgsSentToCommissioning";
        private static readonly string s_mcPkgsAcceptedByCommissioning = "McPkgsAcceptedByCommissioning";
        private static readonly string s_mcPkgsRejectedByCommissioning = "McPkgsRejectedByCommissioning";
        private static readonly string s_mcPkgsSentToOperation = "McPkgsSentToOperation";
        private static readonly string s_mcPkgsAcceptedByOperation = "McPkgsAcceptedByOperation";
        private static readonly string s_mcPkgsRejectedByOperation = "McPkgsRejectedByOperation";

        private static readonly string s_commPkgIdsToken = "[COMMPKGIDS]";
        private static readonly string s_commPkgQuery =
            @$"SELECT 
                    CP.COMMPKG_ID AS ""{s_commPkg_Id}"",
                    (SELECT COUNT (*)
                        FROM MCPKG MP
                            JOIN ELEMENT MP_
                                ON (MP_.ELEMENT_ID = MP.MCPKG_ID AND MP_.ISVOIDED = 'N')
                        WHERE MP.COMMPKG_ID = CP.COMMPKG_ID)
                        AS ""{s_mcPkgCount}"",
                    (SELECT COUNT (MP.MCPKG_ID)
                        FROM V$CERTIFICATE C
                            JOIN CERTIFICATESCOPE CS
                                ON (CS.CERTIFICATE_ID = C.CERTIFICATE_ID AND REJECTED = 'N')
                            JOIN MCPKG MP ON (MP.MCPKG_ID = CS.MCPKG_ID)
                        WHERE     MP.COMMPKG_ID = CP.COMMPKG_ID
                            AND C.CERTIFICATETYPE IN ('RFCC', 'RFSC')
                            AND C.ISVOIDED = 'N'
                            AND C.ISSENT = 'Y'
                            AND C.ISREJECTED = 'N')
                        AS ""{s_mcPkgsSentToCommissioning}"",
                    (  (SELECT COUNT (MP.MCPKG_ID)
                            FROM V$CERTIFICATE C
                                JOIN CERTIFICATESCOPE CS
                                    ON (    CS.CERTIFICATE_ID = C.CERTIFICATE_ID
                                        AND REJECTED = 'N')
                                JOIN MCPKG MP ON (MP.MCPKG_ID = CS.MCPKG_ID)
                        WHERE     MP.COMMPKG_ID = CP.COMMPKG_ID
                                AND C.CERTIFICATETYPE = 'RFCC'
                                AND C.ISVOIDED = 'N'
                                AND C.ISACCEPTED = 'Y')

                    /* RFSC HAS TWO ACCEPTED SIGNATURES, ONE FOR COMM AND ONE FOR OP. V$CERTIFICATE.ISACCEPTED ONLY REFLECTS OP. */
                    + (SELECT COUNT (MP.MCPKG_ID)
                            FROM V$CERTIFICATE C
                                JOIN CERTIFICATESCOPE CS
                                    ON (    CS.CERTIFICATE_ID = C.CERTIFICATE_ID
                                        AND REJECTED = 'N')
                                JOIN MCPKG MP ON (MP.MCPKG_ID = CS.MCPKG_ID)
                        WHERE     MP.COMMPKG_ID = CP.COMMPKG_ID
                                AND C.CERTIFICATETYPE = 'RFSC'
                                AND C.ISVOIDED = 'N'
                                AND EXISTS
                                        (SELECT 1
                                        FROM CERTIFICATESIGNATURE SIG
                                                JOIN LIBTOLIBRELATION LTL
                                                ON (    LTL.LIBRARY_ID = SIG.STATUS_ID
                                                    AND LTL.ROLE = 'Classification')
                                                JOIN LIBRARY RL
                                                ON (RL.LIBRARY_ID = LTL.RELATEDLIBRARY_ID)
                                        WHERE     SIG.CERTIFICATE_ID = C.CERTIFICATE_ID
                                                AND RL.CODE = 'ACCEPTED'
                                                AND SIGNEDAT IS NOT NULL)))
                        AS ""{s_mcPkgsAcceptedByCommissioning}"",
                    (SELECT COUNT (DISTINCT MP.MCPKG_ID)
                        FROM V$CERTIFICATE C
                            JOIN CERTIFICATESCOPE CS
                                ON (CS.CERTIFICATE_ID = C.CERTIFICATE_ID AND REJECTED = 'Y')
                            JOIN MCPKG MP ON (MP.MCPKG_ID = CS.MCPKG_ID)
                        WHERE     MP.COMMPKG_ID = CP.COMMPKG_ID
                            AND C.CERTIFICATETYPE IN ('RFCC', 'RFSC')
                            AND C.ISVOIDED = 'N')
                        AS ""{s_mcPkgsRejectedByCommissioning}"",
                    (SELECT COUNT (MP.MCPKG_ID)
                        FROM V$CERTIFICATE C
                            JOIN CERTIFICATESCOPE CS
                                ON (CS.CERTIFICATE_ID = C.CERTIFICATE_ID AND REJECTED = 'N')
                            JOIN MCPKG MP ON (MP.MCPKG_ID = CS.MCPKG_ID)
                        WHERE     MP.COMMPKG_ID = CP.COMMPKG_ID
                            AND C.CERTIFICATETYPE IN ('RFOC', 'RFSC')
                            AND C.ISVOIDED = 'N'
                            AND C.ISSENT = 'Y'
                            AND C.ISREJECTED = 'N')
                        AS ""{s_mcPkgsSentToOperation}"",
                    (SELECT COUNT (MP.MCPKG_ID)
                        FROM V$CERTIFICATE C
                            JOIN CERTIFICATESCOPE CS
                                ON (CS.CERTIFICATE_ID = C.CERTIFICATE_ID AND REJECTED = 'N')
                            JOIN MCPKG MP ON (MP.MCPKG_ID = CS.MCPKG_ID)
                        WHERE     MP.COMMPKG_ID = CP.COMMPKG_ID
                            AND C.CERTIFICATETYPE IN ('RFOC', 'RFSC')
                            AND C.ISVOIDED = 'N'
                            AND C.ISACCEPTED = 'Y')
                        AS ""{s_mcPkgsAcceptedByOperation}"",
                    (SELECT COUNT (DISTINCT MP.MCPKG_ID)
                        FROM V$CERTIFICATE C
                            JOIN CERTIFICATESCOPE CS
                                ON (CS.CERTIFICATE_ID = C.CERTIFICATE_ID AND REJECTED = 'Y')
                            JOIN MCPKG MP ON (MP.MCPKG_ID = CS.MCPKG_ID)
                        WHERE     MP.COMMPKG_ID = CP.COMMPKG_ID
                            AND C.CERTIFICATETYPE IN ('RFOC', 'RFSC')
                            AND C.ISVOIDED = 'N')
                        AS ""{s_mcPkgsRejectedByOperation}""
                FROM COMMPKG CP
                WHERE CP.COMMPKG_ID IN ({s_commPkgIdsToken})";
        public TagRepository(
            IConfiguration configuration,
            ILogger<TagRepository> logger)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("ProCoSys");
        }

        public TagModel GetPage(string installationCode, int currentPage, int itemsPerPage, int takeMax = 0)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var msgPrefix = $"3D Ecosystems for code '{installationCode}':";

            _logger.LogInformation($"{msgPrefix} Getting {itemsPerPage} tags at page {currentPage}");
            var (tags, timeUsedGettingTags) = GetTags(installationCode, currentPage, itemsPerPage);
            _logger.LogInformation($"{msgPrefix} Got {tags.Count()} tags during {FormatTimeSpan(timeUsedGettingTags)}");

            var uniqueCommPkgIds = tags.Where(t => t.CommPkgId.HasValue).Select(t => t.CommPkgId.Value).Distinct();

            _logger.LogInformation($"{msgPrefix} Getting CommPkg info for {uniqueCommPkgIds.Count()} CommPkgs");
            var (commPkgs, timeUsedGettingCommPkgs) = GetCommPkgs(uniqueCommPkgIds);
            _logger.LogInformation($"{msgPrefix} Got {commPkgs.Count()} commPkgs during {FormatTimeSpan(timeUsedGettingCommPkgs)}");

            FillTagsWithHandoverInfo(tags, commPkgs);

            var tagProperties = GeTagPropertiesInstances(tags);

            var tagModel = new TagModel
            {
                TimeUsedGettingTags = FormatTimeSpan(timeUsedGettingTags),
                TimeUsedGettingCommPkgs = FormatTimeSpan(timeUsedGettingCommPkgs),
                TimeUsedTotal = FormatTimeSpan(stopWatch.Elapsed),
                Heading = GetTagPropertiesHeading(),
                Tags = takeMax > 0 ? tagProperties.Take(takeMax) : tagProperties,
                Count = tags.Count()
            };

            stopWatch.Stop();
            _logger.LogInformation($"{msgPrefix} Finished during {tagModel.TimeUsedTotal}");

            return tagModel;
        }

        private void FillTagsWithHandoverInfo(IList<Tag> tags, IList<CommPkg> commPkgs)
        {
            foreach (var commPkg in commPkgs)
            {
                tags.Where(t => t.CommPkgId.HasValue && t.CommPkgId.Value == commPkg.CommPkgId)
                    .ToList()
                    .ForEach(t =>
                {
                    t.Rfcc = commPkg.RFCCHandoverStatus;
                    t.Rfoc = commPkg.RFOCHandoverStatus;
                });
            }
        }

        private string FormatTimeSpan(TimeSpan ts) => $"{ts.Hours:00}h {ts.Minutes:00}m {ts.Seconds:00}s";
        
        private IList<string> GetTagPropertiesHeading()
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

        private static IEnumerable<IEnumerable<object>> GeTagPropertiesInstances(IList<Tag> tags)
        {
            var tagProperties = tags.Select(t => new List<object>
            {
                t.TagNo,
                    t.Project,
                    t.PunchCount,
                    t.CommPkgNo,
                    t.CommPkgDesc,
                    t.McPkgNo,
                    t.McPkgDesc,
                    t.Priority,
                    t.Phase,
                    t.Rfcc,
                    t.Rfoc,
                    t.Responsible,
                    t.Status
                });
            return tagProperties;
        }

        private static IList<Tag> CreateTagInstances(DataTable dataTable)
        {
            var tags = (from DataRow row in dataTable.Rows
                             select new Tag
            {
                    TagNo = row[s_tagNo] as string,
                    Project = row[s_project] as string,
                    PunchCount = Convert.ToInt32(row[s_punchCount]),
                    CommPkgNo = row[s_commPkgNo] as string,
                    CommPkgDesc = row[s_commPkgDesc] as string,
                    McPkgNo = row[s_mcPkgNo] as string,
                    McPkgDesc = row[s_mcPkgDesc] as string,
                    Priority = row[s_priority] as string,
                    Phase = row[s_phase] as string,
                    Responsible = row[s_responsible] as string,
                    Status = row[s_status] as string,
                    CommPkgId = row[s_commPkg_Id] == DBNull.Value ? null : Convert.ToInt32(row[s_commPkg_Id]),
                }
                ).ToList();
            return tags;
        }

        private (IList<Tag>, TimeSpan) GetTags(string installationCode, int currentPage, int itemsPerPage)
        {
            DataTable result = null;

            var oracleDatabase = new OracleDb(_connectionString);
            try
            {
                var strSql = s_tagQuery.Replace(s_instCodeToken, installationCode);
                strSql += 
                    $@"
                        ORDER BY TAG_ID,
                        PROJECT_ID,
                        COMMPKG_ID,
                        RESPONSIBLE_ID
                    OFFSET {currentPage * itemsPerPage} ROWS FETCH NEXT {itemsPerPage} ROWS ONLY";
                
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                result = oracleDatabase.QueryDataTable(strSql);
                var tags = CreateTagInstances(result);
                stopWatch.Stop();
                
                return (tags, stopWatch.Elapsed);
            }
            finally
            {
                result?.Dispose();
                oracleDatabase.Close();
            }
        }

        private (IList<CommPkg>, TimeSpan) GetCommPkgs(IEnumerable<int> ids)
        {
            DataTable result = null;
            const int maxToGetFromOracle = 1000;
            var oracleDatabase = new OracleDb(_connectionString);
            try
            {
                var commPkgs = new List<CommPkg>();

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var pageSize = maxToGetFromOracle;
                var pageCounter = 0;
                var pageItems = ids.Take(pageSize).ToList();
                while (pageItems.Count() > 0)
                {
                    var commPkgIdList = string.Join(",", pageItems);
                    var strSql = s_commPkgQuery.Replace(s_commPkgIdsToken, commPkgIdList);

                    result = oracleDatabase.QueryDataTable(strSql);
                    commPkgs.AddRange(CreateCommPkgsFromResult(result));

                    pageCounter++;
                    pageItems = ids.Skip(pageSize * pageCounter).Take(pageSize).ToList();
                }

                stopWatch.Stop();
                return (commPkgs, stopWatch.Elapsed);
            }
            finally
            {
                result?.Dispose();
                oracleDatabase.Close();
            }
        }

        private IList<CommPkg> CreateCommPkgsFromResult(DataTable dataTable)
        {
            var instances =
                (from DataRow row in dataTable.Rows
                 select new CommPkg
                 {
                     CommPkgId = Convert.ToInt32(row[s_commPkg_Id]),
                     McPkgCount = Convert.ToInt32(row[s_mcPkgCount]),
                     McPkgsSentToCommissioning = Convert.ToInt32(row[s_mcPkgCount]),
                     McPkgsAcceptedByCommissioning = Convert.ToInt32(row[s_mcPkgsSentToCommissioning]),
                     McPkgsRejectedByCommissioning = Convert.ToInt32(row[s_mcPkgsRejectedByCommissioning]),
                     McPkgsSentToOperation = Convert.ToInt32(row[s_mcPkgsSentToOperation]),
                     McPkgsAcceptedByOperation = Convert.ToInt32(row[s_mcPkgsAcceptedByOperation]),
                     McPkgsRejectedByOperation = Convert.ToInt32(row[s_mcPkgsRejectedByOperation])
                 }
                ).ToList();
            return instances;
        }
    }
}
