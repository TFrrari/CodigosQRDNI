# LectorDNI – v0.6 (Ping Oracle)

Esta versión SOLO prueba la **conexión a Oracle**. No crea tablas ni toca datos.
Hace un `SELECT 1 FROM DUAL` y muestra el resultado en pantalla.

## Requisitos
- Windows + Visual Studio 2022 (workload .NET Desktop)
- .NET 8 SDK (si compilas fuera de VS)
- Paquete NuGet: `Oracle.ManagedDataAccess` (referenciado en el `.csproj`)

## Configurar
Editá `appsettings.json` y dejá la cadena de conexión que te dieron (ya viene cargada):
```
{
  "Oracle": {
    "ConnectionString": "User Id=CLINICA;Password=rep;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=172.10.10.5)(PORT=1521))(CONNECT_DATA=(SID=wg)));"
  }
}
```

> Si tu servidor usa SERVICE_NAME en vez de SID, probá:
> `User Id=CLINICA;Password=rep;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=172.10.10.5)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=wg)));`
> O bien EZCONNECT: `User Id=CLINICA;Password=rep;Data Source=172.10.10.5:1521/wg;`

## Ejecutar
1. Abrí `LectorDNI.Demo.csproj` en Visual Studio 2022.
2. Restaurá paquetes (VS lo hace solo).
3. F5.
4. Presioná **"Probar conexión Oracle"**. Deberías ver `Conexión OK (Oracle)` y en el log `Conexión exitosa.`

## Errores comunes
- ORA-12514 / ORA-12505: probá SERVICE_NAME en lugar de SID, o EZCONNECT.
- Tiempo de espera: confirmá reachability a `172.10.10.5:1521` (firewall).
- Usuario/clave inválidos: revisá credenciales `CLINICA/rep`.
- NLS: no aplica (solo `SELECT 1`).

## Próximo paso (v0.7)
- Usar el mismo proyecto y agregar `GetByDni` / `Upsert` contra tablas reales.
