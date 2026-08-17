# Guía de Deploy - FullStack CRM

## Arquitectura

- **Frontend**: Angular 20 → Vercel
- **Backend**: ASP.NET Core 8.0 (Docker) → Render
- **Base de Datos**: PostgreSQL → Supabase

---

## 1. Configuración de Supabase

1. Ve a https://supabase.com/dashboard
2. Crea un nuevo proyecto (o usa el existente)
3. Ve a **Settings** → **Database** → **Connection string**
4. Copia la connection string de PostgreSQL (usar modo **Transaction** o **Session** pooler)
5. Ejecuta los scripts SQL en el SQL Editor:
   - `database/CRM_Demo_001_create.sql`
   - `database/CRM_Demo_002_seed.sql` (opcional, para datos demo)

**Connection String de ejemplo:**
```
postgresql://postgres:TU_PASSWORD@aws-0-us-east-1.pooler.supabase.com:5432/postgres?sslmode=require
```

---

## 2. Deploy Backend (Render)

### Opción A: Usando Docker (Recomendado)

1. Ve a https://render.com y crea una cuenta
2. Click en **New** → **Web Service**
3. Conecta tu repositorio de GitHub/GitLab
4. Configuración:
   - **Name**: `back-crm`
   - **Environment**: `Docker`
   - **Dockerfile Path**: `back_crm/dockerfile`
   - **Docker Build Context Directory**: `back_crm`
   - **Branch**: `main` (o tu rama)
   - **Plan**: Free (o Starter para producción)

### Variables de Entorno en Render

Ve a **Environment** y agrega:

```bash
# Conexión a Supabase (Requerido)
ConnectionStrings__DefaultConnection=postgresql://postgres:TU_PASSWORD@aws-0-us-east-1.pooler.supabase.com:5432/postgres?sslmode=require

# JWT (Requerido - CAMBIAR en producción!)
JwtSettings__SecretKey=GENERA_UNA_CLAVE_SEGURA_DE_AL_MENOS_32_CARACTERES
JwtSettings__Issuer=CRM-API
JwtSettings__Audience=CRM-Client
JwtSettings__ExpiryInMinutes=60

# CORS (Requerido - Agregar tu dominio de Vercel)
CorsSettings__AllowedOrigins__0=https://tu-frontend-vercel.app

# Opcional: Email SMTP
SmtpSettings__Server=smtp.gmail.com
SmtpSettings__Port=587
SmtpSettings__SenderEmail=tu-email@gmail.com
SmtpSettings__Username=tu-email@gmail.com
SmtpSettings__Password=tu-contraseña-de-aplicacion

# Opcional: Redis (si usas addon en Render)
# ConnectionStrings__RedisConnection=redis://default:password@host:port
```

### Configuración Adicional

- **Health Check Path**: `/health`
- **Auto-Deploy**: Yes (para despliegue automático en cada push)

---

## 3. Deploy Frontend (Vercel)

### Opción A: Usando Vercel CLI

1. Instala Vercel CLI:
   ```bash
   npm i -g vercel
   ```

2. Ve a la carpeta del frontend:
   ```bash
   cd front_crm
   ```

3. Configura variables de entorno:
   ```bash
   vercel env add VITE_API_URL production
   # Ingresa: https://tu-backend-render.onrender.com/api
   
   vercel env add VITE_ALLOWED_ORIGINS production
   # Ingresa: https://tu-frontend-vercel.app
   ```

4. Deploy:
   ```bash
   vercel --prod
   ```

### Opción B: Usando GitHub Integration

1. Ve a https://vercel.com y crea una cuenta
2. Click en **Add New Project**
3. Importa tu repositorio
4. Configuración:
   - **Framework Preset**: Angular
   - **Root Directory**: `front_crm`
   - **Build Command**: `npm run build`
   - **Output Directory**: `dist/frontend`

5. Variables de Entorno:
   - `VITE_API_URL` = `https://tu-backend-render.onrender.com/api`
   - `VITE_ALLOWED_ORIGINS` = `https://tu-frontend-vercel.app`

---

## 4. Post-Deploy

### Verificar Backend

```bash
# Health check
curl https://tu-backend-render.onrender.com/health

# Swagger (si está habilitado)
https://tu-backend-render.onrender.com/swagger
```

### Verificar Frontend

1. Abre tu dominio de Vercel
2. Prueba login con credenciales de prueba
3. Verifica que las llamadas a la API funcionen

---

## 5. Credenciales de Prueba (si ejecutaste seed data)

- **Email**: `admin@crm.com`
- **Password**: `Admin123!`

---

## 6. Troubleshooting

### Backend no inicia
- Revisa los logs en Render
- Verifica que las variables de entorno estén correctas
- Asegúrate de que la connection string de Supabase sea correcta

### CORS Error
- Verifica que el dominio de Vercel esté en `CorsSettings__AllowedOrigins`
- Asegúrate de que el backend use HTTPS

### Frontend no se conecta al backend
- Verifica `VITE_API_URL` en Vercel
- Asegúrate de que el backend esté corriendo
- Revisa la consola del navegador para errores

---

## 7. Notas Importantes

- **JWT Secret**: Genera una clave segura para producción (mínimo 32 caracteres)
- **HTTPS**: Ambos servicios usan HTTPS automáticamente (Render y Vercel)
- **SSL Mode**: Supabase requiere `SSL Mode=Require` o `sslmode=require`
- **Redis**: Es opcional. Si no lo configuras, el backend funcionará sin caché
