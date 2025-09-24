using System;
using System.Configuration;
using System.IO;
using Newtonsoft.Json.Linq;

namespace QRCMSL.Core
{
    public static class ConnectionConfig
    {
        public static string GetOracleConnectionString(out string error)
        {
            error = null;
            try
            {
                var exeDir = AppDomain.CurrentDomain.BaseDirectory ?? "";
                var jsonPath = Path.Combine(exeDir, "config.json");
                if (File.Exists(jsonPath))
                {
                    var txt = File.ReadAllText(jsonPath);
                    var j = JObject.Parse(txt);
                    var val = (string)j["OracleConnection"];
                    if (!string.IsNullOrWhiteSpace(val))
                        return val;
                }
            }
            catch (Exception ex)
            {
                error = "Error leyendo config.json: " + ex.Message;
            }

            try
            {
                var item = ConfigurationManager.ConnectionStrings["Oracle"];
                if (item != null && !string.IsNullOrWhiteSpace(item.ConnectionString))
                    return item.ConnectionString;
            }
            catch (Exception ex)
            {
                error = (error == null ? "" : error + " | ") + "Error leyendo App.config: " + ex.Message;
            }

            if (string.IsNullOrEmpty(error)) error = "No se encontró cadena de conexión en config.json ni App.config.";
            return null;
        }
    }
}
