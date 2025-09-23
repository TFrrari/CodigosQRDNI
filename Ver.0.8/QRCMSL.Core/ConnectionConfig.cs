using System;
using System.Configuration;

namespace QRCMSL.Core
{
    /// <summary>Lee la cadena de conexión "Oracle" desde App.config.</summary>
    public static class ConnectionConfig
    {
        public static string GetOracleConnectionString(out string error)
        {
            error = null;
            try
            {
                var s = ConfigurationManager.ConnectionStrings["Oracle"];
                if (s == null || string.IsNullOrWhiteSpace(s.ConnectionString))
                {
                    error = "Falta connectionStrings[Oracle] en App.config.";
                    return null;
                }
                return s.ConnectionString;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }
    }
}
