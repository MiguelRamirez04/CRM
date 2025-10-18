# 🚀 **GUÍA RÁPIDA: API EJECUCIÓN DE ÓRDENES**

## 📋 **ANTES DE COMENZAR**

### **Paso 1: Verificar IDs Válidos en tu Base de Datos**

Ejecuta estas consultas SQL para obtener IDs reales:

```sql
-- 1. Ver órdenes disponibles
SELECT TOP 5 id, nombre_cliente, estado, tipo_orden 
FROM ops_ordenes_trabajo 
WHERE estado IN ('ASIGNADA', 'EN_PROGRESO')
ORDER BY creado_en DESC;

-- 2. Ver técnicos disponibles (ROL SOPORTE)
SELECT id, nombre, apellido, rol, activo 
FROM auth_usuarios 
WHERE rol = 'SOPORTE' AND activo = 1;

-- 3. Ver vehículos disponibles
SELECT id, placas, tipo_vehiculo, activo, es_de_empresa 
FROM fleet_vehiculos 
WHERE activo = 1;
```

### **Paso 2: Actualizar JSONs de Prueba**

Edita los archivos con los IDs obtenidos:

- `test_ejecucion_remoto.json` → Usa `ordenId` y `tecnicoId` reales
- `test_ejecucion_campo.json` → Usa `ordenId`, `tecnicoId` y `vehiculoId` reales

---

## 🔥 **FLUJO COMPLETO DE PRUEBAS**

### **1️⃣ CREAR EJECUCIÓN REMOTO**

**Endpoint:** `POST /api/EjecucionOrden`

**Body:** `test_ejecucion_remoto.json`

```bash
curl -X POST http://localhost:5176/api/EjecucionOrden \
  -H "Authorization: Bearer TU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d @test_ejecucion_remoto.json
```

**Respuesta Esperada: 201 Created**
```json
{
  "id": 1,
  "ordenId": 16,
  "tipoEjecucion": "REMOTO",
  "tecnicoId": 4,
  "tecnicoNombre": "Carlos Méndez",
  "hrInicio": "2025-10-18T08:30:00",
  "hrFin": null,
  "duracionMinutos": null,
  "estadoEjecucion": "EN_CURSO",
  "comentarios": "Sesión remota para actualización de software en equipo del cliente",
  "vehiculoId": null,
  "vehiculoPlacas": null,
  "kmInicial": null,
  "kmFinal": null,
  "kmRecorridos": null,
  "herramientas": "AnyDesk, TeamViewer, Zoom",
  "codigoSesion": "123-456-789",
  "contrasenaSesion": "TempPass2024!"
}
```

---

### **2️⃣ CREAR EJECUCIÓN CAMPO**

**Endpoint:** `POST /api/EjecucionOrden`

**Body:** `test_ejecucion_campo.json`

```bash
curl -X POST http://localhost:5176/api/EjecucionOrden \
  -H "Authorization: Bearer TU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d @test_ejecucion_campo.json
```

**Respuesta Esperada: 201 Created**
```json
{
  "id": 2,
  "ordenId": 16,
  "tipoEjecucion": "CAMPO",
  "tecnicoId": 4,
  "tecnicoNombre": "Carlos Méndez",
  "hrInicio": "2025-10-18T09:00:00",
  "hrFin": null,
  "duracionMinutos": null,
  "estadoEjecucion": "EN_CURSO",
  "comentarios": "Visita a cliente para instalación de equipo nuevo y capacitación al personal",
  "vehiculoId": 1,
  "vehiculoPlacas": "ABC-123",
  "kmInicial": 45320,
  "kmFinal": null,
  "kmRecorridos": null,
  "herramientas": null,
  "codigoSesion": null,
  "contrasenaSesion": null
}
```

---

### **3️⃣ LISTAR TODAS LAS EJECUCIONES**

**Endpoint:** `GET /api/EjecucionOrden`

```bash
curl -X GET http://localhost:5176/api/EjecucionOrden \
  -H "Authorization: Bearer TU_TOKEN_JWT"
```

---

### **4️⃣ OBTENER EJECUCIÓN POR ID**

**Endpoint:** `GET /api/EjecucionOrden/{id}`

