# LectorDNI.Demo v0.5 (SQLite con listado)

## Novedades
- DataGridView en la UI para **ver todos los pacientes** guardados.
- Repositorio SQLite con método `GetAll()`.
- Flujo: escanear → procesar → guardar → la tabla se actualiza.

## Requisitos
- Visual Studio 2022 (.NET Desktop)
- .NET 8 SDK
- Paquete NuGet: `Microsoft.Data.Sqlite` (referenciado en el .csproj)

## Cómo correr
1. Abrí `LectorDNI.Demo.csproj` con VS2022.
2. Compilá y ejecutá (F5).
3. La BD `pacientes.db` se crea en `%LOCALAPPDATA%\LectorDNI`.
