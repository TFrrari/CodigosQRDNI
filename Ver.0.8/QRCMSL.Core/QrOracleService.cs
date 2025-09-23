using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace QRCMSL.Core
{
    /// <summary>Servicio Oracle con utilidades comunes.</summary>
    public sealed class QrOracleService : IDisposable
    {
        private readonly OracleConnection _conn;
        public QrOracleService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("ConnectionString inválida.", nameof(connectionString));
            _conn = new OracleConnection(connectionString);
        }

        public async Task OpenAsync()
        {
            if (_conn.State != ConnectionState.Open)
                await _conn.OpenAsync();
        }

        public async Task<string> GetUserAsync()
        {
            await OpenAsync();
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT USER FROM dual";
                var o = await cmd.ExecuteScalarAsync();
                return o?.ToString();
            }
        }

        public async Task<List<string>> ListTablesAsync()
        {
            await OpenAsync();
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT table_name FROM user_tables ORDER BY table_name";
                var list = new List<string>();
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                        list.Add(r.GetString(0));
                }
                return list;
            }
        }

        public async Task<DataTable> DescribeAsync(string table)
        {
            if (string.IsNullOrWhiteSpace(table)) throw new ArgumentException("Tabla requerida.", nameof(table));
            await OpenAsync();
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = @"SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE 
                                    FROM USER_TAB_COLUMNS 
                                    WHERE TABLE_NAME = :t ORDER BY COLUMN_ID";
                cmd.Parameters.Add(new OracleParameter("t", table.ToUpperInvariant()));
                var dt = new DataTable();
                using (var adp = new OracleDataAdapter(cmd))
                {
                    adp.Fill(dt);
                }
                return dt;
            }
        }

        public async Task<DataTable> TopNAsync(string table, int n)
        {
            if (string.IsNullOrWhiteSpace(table)) throw new ArgumentException("Tabla requerida.", nameof(table));
            if (n <= 0) n = 1;
            await OpenAsync();
            using (var cmd = _conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM {table} WHERE ROWNUM <= :n";
                cmd.Parameters.Add(new OracleParameter("n", n));
                var dt = new DataTable();
                using (var adp = new OracleDataAdapter(cmd))
                {
                    adp.Fill(dt);
                }
                return dt;
            }
        }

        public async Task<(bool Exists, DataTable Data)> BuscarPacientePorDniAsync(
            string dni, string tableName = "PACIENTES", string dniColumn = "DNI")
        {
            if (string.IsNullOrWhiteSpace(dni)) return (false, null);
            await OpenAsync();
            using (var cmd = _conn.CreateCommand())
            {
                cmd.BindByName = true;
                cmd.CommandText = $"SELECT * FROM {tableName} WHERE {dniColumn} = :dni AND ROWNUM = 1";
                cmd.Parameters.Add(new OracleParameter("dni", dni));
                var dt = new DataTable();
                using (var adp = new OracleDataAdapter(cmd))
                {
                    adp.Fill(dt);
                }
                return (dt.Rows.Count > 0, dt);
            }
        }

        public void Dispose() => _conn?.Dispose();
    }
}
