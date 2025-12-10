## Opción B: Despliegue con Docker

Docker permite desplegar la aplicación de manera consistente y portable. Esta sección cubre el despliegue con Docker en Windows Server.

### Requisitos Previos para Docker

- ✅ **Docker Desktop for Windows** o **Docker Engine** (Windows Server 2019+)
- ✅ **Docker Compose** (incluido con Docker Desktop)
- ✅ **SQL Server** (puede ser contenedor o instalación nativa)
- ✅ **Redis** (recomendado como contenedor)

### 1. Crear Dockerfile para Backend

Crear `Dockerfile` en la raíz del proyecto backend (`back_cabs/`):

```dockerfile
# Dockerfile para ASP.NET Core Backend
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["back_cabs.csproj", "./"]
RUN dotnet restore "back_cabs.csproj"
COPY . .
RUN dotnet build "back_cabs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "back_cabs.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear directorio para logs
RUN mkdir -p /app/logs

# Configurar variable de entorno
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "back_cabs.dll"]
```

### 2. Crear Dockerfile para Frontend

Crear `Dockerfile` en la raíz del proyecto frontend (`front_cabs/`):

```dockerfile
# Dockerfile para Angular Frontend
# Etapa 1: Build
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build -- --configuration=production

# Etapa 2: Servir con nginx
FROM nginx:alpine
COPY --from=build /app/dist/front_cabs/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### 3. Crear Configuración de Nginx

Crear `nginx.conf` en la raíz del proyecto frontend:

```nginx
events {
    worker_connections 1024;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    # Configuración de logging
    access_log /var/log/nginx/access.log;
    error_log /var/log/nginx/error.log;

    # Configuración de compresión
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript 
               application/x-javascript application/xml+rss 
               application/json application/javascript;

    server {
        listen 80;
        server_name localhost;
        root /usr/share/nginx/html;
        index index.html;

        # Security headers
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-XSS-Protection "1; mode=block" always;
        add_header Referrer-Policy "strict-origin-when-cross-origin" always;

        # Angular routing
        location / {
            try_files $uri $uri/ /index.html;
        }

        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }

        # Disable caching for index.html
        location = /index.html {
            add_header Cache-Control "no-store, no-cache, must-revalidate";
        }
    }
}
```

### 4. Crear docker-compose.yml

Crear `docker-compose.yml` en la raíz del proyecto:

```yaml
version: '3.8'

services:
  # Backend API
  backend:
    build:
      context: ./back_cabs
      dockerfile: Dockerfile
    container_name: cabs-backend
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=CABS_Produccion;User Id=sa;Password=${SQL_PASSWORD};MultipleActiveResultSets=true;TrustServerCertificate=True;
      - ConnectionStrings__RedisConnection=redis:6379,abortConnect=false,connectTimeout=10000,syncTimeout=10000
      - JwtSettings__SecretKey=${JWT_SECRET_KEY}
      - JwtSettings__Issuer=CABS-API-PROD
      - JwtSettings__Audience=CABS-Client-PROD
      - JwtSettings__ExpiryInMinutes=30
    depends_on:
      - sqlserver
      - redis
    networks:
      - cabs-network
    restart: unless-stopped
    volumes:
      - ./logs:/app/logs
      - ./uploads:/app/uploads

  # Frontend Angular
  frontend:
    build:
      context: ./front_cabs
      dockerfile: Dockerfile
    container_name: cabs-frontend
    ports:
      - "80:80"
      - "443:443"
    depends_on:
      - backend
    networks:
      - cabs-network
    restart: unless-stopped

  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: cabs-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SQL_PASSWORD}
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    networks:
      - cabs-network
    volumes:
      - sqlserver-data:/var/opt/mssql
    restart: unless-stopped

  # Redis Cache
  redis:
    image: redis:7-alpine
    container_name: cabs-redis
    ports:
      - "6379:6379"
    networks:
      - cabs-network
    volumes:
      - redis-data:/data
    restart: unless-stopped
    command: redis-server --appendonly yes

networks:
  cabs-network:
    driver: bridge

volumes:
  sqlserver-data:
  redis-data:
```

### 5. Crear Archivo .env

Crear `.env` en la raíz del proyecto (⚠️ **NO SUBIR A GIT**):

```env
# SQL Server
SQL_PASSWORD=TuPasswordSuperSeguro123!

# JWT
JWT_SECRET_KEY=GENERA_UNA_CLAVE_SUPER_SEGURA_DE_AL_MENOS_64_CARACTERES_ALEATORIOS_AQUI!

# SMTP (opcional)
SMTP_USERNAME=tu_usuario_smtp
SMTP_PASSWORD=tu_password_smtp
```

### 6. Crear .dockerignore

Crear `.dockerignore` en ambos proyectos:

**Backend (.dockerignore):**
```
bin/
obj/
.vs/
*.user
*.suo
logs/
uploads/
appsettings.Development.json
```

**Frontend (.dockerignore):**
```
node_modules/
dist/
.angular/
.vscode/
*.log
```

### 7. Construir y Ejecutar con Docker Compose

```powershell
# Navegar a la raíz del proyecto
cd c:\Users\ANA\Documents\dev\FullStack_CABS

# Construir las imágenes
docker-compose build

# Iniciar todos los servicios
docker-compose up -d

# Ver logs
docker-compose logs -f

# Ver estado de los contenedores
docker-compose ps
```

### 8. Ejecutar Migraciones en Contenedor

```powershell
# Esperar a que SQL Server esté listo
Start-Sleep -Seconds 30