```bash
curl -X GET http://localhost:5176/api/EjecucionOrden/1 \
  -H "Authorization: Bearer TU_TOKEN_JWT"
```

---

### **5️⃣ FINALIZAR EJECUCIÓN CAMPO**

**Endpoint:** `PATCH /api/EjecucionOrden/{id}`

**Body:** `test_finalizar_ejecucion_campo.json`

```bash
curl -X PATCH http://localhost:5176/api/EjecucionOrden/2 \
  -H "Authorization: Bearer TU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d @test_finalizar_ejecucion_campo.json
```

**Respuesta Esperada: 204 No Content**

---

### **6️⃣ VERIFICAR CAMPOS CALCULADOS**

**Endpoint:** `GET /api/EjecucionOrden/2`

```bash
curl -X GET http://localhost:5176/api/EjecucionOrden/2 \
  -H "Authorization: Bearer TU_TOKEN_JWT"
```

**Respuesta Esperada:**
```json
{
  "id": 2,
  "ordenId": 16,
  "tipoEjecucion": "CAMPO",
  "tecnicoId": 4,
  "tecnicoNombre": "Carlos Méndez",
  "hrInicio": "2025-10-18T09:00:00",
  "hrFin": "2025-10-18T14:30:00",
  "duracionMinutos": 330,
  "estadoEjecucion": "FINALIZADA",
  "comentarios": "Visita a cliente para instalación de equipo nuevo y capacitación al personal\n[2025-10-18 14:30] Trabajo completado exitosamente. Equipo instalado y configurado. Cliente satisfecho con el servicio.",
  "vehiculoId": 1,
  "vehiculoPlacas": "ABC-123",
  "kmInicial": 45320,
  "kmFinal": 45382,
  "kmRecorridos": 62,
  "herramientas": null,
  "codigoSesion": null,
  "contrasenaSesion": null
}
```

✅ **Nota:** Observa los campos calculados:
- `duracionMinutos`: 330 minutos (5.5 horas)
- `estadoEjecucion`: "FINALIZADA"
- `kmRecorridos`: 62 km
- `comentarios`: Con timestamp automático

---

### **7️⃣ FILTRAR POR ORDEN**

**Endpoint:** `GET /api/EjecucionOrden?ordenId=16`

```bash
curl -X GET "http://localhost:5176/api/EjecucionOrden?ordenId=16" \
  -H "Authorization: Bearer TU_TOKEN_JWT"
```

---

### **8️⃣ FILTRAR POR TÉCNICO**

**Endpoint:** `GET /api/EjecucionOrden?tecnicoId=4`

```bash
curl -X GET "http://localhost:5176/api/EjecucionOrden?tecnicoId=4" \
  -H "Authorization: Bearer TU_TOKEN_JWT"
```

---

### **9️⃣ FILTRAR POR TIPO**

**Endpoint:** `GET /api/EjecucionOrden?tipoEjecucion=CAMPO`

```bash
curl -X GET "http://localhost:5176/api/EjecucionOrden?tipoEjecucion=CAMPO" \
  -H "Authorization: Bearer TU_TOKEN_JWT"
```

---

### **🔟 FILTRAR POR RANGO DE FECHAS**

**Endpoint:** `GET /api/EjecucionOrden?fechaDesde=2025-10-01&fechaHasta=2025-10-31`

```bash
curl -X GET "http://localhost:5176/api/EjecucionOrden?fechaDesde=2025-10-01&fechaHasta=2025-10-31" \
  -H "Authorization: Bearer TU_TOKEN_JWT"
```

---

## ❌ **VALIDACIONES - CASOS DE ERROR**

### **ERROR 1: CAMPO sin VehiculoId**
```json
{
  "ordenId": 16,
  "tipoEjecucion": "CAMPO",
  "tecnicoId": 4,
  "hrInicio": "2025-10-18T09:00:00"
}
```
**Respuesta: 400 Bad Request**
```
Las ejecuciones de tipo CAMPO requieren un vehículo asignado.
```

---

### **ERROR 2: REMOTO con VehiculoId**
```json
{
  "ordenId": 16,
  "tipoEjecucion": "REMOTO",
  "tecnicoId": 4,
  "vehiculoId": 1
}
```
**Respuesta: 400 Bad Request**
```
Las ejecuciones de tipo REMOTO no deben incluir datos de vehículo.
```

