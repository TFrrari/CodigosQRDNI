using System;
using System.Globalization;

namespace QrOracleConnector.Core
{
    /// <summary>
    /// Parser simple de string QR tipo: DNI=123;APELLIDO=PEREZ;NOMBRE=JUAN;FECHA_NAC=1990-05-10
    /// </summary>
    public sealed class DniParser : IDniParser
    {
        public ParsedQr Parse(string input)
        {
            var r = new ParsedQr();
            if (string.IsNullOrWhiteSpace(input)) return r;
            var parts = input.Split(';');
            foreach (var part in parts)
            {
                var kv = part.Split('=');
                if (kv.Length != 2) continue;
                var k = kv[0].Trim().ToUpperInvariant();
                var v = kv[1].Trim();
                switch (k)
                {
                    case "DNI": r.Dni = v; break;
                    case "APELLIDO": r.Apellido = v; break;
                    case "NOMBRE": r.Nombre = v; break;
                    case "FECHA_NAC":
                        DateTime d;
                        if (DateTime.TryParseExact(v, new[] {"yyyy-MM-dd","dd/MM/yyyy","yyyyMMdd"}, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                            r.FechaNac = d;
                        break;
                }
            }
            return r;
        }
    }
}
