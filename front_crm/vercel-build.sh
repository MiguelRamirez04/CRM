#!/bin/bash

set -e

echo "🚀 Iniciando build para Vercel..."

# Variables de entorno con valores por defecto
API_URL="${VITE_API_URL:-https://back-crm-api.onrender.com}"
ALLOWED_ORIGINS="${VITE_ALLOWED_ORIGINS:-https://crm-nine-tau-43.vercel.app}"

echo "📝 Variables de entorno:"
echo "  API_URL=$API_URL"
echo "  ALLOWED_ORIGINS=$ALLOWED_ORIGINS"

# Modificar environment.prod.ts con los valores de las variables de entorno
sed -i "s|apiUrl: '.*'|apiUrl: '$API_URL'|" src/environments/environment.prod.ts

# Reemplazar allowedOrigins
sed -i "s|allowedOrigins: \[.*\]|allowedOrigins: ['$ALLOWED_ORIGINS']|" src/environments/environment.prod.ts

echo "📦 Instalando dependencias..."
npm ci

echo "🔨 Construyendo aplicación..."
npm run build

echo "✅ Build completado!"
