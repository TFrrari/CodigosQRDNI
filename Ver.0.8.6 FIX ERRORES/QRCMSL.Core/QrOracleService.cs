using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace QRCMSL.Core
{
    /// <summary>
    /// Servicio mínimo para: probar conexión, listar tablas, describir columnas,
    /// ver N filas y buscar por DNI en una tabla dada.
    /// </summary>
    public sealed class QrOracleService : IDisposable
    {
        private readonly OracleConnection _conn;

        public QrOracleService(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("ConnectionString inválida.", "connectionString");

            _conn = new OracleConnection(connectionString);
        }

        private async Task EnsureOpenAsync()
        {
            if (_conn.State != ConnectionState.Open)
                await _conn.OpenAsync().ConfigureAwait(false);
        }

        public async Task<string> GetUserAsync()
        {
            await EnsureOpenAsync().ConfigureAwait(false);
            using (OracleCommand cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT USER FROM dual";
                object o = await cmd.ExecuteScalarAsync().ConfigureAwait(false);
                return o != null ? o.ToString() : null;
            }
        }

        public async Task<List<string>> ListTablesAsync()
        {
            await EnsureOpenAsync().ConfigureAwait(false);
            List<string> list = new List<string>();

            using (OracleCommand cmd = _conn.CreateCommand())
            {
                cmd.CommandText = "SELECT table_name FROM user_tables ORDER BY table_name";
                using (OracleDataReader rd = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await rd.ReadAsync().ConfigureAwait(false))
                    {
                        list.Add(rd.GetString(0));
                    }
                }
            }
            return list;
        }

        public async Task<DataTable> DescribeAsync(string table)
        {
            if (string.IsNullOrWhiteSpace(table))
                throw new ArgumentException("Nombre de tabla vacío.", "table");

            await EnsureOpenAsync().ConfigureAwait(false);

            using (OracleCommand cmd = _conn.CreateCommand())
            {
                cmd.CommandText =
                    "SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE " +
                    "FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :t ORDER BY COLUMN_ID";
                cmd.Parameters.Add(new OracleParameter("t", table.ToUpperInvariant()));

                DataTable dt = new DataTable();
                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                return dt;
            }
        }

        public async Task<DataTable> TopNAsync(string table, int n)
        {
            if (string.IsNullOrWhiteSpace(table))
                throw new ArgumentException("Nombre de tabla vacío.", "table");
            if (n <= 0) n = 1;

            await EnsureOpenAsync().ConfigureAwait(false);

            using (OracleCommand cmd = _conn.CreateCommand())
            {
                // Ojo: esto es seguro porque "n" es parámetro; el nombre de tabla
                // viene del combo de la propia base (ListTablesAsync).
                cmd.CommandText = "SELECT * FROM " + table + " WHERE ROWNUM <= :n";
                cmd.Parameters.Add(new OracleParameter("n", n));

                DataTable dt = new DataTable();
                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                return dt;
            }
        }

        public async Task<DataTable> BuscarPorDniAsync(string table, string dni, string dniColumn)
        {
            if (string.IsNullOrWhiteSpace(table))
                throw new ArgumentException("Nombre de tabla vacío.", "table");
            if (string.IsNullOrWhiteSpace(dniColumn))
                dniColumn = "DNI";
            if (dni == null) dni = string.Empty;

            await EnsureOpenAsync().ConfigureAwait(false);

            using (OracleCommand cmd = _conn.CreateCommand())
            {
                cmd.BindByName = true;
                // IMPORTANTE: nombre de columna y tabla no pueden ir como parámetro.
                // Se asume que provienen de la UI controlada.
                cmd.CommandText = "SELECT * FROM " + table + " WHERE " + dniColumn + " = :dni";
                cmd.Parameters.Add(new OracleParameter("dni", dni));

                DataTable dt = new DataTable();
                using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                return dt;
            }
        }

        public void Dispose()
        {
            _conn.Dispose();
        }
    }
}
