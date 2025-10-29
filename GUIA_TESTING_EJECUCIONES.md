# 🧪 Guía de Testing - Creación de Ejecuciones

## 🎯 Objetivo

Verificar que el flujo de creación de ejecuciones funciona correctamente después de las mejoras implementadas.

---

## 📋 Pre-requisitos

### **Backend:**
- ✅ Servidor corriendo en `http://localhost:5176`
- ✅ Base de datos con datos de prueba:
  - Orden ID: 16 (activa)
  - Técnico ID: 7 (rol SOPORTE, activo)
  - Vehículo ID: 1 (activo)

### **Frontend:**
- ✅ Servidor corriendo en `http://localhost:4200`
- ✅ Usuario con rol RECEPCION logueado

---

## 🧪 Casos de Prueba

### **Test 1: Flujo Correcto desde Órdenes de Trabajo**

**Objetivo:** Verificar que se puede crear una ejecución desde la página de órdenes.

**Pasos:**
1. Navegar a `http://localhost:4200/recepcion/ordenes-trabajo`
2. Localizar una orden en la lista (ejemplo: Orden #16)
3. Click en el botón "🚀 Crear Ejecución" de esa orden
4. **Verificar:**
   - ✅ Navegación a `/recepcion/ordenes-trabajo/ejecuciones`
   - ✅ Banner azul muestra: "Orden Pre-seleccionada: [Cliente] (Orden #16)"
   - ✅ Modal se abre automáticamente
   - ✅ Campo "Técnico Asignado" visible
   - ✅ Campo "Hora de Inicio" con fecha/hora actual
   - ✅ Tipo de ejecución por defecto: CAMPO

5. Seleccionar "Técnico Asignado" (ejemplo: Carlos González)
6. Completar campos para CAMPO:
   - Vehículo: Seleccionar uno
   - Km Inicial: 45000
7. Click "✓ Crear Ejecución"

**Resultado Esperado:**
- ✅ Consola muestra: `📤 Intentando crear ejecución con DTO:`
- ✅ DTO incluye `ordenId: 16`
- ✅ Sin mensajes de error
- ✅ Consola muestra: `✅ Ejecución creada exitosamente`
- ✅ Modal se cierra
- ✅ Nueva ejecución aparece en la lista

**Verificación en Backend:**
```
[INF] POST /api/EjecucionOrden
[INF] Ejecución creada exitosamente con ID {Id}
```

---

### **Test 2: Validación de Campo Técnico**

**Objetivo:** Verificar que no se puede crear sin seleccionar técnico.

**Pasos:**
1. Abrir modal de crear ejecución (desde orden)
2. NO seleccionar técnico (dejar en "-- Seleccione un técnico --")
3. Completar otros campos obligatorios
4. Click "✓ Crear Ejecución"

**Resultado Esperado:**
- ✅ Mensaje de error aparece: "Debe seleccionar un técnico"
- ✅ Mensaje tiene fondo rojo claro
- ✅ Animación "shake" al aparecer
- ✅ Consola muestra: `⚠️ Validación fallida: Debe seleccionar un técnico`
- ✅ NO se envía request al backend
- ✅ Modal permanece abierto

---

### **Test 3: Validación de Campos CAMPO**

**Objetivo:** Verificar validaciones específicas para ejecución tipo CAMPO.

**Pasos:**
1. Abrir modal de crear ejecución (desde orden)
2. Tipo: CAMPO (default)
3. Seleccionar técnico
4. NO seleccionar vehículo
5. NO ingresar Km Inicial
6. Click "✓ Crear Ejecución"

**Resultado Esperado:**
- ✅ Mensaje: "Vehículo y Km Inicial son obligatorios para ejecución en campo"
- ✅ Animación "shake"
- ✅ NO se envía request

**Paso Adicional - Kilometraje Negativo:**
1. Seleccionar vehículo
2. Ingresar Km Inicial: -100
3. Click "✓ Crear Ejecución"

**Resultado Esperado:**
- ✅ Mensaje: "El kilometraje inicial debe ser mayor o igual a 0"

---

### **Test 4: Ejecución Tipo REMOTO**

**Objetivo:** Verificar creación de ejecución REMOTO.

**Pasos:**
1. Abrir modal de crear ejecución (desde orden)
2. Click en "💻 Remoto"
3. **Verificar:**
   - ✅ Botón "💻 Remoto" tiene clase `active`
   - ✅ Campos de vehículo/km desaparecen
   - ✅ Aparecen campos: Herramientas, Código Sesión, Contraseña Sesión
4. Seleccionar técnico
5. Llenar campos opcionales de REMOTO:
   - Herramientas: "TeamViewer"
   - Código Sesión: "123456789"
   - Contraseña Sesión: "abc123"
6. Click "✓ Crear Ejecución"

**Resultado Esperado:**
- ✅ Consola muestra DTO con `tipoEjecucion: "REMOTO"`
- ✅ DTO incluye `herramientas`, `codigoSesion`, `contrasenaSesion`
- ✅ Ejecución creada exitosamente
- ✅ Card de ejecución muestra icono 💻

---

### **Test 5: Campo Hora de Inicio**

**Objetivo:** Verificar que se puede modificar la hora de inicio.

**Pasos:**
1. Abrir modal de crear ejecución
2. **Verificar:** Input "Hora de Inicio" tiene valor por defecto (fecha/hora actual)
3. Modificar la hora (ejemplo: 2 horas antes)
4. Completar formulario
5. Click "✓ Crear Ejecución"

**Resultado Esperado:**
- ✅ DTO en consola muestra `hrInicio` con la hora modificada (formato ISO)
- ✅ Ejecución creada con la hora especificada
- ✅ Card de ejecución muestra hora correcta

---

### **Test 6: Botones Deshabilitados Durante Carga**

**Objetivo:** Verificar que no se puede hacer doble envío.

**Pasos:**
1. Abrir modal de crear ejecución
2. Completar formulario correctamente
3. Click "✓ Crear Ejecución"
4. **INMEDIATAMENTE** intentar hacer click de nuevo

**Resultado Esperado:**
- ✅ Después del primer click, botón cambia a "⏳ Creando..."
- ✅ Botón se deshabilita (no hace nada al hacer click)
- ✅ Botón "Cancelar" también se deshabilita
- ✅ Solo se envía 1 request al backend
- ✅ Después de respuesta, botones vuelven a habilitarse (o modal cierra)

---

### **Test 7: Manejo de Error HTTP 404**

**Objetivo:** Verificar mensaje de error cuando recurso no existe.

**Setup:** Modificar temporalmente el código para usar IDs inexistentes.

**Opción A - Modificar en Navegador (DevTools):**
```javascript
// En consola del navegador:
const component = ng.getComponent(document.querySelector('app-ejecuciones-orden'));
component.formularioCrear.update(f => ({ ...f, ordenId: 99999 }));
```

**Opción B - Backend Testing (usar archivo .http):**
```http
POST http://localhost:5176/api/EjecucionOrden
{
  "ordenId": 99999,
  "tecnicoId": 7,
  "tipoEjecucion": "CAMPO",
  "vehiculoId": 1,
  "kmInicial": 45000
}
```

**Resultado Esperado:**
- ✅ Backend retorna 404 Not Found
- ✅ Frontend muestra: "Recurso no encontrado. Verifique que la orden, técnico o vehículo existan."
- ✅ Consola muestra: `❌ Error al crear ejecución:` con status 404
- ✅ Modal permanece abierto
- ✅ Botones vuelven a habilitarse

---

### **Test 8: Intentar Crear sin Contexto de Orden**

**Objetivo:** Verificar validación cuando falta ordenId.

**Pasos:**
1. Navegar DIRECTAMENTE a `http://localhost:4200/recepcion/ordenes-trabajo/ejecuciones`
   (sin pasar por página de órdenes)
2. Click "➕ Nueva Ejecución"
3. **Verificar:** Banner azul NO aparece (no hay contexto)
4. Seleccionar técnico y otros campos
5. Click "✓ Crear Ejecución"

**Resultado Esperado:**
- ✅ Mensaje de error: "No se ha especificado una orden de trabajo. Por favor, cree la ejecución desde la página de órdenes."
- ✅ Consola muestra: `⚠️ Validación fallida`
- ✅ NO se envía request al backend
- ✅ Usuario debe cancelar y usar flujo correcto

---

### **Test 9: Cargar Técnicos desde Backend**

**Objetivo:** Verificar que técnicos se cargan de API real (no mock).

**Pasos:**
1. Abrir modal de crear ejecución
2. Abrir DevTools → Network tab
3. **Verificar request:**
   - URL: `http://localhost:5176/api/auth/tecnicos`
   - Method: GET
   - Status: 200 OK

4. Click en el select "Técnico Asignado"
5. **Verificar:**
   - ✅ Lista de técnicos se llena
   - ✅ Nombres completos visibles (no IDs)
   - ✅ Solo técnicos activos con rol SOPORTE o ADMINISTRACION

6. En consola del navegador verificar:
```
👷 Técnicos disponibles desde backend: [número]
```

---

### **Test 10: Comentarios Opcionales**

**Objetivo:** Verificar que se pueden agregar comentarios.

**Pasos:**
1. Abrir modal de crear ejecución
2. Completar campos obligatorios
3. En campo "Comentarios" escribir:
   ```
   Reparación programada del sistema de frenos.
   Verificar pastillas y discos.
   ```
4. Click "✓ Crear Ejecución"

**Resultado Esperado:**
- ✅ DTO en consola incluye `comentarios` con el texto ingresado
- ✅ Ejecución creada exitosamente
- ✅ Card de ejecución muestra icono 💬 y el texto de comentarios

---

## 🔍 Verificaciones en Backend

### **Request Logs Esperados:**

```
[INF] Request starting HTTP/1.1 POST http://localhost:5176/api/EjecucionOrden
[INF] Request: POST /api/EjecucionOrden from ::1
[INF] CORS policy execution successful
[INF] JWT Token validated successfully for user: [usuario]
[INF] Executing endpoint 'EjecucionOrdenController.CreateEjecucion'
[INF] Ejecución creada exitosamente con ID [X]
[INF] Response: 201 for POST /api/EjecucionOrden
```

### **Database Verification:**

```sql
-- Verificar que la ejecución se insertó
SELECT TOP 1 * FROM EjecucionOrden 
ORDER BY Id DESC;

-- Verificar campos
SELECT 
  Id, OrdenId, TecnicoId, TipoEjecucion, 
  VehiculoId, KmInicial, HrInicio, Comentarios
FROM EjecucionOrden 
WHERE Id = [ID_CREADO];
```

---

## 📊 Checklist de Testing

### **Funcionalidad:**
- [ ] Test 1: Crear ejecución CAMPO desde órdenes ✅
- [ ] Test 2: Validación técnico requerido ✅
- [ ] Test 3: Validación campos CAMPO ✅
- [ ] Test 4: Crear ejecución REMOTO ✅
- [ ] Test 5: Modificar hora de inicio ✅
- [ ] Test 6: Botones deshabilitados durante carga ✅
- [ ] Test 7: Error HTTP 404 ✅
- [ ] Test 8: Validación sin ordenId ✅
- [ ] Test 9: Técnicos desde backend ✅
- [ ] Test 10: Comentarios opcionales ✅

### **UI/UX:**
- [ ] Banner azul aparece con contexto de orden
- [ ] Mensaje de error tiene animación "shake"
- [ ] Botones deshabilitados durante carga
- [ ] Texto del botón cambia a "⏳ Creando..."
- [ ] Modal se cierra después de éxito
- [ ] Lista se recarga automáticamente

### **Backend:**
- [ ] Endpoint `/api/EjecucionOrden` (POST) responde 201
- [ ] Endpoint `/api/auth/tecnicos` (GET) responde 200
- [ ] Logs muestran creación exitosa
- [ ] Registro insertado en base de datos

---

## 🐛 Troubleshooting

### **Problema:** "No se ha especificado una orden de trabajo"

**Solución:**  
No intentar crear ejecuciones directamente desde la página de ejecuciones. Usar el botón "🚀 Crear Ejecución" desde la página de órdenes.

---

### **Problema:** Lista de técnicos vacía

**Verificar:**
1. Backend corriendo en `http://localhost:5176`
2. Endpoint `/api/auth/tecnicos` accesible
3. Existen usuarios con rol SOPORTE o ADMINISTRACION en BD
4. Usuarios están activos (`Activo = 1`)

**Consola del navegador:**
```javascript
// Verificar request
fetch('http://localhost:5176/api/auth/tecnicos', {
  headers: { 'Authorization': 'Bearer [TOKEN]' }
}).then(r => r.json()).then(console.log);
```

---

### **Problema:** Error CORS

**Solución:**  
Verificar configuración de CORS en `Program.cs`:
```csharp
builder.Services.AddCors(options => {
  options.AddDefaultPolicy(policy => {
    policy.WithOrigins("http://localhost:4200")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
  });
});
```

---

### **Problema:** Token inválido/expirado

**Solución:**
1. Hacer logout
2. Hacer login nuevamente
3. Verificar que cookie `AuthToken` existe en DevTools → Application → Cookies

---

## ✅ Criterios de Aceptación

### **PASA si:**
- ✅ Se puede crear ejecución CAMPO desde órdenes
- ✅ Se puede crear ejecución REMOTO desde órdenes
- ✅ Validaciones muestran mensajes claros
- ✅ Técnicos se cargan desde backend
- ✅ Errores HTTP se manejan correctamente
- ✅ Botones se deshabilitan durante carga
- ✅ Modal se cierra después de éxito
- ✅ Lista se recarga automáticamente

### **FALLA si:**
- ❌ Se puede crear ejecución sin ordenId
- ❌ Lista de técnicos está vacía (sin error de red)
- ❌ Doble click crea 2 ejecuciones
- ❌ Errores HTTP no muestran mensaje
- ❌ Modal no se cierra después de éxito
- ❌ Validaciones no funcionan

---

## 📌 Notas Finales

1. **Usar siempre el flujo correcto:** Orden → Crear Ejecución
2. **Verificar logs en consola:** Deben aparecer emojis (📤 ⚠️ ✅ ❌)
3. **Revisar Network tab:** Para debugging de requests HTTP
4. **Probar ambos tipos:** CAMPO y REMOTO
5. **Validar en base de datos:** Verificar que datos se insertaron correctamente

---

**✅ Testing completado exitosamente cuando todos los tests pasan sin errores.**
