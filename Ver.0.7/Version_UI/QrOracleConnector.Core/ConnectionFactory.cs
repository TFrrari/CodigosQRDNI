using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace QrOracleConnector.Core
{
    public interface IConnectionFactory { IDbConnection Create(); }

    public sealed class OracleConnectionFactory : IConnectionFactory
    {
        private readonly string _connString;
        public OracleConnectionFactory(string connString) { _connString = connString; }
        public IDbConnection Create() { return new OracleConnection(_connString); }
    }
}
