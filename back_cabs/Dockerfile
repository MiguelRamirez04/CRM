# --- Etapa 1: Build (Compilación) ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia archivos de proyecto y solución
COPY back_cabs.sln .
COPY back_cabs.csproj .

# 2. Restaura las dependencia
RUN dotnet restore "back_cabs.sln"
# 3. Copia TODO el código fuente
COPY . .

# 4. Limpia archivos auto-generados previos que causan conflictos
RUN rm -rf obj/Release bin/Release

# 5. Publica la aplicación
RUN dotnet publish "back_cabs.csproj" -c Release -o /app/publish /p:UseAppHost=false

# --- Etapa 2: Final (Ejecución) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

# 6. Entrypoint (CORREGIDO)
# El DLL se llama como tu proyecto: 'back_cabs.dll'
ENTRYPOINT ["dotnet", "back_cabs.dll"]


#Resumen de los Cambios Clave:
#COPY back_cabs.csproj .: Copiamos el .csproj a la raíz /src, no a la subcarpeta crm.
#RUN dotnet publish ...: Lo ejecutamos desde /src, donde realmente vive el .csproj. Eliminamos el WORKDIR /src/crm que era incorrecto.
#ENTRYPOINT ["dotnet", "back_cabs.dll"]: Tu DLL final se llama back_cabs.dll porque tu proyecto se llama back_cabs.csproj.