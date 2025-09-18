using System.Data;
using System.Threading.Tasks;

namespace QrOracleConnector.Core
{
    public interface IQrOracleService
    {
        Task<bool> TestConnectionAsync();
        Task<string[]> ListTablesAsync();
        Task<DataTable> PreviewAsync(string tableName, int top = 100);
        Task<string> UpsertPatientAsync(ParsedQr p);
        Task<string> UpsertFromRawAsync(string rawQr);
        Task<string> EnsurePacientesStructureAsync();
        Task<DataTable> DescribeTableAsync(string tableName);
        Task<string> GetCurrentUserAsync();
    }

    public interface IDniParser
    {
        ParsedQr Parse(string input);
    }
}
