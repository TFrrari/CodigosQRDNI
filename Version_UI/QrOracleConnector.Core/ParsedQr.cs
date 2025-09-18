using System;

namespace QrOracleConnector.Core
{
    public sealed class ParsedQr
    {
        public string Dni { get; set; }
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public DateTime? FechaNac { get; set; }
    }
}
