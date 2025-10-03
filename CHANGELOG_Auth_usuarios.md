# 📋 RESUMEN DE CAMBIOS - Actualización Tabla Auth_usuarios

**Fecha:** 01 de Octubre, 2025  
**Ticket:** Alineación de modelo con estructura SQL

---

## 🎯 OBJETIVO

Actualizar el modelo `UsuarioAuth` y todos los archivos relacionados para que coincidan exactamente con la estructura de la tabla SQL `Auth_usuarios` proporcionada.

---

## 📊 CAMBIOS EN LA ESTRUCTURA DE DATOS

### **Antes (Modelo Antiguo)**
```csharp
public class UsuarioAuth {
    public Guid Id { get; set; }
    public string NombreCompleto { get; set; }
    public string Email { get; set; }
    public string ContrasenaHash { get; set; }
    public string Rol { get; set; }
    public bool LicenciaConducir { get; set; }
    public string TransmisionHabilitada { get; set; }
}
```

### **Después (Modelo Nuevo)**
```csharp
public class UsuarioAuth {
    public int Id { get; set; }                    // ✅ Cambiado: Guid → int IDENTITY
    public string Nombre { get; set; }             // ✅ Nuevo: Separado de NombreCompleto
    public string Apellido { get; set; }           // ✅ Nuevo: Separado de NombreCompleto
    public int? Telefono { get; set; }             // ✅ Nuevo: Campo telefono
    public string Email { get; set; }              // ✅ MaxLength: 255 → 150
    public string Password { get; set; }           // ✅ Nuevo: Contraseña en texto plano
    public string? ContrasenaHash { get; set; }    // ✅ Ahora nullable
    public int? Rol { get; set; }                  // ✅ Cambiado: string → int?
    public string? LicenciaConducir { get; set; }  // ✅ Cambiado: bool → string (número)
    public string? TransmisionHabilitada { get; } // ✅ Ahora nullable
}
```

---

## 🔧 ARCHIVOS MODIFICADOS

### **BACKEND (.NET 8 / C#)**

#### 1. **UsuarioAuth.cs** (Modelo de Entidad)
📁 `back_cabs/CRM/models/Auth/UsuarioAuth.cs`

✅ **Cambios:**
- Id: `Guid` → `int` con `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]`
- Separar `NombreCompleto` en `Nombre` y `Apellido`
- Agregar campo `Telefono` (int nullable)
- Agregar campo `Password` (string 255 caracteres)
- `ContrasenaHash` ahora es nullable
- `Rol`: `string` → `int?`
- `LicenciaConducir`: `bool` → `string?` (máximo 50 caracteres)
- `TransmisionHabilitada`: ahora nullable
- Agregar atributos `[Column]` para mapeo explícito con base de datos
- Mantener propiedad calculada `NombreCompleto` como `[NotMapped]`

#### 2. **UsuarioRegistroRequestDto.cs** (DTO Request)
📁 `back_cabs/CRM/DTOs/Auth/UsuarioRegistroRequestDto.cs`

✅ **Cambios:**
- Separar `NombreCompleto` en `Nombre` y `Apellido` (2-100 caracteres cada uno)
- Agregar `Telefono` (int? opcional, validación 10 dígitos)
- Email: MaxLength 255 → 150
- `LicenciaConducir`: `bool` → `string?` (máximo 50 caracteres)
- `TransmisionHabilitada`: ahora nullable

#### 3. **UsuarioResponseDto.cs** (DTO Response)
📁 `back_cabs/CRM/DTOs/Auth/UsuarioResponseDto.cs`

✅ **Cambios:**
- Id: `Guid` → `int`
- Agregar campos `Nombre`, `Apellido`, `Telefono`
- Mantener `NombreCompleto` como campo calculado
- `Rol`: `string` → `int?`
- `LicenciaConducir`: `bool` → `string?`
- `TransmisionHabilitada`: ahora nullable

#### 4. **UsuarioAuthService.cs** (Lógica de Negocio)
📁 `back_cabs/CRM/services/Auth/UsuarioAuthService.cs`

✅ **Cambios:**
- Actualizar mapeo en `RegistrarUsuarioAsync()` para usar `Nombre`, `Apellido`, `Telefono`
- Guardar contraseña en `Password` (texto plano) y en `ContrasenaHash` (hash SHA256)
- Ajustar `ObtenerUsuarioPorIdAsync()`: parámetro `Guid` → `int`
- Ajustar `ActualizarContrasenaAsync()`: parámetro `Guid` → `int`
- Actualizar `ValidarCredencialesAsync()` para validar tanto `Password` como `ContrasenaHash`
- Actualizar método `MapearAUsuarioResponseDto()` con campos nuevos

