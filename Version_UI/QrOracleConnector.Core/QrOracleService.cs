using System.Data;
using System.Threading.Tasks;

namespace QrOracleConnector.Core
{
    public sealed class QrOracleService : IQrOracleService
    {
        private readonly IConnectionFactory _factory;
        private readonly IPatientRepository _repo;
        private readonly IDniParser _parser;

        public QrOracleService(string oracleConnectionString)
        {
            _factory = new OracleConnectionFactory(oracleConnectionString);
            _repo = new OraclePatientRepository(_factory);
            _parser = new DniParser();
        }

        public Task<bool> TestConnectionAsync() { return DbDiagnostics.TestConnectionAsync(_factory); }
        public Task<string[]> ListTablesAsync() { return _repo.ListTablesAsync(); }
        public Task<DataTable> PreviewAsync(string tableName, int top = 100) { return _repo.PreviewAsync(tableName, top); }
        public Task<string> UpsertPatientAsync(ParsedQr p) { return _repo.UpsertAsync(p); }
        public Task<string> UpsertFromRawAsync(string rawQr) { var parsed = _parser.Parse(rawQr); return _repo.UpsertAsync(parsed); }
        public Task<string> EnsurePacientesStructureAsync() { return _repo.EnsurePacientesStructureAsync(); }
        public Task<DataTable> DescribeTableAsync(string tableName) { return _repo.DescribeTableAsync(tableName); }
        public Task<string> GetCurrentUserAsync() { return DbDiagnostics.GetCurrentUserAsync(_factory); }
    }
}
