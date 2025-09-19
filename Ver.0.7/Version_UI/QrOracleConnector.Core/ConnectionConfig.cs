using System;
using System.IO;
using System.Web.Script.Serialization;

namespace QrOracleConnector.Core
{
    public static class ConnectionConfig
    {
        public sealed class Plan
        {
            public string Host { get; set; }
            public int Port { get; set; }
            public string ServiceName { get; set; }
            public string UserId { get; set; }
            public string Password { get; set; }
        }

        /// <summary>Genera una connection string Oracle Managed con SERVICE_NAME.</summary>
        public static string BuildEzConnect(Plan p)
        {
            var dataSource = string.Format("(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={2}))))", p.Host, p.Port, p.ServiceName);
            return "Data Source=" + dataSource + ";User Id=" + p.UserId + ";Password=" + p.Password + ";";
        }

        public static string TryLoadFromJson(string path, out string error)
        {
            error = null;
            try
            {
                if (!File.Exists(path)) { error = "No existe " + path; return null; }
                var json = File.ReadAllText(path);
                var ser = new JavaScriptSerializer();
                var plan = ser.Deserialize<Plan>(json);
                if (plan == null) { error = "JSON inválido"; return null; }
                return BuildEzConnect(plan);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return null;
            }
        }
    }
}