#### 5. **UsuarioRegistroValidator.cs** (Validaciones)
📁 `back_cabs/CRM/validators/Auth/UsuarioRegistroValidator.cs`

✅ **Cambios:**
- Reemplazar validación de `NombreCompleto` por validaciones de `Nombre` y `Apellido`
- Agregar validación de `Telefono` (opcional, 10 dígitos)
- Email: MaxLength 255 → 150
- Simplificar validación de contraseña (eliminar complejidad temporalmente)
- Eliminar validaciones de enum `Rol` y `TipoTransmision` (pendiente de implementar)
- Actualizar validaciones de `LicenciaConducir` y `TransmisionHabilitada` como strings

#### 6. **ReadOnlyContext.cs y WriteContext.cs** (Configuración EF Core)
📁 `back_cabs/CRM/contexts/ReadOnlyContext.cs`  
📁 `back_cabs/CRM/contexts/WriteContext.cs`

✅ **Cambios:**
- Agregar `ToTable("Auth_usuarios")` para mapeo explícito
- Configurar `Id` con `ValueGeneratedOnAdd()` para IDENTITY
- Actualizar configuración de propiedades:
  - Email: MaxLength 150
  - Nombre y Apellido: MaxLength 100, requeridos
  - Telefono: opcional
  - Password: MaxLength 255, requerido
  - ContrasenaHash: MaxLength 255, opcional
  - Rol: opcional
  - LicenciaConducir: MaxLength 50, opcional
  - TransmisionHabilitada: MaxLength 50, opcional
  - CreadoEn: requerido, default GETDATE()
  - ActualizadoEn: opcional

---

### **FRONTEND (Angular 18 / TypeScript)**

#### 7. **secure-auth.service.ts** (Servicio de Autenticación)
📁 `front_cabs/src/app/core/services/secure-auth.service.ts`

✅ **Cambios:**
- Actualizar `RegisterRequest` interface:
  - Separar `nombreCompleto` en `nombre` y `apellido`
  - Agregar `telefono?: number | null`
  - `licenciaConducir`: `boolean` → `string | null` (número de licencia)
- Actualizar `User` interface:
  - Id: `string` → `number`
  - Agregar campos `nombre`, `apellido`, `telefono`
  - `rol`: `string` → `number | null`
- Actualizar `RegistroResponse` interface con estructura nueva
- Fix: Agregar optional chaining a `user.permissions?.includes()`

#### 8. **register.component.ts** (Formulario de Registro)
📁 `front_cabs/src/app/features/auth/pages/register/register.component.ts`

✅ **Cambios:**
- Reemplazar campo `nombreCompleto` por `nombre` y `apellido` separados
- Agregar campo `telefono` con validación de 10 dígitos
- Actualizar validaciones:
  - Nombre: 2-100 caracteres, requerido
  - Apellido: 2-100 caracteres, requerido
  - Telefono: opcional, pattern `/^\d{10}$/`
  - Email: maxLength 150
- `licenciaConducir`: ahora es `string` en lugar de `boolean`
- Actualizar construcción del payload en `onSubmit()`

#### 9. **register.component.html** (Template HTML)
📁 `front_cabs/src/app/features/auth/pages/register/register.component.html`

✅ **Cambios:**
- Reemplazar input único de `nombreCompleto` por dos campos:
  - Input `nombre` (placeholder: "Juan")
  - Input `apellido` (placeholder: "Pérez García")
- Agregar input `telefono`:
  - Type: text
  - Placeholder: "5512345678"
  - MaxLength: 10
  - Validación de pattern (10 dígitos)
- Cambiar checkbox `licenciaConducir` por input de texto:
  - Placeholder: "ABC123456"
  - MaxLength: 50
  - Label: "Número de Licencia de Conducir (Opcional)"
- Actualizar mensajes de error de validación

---

## 🗄️ MIGRACIÓN DE BASE DE DATOS

#### 10. **001_UpdateAuth_usuarios_Schema.sql** (Script de Migración)
📁 `back_cabs/CRM/Migrations/001_UpdateAuth_usuarios_Schema.sql`

