# Seguridad JWT — IdentityCore

> Documentación técnica del sistema de autenticación y autorización para el ecosistema de microservicios.  
> Última actualización: 2026-04-30

---

## Índice

1. [Arquitectura general](#1-arquitectura-general)
2. [Flujo completo de autenticación](#2-flujo-completo-de-autenticación)
3. [Endpoints de autenticación](#3-endpoints-de-autenticación)
4. [Estructura del JWT](#4-estructura-del-jwt)
5. [Refresh Token](#5-refresh-token)
6. [Roles disponibles](#6-roles-disponibles)
7. [Guía: agregar auth a un nuevo microservicio](#7-guía-agregar-auth-a-un-nuevo-microservicio)
8. [Estrategia de permisos granulares (futuro)](#8-estrategia-de-permisos-granulares-futuro)
9. [Mejoras futuras](#9-mejoras-futuras)

---

## 1. Arquitectura general

```
┌─────────────┐     POST /Login      ┌───────────────────┐
│   Frontend  │ ──────────────────►  │   IdentityCore    │
│   (Web)     │ ◄──────────────────  │  (Identity Server)│
│             │   { accessToken }    │                   │
│             │   cookie:refreshToken│   - ASP.NET Identity
└─────────────┘                      │   - JWT Bearer HS256
        │                            │   - SQL Server     │
        │ Authorization: Bearer xxx  └───────────────────┘
        ▼
┌─────────────────────────────────────────────────────┐
│              Microservicios consumidores            │
│                                                     │
│   MicroservicioA    MicroservicioB    MicroservicioC│
│   (valida JWT       (valida JWT       (valida JWT   │
│    localmente)       localmente)       localmente)  │
└─────────────────────────────────────────────────────┘
```

**Puntos clave:**
- **IdentityCore** es el único que **emite** tokens (tiene la `SecretKey`).
- **Los demás microservicios** solo **validan** tokens usando la misma `SecretKey` — sin llamar a IdentityCore en cada request.
- El algoritmo usado es **HS256** (clave simétrica compartida).
- El **access token** vive **1 hora**.
- El **refresh token** vive **7 días** y se almacena en `httpOnly cookie`.

---

## 2. Flujo completo de autenticación

```
1. REGISTRO
   Frontend ──POST /api/v1/user/Register──► IdentityCore
   { email, userName, password }
   ◄── 201 Created { data: userId }

2. LOGIN
   Frontend ──POST /api/v1/user/Login──► IdentityCore
   { email, password }
   ◄── 200 OK { data: { accessToken, expiresAt, roles, ... } }
         + Set-Cookie: refreshToken=xxx; HttpOnly; SameSite=Strict

3. USO DEL TOKEN (en cualquier microservicio)
   Frontend ──GET /api/v1/resource──► MicroservicioX
   Header: Authorization: Bearer {accessToken}
   ◄── 200 OK { data: ... }

4. RENOVAR TOKEN (cuando el access token expira)
   Frontend ──POST /api/v1/user/Refresh──► IdentityCore
   (la cookie refreshToken se envía automáticamente)
   ◄── 200 OK { data: { accessToken nuevo, ... } }
         + Set-Cookie: refreshToken=nuevo; HttpOnly

5. LOGOUT
   Frontend ──POST /api/v1/user/Logout──► IdentityCore
   ◄── 200 OK { "Sesión cerrada exitosamente" }
         + Set-Cookie: refreshToken=; Expires=pasado (borra la cookie)
```

---

## 3. Endpoints de autenticación

### POST `/api/v1/user/Register`

**Request:**
```json
{
  "email": "usuario@ejemplo.com",
  "userName": "johndoe",
  "password": "MiP@ssword123"
}
```

**Response 201:**
```json
{
  "success": true,
  "message": "Usuario creado exitosamente",
  "data": "a1b2c3d4-uuid-del-usuario"
}
```

**Errores:**
- `409 Conflict` — el email ya está registrado

---

### POST `/api/v1/user/Login`

**Request:**
```json
{
  "email": "usuario@ejemplo.com",
  "password": "MiP@ssword123"
}
```

**Response 200:**
```json
{
  "success": true,
  "message": "Login exitoso",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiresAt": "2026-04-30T21:00:00Z",
    "userId": "a1b2c3d4-uuid",
    "userName": "johndoe",
    "email": "usuario@ejemplo.com",
    "roles": ["User"]
  }
}
```

> ⚠️ El `refreshToken` **no aparece en el body**. Se envía automáticamente como `httpOnly cookie`.

**Errores:**
- `401 Unauthorized` — credenciales inválidas

---

### POST `/api/v1/user/Refresh`

No requiere body. Lee el `refreshToken` automáticamente de la cookie.

**Response 200:** mismo formato que Login con nuevos tokens.

**Errores:**
- `401 Unauthorized` — cookie ausente, token inválido o expirado

---

### POST `/api/v1/user/Logout`

No requiere body. Revoca el refresh token y borra la cookie.

**Response 200:**
```json
{
  "success": true,
  "message": "Sesión cerrada exitosamente",
  "data": ""
}
```

---

### GET `/api/v1/user/GetUsers` 🔒 `[UserOnly]`

Requiere token JWT con rol `User`.

**Header:** `Authorization: Bearer {accessToken}`

**Response 200:**
```json
{
  "success": true,
  "message": "Usuarios obtenidos exitosamente",
  "data": [
    { "userName": "johndoe", "email": "john@ejemplo.com" },
    { "userName": "janedoe", "email": "jane@ejemplo.com" }
  ]
}
```

---

## 4. Estructura del JWT

El access token decodificado tiene la siguiente estructura:

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload (claims):**
```json
{
  "sub": "a1b2c3d4-uuid-del-usuario",
  "email": "usuario@ejemplo.com",
  "jti": "uuid-unico-del-token",
  "nameid": "a1b2c3d4-uuid-del-usuario",
  "unique_name": "johndoe",
  "role": "User",
  "iss": "IdentityCore",
  "aud": "IdentityCore.Microservices",
  "exp": 1746050400,
  "iat": 1746046800
}
```

> Si el usuario tiene múltiples roles, el campo `role` será un array:  
> `"role": ["User", "Partner"]`

**Claims importantes:**
| Claim | Tipo .NET | Descripción |
|---|---|---|
| `sub` | `JwtRegisteredClaimNames.Sub` | ID único del usuario |
| `email` | `JwtRegisteredClaimNames.Email` | Email del usuario |
| `jti` | `JwtRegisteredClaimNames.Jti` | ID único del token (para blacklist futuro) |
| `role` | `ClaimTypes.Role` | Rol(es) del usuario |
| `unique_name` | `ClaimTypes.Name` | Username |
| `exp` | Automático | Fecha de expiración (Unix timestamp) |

---

## 5. Refresh Token

- Se genera como un GUID doble (`Guid.NewGuid() + Guid.NewGuid()`) = 64 caracteres hex.
- Se almacena en la tabla `dbo.RefreshTokens` en SQL Server.
- Se envía al cliente como `httpOnly cookie` (JavaScript no puede leerlo).
- Dura **7 días**.
- Se **rota** en cada uso: el token anterior se revoca y se emite uno nuevo.
- Se **revoca** al hacer logout o al detectar uso inválido.

**Cuándo llamar a `/Refresh`:**
- Cuando recibes `401 Unauthorized` en cualquier microservicio.
- O proactivamente cuando `expiresAt` del access token esté próximo a vencer.

---

## 6. Roles disponibles

| Rol | Descripción |
|---|---|
| `Admin` | Acceso total al sistema |
| `User` | Usuario estándar (rol por defecto al registrarse) |
| `Partner` | Acceso a funcionalidades de partners |

Los roles se crean automáticamente al iniciar la aplicación si no existen (seed idempotente).

**Para asignar un rol a un usuario** (desde código):
```csharp
await _userManager.AddToRoleAsync(user, "Admin");
```

**Para verificar un rol en un endpoint:**
```csharp
// Por atributo (MVC / Razor):
[Authorize(Roles = "Admin")]

// Por policy (Minimal API):
.RequireAuthorization("AdminOnly")
.RequireAuthorization("AdminOrPartner")

// Inline en la definición:
group.MapGet("/ruta", handler).RequireAuthorization("AdminOnly");
```

---

## 7. Guía: agregar auth a un nuevo microservicio

**5 pasos para que cualquier microservicio valide tokens de IdentityCore:**

### Paso 1 — Instalar paquete

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0
```

### Paso 2 — Agregar `JwtSettings` a `appsettings.json`

```json
{
  "JwtSettings": {
    "SecretKey": "LA_MISMA_SECRET_KEY_QUE_IDENTITY_CORE",
    "Issuer": "IdentityCore",
    "Audience": "IdentityCore.Microservices",
    "ExpirationMinutes": "60"
  }
}
```

> ⚠️ La `SecretKey`, `Issuer` y `Audience` deben ser **idénticas** a las de IdentityCore.  
> En producción, usa variables de entorno o un vault, nunca hardcodees la clave.

### Paso 3 — Configurar autenticación en `Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // sin margen de tolerancia
    };
});

builder.Services.AddAuthorization();
```

### Paso 4 — Activar middleware (orden importa)

```csharp
var app = builder.Build();

app.UseAuthentication(); // ← primero
app.UseAuthorization();  // ← segundo
// luego tus endpoints/controllers
```

### Paso 5 — Proteger endpoints

```csharp
// Cualquier usuario autenticado:
app.MapGet("/recurso", handler).RequireAuthorization();

// Solo admins:
app.MapGet("/admin/recurso", handler).RequireAuthorization(p => p.RequireRole("Admin"));

// Leer el userId del token dentro del handler:
app.MapGet("/mi-perfil", (HttpContext ctx) =>
{
    var userId = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email  = ctx.User.FindFirst(ClaimTypes.Email)?.Value;
    var roles  = ctx.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    return Results.Ok(new { userId, email, roles });
}).RequireAuthorization();
```

---

## 8. Estrategia de permisos granulares (futuro)

Cuando los roles no sean suficientes y necesites permisos específicos por acción  
(ej. `partner:read`, `partner:write`, `user:delete`), puedes agregar un claim `permissions` al JWT.

### Paso A — Agregar permisos al token (en `JwtTokenService.GenerateToken`)

```csharp
// Opción 1: permisos fijos por rol (en JwtTokenService)
var permissions = role switch
{
    "Admin"   => new[] { "user:read", "user:write", "user:delete", "partner:read", "partner:write" },
    "Partner" => new[] { "partner:read", "partner:write" },
    "User"    => new[] { "user:read" },
    _         => Array.Empty<string>()
};

foreach (var permission in permissions)
    claims.Add(new Claim("permissions", permission));

// Opción 2: permisos dinámicos desde BD (tabla UserPermissions)
// var permissions = await _permissionService.GetUserPermissionsAsync(userId);
```

### Paso B — Crear extension de policy

```csharp
// En cualquier microservicio
public static class PermissionPolicy
{
    public static AuthorizationPolicy Require(string permission) =>
        new AuthorizationPolicyBuilder()
            .RequireClaim("permissions", permission)
            .Build();
}
```

### Paso C — Registrar policies granulares

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanReadPartners",  p => p.RequireClaim("permissions", "partner:read"));
    options.AddPolicy("CanWritePartners", p => p.RequireClaim("permissions", "partner:write"));
    options.AddPolicy("CanDeleteUsers",   p => p.RequireClaim("permissions", "user:delete"));
});
```

### Paso D — Usar en endpoints

```csharp
group.MapGet("/partners",    handler).RequireAuthorization("CanReadPartners");
group.MapPost("/partners",   handler).RequireAuthorization("CanWritePartners");
group.MapDelete("/users/{id}", handler).RequireAuthorization("CanDeleteUsers");
```

### Ventaja sobre roles

| Roles | Permisos granulares |
|---|---|
| `[Authorize(Roles = "Admin")]` | `[Authorize(Policy = "CanWritePartners")]` |
| Acoplado al nombre del rol | Desacoplado — el rol puede cambiar |
| Difícil de escalar | Escala a cientos de permisos |
| | Compatible con sistemas RBAC/ABAC |

---

## 9. Mejoras futuras

| Prioridad | Tema | Descripción | Por dónde investigar |
|---|---|---|---|
| 🔴 Alta | **API Gateway** (YARP u Ocelot) | Punto único de entrada, valida el token una sola vez antes de enrutar a los microservicios | [YARP docs](https://microsoft.github.io/reverse-proxy/) · [Ocelot docs](https://ocelot.readthedocs.io/) |
| 🔴 Alta | **RS256 (asimétrico)** | IdentityCore firma con clave privada; los microservicios validan con clave pública (más seguro, nadie más puede emitir tokens) | [JWT RS256 con .NET](https://learn.microsoft.com/security) |
| 🟡 Media | **Redis para blacklist de tokens** | Permite revocar un access token antes de que expire (útil para ban de usuarios, forzar logout desde admin) | [StackExchange.Redis](https://stackexchange.github.io/StackExchange.Redis/) |
| 🟡 Media | **Refresh Token Family** | Detecta reutilización de refresh tokens robados: si se usa un token ya revocado, se revocan TODOS los de ese usuario | [OAuth 2.0 Token Family](https://auth0.com/docs/secure/tokens/refresh-tokens/refresh-token-rotation) |
| 🟡 Media | **Permisos granulares** | Evolucionar de roles a claims de permisos (`permissions[]`) como se describe en la sección 8 | Ver sección 8 más arriba |
| 🟡 Media | **Variables de entorno para SecretKey** | Nunca hardcodear la clave en `appsettings.json`. Usar `Environment.GetEnvironmentVariable` o un vault | [.NET Secret Manager](https://learn.microsoft.com/aspnet/core/security/app-secrets) |
| 🟢 Baja | **Machine-to-Machine (Client Credentials)** | Cuando un microservicio necesita llamar a otro en nombre del sistema (sin usuario): OAuth 2.0 Client Credentials flow | [OAuth2 Client Credentials](https://oauth.net/2/grant-types/client-credentials/) |
| 🟢 Baja | **Azure Key Vault / HashiCorp Vault** | Gestión centralizada y segura de secretos en producción | [Azure Key Vault con .NET](https://learn.microsoft.com/azure/key-vault/) |
| 🟢 Baja | **PKCE para frontend SPA** | Estándar de seguridad recomendado para aplicaciones web que usan OAuth2 | [PKCE explanation](https://oauth.net/2/pkce/) |
| 🟢 Baja | **Keycloak / Azure AD B2C** | Cuando el proyecto crezca, considerar delegar la identidad a un proveedor externo | [Keycloak](https://www.keycloak.org/) · [Azure AD B2C](https://learn.microsoft.com/azure/active-directory-b2c/) |

---

> **Nota de seguridad general:**  
> - Nunca expongas el `SecretKey` en logs, respuestas de error ni repositorios públicos.  
> - Rota el `SecretKey` periódicamente en producción (esto invalida todos los tokens activos — planifícalo).  
> - El access token no se puede revocar antes de expirar con HS256 puro. Para revocación inmediata, implementa Redis blacklist (ver mejoras futuras).

