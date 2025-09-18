using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Threading.Tasks;

namespace QrOracleConnector.Core
{
    public interface IPatientRepository
    {
        Task<string> UpsertAsync(ParsedQr p);
        Task<DataTable> PreviewAsync(string tableName, int top = 100);
        Task<string[]> ListTablesAsync();
        Task<DataTable> DescribeTableAsync(string tableName);
        Task<string> EnsurePacientesStructureAsync();
    }

    public sealed class OraclePatientRepository : IPatientRepository
    {
        private readonly IConnectionFactory _factory;
        public OraclePatientRepository(IConnectionFactory factory) { _factory = factory; }

        public Task<string> UpsertAsync(ParsedQr p)
        {
            if (string.IsNullOrWhiteSpace(p.Dni)) return Task.FromResult("DNI vacío");

            using (var cn = (OracleConnection)_factory.Create())
            {
                cn.Open();

                using (var checkCmd = new OracleCommand("SELECT ID_PACIENTE FROM PACIENTES WHERE DNI = :dni", cn))
                {
                    checkCmd.Parameters.Add(":dni", OracleDbType.Varchar2, 20).Value = p.Dni;
                    var exists = checkCmd.ExecuteScalar();

                    if (exists == null || exists == DBNull.Value)
                    {
                        const string insertSql = "INSERT INTO PACIENTES (ID_PACIENTE, DNI, APELLIDO, NOMBRE, FECHA_NAC) " +
                                                 "VALUES (SEQ_PACIENTES.NEXTVAL, :dni, :ap, :no, :fn)";
                        using (var icmd = new OracleCommand(insertSql, cn))
                        {
                            icmd.BindByName = true;
                            icmd.Parameters.Add(":dni", OracleDbType.Varchar2, 20).Value = p.Dni;
                            icmd.Parameters.Add(":ap", OracleDbType.Varchar2, 100).Value = (object)p.Apellido ?? DBNull.Value;
                            icmd.Parameters.Add(":no", OracleDbType.Varchar2, 100).Value = (object)p.Nombre ?? DBNull.Value;
                            icmd.Parameters.Add(":fn", OracleDbType.Date).Value = (object)p.FechaNac ?? DBNull.Value;
                            var n = icmd.ExecuteNonQuery();
                            return Task.FromResult(n > 0 ? "Paciente creado" : "No se insertó registro");
                        }
                    }
                    else
                    {
                        const string upSql = "UPDATE PACIENTES SET APELLIDO = :ap, NOMBRE = :no, FECHA_NAC = :fn WHERE DNI = :dni";
                        using (var ucmd = new OracleCommand(upSql, cn))
                        {
                            ucmd.BindByName = true;
                            ucmd.Parameters.Add(":ap", OracleDbType.Varchar2, 100).Value = (object)p.Apellido ?? DBNull.Value;
                            ucmd.Parameters.Add(":no", OracleDbType.Varchar2, 100).Value = (object)p.Nombre ?? DBNull.Value;
                            ucmd.Parameters.Add(":fn", OracleDbType.Date).Value = (object)p.FechaNac ?? DBNull.Value;
                            ucmd.Parameters.Add(":dni", OracleDbType.Varchar2, 20).Value = p.Dni;
                            var n = ucmd.ExecuteNonQuery();
                            return Task.FromResult(n > 0 ? "Paciente actualizado" : "No se actualizó registro");
                        }
                    }
                }
            }
        }

        public Task<DataTable> PreviewAsync(string tableName, int top = 100)
        {
            var dt = new DataTable();
            using (var cn = (OracleConnection)_factory.Create())
            {
                cn.Open();
                var sql = "SELECT * FROM " + tableName + " WHERE ROWNUM <= :top";
                using (var cmd = new OracleCommand(sql, cn))
                {
                    cmd.Parameters.Add(":top", OracleDbType.Int32).Value = top;
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        return Task.FromResult(dt);
                    }
                }
            }
        }

        public Task<string[]> ListTablesAsync()
        {
            using (var cn = (OracleConnection)_factory.Create())
            {
                cn.Open();
                using (var cmd = new OracleCommand("SELECT table_name FROM user_tables ORDER BY 1", cn))
                {
                    using (var rd = cmd.ExecuteReader())
                    {
                        var list = new System.Collections.Generic.List<string>();
                        while (rd.Read()) list.Add(rd.GetString(0));
                        return Task.FromResult(list.ToArray());
                    }
                }
            }
        }

        public Task<DataTable> DescribeTableAsync(string tableName)
        {
            var dt = new DataTable();
            using (var cn = (OracleConnection)_factory.Create())
            {
                cn.Open();
                var sql = @"SELECT column_name, data_type, data_length, nullable 
                            FROM user_tab_columns 
                            WHERE table_name = :t ORDER BY column_id";
                using (var cmd = new OracleCommand(sql, cn))
                {
                    cmd.Parameters.Add(":t", OracleDbType.Varchar2, 200).Value = tableName.ToUpperInvariant();
                    using (var da = new OracleDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        return Task.FromResult(dt);
                    }
                }
            }
        }

        public Task<string> EnsurePacientesStructureAsync()
        {
            using (var cn = (OracleConnection)_factory.Create())
            {
                cn.Open();

                // Crear tabla PACIENTES si no existe
                try
                {
                    using (var cmd = new OracleCommand(
                        @"CREATE TABLE PACIENTES (
                            ID_PACIENTE NUMBER PRIMARY KEY,
                            DNI VARCHAR2(20) UNIQUE,
                            APELLIDO VARCHAR2(100),
                            NOMBRE VARCHAR2(100),
                            FECHA_NAC DATE
                          )", cn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { /* ya existe */ }

                // Crear secuencia SEQ_PACIENTES si no existe
                try
                {
                    using (var cmd = new OracleCommand(
                        @"CREATE SEQUENCE SEQ_PACIENTES START WITH 1 INCREMENT BY 1", cn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { /* ya existe */ }

                // Crear trigger si no existe
                try
                {
                    using (var cmd = new OracleCommand(
                        @"CREATE OR REPLACE TRIGGER TRG_PACIENTES_AI
                          BEFORE INSERT ON PACIENTES
                          FOR EACH ROW
                          WHEN (NEW.ID_PACIENTE IS NULL)
                          BEGIN
                            SELECT SEQ_PACIENTES.NEXTVAL INTO :NEW.ID_PACIENTE FROM DUAL;
                          END;", cn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch { /* ignorar */ }

                return Task.FromResult("Estructura PACIENTES verificada/creada");
            }
        }
    }
}
