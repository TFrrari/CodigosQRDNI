using System;
using System.IO;
using System.Text.Json;

namespace LectorDNI.Demo
{
    public static class AppConfig
    {
        public static string GetOracleConnectionString()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                var json = File.ReadAllText(path);
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.GetProperty("Oracle").GetProperty("ConnectionString").GetString() ?? "";
            }
            catch
            {
                // Fallback (puede editarse aquí si hiciera falta)
                return "User Id=CLINICA;Password=rep;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=172.10.10.5)(PORT=1521))(CONNECT_DATA=(SID=wg)));";
            }
        }
    }
}