# Ejecutar migraciones desde el contenedor backend
docker-compose exec backend dotnet ef database update

# O desde tu máquina local apuntando al contenedor
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=CABS_Produccion;User Id=sa;Password=TuPasswordSuperSeguro123!;MultipleActiveResultSets=true;TrustServerCertificate=True;"
dotnet ef database update --project ./back_cabs
```

### 9. Configurar HTTPS con Docker

#### Opción A: Usar Reverse Proxy (Nginx/Traefik)

Crear `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  nginx-proxy:
    image: nginx:alpine
    container_name: cabs-nginx-proxy
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx-proxy.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - frontend
      - backend
    networks:
      - cabs-network
    restart: unless-stopped
```

**nginx-proxy.conf:**
```nginx
events {
    worker_connections 1024;
}

http {
    upstream backend {
        server backend:80;
    }

    upstream frontend {
        server frontend:80;
    }

    # Redirect HTTP to HTTPS
    server {
        listen 80;
        server_name tudominio.com api.tudominio.com;
        return 301 https://$server_name$request_uri;
    }

    # Frontend HTTPS
    server {
        listen 443 ssl http2;
        server_name tudominio.com;

        ssl_certificate /etc/nginx/ssl/cert.pem;
        ssl_certificate_key /etc/nginx/ssl/key.pem;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;

        location / {
            proxy_pass http://frontend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }

    # Backend API HTTPS
    server {
        listen 443 ssl http2;
        server_name api.tudominio.com;

        ssl_certificate /etc/nginx/ssl/cert.pem;
        ssl_certificate_key /etc/nginx/ssl/key.pem;
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;

        location / {
            proxy_pass http://backend;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }
}
```

### 10. Comandos Útiles de Docker

```powershell
# Ver logs de un servicio específico
docker-compose logs -f backend
docker-compose logs -f frontend

# Reiniciar un servicio
docker-compose restart backend

# Detener todos los servicios
docker-compose down

# Detener y eliminar volúmenes (⚠️ CUIDADO: Borra datos)
docker-compose down -v

# Reconstruir un servicio específico
docker-compose build --no-cache backend
docker-compose up -d backend

# Ejecutar comando en contenedor
docker-compose exec backend bash
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'TuPassword'

# Ver uso de recursos
docker stats

# Limpiar imágenes no usadas
docker system prune -a
```

### 11. Monitoreo y Logs

```powershell
# Ver logs en tiempo real de todos los servicios
docker-compose logs -f

# Ver logs de las últimas 100 líneas
docker-compose logs --tail=100

# Exportar logs a archivo
docker-compose logs > cabs-logs.txt

# Ver métricas de contenedores
docker stats cabs-backend cabs-frontend cabs-sqlserver cabs-redis
```

### 12. Backup y Restauración con Docker

```powershell
# Backup de base de datos SQL Server
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd `
    -S localhost -U sa -P 'TuPassword' `
    -Q "BACKUP DATABASE [CABS_Produccion] TO DISK = N'/var/opt/mssql/backup/CABS_Produccion.bak' WITH FORMAT, INIT, COMPRESSION"

# Copiar backup al host
docker cp cabs-sqlserver:/var/opt/mssql/backup/CABS_Produccion.bak ./backups/

# Restaurar desde backup
docker cp ./backups/CABS_Produccion.bak cabs-sqlserver:/var/opt/mssql/backup/
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd `
    -S localhost -U sa -P 'TuPassword' `
    -Q "RESTORE DATABASE [CABS_Produccion] FROM DISK = N'/var/opt/mssql/backup/CABS_Produccion.bak' WITH REPLACE"
```

### 13. Consideraciones de Producción con Docker

> [!IMPORTANT]
> **Mejores Prácticas Docker en Producción:**

1. ✅ **Usar Docker Secrets** para contraseñas en lugar de variables de entorno
2. ✅ **Limitar recursos** de contenedores (CPU, memoria)
3. ✅ **Configurar health checks** en docker-compose
4. ✅ **Usar volúmenes nombrados** para persistencia de datos
5. ✅ **Implementar logging centralizado** (ELK Stack, Seq, etc.)
6. ✅ **Configurar restart policies** apropiadas
7. ✅ **Usar redes personalizadas** para aislamiento
8. ✅ **Actualizar imágenes base** regularmente

**Ejemplo con Health Checks:**

```yaml
services:
  backend:
    # ... otras configuraciones
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    deploy:
      resources:
        limits:
          cpus: '2.0'
          memory: 2G
        reservations:
          cpus: '0.5'
          memory: 512M
```

### 14. Actualización de Aplicación

```powershell
# 1. Hacer pull de cambios
git pull origin main

# 2. Reconstruir imágenes
docker-compose build

# 3. Detener servicios actuales
docker-compose down

# 4. Iniciar con nuevas imágenes
docker-compose up -d

# 5. Verificar que todo funciona
docker-compose ps
docker-compose logs -f
```

### 15. Troubleshooting Docker

#### Contenedor no inicia

```powershell
# Ver logs detallados
docker-compose logs backend

# Verificar configuración
docker-compose config

# Ejecutar en modo interactivo
docker-compose run --rm backend bash
```

#### Problemas de red

```powershell
# Inspeccionar red
docker network inspect fullstack_cabs_cabs-network

# Recrear red
docker-compose down
docker network prune
docker-compose up -d
```

#### Problemas de volúmenes

```powershell
# Listar volúmenes
docker volume ls

# Inspeccionar volumen
docker volume inspect fullstack_cabs_sqlserver-data

# Limpiar volúmenes no usados
docker volume prune
```

---
