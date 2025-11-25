# ============================================
# BACKUP DE ARCHIVOS FÍSICOS - Sistema de Fotos
# Fecha: 2025-10-18
# Propósito: Respaldar directorio CRM/uploads antes de refactorización
# ============================================

# Configuración
$backupDate = Get-Date -Format "yyyyMMdd_HHmmss"
$sourceDir = "C:\Users\adria\source\repos\fullstack_cabs\back_cabs\CRM\uploads"
$backupRootDir = "C:\Users\adria\source\repos\fullstack_cabs\back_cabs\CRM\backups"
$backupDir = Join-Path $backupRootDir "uploads_backup_$backupDate"

# Colores para output
$colorSuccess = "Green"
$colorWarning = "Yellow"
$colorError = "Red"
$colorInfo = "Cyan"

# ============================================
# FUNCIÓN: Mostrar Header
# ============================================
function Show-Header {
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor $colorInfo
    Write-Host "║  BACKUP DE ARCHIVOS FÍSICOS - SISTEMA DE FOTOS           ║" -ForegroundColor $colorInfo
    Write-Host "║  Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')                      ║" -ForegroundColor $colorInfo
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor $colorInfo
    Write-Host ""
}

# ============================================
# FUNCIÓN: Validar Directorio Origen
# ============================================
function Test-SourceDirectory {
    if (-not (Test-Path $sourceDir)) {
        Write-Host "❌ ERROR: Directorio origen no encontrado" -ForegroundColor $colorError
        Write-Host "   Ruta: $sourceDir" -ForegroundColor $colorError
        Write-Host ""
        Write-Host "   Creando directorio..." -ForegroundColor $colorWarning
        New-Item -ItemType Directory -Path $sourceDir -Force | Out-Null
        Write-Host "✅ Directorio creado (vacío)" -ForegroundColor $colorSuccess
        return $false
    }
    return $true
}

# ============================================
# FUNCIÓN: Crear Directorio de Backup
# ============================================
function New-BackupDirectory {
    if (-not (Test-Path $backupRootDir)) {
        Write-Host "📁 Creando directorio raíz de backups..." -ForegroundColor $colorInfo
        New-Item -ItemType Directory -Path $backupRootDir -Force | Out-Null
        Write-Host "✅ Directorio raíz creado: $backupRootDir" -ForegroundColor $colorSuccess
    }
    
    Write-Host "📁 Creando directorio de backup..." -ForegroundColor $colorInfo
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    Write-Host "✅ Directorio de backup creado" -ForegroundColor $colorSuccess
    Write-Host ""
}

