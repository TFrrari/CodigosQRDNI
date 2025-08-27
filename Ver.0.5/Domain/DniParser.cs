using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace LectorDNI.Demo.Domain
{
    public static class DniParser
    {
        private static readonly Regex Solo7a9 = new Regex(@"^\d{7,9}$", RegexOptions.Compiled);
        private static readonly string[] DateFormats = new[] { "dd/MM/yyyy", "dd-MM-yyyy" };

        public static DniParsed Parse(string? raw)
        {
            var r = new DniParsed();
            if (string.IsNullOrWhiteSpace(raw)) return r;

            var text = raw.Trim().Replace("\r", "").Replace("\n", "");

            if (text.Contains('@'))
            {
                var p = text.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (p.Length >= 7 && Solo7a9.IsMatch(OnlyDigits(p[4])))
                {
                    r.Apellido = p[1];
                    r.Nombre = p[2];
                    r.Sexo = (p[3] ?? "").Trim().ToUpperInvariant();
                    r.DNI = OnlyDigits(p[4]);
                    r.FechaNacimientoRaw = (p[6] ?? "").Trim();
                    r.FechaNacimiento = TryParseDate(r.FechaNacimientoRaw);
                    var nat = p.Select(x => (x ?? "").Trim().ToUpperInvariant())
                               .FirstOrDefault(x => x == "ARG" || x == "ARGENTINA");
                    if (!string.IsNullOrEmpty(nat)) r.Nacionalidad = "ARGENTINA";
                    return r;
                }
            }

            if (text.Contains('\"'))
            {
                var p = text.Split('\"', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (p.Length >= 7)
                {
                    r.Apellido = Safe(p, 1);
                    r.Nombre = Safe(p, 2);
                    r.Sexo = Safe(p, 3).ToUpperInvariant();
                    r.DNI = OnlyDigits(Safe(p, 4));
                    r.FechaNacimientoRaw = Safe(p, 6);
                    r.FechaNacimiento = TryParseDate(r.FechaNacimientoRaw);
                    return r;
                }
            }

            var parts = text.Split(new[] { '@', '\"' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            int dniIdx = -1;
            for (int i = 0; i < parts.Length; i++)
            {
                var dig = OnlyDigits(parts[i]);
                if (Solo7a9.IsMatch(dig)) { r.DNI = dig; dniIdx = i; break; }
            }
            if (dniIdx >= 1) r.Apellido = parts[dniIdx - 1].Trim();
            if (dniIdx + 1 < parts.Length) r.Nombre = parts[dniIdx + 1].Trim();

            foreach (var token in parts)
            {
                var t = token.Trim().ToUpperInvariant();
                if (t == "M" || t == "F" || t == "X") { r.Sexo = t; break; }
            }
            foreach (var token in parts)
            {
                var s = token.Trim();
                var dt = TryParseDate(s);
                if (dt.HasValue) { r.FechaNacimiento = dt.Value; r.FechaNacimientoRaw = s; break; }
            }
            foreach (var token in parts)
            {
                var t = token.Trim().ToUpperInvariant();
                if (t == "ARG" || t == "ARGENTINA") { r.Nacionalidad = "ARGENTINA"; break; }
            }

            return r;
        }

        private static DateTime? TryParseDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParseExact(s.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;
            return null;
        }

        private static string OnlyDigits(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var arr = s.ToCharArray();
            var buf = new char[arr.Length];
            int j = 0;
            for (int i = 0; i < arr.Length; i++)
                if (char.IsDigit(arr[i])) buf[j++] = arr[i];
            return new string(buf, 0, j);
        }

        private static string Safe(string[] arr, int idx) => (idx >= 0 && idx < arr.Length) ? (arr[idx] ?? "").Trim() : string.Empty;
    }
}