---

### **ERROR 3: VehiculoId No Existe**
```json
{
  "ordenId": 16,
  "tipoEjecucion": "CAMPO",
  "tecnicoId": 4,
  "vehiculoId": 99999,
  "kmInicial": 45320
}
```
**Respuesta: 400 Bad Request**
```
El vehículo con ID 99999 no existe.
```

---

### **ERROR 4: Técnico sin Rol SOPORTE**
```json
{
  "ordenId": 16,
  "tipoEjecucion": "CAMPO",
  "tecnicoId": 1,
  "vehiculoId": 1,
  "kmInicial": 45320
}
```
**Respuesta: 400 Bad Request**
```
El usuario Admin Principal no tiene rol SOPORTE.
```

---

### **ERROR 5: KmFinal < KmInicial**
```json
{
  "hrFin": "2025-10-18T14:30:00",
  "kmFinal": 45000
}
```
**Respuesta: 400 Bad Request**
```
El kilometraje final (45000) no puede ser menor que el inicial (45320).
```

---

### **ERROR 6: HrFin < HrInicio**
```json
{
  "hrFin": "2025-10-18T08:00:00"
}
```
**Respuesta: 400 Bad Request**
```
La hora de fin no puede ser anterior a la hora de inicio.
```

---

## 📊 **ENDPOINTS DISPONIBLES**

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/EjecucionOrden` | Crear ejecución | ✅ SOPORTE/ADMIN |
| `GET` | `/api/EjecucionOrden` | Listar con filtros | ✅ SOPORTE/ADMIN |
| `GET` | `/api/EjecucionOrden/{id}` | Obtener por ID | ✅ SOPORTE/ADMIN |
| `PATCH` | `/api/EjecucionOrden/{id}` | Actualizar/Finalizar | ✅ SOLO TÉCNICO ASIGNADO |
| `PATCH` | `/api/EjecucionOrden/{id}/delegate` | Delegar a otro técnico | ✅ SOLO SOPORTE |

---

## 🔐 **AUTORIZACIÓN**

### **Roles Permitidos:**
- `SOPORTE`: Puede crear, leer, actualizar (solo sus ejecuciones) y delegar
- `ADMINISTRACION`: Puede crear y leer (no puede actualizar ni delegar)

### **Obtener Token JWT:**

**Endpoint:** `POST /api/Auth/login`

```bash
curl -X POST http://localhost:5176/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "soporte@empresa.com",
    "password": "tu_password"
  }'
```

---

## ✅ **CHECKLIST DE VALIDACIÓN**

Antes de crear una ejecución, verifica:

- [ ] La orden existe y está en estado válido
- [ ] El técnico existe y tiene rol SOPORTE
- [ ] Si es CAMPO: El vehículo existe y está activo
- [ ] Si es CAMPO: El KmInicial es >= 0
- [ ] Si es REMOTO: No incluyes datos de vehículo
- [ ] Tienes un token JWT válido

---

## 📁 **ARCHIVOS DE PRUEBA INCLUIDOS**

- ✅ `test_ejecucion_remoto.json` - Crear ejecución remota
- ✅ `test_ejecucion_campo.json` - Crear ejecución de campo
- ✅ `test_finalizar_ejecucion_campo.json` - Finalizar con kilometraje
- ✅ `test_finalizar_ejecucion_remoto.json` - Finalizar sesión remota
- ✅ `Pruebas_Gnrales/ejecucion_orden_pruebas.http` - Archivo HTTP completo

---

## 🎯 **PRÓXIMOS PASOS**

1. ✅ **API funcionando correctamente**
2. ⚠️ Crear índices en base de datos
3. ⚠️ Agregar tests unitarios
4. ⚠️ Configurar monitoreo y alertas
5. ⚠️ Documentar en Swagger

---

## 📞 **SOPORTE**

Si encuentras algún error:

1. Verifica los logs en `logs/` folder
2. Revisa que los IDs sean válidos en tu BD
3. Confirma que el token JWT no haya expirado
4. Consulta `CORRECCION_COMPLETA_EJECUCION_ORDEN.md` para detalles técnicos

---

**✅ API LISTA PARA USO EN PRODUCCIÓN**

Última actualización: 18 de Octubre, 2025
