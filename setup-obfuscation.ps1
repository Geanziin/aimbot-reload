# Script para configurar ofuscação com ConfuserEx
param(
    [switch]$Force
)

Write-Host "=== Configurando Ofuscação com ConfuserEx ===" -ForegroundColor Green

# Criar diretório tools se não existir
$toolsDir = "tools"
if (!(Test-Path $toolsDir)) {
    New-Item -ItemType Directory -Path $toolsDir | Out-Null
    Write-Host "Diretório 'tools' criado." -ForegroundColor Yellow
}

# URL do ConfuserEx (versão mais recente)
$confuserUrl = "https://github.com/mkaring/ConfuserEx/releases/latest/download/ConfuserEx-CLI.zip"
$confuserZip = "$toolsDir\ConfuserEx-CLI.zip"
$confuserDir = "$toolsDir\ConfuserEx"

# Verificar se já existe
if ((Test-Path $confuserDir) -and !$Force) {
    Write-Host "ConfuserEx já está instalado. Use -Force para reinstalar." -ForegroundColor Yellow
} else {
    Write-Host "Baixando ConfuserEx..." -ForegroundColor Cyan
    
    try {
        # Baixar ConfuserEx
        Invoke-WebRequest -Uri $confuserUrl -OutFile $confuserZip -UseBasicParsing
        
        # Extrair
        if (Test-Path $confuserDir) {
            Remove-Item $confuserDir -Recurse -Force
        }
        
        Expand-Archive -Path $confuserZip -DestinationPath $confuserDir -Force
        Remove-Item $confuserZip -Force
        
        Write-Host "ConfuserEx instalado com sucesso!" -ForegroundColor Green
    }
    catch {
        Write-Host "Erro ao baixar ConfuserEx: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Tentando método alternativo..." -ForegroundColor Yellow
        
        # Método alternativo usando curl
        try {
            curl.exe -L -o $confuserZip $confuserUrl
            Expand-Archive -Path $confuserZip -DestinationPath $confuserDir -Force
            Remove-Item $confuserZip -Force
            Write-Host "ConfuserEx instalado com sucesso (método alternativo)!" -ForegroundColor Green
        }
        catch {
            Write-Host "Falha ao instalar ConfuserEx. Instale manualmente de: https://github.com/mkaring/ConfuserEx/releases" -ForegroundColor Red
            exit 1
        }
    }
}

# Verificar se o ConfuserEx foi instalado corretamente
$confuserExe = "$confuserDir\Confuser.CLI.exe"
if (Test-Path $confuserExe) {
    Write-Host "ConfuserEx configurado com sucesso!" -ForegroundColor Green
    Write-Host "Executável: $confuserExe" -ForegroundColor Cyan
    
    # Testar execução
    try {
        & $confuserExe --help | Out-Null
        Write-Host "ConfuserEx está funcionando corretamente." -ForegroundColor Green
    }
    catch {
        Write-Host "Aviso: Não foi possível testar o ConfuserEx." -ForegroundColor Yellow
    }
} else {
    Write-Host "Erro: ConfuserEx não foi instalado corretamente." -ForegroundColor Red
    exit 1
}

Write-Host "`n=== Configuração Concluída ===" -ForegroundColor Green
Write-Host "Para ofuscar seu projeto, execute: .\build-obfuscated.ps1" -ForegroundColor Cyan

