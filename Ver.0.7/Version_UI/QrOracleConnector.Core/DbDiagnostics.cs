using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;

namespace QrOracleConnector.Core
{
    public static class DbDiagnostics
    {
        public static Task<bool> TestConnectionAsync(IConnectionFactory factory)
        {
            using (var cn = (OracleConnection)factory.Create())
            {
                cn.Open();
                using (var cmd = new OracleCommand("SELECT 1 FROM DUAL", cn))
                {
                    var r = cmd.ExecuteScalar();
                    return Task.FromResult(r != null && r.ToString() == "1");
                }
            }
        }

        public static Task<string> GetCurrentUserAsync(IConnectionFactory factory)
        {
            using (var cn = (OracleConnection)factory.Create())
            {
                cn.Open();
                using (var cmd = new OracleCommand("SELECT USER FROM DUAL", cn))
                {
                    var r = cmd.ExecuteScalar();
                    return Task.FromResult(r == null ? "" : r.ToString());
                }
            }
        }
    }
}
