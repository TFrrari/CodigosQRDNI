
using System;
using System.Text.RegularExpressions;

namespace QRCMSL.Core
{
    public class ParsedQr
    {
        public string DNI { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Direccion { get; set; }

        public static bool TryParse(string input, out ParsedQr parsed)
        {
            parsed = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var p = new ParsedQr();

            var mDni = Regex.Match(input, @"\b(\d{7,11})\b");
            if (mDni.Success) p.DNI = mDni.Groups[1].Value;

            var mNombre = Regex.Match(input, @"(?i)\bNOMBRE\b\s*[:=]\s*([^|;,]+)");
            if (mNombre.Success) p.Nombre = mNombre.Groups[1].Value.Trim();

            var mApe = Regex.Match(input, @"(?i)\bAPELLIDO\b\s*[:=]\s*([^|;,]+)");
            if (mApe.Success) p.Apellido = mApe.Groups[1].Value.Trim();

            var mDir = Regex.Match(input, @"(?i)\b(DIR|DIRECCION|DOMICILIO)\b\s*[:=]\s*([^|;,]+)");
            if (mDir.Success) p.Direccion = mDir.Groups[2].Value.Trim();

            var mFn = Regex.Match(input, @"(?i)\b(FN|FECHA\s*NAC(IMIENTO)?)\b\s*[:=]\s*([^|;,]+)");
            DateTime fn;
            if (mFn.Success && DateTime.TryParse(mFn.Groups[mFn.Groups.Count - 1].Value.Trim(), out fn))
                p.FechaNacimiento = fn;

            if (string.IsNullOrWhiteSpace(p.DNI)) return false;
            parsed = p;
            return true;
        }
    }
}
