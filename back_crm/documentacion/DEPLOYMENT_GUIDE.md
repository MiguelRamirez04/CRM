# Guía de Despliegue en Producción - Windows Server

Esta guía detalla todos los pasos y configuraciones necesarias para desplegar el proyecto CABS en un servidor Windows Server con IIS **o Docker**.

---

## 📋 Tabla de Contenidos

1. [Requisitos Previos](#requisitos-previos)
2. [Configuración del Backend (ASP.NET Core)](#configuración-del-backend)
3. [Configuración del Frontend (Angular)](#configuración-del-frontend)
4. [Opción A: Despliegue con IIS](#opción-a-despliegue-con-iis)
5. [Opción B: Despliegue con Docker](#opción-b-despliegue-con-docker)
6. [Configuración de Base de Datos](#configuración-de-base-de-datos)
7. [Seguridad y SSL/HTTPS](#seguridad-y-sslhttps)
8. [Variables de Entorno](#variables-de-entorno)
9. [Verificación Post-Despliegue](#verificación-post-despliegue)

---

## Requisitos Previos

### Software Necesario

- ✅ **Windows Server 2016+** (recomendado 2019 o 2022)
- ✅ **IIS 10+** con módulos:
  - URL Rewrite Module
  - Application Request Routing (ARR)
  - ASP.NET Core Hosting Bundle
- ✅ **.NET 8.0 Runtime** (ASP.NET Core Runtime + Hosting Bundle)
- ✅ **SQL Server 2016+** (Express, Standard o Enterprise)
- ✅ **Redis** (opcional pero recomendado para cache)
- ✅ **Certificado SSL** (para HTTPS)

### Instalación de Componentes

```powershell
# Instalar IIS con características necesarias
Install-WindowsFeature -name Web-Server -IncludeManagementTools
Install-WindowsFeature -name Web-Asp-Net45
Install-WindowsFeature -name Web-WebSockets

# Descargar e instalar .NET 8.0 Hosting Bundle
# https://dotnet.microsoft.com/download/dotnet/8.0
```

---

## Configuración del Backend

### 1. Crear `appsettings.Production.json`

Crear un nuevo archivo `appsettings.Production.json` en la raíz del proyecto backend:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR_SQL;Database=CABS_Produccion;User Id=cabs_user;Password=TU_PASSWORD_SEGURO;MultipleActiveResultSets=true;TrustServerCertificate=False;Encrypt=True;",
    "CompacConnection": "Server=TU_SERVIDOR_COMPAC;Database=adCABS2016;User Id=compac_user;Password=TU_PASSWORD_SEGURO;MultipleActiveResultSets=true;TrustServerCertificate=False;Encrypt=True;",
    "RedisConnection": "TU_SERVIDOR_REDIS:6379,password=TU_REDIS_PASSWORD,abortConnect=false,connectTimeout=10000,syncTimeout=10000,ssl=true"
  },
  "SmtpSettings": {
    "Server": "smtp.tudominio.com",
    "Port": 587,
    "SenderName": "CABS Producción",
    "SenderEmail": "noreply@tudominio.com",
    "Username": "TU_SMTP_USERNAME",
    "Password": "TU_SMTP_PASSWORD"
  },
  "GmailSettings": {
    "ClientId": "TU_CLIENT_ID_REAL",
    "ClientSecret": "TU_CLIENT_SECRET_REAL",
    "RefreshToken": "TU_REFRESH_TOKEN_REAL",
    "SenderEmail": "noreply@tudominio.com",
    "UseGmailApi": true
  },
  "JwtSettings": {
    "SecretKey": "GENERA_UNA_CLAVE_SUPER_SEGURA_DE_AL_MENOS_64_CARACTERES_ALEATORIOS_AQUI!",
    "Issuer": "CABS-API-PROD",
    "Audience": "CABS-Client-PROD",
    "ExpiryInMinutes": 30
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information",
        "Microsoft.EntityFrameworkCore": "Error"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "C:\\Logs\\CABS\\app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 90,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "tudominio.com;*.tudominio.com",
  "CorsSettings": {
    "AllowedOrigins": [
      "https://tudominio.com",
      "https://www.tudominio.com",
      "https://app.tudominio.com"
    ]
  },
  "FileStorage": {
    "UploadPath": "C:\\inetpub\\wwwroot\\CABS\\uploads",
    "MaxFileSizeMB": 10,
    "WebPQuality": 80,
    "MaxImageWidth": 1920,
    "MaxImageHeight": 1080
  }
}
```

> [!IMPORTANT]
> **Seguridad Crítica:**
> - ❌ **NUNCA** uses `Trusted_Connection=True` en producción
> - ✅ Usa credenciales SQL específicas con permisos mínimos
> - ✅ Genera una clave JWT única y segura (64+ caracteres aleatorios)
> - ✅ Habilita `Encrypt=True` en las cadenas de conexión
> - ✅ Usa contraseñas fuertes y únicas para cada servicio

### 2. Modificar `Program.cs` - Configuración CORS

Actualizar la sección de CORS en [Program.cs](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/back_cabs/Program.cs) (línea 252):

```csharp
options.AddPolicy("Production", policy =>
{
    policy.WithOrigins("https://tudominio.com", "https://www.tudominio.com", "https://app.tudominio.com")
        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
        .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-CSRF-Token")
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromHours(24));
});
```

### 3. Modificar Configuración de Cookies (Program.cs línea 265-270)

```csharp
options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // OBLIGATORIO en producción
options.Cookie.SameSite = SameSiteMode.Strict; // Máxima protección CSRF
```

### 4. Compilar para Producción

```powershell
cd c:\Users\ANA\Documents\dev\FullStack_CABS\back_cabs

# Limpiar builds anteriores
dotnet clean

# Compilar en modo Release
dotnet publish -c Release -o C:\Publish\CABS\Backend

# Verificar que se generó correctamente
dir C:\Publish\CABS\Backend
```

---

## Configuración del Frontend

### 1. Actualizar `environment.prod.ts`

Editar [environment.prod.ts](file:///c:/Users/ANA/Documents/dev/FullStack_CABS/front_cabs/src/environments/environment.prod.ts):

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.tudominio.com',  // ⚠️ CAMBIAR A TU URL DE PRODUCCIÓN
  
  security: {
    useHttpOnlyCookies: true,
    csrfTokenHeader: 'X-CSRF-Token',
    
    cookieSettings: {
      secure: true,        // OBLIGATORIO con HTTPS
      sameSite: 'Strict',  // Máxima protección CSRF
      httpOnly: true
    },
    
    sessionTimeout: 15 * 60 * 1000,      // 15 minutos
    refreshTokenBefore: 2 * 60 * 1000,   // Refresh 2 min antes
    
    rateLimiting: {
      loginAttempts: 3,
      lockoutTime: 30 * 60 * 1000  // 30 minutos
    }
  },
  
  allowedOrigins: [
    'https://tudominio.com'  // ⚠️ CAMBIAR A TU DOMINIO
  ],
  
  logging: {
    level: 'error',  // Solo errores en producción
    logSecurityEvents: true
  }
};
```

> [!WARNING]
> **URLs Críticas a Configurar:**
> - `apiUrl`: URL completa del backend (ej: `https://api.tudominio.com`)
> - `allowedOrigins`: Dominios permitidos para CORS

### 2. Compilar para Producción

```powershell
cd c:\Users\ANA\Documents\dev\FullStack_CABS\front_cabs

# Instalar dependencias (si es necesario)
npm install

# Compilar en modo producción
npm run build -- --configuration=production

# O usar el comando directo de Angular
ng build --configuration=production

# Verificar que se generó en dist/
dir dist\front_cabs\browser
```

El build generará archivos optimizados en `dist/front_cabs/browser/`.

---

## Opción A: Despliegue con IIS

### 1. Crear Sitios Web en IIS

#### Backend API

```powershell
# Crear Application Pool para el backend
New-WebAppPool -Name "CABS_Backend_Pool"
Set-ItemProperty IIS:\AppPools\CABS_Backend_Pool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\CABS_Backend_Pool -Name processModel.identityType -Value "ApplicationPoolIdentity"

# Crear sitio web
New-Website -Name "CABS_Backend" `
    -PhysicalPath "C:\Publish\CABS\Backend" `
    -ApplicationPool "CABS_Backend_Pool" `
    -Port 443 `
    -Ssl `
    -HostHeader "api.tudominio.com"
```

#### Frontend

```powershell
# Crear Application Pool para el frontend
New-WebAppPool -Name "CABS_Frontend_Pool"

# Crear sitio web
New-Website -Name "CABS_Frontend" `
    -PhysicalPath "C:\inetpub\wwwroot\CABS\Frontend" `
    -ApplicationPool "CABS_Frontend_Pool" `
    -Port 443 `
    -Ssl `
    -HostHeader "tudominio.com"

# Copiar archivos del frontend
Copy-Item -Path "c:\Users\ANA\Documents\dev\FullStack_CABS\front_cabs\dist\front_cabs\browser\*" `
    -Destination "C:\inetpub\wwwroot\CABS\Frontend" -Recurse -Force
```

### 2. Configurar URL Rewrite para Angular

Crear `web.config` en `C:\inetpub\wwwroot\CABS\Frontend\`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Angular Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
      <mimeMap fileExtension=".woff2" mimeType="application/font-woff2" />
    </staticContent>
    <httpProtocol>
      <customHeaders>
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-Frame-Options" value="SAMEORIGIN" />
        <add name="X-XSS-Protection" value="1; mode=block" />
        <add name="Referrer-Policy" value="strict-origin-when-cross-origin" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

### 3. Configurar `web.config` para Backend

El archivo `web.config` debe generarse automáticamente, pero verifica que contenga:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\back_cabs.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

---

## Configuración de Base de Datos

### 1. Crear Base de Datos de Producción

```sql
-- Conectar a SQL Server Management Studio

-- Crear base de datos
CREATE DATABASE CABS_Produccion;
GO

-- Crear usuario específico para la aplicación
USE [master];
GO
CREATE LOGIN cabs_user WITH PASSWORD = 'TU_PASSWORD_SUPER_SEGURO_AQUI';
GO

USE [CABS_Produccion];
GO
CREATE USER cabs_user FOR LOGIN cabs_user;
GO

-- Otorgar permisos mínimos necesarios
ALTER ROLE db_datareader ADD MEMBER cabs_user;
ALTER ROLE db_datawriter ADD MEMBER cabs_user;
ALTER ROLE db_ddladmin ADD MEMBER cabs_user;  -- Solo si necesitas migrations
GO
```

### 2. Ejecutar Migraciones

```powershell
cd c:\Users\ANA\Documents\dev\FullStack_CABS\back_cabs

# Configurar variable de entorno temporal
$env:ASPNETCORE_ENVIRONMENT="Production"

# Ejecutar migraciones
dotnet ef database update --connection "Server=TU_SERVIDOR;Database=CABS_Produccion;User Id=cabs_user;Password=TU_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=False;Encrypt=True;"
```

### 3. Backup y Restauración

```sql
-- Crear backup automático diario
BACKUP DATABASE [CABS_Produccion] 
TO DISK = N'C:\SQLBackups\CABS_Produccion_Full.bak' 
WITH FORMAT, INIT, COMPRESSION;
GO

-- Configurar plan de mantenimiento en SQL Server Agent
```

---

## Seguridad y SSL/HTTPS

### 1. Instalar Certificado SSL

#### Opción A: Certificado Comercial (Recomendado)

1. Comprar certificado SSL de una CA confiable (DigiCert, Sectigo, etc.)
2. Generar CSR desde IIS
3. Instalar certificado en IIS Manager

#### Opción B: Let's Encrypt (Gratuito)

```powershell
# Instalar win-acme
choco install win-acme

# Ejecutar win-acme y seguir wizard
wacs.exe
```

### 2. Configurar Bindings HTTPS en IIS

```powershell
# Backend API
New-WebBinding -Name "CABS_Backend" -Protocol https -Port 443 -HostHeader "api.tudominio.com" -SslFlags 1

# Frontend
New-WebBinding -Name "CABS_Frontend" -Protocol https -Port 443 -HostHeader "tudominio.com" -SslFlags 1
New-WebBinding -Name "CABS_Frontend" -Protocol https -Port 443 -HostHeader "www.tudominio.com" -SslFlags 1
```

### 3. Forzar HTTPS (Redirección)

Agregar a `web.config` del frontend:

```xml
<rule name="HTTPS Redirect" stopProcessing="true">
  <match url="(.*)" />
  <conditions>
    <add input="{HTTPS}" pattern="off" />
  </conditions>
  <action type="Redirect" url="https://{HTTP_HOST}/{R:1}" redirectType="Permanent" />
</rule>
```

### 4. Configurar HSTS (HTTP Strict Transport Security)

En `Program.cs` del backend, agregar:

```csharp
app.UseHsts();  // Ya está incluido, verificar que esté activo en producción
```

---

## Variables de Entorno

### Configurar en IIS

```powershell
# Establecer variable de entorno para el Application Pool
$appPool = Get-Item "IIS:\AppPools\CABS_Backend_Pool"
$appPool.processModel.environmentVariables.Add("ASPNETCORE_ENVIRONMENT", "Production")
$appPool | Set-Item
```

### Variables Críticas

| Variable | Valor | Descripción |
|----------|-------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Activa configuración de producción |
| `ASPNETCORE_URLS` | `http://*:80;https://*:443` | URLs de escucha |

---

## Verificación Post-Despliegue

### 1. Checklist de Verificación

- [ ] **Backend API responde en HTTPS**
  ```powershell
  Invoke-WebRequest -Uri "https://api.tudominio.com/health" -UseBasicParsing
  ```

- [ ] **Frontend carga correctamente**
  ```powershell
  Invoke-WebRequest -Uri "https://tudominio.com" -UseBasicParsing
  ```

- [ ] **CORS configurado correctamente**
  - Probar login desde el frontend
  - Verificar que las peticiones cross-origin funcionen

- [ ] **Base de datos conecta correctamente**
  - Verificar logs en `C:\Logs\CABS\`
  - Probar operaciones CRUD

- [ ] **SSL/HTTPS funcionando**
  - Verificar certificado válido en navegador
  - Probar redirección HTTP → HTTPS

- [ ] **Logs funcionando**
  - Verificar que se escriben en `C:\Logs\CABS\`
  - Revisar que no haya errores críticos

- [ ] **File uploads funcionando**
  - Verificar permisos en `C:\inetpub\wwwroot\CABS\uploads`
  - Probar subida de archivos

### 2. Monitoreo

```powershell
# Ver logs en tiempo real
Get-Content "C:\Logs\CABS\app-$(Get-Date -Format 'yyyyMMdd').log" -Wait -Tail 50

# Verificar estado del Application Pool
Get-WebAppPoolState -Name "CABS_Backend_Pool"

# Verificar estado del sitio
Get-Website -Name "CABS_Backend"
```

### 3. Troubleshooting Común

#### Error 500.19 - Configuration Error

```powershell
# Instalar URL Rewrite Module
# https://www.iis.net/downloads/microsoft/url-rewrite
```

#### Error 502.5 - Process Failure

```powershell
# Verificar que .NET 8.0 Hosting Bundle esté instalado
dotnet --list-runtimes

# Reiniciar IIS
iisreset
```

#### CORS Errors

- Verificar `CorsSettings.AllowedOrigins` en `appsettings.Production.json`
- Verificar que el frontend use la URL correcta en `environment.prod.ts`
- Verificar que `AllowCredentials()` esté configurado

---

## Resumen de Archivos a Modificar

### Backend

| Archivo | Cambios Requeridos |
|---------|-------------------|
| `appsettings.Production.json` | ✅ Crear nuevo con configuración de producción |
| `Program.cs` (línea 252) | ✅ Actualizar CORS con dominios de producción |
| `Program.cs` (línea 265-270) | ✅ Configurar cookies seguras |

### Frontend

| Archivo | Cambios Requeridos |
|---------|-------------------|
| `environment.prod.ts` | ✅ Configurar `apiUrl` con URL de producción |
| `environment.prod.ts` | ✅ Configurar `allowedOrigins` |

### Configuración de Servidor

| Componente | Acción |
|------------|--------|
| IIS | ✅ Crear sitios web y application pools |
| SSL Certificate | ✅ Instalar y configurar |
| SQL Server | ✅ Crear base de datos y usuario |
| Firewall | ✅ Abrir puertos 80 y 443 |
| DNS | ✅ Configurar registros A para dominios |

---

## 🔒 Consideraciones de Seguridad Final

1. ✅ **Nunca** expongas `appsettings.Production.json` en control de versiones
2. ✅ Usa **Azure Key Vault** o **Windows Credential Manager** para secretos
3. ✅ Configura **rate limiting** en IIS o usa un WAF
4. ✅ Habilita **Application Insights** o similar para monitoreo
5. ✅ Configura **backups automáticos** de la base de datos
6. ✅ Implementa **log rotation** para evitar llenar el disco
7. ✅ Usa **usuarios de servicio** con permisos mínimos
8. ✅ Mantén **Windows Server y SQL Server actualizados**

---

**¿Necesitas ayuda?** Revisa los logs en `C:\Logs\CABS\` para diagnosticar problemas.
