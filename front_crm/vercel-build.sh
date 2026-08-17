#!/bin/bash

# Script de deploy para Vercel
# Inyecta variables de entorno en el build de Angular

set -e

echo "🚀 Iniciando build para Vercel..."

# Variables de entorno con valores por defecto
export NG_APP_API_URL="${VITE_API_URL:-https://tu-backend-render.onrender.com/api}"
export NG_APP_ALLOWED_ORIGINS="${VITE_ALLOWED_ORIGINS:-https://tu-frontend-vercel.app}"

echo "📝 Variables de entorno:"
echo "  NG_APP_API_URL=$NG_APP_API_URL"
echo "  NG_APP_ALLOWED_ORIGINS=$NG_APP_ALLOWED_ORIGINS"

# Instalar dependencias
echo "📦 Instalando dependencias..."
npm ci

# Build de producción
echo "🔨 Construyendo aplicación..."
npm run build

echo "✅ Build completado!"
echo ""
echo "Para desplegar en Vercel:"
echo "1. Asegúrate de tener las variables de entorno configuradas en Vercel:"
echo "   - VITE_API_URL"
echo "   - VITE_ALLOWED_ORIGINS"
echo "2. Ejecuta: vercel --prod"
