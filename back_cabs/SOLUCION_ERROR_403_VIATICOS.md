# Solución: Error 403 Forbidden en API de Viáticos

## 🔴 Problema Identificado

Al intentar acceder a los endpoints de `GastoViaticos`, el backend devuelve **403 Forbidden**.

### Logs del Error:
```
[20:39:52 INF] Authorization failed. These requirements were not met:
RolesAuthorizationRequirement:User.IsInRole must be true for one of the following roles: (Soporte)
[20:39:52 INF] AuthenticationScheme: Bearer was forbidden.
[20:39:52 INF] Response: 403 for GET /api/GastoViaticos
```

### Causa Raíz:
1. **Controlador muy restrictivo**: El `GastoViaticosController` tenía `[Authorize(Roles = "Soporte")]`
2. **Usuario sin rol válido**: El usuario de prueba no tiene el rol "ADMINISTRACION", "RECEPCION" o "SOPORTE"

---

## ✅ Soluciones Implementadas

### 1. **Actualización del Controlador** ✅

**Archivo**: `back_cabs/CRM/controllers/GastoViaticosController.cs`

**Cambio**:
```csharp
// ANTES (solo rol Soporte)
[Authorize(Roles = "Soporte")]

// DESPUÉS (roles múltiples como en sidebar)
[Authorize(Roles = "ADMINISTRACION,RECEPCION,SOPORTE")]
```

**Justificación**:
- Consistente con la configuración del sidebar frontend
- Permite acceso a todos los roles operativos
- Alineado con la política de permisos de la aplicación

---

### 2. **Script SQL para Actualizar Usuarios** ✅

**Archivo**: `back_cabs/update_user_roles_viaticos.sql`

**Funcionalidad**:
1. Muestra usuarios actuales y sus roles
2. Actualiza usuarios sin rol válido
3. Verifica la actualización

**Uso**:
```sql
-- Ejecutar en SQL Server Management Studio o Azure Data Studio
USE [TU_BASE_DE_DATOS];
GO

-- Ver usuarios actuales
SELECT id, nombre, apellido, correo, rol FROM auth_usuarios;

-- Actualizar rol del usuario de prueba
UPDATE auth_usuarios 
SET rol = 'ADMINISTRACION', actualizado_en = GETDATE()
WHERE correo LIKE '%tu-email%';

-- Verificar cambio
SELECT id, nombre, apellido, correo, rol FROM auth_usuarios;
```

---

## 🔧 Pasos para Resolver

### Opción A: Ejecutar Script SQL (Recomendado)

1. **Abrir SQL Server Management Studio** o **Azure Data Studio**

2. **Conectar a tu base de datos**

3. **Ejecutar el script**:
   ```bash
   # Desde el directorio back_cabs
   sqlcmd -S localhost -d tu_base_de_datos -i update_user_roles_viaticos.sql
   ```

4. **Verificar que el usuario tiene rol válido**:
   ```sql
   SELECT correo, rol FROM auth_usuarios WHERE activo = 1;
   ```

5. **Reiniciar el backend** (opcional):
   ```bash
   # En el terminal del backend
   Ctrl + C
   dotnet run
   ```

### Opción B: Actualizar Manualmente en Base de Datos

1. Conectar a la base de datos
2. Ejecutar:
   ```sql
   UPDATE auth_usuarios 
   SET rol = 'ADMINISTRACION' 
   WHERE correo = 'tu-email@ejemplo.com';
   ```

### Opción C: Crear Usuario de Prueba con Rol Correcto

```sql
INSERT INTO auth_usuarios 
(nombre, apellido, correo, password_hash, rol, telefono, activo, creado_en)
VALUES 
('Admin', 'Prueba', 'admin@test.com', 'hash_password_aqui', 'ADMINISTRACION', 1234567890, 1, GETDATE());
```

---

## 📋 Verificación Post-Solución