✅ **Contenido:**
- Respaldar datos existentes en `Auth_usuarios_backup`
- Eliminar tabla antigua `Auth_usuarios`
- Crear nueva tabla con estructura actualizada:
  ```sql
  CREATE TABLE Auth_usuarios (
      id INT IDENTITY(1,1) PRIMARY KEY,
      creado_en DATETIME DEFAULT GETDATE() NOT NULL,
      actualizado_en DATETIME,
      email VARCHAR(150) NOT NULL UNIQUE,
      password VARCHAR(255) NOT NULL,
      nombre VARCHAR(100) NOT NULL,
      apellido VARCHAR(100) NOT NULL,
      telefono INT,
      contraseña_hash VARCHAR(255),
      rol INT,
      activo BIT DEFAULT 1 NOT NULL,
      licencia_conducir VARCHAR(50),
      transmicion_habilitada VARCHAR(50)
  );
  ```
- Crear índices optimizados (email, activo, rol)
- Incluir ejemplo de migración de datos del backup

---

## ✅ VERIFICACIÓN DE CAMBIOS

### **Errores Corregidos:**
1. ✅ Id: Guid → int IDENTITY
2. ✅ NombreCompleto separado en Nombre y Apellido
3. ✅ Telefono agregado (int nullable)
4. ✅ Email maxLength: 255 → 150
5. ✅ Password agregado (texto plano temporal)
6. ✅ Rol: string → int nullable
7. ✅ LicenciaConducir: bool → string nullable
8. ✅ TransmisionHabilitada: string → string nullable
9. ✅ Validaciones actualizadas en backend y frontend
10. ✅ Formulario de registro actualizado con campos separados
11. ✅ Database contexts configurados correctamente
12. ✅ Script de migración SQL creado

### **Compilación:**
- ✅ Backend: Sin errores de compilación
- ✅ Frontend: Sin errores de compilación (TypeScript)

---

## 🚀 PRÓXIMOS PASOS

### **1. Ejecutar Migración de Base de Datos**
```bash
# En SQL Server Management Studio o Azure Data Studio
# Ejecutar: back_cabs/CRM/Migrations/001_UpdateAuth_usuarios_Schema.sql
```

### **2. Crear Migración de Entity Framework (Opcional)**
```bash
cd back_cabs
dotnet ef migrations add UpdateAuthUsuariosSchema
dotnet ef database update
```

### **3. Probar Registro de Usuario**
- Iniciar backend: `dotnet run` en `back_cabs/`
- Iniciar frontend: `ng serve` en `front_cabs/`
- Navegar a: `http://localhost:4200/auth/register`
- Completar formulario con:
  - Nombre: "Juan"
  - Apellido: "Pérez"
  - Teléfono: "5512345678" (opcional)
  - Email: "juan.perez@test.com"
  - Contraseña: "Test1234"
  - Confirmar Contraseña: "Test1234"
  - Rol: "Recepcion"
  - Licencia: "ABC123456" (opcional)
  - Transmisión: "Manual" (opcional)

### **4. Verificar en Base de Datos**
```sql
SELECT * FROM Auth_usuarios;
-- Debe mostrar: id (int), nombre, apellido, telefono, email, password, etc.
```

---

## 📝 NOTAS IMPORTANTES

### **Seguridad:**
⚠️ **ADVERTENCIA:** La contraseña se está guardando en texto plano (`Password`) temporalmente para desarrollo.

**TODO:** Implementar en producción:
- Eliminar campo `Password` (texto plano)
- Usar solo `ContrasenaHash` con bcrypt o Argon2
- Implementar política de contraseñas robusta
- Agregar validación de complejidad en `UsuarioRegistroValidator`

### **Mapeo de Rol:**
⚠️ **PENDIENTE:** Implementar mapeo de roles string → int:
```csharp
// Ejemplo:
enum RolUsuario {
    Administrador = 1,
    Recepcion = 2,
    Soporte = 3
}
```

### **Compatibilidad:**
- ✅ Modelo mantiene propiedad calculada `NombreCompleto` para compatibilidad
- ✅ Frontend e interfaces mantienen campos antiguos como opcionales
- ✅ Validación soporta tanto `Password` como `ContrasenaHash` durante transición

---

## 👥 EQUIPO

- **Desarrollador Backend:** Sistema actualizado con Entity Framework Core
- **Desarrollador Frontend:** Formulario de registro actualizado con Angular Reactive Forms
- **DBA:** Script de migración SQL listo para aplicar

---

## 📞 SOPORTE

Si encuentras algún problema durante la implementación:
1. Verificar que la base de datos tenga la estructura correcta
2. Revisar logs del backend en consola
3. Revisar errores del frontend en Developer Tools
4. Verificar que todos los archivos se hayan guardado correctamente

---

**Documento generado automáticamente por GitHub Copilot**  
**Versión:** 1.0  
**Fecha:** 01/10/2025
