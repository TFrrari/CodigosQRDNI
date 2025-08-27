using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;

namespace LectorDNI.Demo.Data
{
    public sealed class SqlitePatientRepository
    {
        private readonly string _connString;

        public SqlitePatientRepository(string? dataDir = null)
        {
            var baseDir = dataDir;
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "LectorDNI");
            }
            Directory.CreateDirectory(baseDir);
            var dbPath = Path.Combine(baseDir, "pacientes.db");
            _connString = $"Data Source={dbPath};Cache=Shared";
            EnsureDatabase();
        }

        private void EnsureDatabase()
        {
            using var conn = new SqliteConnection(_connString);
            conn.Open();
            var ddl = @"
CREATE TABLE IF NOT EXISTS Pacientes (
    Id            INTEGER PRIMARY KEY AUTOINCREMENT,
    Dni           TEXT    NOT NULL UNIQUE,
    Apellido      TEXT    NOT NULL,
    Nombre        TEXT    NOT NULL,
    Sexo          TEXT    NOT NULL,
    FechaNac      TEXT    NOT NULL, -- ISO yyyy-MM-dd
    Nacionalidad  TEXT,
    CreatedAt     TEXT    NOT NULL, -- ISO 8601
    UpdatedAt     TEXT
);

CREATE INDEX IF NOT EXISTS IX_Pacientes_Apellido ON Pacientes(Apellido);
";
            using var cmd = conn.CreateCommand();
            cmd.CommandText = ddl;
            cmd.ExecuteNonQuery();
        }

        public (bool found, PatientDto? patient) GetByDni(string dni)
        {
            using var conn = new SqliteConnection(_connString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id, Dni, Apellido, Nombre, Sexo, FechaNac, Nacionalidad, CreatedAt, UpdatedAt
                                FROM Pacientes WHERE Dni = $dni;";
            cmd.Parameters.AddWithValue("$dni", dni);
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return (false, null);
            return (true, Map(rd));
        }

        public List<PatientDto> GetAll()
        {
            var list = new List<PatientDto>();
            using var conn = new SqliteConnection(_connString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id, Dni, Apellido, Nombre, Sexo, FechaNac, Nacionalidad, CreatedAt, UpdatedAt
                                FROM Pacientes ORDER BY Id DESC;";
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(Map(rd));
            return list;
        }

        public long Upsert(PatientDto p)
        {
            using var conn = new SqliteConnection(_connString);
            conn.Open();

            using (var cmdUp = conn.CreateCommand())
            {
                cmdUp.CommandText = @"
UPDATE Pacientes
   SET Apellido = $ape,
       Nombre = $nom,
       Sexo = $sexo,
       FechaNac = $fec,
       Nacionalidad = $nac,
       UpdatedAt = $ts
 WHERE Dni = $dni;";
                cmdUp.Parameters.AddWithValue("$ape", p.Apellido);
                cmdUp.Parameters.AddWithValue("$nom", p.Nombre);
                cmdUp.Parameters.AddWithValue("$sexo", p.Sexo);
                cmdUp.Parameters.AddWithValue("$fec", p.FechaNacIso);
                cmdUp.Parameters.AddWithValue("$nac", (object?)p.Nacionalidad ?? DBNull.Value);
                cmdUp.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));
                cmdUp.Parameters.AddWithValue("$dni", p.Dni);
                var rows = cmdUp.ExecuteNonQuery();
                if (rows > 0)
                {
                    using var q = conn.CreateCommand();
                    q.CommandText = "SELECT Id FROM Pacientes WHERE Dni = $dni;";
                    q.Parameters.AddWithValue("$dni", p.Dni);
                    return (long)(q.ExecuteScalar() ?? 0L);
                }
            }

            using (var cmdIns = conn.CreateCommand())
            {
                cmdIns.CommandText = @"
INSERT INTO Pacientes (Dni, Apellido, Nombre, Sexo, FechaNac, Nacionalidad, CreatedAt)
VALUES ($dni, $ape, $nom, $sexo, $fec, $nac, $ts);
SELECT last_insert_rowid();";
                cmdIns.Parameters.AddWithValue("$dni", p.Dni);
                cmdIns.Parameters.AddWithValue("$ape", p.Apellido);
                cmdIns.Parameters.AddWithValue("$nom", p.Nombre);
                cmdIns.Parameters.AddWithValue("$sexo", p.Sexo);
                cmdIns.Parameters.AddWithValue("$fec", p.FechaNacIso);
                cmdIns.Parameters.AddWithValue("$nac", (object?)p.Nacionalidad ?? DBNull.Value);
                cmdIns.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));
                var id = (long)(cmdIns.ExecuteScalar() ?? 0L);
                return id;
            }
        }

        private static PatientDto Map(SqliteDataReader rd) => new PatientDto
        {
            Id = rd.GetInt64(0),
            Dni = rd.GetString(1),
            Apellido = rd.GetString(2),
            Nombre = rd.GetString(3),
            Sexo = rd.GetString(4),
            FechaNacIso = rd.GetString(5),
            Nacionalidad = rd.IsDBNull(6) ? null : rd.GetString(6),
            CreatedAtIso = rd.GetString(7),
            UpdatedAtIso = rd.IsDBNull(8) ? null : rd.GetString(8)
        };
    }

    public sealed class PatientDto
    {
        public long   Id            { get; set; }
        public string Dni           { get; set; } = "";
        public string Apellido      { get; set; } = "";
        public string Nombre        { get; set; } = "";
        public string Sexo          { get; set; } = "";
        public string FechaNacIso   { get; set; } = ""; // yyyy-MM-dd
        public string? Nacionalidad { get; set; }
        public string CreatedAtIso  { get; set; } = "";
        public string? UpdatedAtIso { get; set; }
    }
}