### 1. Verificar Usuario en DB
```sql
SELECT id, nombre, apellido, correo, rol, activo 
FROM auth_usuarios 
WHERE correo = 'tu-email';
```

**Resultado esperado**:
```
id | nombre | apellido | correo         | rol            | activo
---|--------|----------|----------------|----------------|-------
1  | Admin  | Test     | test@test.com  | ADMINISTRACION | 1
```

### 2. Probar API con Postman/cURL

**GET - Listar Viáticos**:
```bash
curl -X GET "http://localhost:5176/api/GastoViaticos?pageNumber=1&pageSize=10" \
  -H "Cookie: accessToken=TU_TOKEN_JWT"
```

**Respuesta esperada**: `200 OK` con JSON de viáticos

**POST - Crear Viático**:
```bash
curl -X POST "http://localhost:5176/api/GastoViaticos" \
  -H "Content-Type: application/json" \
  -H "Cookie: accessToken=TU_TOKEN_JWT" \
  -d '{
    "tieneFactura": false,
    "fecha": "2025-01-04",
    "gastos": "Gasolina, Peajes",
    "montoTotal": 500.00
  }'
```

**Respuesta esperada**: `201 Created` con JSON del viático creado

### 3. Probar desde Frontend

1. **Login** en `http://localhost:4200/auth/login`
2. **Navegar** a `/viaticos`
3. **Verificar**:
   - ✅ Tabla carga sin error 403
   - ✅ Botón "Nuevo Viático" funciona
   - ✅ Modal se abre correctamente
   - ✅ Crear viático funciona (POST)
   - ✅ Editar viático funciona (PUT)

---

## 🎯 Roles Válidos en el Sistema

| Rol | Permisos | Módulos Accesibles |
|-----|----------|-------------------|
| **ADMINISTRACION** | Acceso completo | Todos los módulos incluyendo Viáticos |
| **RECEPCION** | Gestión de clientes, órdenes, cotizaciones | Dashboard Recepción, Órdenes, Viáticos |
| **SOPORTE** | Gestión de reparaciones, evaluaciones | Reparaciones, Evaluaciones, Viáticos |

---

## 📝 Notas Importantes

### Seguridad
- ⚠️ **No** asignar rol "ADMINISTRACION" a usuarios reales sin evaluar permisos
- ✅ Usar roles específicos según funciones del usuario
- ✅ Revisar permisos de cada endpoint antes de producción

### Consistencia
- ✅ Los roles deben coincidir exactamente: "ADMINISTRACION" (no "Administracion" o "Admin")
- ✅ Mantener consistencia entre frontend (sidebar) y backend (controllers)
- ✅ Documentar roles en el código y base de datos

### Testing
- Probar cada rol para verificar acceso correcto
- Verificar que usuarios sin rol NO puedan acceder
- Validar que el JWT incluye el claim de rol

---

## 🔄 Cambios Necesarios en Producción

### Backend
```csharp
// Verificar todos los controllers tienen roles correctos
[Authorize(Roles = "ADMINISTRACION,RECEPCION,SOPORTE")]
```

### Base de Datos
```sql
-- Asegurar que todos los usuarios activos tienen rol válido
UPDATE auth_usuarios 
SET rol = 'RECEPCION' 
WHERE rol IS NULL AND activo = 1;
```

### Frontend
- No requiere cambios (ya está configurado correctamente)

---

## ✅ Checklist de Validación

- [ ] Controlador actualizado con roles múltiples
- [ ] Usuario de prueba tiene rol válido en DB
- [ ] Backend reiniciado
- [ ] GET /api/GastoViaticos devuelve 200 OK
- [ ] POST /api/GastoViaticos devuelve 201 Created
- [ ] Frontend carga la tabla sin errores
- [ ] Modal de crear funciona correctamente
- [ ] Modal de editar funciona correctamente
- [ ] Logs no muestran error 403

---

**Fecha**: Enero 4, 2025  
**Desarrollador**: GitHub Copilot  
**Estado**: ✅ Solucionado - Listo para pruebas
