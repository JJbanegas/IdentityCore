# Despliegue de IdentityCore (primera etapa)

Guia practica para publicar la API con bajo costo (incluso plan gratis) y probar login desde frontend.

## 1) Opcion recomendada para primera prueba

Para evitar cambios grandes de codigo (tu API usa SQL Server), usa un hosting compartido ASP.NET + SQL Server.

- Opcion tipica de entrada: proveedor de ASP.NET con plan free/trial y base SQL Server incluida.
- Ventaja: no migras motor de base de datos, solo publicas y configuras variables.

> Nota: los planes gratis cambian seguido. Antes de elegir, valida que soporte:
> - ASP.NET Core
> - SQL Server
> - Variables de entorno o `appsettings.Production.json`

## 2) Configuracion que ya quedo lista en el proyecto

- Conexion de BD por `ConnectionStrings:DefaultConnection` (recomendado) o fallback a variable `connectionstring`.
- CORS configurable por `Cors:AllowedOrigins`.
- Migraciones en startup configurables con `Database:RunMigrationsOnStartup`.
- Endpoint de salud: `GET /health`.

## 3) Variables que debes definir en produccion

Configura estas claves en el panel del hosting:

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__SecretKey`
- `JwtSettings__Issuer`
- `JwtSettings__Audience`
- `JwtSettings__ExpirationMinutes`
- `Database__RunMigrationsOnStartup` = `true` (solo primer deploy) / `false` (luego)
- `Cors__AllowedOrigins__0` = `https://tu-frontend.com`

Si el hosting no soporta `ConnectionStrings__DefaultConnection`, usa:

- `connectionstring`

## 4) Publicar la API

Desde la raiz del repo:

```bash
dotnet publish .\Presentation\Presentation.csproj -c Release -o .\publish
```

Sube el contenido de la carpeta `publish` al hosting (FTP/WebDeploy/panel del proveedor).

## 5) Base de datos y migraciones

### Opcion A (recomendada para inicio)

- Deja `Database__RunMigrationsOnStartup=true` en el primer arranque.
- Inicia la app una vez.
- Verifica en logs que aplico migraciones.
- Cambia a `false` para siguientes despliegues.

### Opcion B (manual)

Ejecuta migracion desde tu equipo contra la cadena remota:

```bash
dotnet ef database update --project .\Infrastructure\Infrastructure.csproj --startup-project .\Presentation\Presentation.csproj
```

## 6) Pruebas minimas post-deploy

1. `GET /health` debe responder 200.
2. `POST /api/v1/user/Register` debe crear usuario.
3. `POST /api/v1/user/Login` debe devolver access token y cookie `refreshToken`.
4. `GET /api/v1/user/GetUsers`:
   - sin token: 401 con payload JSON
   - con token de usuario sin permisos: 403 JSON
   - con `Admin`: 200

## 7) CORS para frontend

En produccion NO dejes CORS abierto. Configura solo dominios del frontend:

- `Cors__AllowedOrigins__0=https://frontend-pruebas.tu-dominio.com`
- `Cors__AllowedOrigins__1=https://localhost:5173` (si pruebas local)

## 8) Checklist rapido de seguridad

- Usa `JwtSettings__SecretKey` largo (minimo 32 caracteres, ideal 64+).
- No subir secretos al repo.
- Mantener HTTPS habilitado en hosting.
- Verificar `Secure` en cookie de refresh token en produccion.

## 9) Siguiente etapa (cuando escale)

- Pasar a CI/CD (GitHub Actions) con deploy automatico.
- Mover secretos a vault.
- Agregar API Gateway.
- Agregar observabilidad (logs centralizados + métricas).

