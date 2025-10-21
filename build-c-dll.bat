@echo off
echo Compilando DLL em C...

REM Verificar se o Visual Studio está instalado
where cl >nul 2>nul
if %errorlevel% neq 0 (
    echo Visual Studio Build Tools nao encontrado!
    echo Instalando via vcpkg...
    
    REM Tentar usar vcpkg para compilar
    if exist "C:\vcpkg\vcpkg.exe" (
        C:\vcpkg\vcpkg.exe install --triplet=x64-windows
    ) else (
        echo Instale o Visual Studio Build Tools primeiro!
        pause
        exit /b 1
    )
)

REM Configurar ambiente do Visual Studio
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat" 2>nul
if %errorlevel% neq 0 (
    call "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\VC\Auxiliary\Build\vcvars64.bat" 2>nul
    if %errorlevel% neq 0 (
        call "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" 2>nul
        if %errorlevel% neq 0 (
            call "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" 2>nul
        )
    )
)

REM Compilar a DLL
echo Compilando update.dll...
cl /LD /O2 /MT /D "WIN32" /D "NDEBUG" /D "_WINDOWS" /D "_USRDLL" /I "C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\um" /I "C:\Program Files (x86)\Windows Kits\10\Include\10.0.19041.0\shared" Update\entry.c /link /OUT:bin\Release\update.dll ntdll.lib psapi.lib shlwapi.lib advapi32.lib kernel32.lib user32.lib /SUBSYSTEM:WINDOWS /MACHINE:X64

if %errorlevel% equ 0 (
    echo.
    echo ✅ DLL compilada com sucesso!
    echo 📁 Arquivo: bin\Release\update.dll
    echo.
    echo 🎯 A DLL em C oferece:
    echo    • Controle de baixo nível
    echo    • Métodos nativos do Windows
    echo    • Melhor performance
    echo    • Menor tamanho do arquivo
    echo    • Execução mais rápida
) else (
    echo.
    echo ❌ Erro na compilação!
    echo.
    echo Soluções possíveis:
    echo 1. Instale o Visual Studio Build Tools
    echo 2. Instale o Windows SDK
    echo 3. Verifique as dependências
)

pause
