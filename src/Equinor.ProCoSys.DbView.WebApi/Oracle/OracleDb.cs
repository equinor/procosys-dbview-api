using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace Equinor.ProCoSys.DbView.WebApi.Oracle
{
    public class OracleDb
    {
        private OracleConnection _oracleConnection;
        private OracleTransaction _trans;

        public OracleDb(string connectionString) 
            => _oracleConnection = new OracleConnection
            {
                ConnectionString = connectionString,
                KeepAlive = true
            };

        private void OpenConnection()
        {
            if (_oracleConnection.State == ConnectionState.Open)
            {
                return;
            }

            _oracleConnection.Open();
        }

        public DataSet QueryDataSet(string strSql)
        {
            OpenConnection();

            var objCmd = new OracleCommand { Connection = _oracleConnection, CommandText = strSql, CommandType = CommandType.Text };
            var dtAdapter = new OracleDataAdapter { SelectCommand = objCmd };

            try
            {
                var ds = new DataSet();
                dtAdapter.Fill(ds);
                return ds;
            }
            finally
            {
                dtAdapter.Dispose();
                objCmd.Dispose();
            }
        }
        
        public DataTable QueryDataTable(string strSql)
        {
            OpenConnection();
            var dtAdapter = new OracleDataAdapter(strSql, _oracleConnection);

            try
            {
                var dt = new DataTable();
                dtAdapter.Fill(dt);
                return dt;
            }
            finally
            {
                dtAdapter.Dispose();
            }
        }

        public void Close()
        {
            _oracleConnection.Close();
            _oracleConnection.Dispose();
            _oracleConnection = null;
        }

        public void TransactionStart()
        {
            OpenConnection();

            _trans = _oracleConnection.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        public void TransactionRollBack()
        {
            _trans.Rollback();
            _trans.Dispose();
            _trans = null;
        }

        public void TransactionCommit()
        {
            _trans.Commit();
            _trans.Dispose();
            _trans = null;
        }
    }
}
