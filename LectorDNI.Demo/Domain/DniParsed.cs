using System;

namespace LectorDNI.Demo.Domain
{
    public class DniParsed
    {
        public string DNI { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public string FechaNacimientoRaw { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string Nacionalidad { get; set; } = string.Empty;
    }
}