# ============================================
# FUNCIÓN: Mostrar Información Previa
# ============================================
function Show-PreBackupInfo {
    Write-Host "📊 INFORMACIÓN DEL ORIGEN:" -ForegroundColor $colorInfo
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $colorInfo
    Write-Host "Directorio: $sourceDir" -ForegroundColor $colorWarning
    
    $sourceFiles = Get-ChildItem -Path $sourceDir -Recurse -File -ErrorAction SilentlyContinue
    $sourceCount = $sourceFiles.Count
    $sourceSize = ($sourceFiles | Measure-Object -Property Length -Sum).Sum / 1MB
    
    Write-Host "Total de archivos: $sourceCount" -ForegroundColor $colorWarning
    Write-Host "Tamaño total: $([math]::Round($sourceSize, 2)) MB" -ForegroundColor $colorWarning
    
    # Mostrar estructura de carpetas
    Write-Host ""
    Write-Host "📂 Estructura de carpetas:" -ForegroundColor $colorInfo
    
    $directories = Get-ChildItem -Path $sourceDir -Recurse -Directory -ErrorAction SilentlyContinue
    foreach ($dir in $directories) {
        $fileCount = (Get-ChildItem -Path $dir.FullName -File -ErrorAction SilentlyContinue).Count
        $relativePath = $dir.FullName.Replace($sourceDir, "").TrimStart('\')
        Write-Host "   📁 $relativePath ($fileCount archivos)" -ForegroundColor $colorWarning
    }
    
    Write-Host ""
    Write-Host "Destino del backup: $backupDir" -ForegroundColor $colorInfo
    Write-Host ""
}

# ============================================
# FUNCIÓN: Realizar Backup
# ============================================
function Start-BackupProcess {
    Write-Host "🔄 INICIANDO BACKUP..." -ForegroundColor $colorSuccess
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $colorSuccess
    Write-Host ""
    
    $startTime = Get-Date
    
    try {
        # Copiar archivos con barra de progreso
        $files = Get-ChildItem -Path $sourceDir -Recurse -File -ErrorAction Stop
        $totalFiles = $files.Count
        $counter = 0
        
        foreach ($file in $files) {
            $counter++
            $percentComplete = [math]::Round(($counter / $totalFiles) * 100, 2)
            
            # Calcular ruta relativa
            $relativePath = $file.FullName.Substring($sourceDir.Length + 1)
            $destPath = Join-Path $backupDir $relativePath
            $destFolder = Split-Path -Parent $destPath
            
            # Crear carpeta si no existe
            if (-not (Test-Path $destFolder)) {
                New-Item -ItemType Directory -Path $destFolder -Force | Out-Null
            }
            
            # Copiar archivo
            Copy-Item -Path $file.FullName -Destination $destPath -Force
            
            # Mostrar progreso cada 10 archivos o en el último
            if ($counter % 10 -eq 0 -or $counter -eq $totalFiles) {
                Write-Progress -Activity "Copiando archivos..." `
                    -Status "$counter de $totalFiles archivos ($percentComplete%)" `
                    -PercentComplete $percentComplete
            }
        }
        
        Write-Progress -Activity "Copiando archivos..." -Completed
        
        $endTime = Get-Date
        $duration = $endTime - $startTime
        
        Write-Host "✅ Copia completada en $($duration.TotalSeconds) segundos" -ForegroundColor $colorSuccess
        Write-Host ""
        
        return $true
    }
    catch {
        Write-Host "❌ ERROR durante el backup:" -ForegroundColor $colorError
        Write-Host "   $($_.Exception.Message)" -ForegroundColor $colorError
        Write-Host ""
        return $false
    }
}

# ============================================
# FUNCIÓN: Verificar Backup
# ============================================
function Test-BackupIntegrity {
    Write-Host "🔍 VERIFICANDO INTEGRIDAD DEL BACKUP..." -ForegroundColor $colorInfo
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $colorInfo
    Write-Host ""
    
    $sourceFiles = Get-ChildItem -Path $sourceDir -Recurse -File -ErrorAction SilentlyContinue
    $backupFiles = Get-ChildItem -Path $backupDir -Recurse -File -ErrorAction SilentlyContinue
    
    $sourceCount = $sourceFiles.Count
    $backupCount = $backupFiles.Count
    
    $sourceSize = ($sourceFiles | Measure-Object -Property Length -Sum).Sum
    $backupSize = ($backupFiles | Measure-Object -Property Length -Sum).Sum
    
    Write-Host "Comparación de archivos:" -ForegroundColor $colorInfo
    Write-Host "  Origen: $sourceCount archivos" -ForegroundColor $colorWarning
    Write-Host "  Backup: $backupCount archivos" -ForegroundColor $colorWarning
    
    Write-Host ""
    Write-Host "Comparación de tamaño:" -ForegroundColor $colorInfo
    Write-Host "  Origen: $([math]::Round($sourceSize / 1MB, 2)) MB" -ForegroundColor $colorWarning
    Write-Host "  Backup: $([math]::Round($backupSize / 1MB, 2)) MB" -ForegroundColor $colorWarning
    
    Write-Host ""
    
    $success = $true
    
    if ($sourceCount -ne $backupCount) {
        Write-Host "⚠️  ADVERTENCIA: Diferencia en cantidad de archivos" -ForegroundColor $colorWarning
        Write-Host "   Diferencia: $($sourceCount - $backupCount) archivos" -ForegroundColor $colorWarning
        $success = $false
    }
    
    if ($sourceSize -ne $backupSize) {
        Write-Host "⚠️  ADVERTENCIA: Diferencia en tamaño total" -ForegroundColor $colorWarning
        Write-Host "   Diferencia: $([math]::Round(($sourceSize - $backupSize) / 1MB, 2)) MB" -ForegroundColor $colorWarning
        $success = $false
    }
    
    if ($success) {
        Write-Host "✅ Verificación exitosa: Origen y backup coinciden" -ForegroundColor $colorSuccess
    }
    
    Write-Host ""
    return $success
}

# ============================================
# FUNCIÓN: Mostrar Resumen
# ============================================
function Show-Summary {
    param (
        [bool]$Success
    )
    
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor $colorInfo
    Write-Host "║  RESUMEN DE BACKUP                                        ║" -ForegroundColor $colorInfo
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor $colorInfo
    Write-Host ""
    
    $backupFiles = Get-ChildItem -Path $backupDir -Recurse -File
    $backupSize = ($backupFiles | Measure-Object -Property Length -Sum).Sum / 1MB
    
    Write-Host "📁 Ubicación del backup:" -ForegroundColor $colorInfo
    Write-Host "   $backupDir" -ForegroundColor $colorWarning
    Write-Host ""
    
    Write-Host "📊 Estadísticas:" -ForegroundColor $colorInfo
    Write-Host "   Total de archivos: $($backupFiles.Count)" -ForegroundColor $colorWarning
    Write-Host "   Tamaño total: $([math]::Round($backupSize, 2)) MB" -ForegroundColor $colorWarning
    Write-Host "   Fecha: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor $colorWarning
    Write-Host ""
    
    if ($Success) {
        Write-Host "✅ ✅ ✅  BACKUP COMPLETADO EXITOSAMENTE  ✅ ✅ ✅" -ForegroundColor $colorSuccess
        Write-Host ""
        Write-Host "👍 Los archivos fueron respaldados correctamente." -ForegroundColor $colorSuccess
        Write-Host "   Puedes proceder con la refactorización." -ForegroundColor $colorSuccess
    }
    else {
        Write-Host "⚠️  ⚠️  ⚠️   BACKUP CON ADVERTENCIAS   ⚠️  ⚠️  ⚠️ " -ForegroundColor $colorWarning
        Write-Host ""
        Write-Host "⚠️  Revisa las advertencias arriba antes de continuar." -ForegroundColor $colorWarning
    }
    
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor $colorInfo
    Write-Host "Para restaurar este backup, ejecuta:" -ForegroundColor $colorInfo
    Write-Host "  .\restore_uploads_backup.ps1 -BackupDir '$backupDir'" -ForegroundColor $colorWarning
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor $colorInfo
    Write-Host ""
}

# ============================================
# EJECUCIÓN PRINCIPAL
# ============================================
try {
    Show-Header
    
    # Validar origen
    if (-not (Test-SourceDirectory)) {
        Write-Host "⚠️  El directorio origen está vacío o fue creado recientemente." -ForegroundColor $colorWarning
        Write-Host "   El backup se realizará de todas formas." -ForegroundColor $colorWarning
        Write-Host ""
    }
    
    # Crear directorio de backup
    New-BackupDirectory
    
    # Mostrar información previa
    Show-PreBackupInfo
    
    # Realizar backup
    $backupSuccess = Start-BackupProcess
    
    if (-not $backupSuccess) {
        Write-Host "❌ El backup falló. Revisa los errores arriba." -ForegroundColor $colorError
        exit 1
    }
    
    # Verificar integridad
    $verificationSuccess = Test-BackupIntegrity
    
    # Mostrar resumen
    Show-Summary -Success $verificationSuccess
    
    if ($verificationSuccess) {
        exit 0
    }
    else {
        exit 1
    }
}
catch {
    Write-Host ""
    Write-Host "❌ ❌ ❌  ERROR CRÍTICO  ❌ ❌ ❌" -ForegroundColor $colorError
    Write-Host ""
    Write-Host "Mensaje: $($_.Exception.Message)" -ForegroundColor $colorError
    Write-Host "Línea: $($_.InvocationInfo.ScriptLineNumber)" -ForegroundColor $colorError
    Write-Host ""
    exit 1
}
