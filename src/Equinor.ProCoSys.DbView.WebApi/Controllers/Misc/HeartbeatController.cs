using System;
using System.Data;
using Equinor.ProCoSys.DbView.WebApi.Oracle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.Misc
{
    [ApiController]
    [Route("api/DbView/Heartbeat")]
    public class HeartbeatController : ControllerBase
    {
        private readonly ILogger<HeartbeatController> _logger;
        private readonly string _connectionString;

        public HeartbeatController(IConfiguration configuration, ILogger<HeartbeatController> logger)
        {
            _connectionString = configuration.GetConnectionString("ProCoSys");
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("IsAlive")]
        public IActionResult IsAlive(bool checkDbIsAlive = false)
        {
            var timestampString = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
            _logger.LogDebug($"The application is running at {timestampString}");
            if (checkDbIsAlive)
            {
                var dbState = GetDbState();
                var dbTimestampString = dbState.Item2.HasValue ? $"{dbState.Item2.Value.ToUniversalTime():yyyy-MM-dd HH:mm:ss} UTC" : null;
                _logger.LogDebug($"Database is running at {timestampString}");
                return new JsonResult(new
                {
                    IsAlive = true,
                    TimeStamp = timestampString,
                    IsDbAlive = dbState.Item1,
                    DbTimeStamp = dbTimestampString
                });
            }
            return new JsonResult(new
            {
                IsAlive = true,
                TimeStamp = timestampString
            });
        }

        private (bool, DateTime?) GetDbState()
        {
            DataTable result = null;
            var oracleDatabase = new OracleDb(_connectionString);
            try
            {
                var strSql = $@"SELECT SYSDATE AS DB_DATE FROM DUAL";
                result = oracleDatabase.QueryDataTable(strSql);

                return (true, (DateTime)result.Rows[0]["DB_DATE"]);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Getting SYSDATE() failed");
                return (false, null);
            }
            finally
            {
                result?.Dispose();
                oracleDatabase.Close();
            }
        }
    }
}
