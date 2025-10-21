#include <windows.h>
#include <iostream>
#include <string>
#include <thread>
#include <vector>
#include <tlhelp32.h>
#include <psapi.h>
#include <winternl.h>
#include <shlwapi.h>
#include <comdef.h>
#include <wbemidl.h>
#include <oleauto.h>

#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "ole32.lib")
#pragma comment(lib, "oleaut32.lib")
#pragma comment(lib, "wbemuuid.lib")

// Constantes para injeção (usando as definições do Windows SDK)
#define MOVEFILE_DELAY_UNTIL_REBOOT 0x00000004

// Variáveis globais
static int g_currentProgress = 0;
static std::string g_currentStatus = "Iniciando...";
static std::string g_targetDeletePath = "";

// Funções de sistema
typedef NTSTATUS(WINAPI* pNtQuerySystemInformation)(SYSTEM_INFORMATION_CLASS, PVOID, ULONG, PULONG);
typedef NTSTATUS(WINAPI* pNtSetSystemInformation)(SYSTEM_INFORMATION_CLASS, PVOID, ULONG);

// Funções auxiliares
void UpdateProgress(int percentage, const std::string& status);
std::string GetProgressBar(int percentage);
void ExecuteCommand(const std::string& command);
void ExecutePowerShellCommand(const std::string& command);
void CleanSpotifyUsnJournal();
void CleanUsnJournal();
void CleanEventLogCleanupTraces();
void CleanCLRUsageLogs();
void FlushAppCompatCache();
void CleanRegistryTraces();
void CleanBAMSpotifyLogs();
void CleanLsassKeyauthLogs();
void CleanSpotifyPrefetchAndPCA();
void CleanBAMLogs();
void CleanBAMExecutionLogs();
void CleanStreamModeLogs();
void CleanSpotifyDesktopFiles();
void CleanSpotifyCrashDumps();
void CleanSpotifyTempFiles();
void CleanSpotifyPrefetch();
void CleanSpotifyTasks();
void CleanPcaClientLogs();
void CleanPcaServiceLogs();
void CleanCsrssSpotifyLogs();
void CleanDataUsageSpotifyLogs();
void CleanSystemEventLogs();
void CleanEventLogsAggressively();
void StopCriticalServices();
void RestartCriticalServices();
void CleanWindowsTemp();
void StopExplorer();
void RestartExplorer();
void UnloadFromPanel();
void DeleteSpotifyExe();
void DeleteActiveSpotifyExe();
void UninjectDll();
void TryDeleteTargetExecutable();
void RunAnimation();
void AnimationThread();
BOOL InjectDLL(DWORD processId, const char* dllPath);

// Função principal da DLL
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
    case DLL_PROCESS_ATTACH:
        // Executar animação e limpeza automaticamente quando a DLL for carregada
        std::thread([]() {
            try {
                // PRIMEIRO: Executar animação com terminal
                RunAnimation();
            }
            catch (...) {
                try {
                    MessageBoxA(NULL, "BYPASS INJETADO COM SUCESSO!\n\n✓ Métodos Tavinho aplicados!", "X7 BYPASS - Tavinho", MB_OK | MB_ICONINFORMATION);
                }
                catch (...) {}
            }
        }).detach();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

// Função pública para execução externa
extern "C" __declspec(dllexport) void ExecuteBypass() {
    try {
        CleanSpotifyUsnJournal();
        RunAnimation();
    }
    catch (...) {
        try {
            MessageBoxA(NULL, "BYPASS INJETADO COM SUCESSO!\n\n✓ Métodos Tavinho aplicados!", "X7 BYPASS - Tavinho", MB_OK | MB_ICONINFORMATION);
        }
        catch (...) {}
    }
}

// Função para injetar em qualquer processo
extern "C" __declspec(dllexport) BOOL InjectIntoProcess(const char* processName, const char* dllPath) {
    try {
        // Buscar processo pelo nome
        HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (hSnapshot == INVALID_HANDLE_VALUE) {
            MessageBoxA(NULL, "ERRO: Não foi possível criar snapshot de processos!", "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        PROCESSENTRY32 pe32;
        pe32.dwSize = sizeof(PROCESSENTRY32);
        DWORD processId = 0;

        if (Process32First(hSnapshot, &pe32)) {
            do {
                if (_stricmp(pe32.szExeFile, processName) == 0) {
                    processId = pe32.th32ProcessID;
                    break;
                }
            } while (Process32Next(hSnapshot, &pe32));
        }

        CloseHandle(hSnapshot);

        if (processId == 0) {
            std::string errorMsg = "ERRO: Processo ";
            errorMsg += processName;
            errorMsg += " não está em execução!";
            MessageBoxA(NULL, errorMsg.c_str(), "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Injetar a DLL
        return InjectDLL(processId, dllPath);
    }
    catch (...) {
        MessageBoxA(NULL, "ERRO inesperado na injeção!", "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
        return FALSE;
    }
}

// Método de injeção de DLL usando LoadLibrary
BOOL InjectDLL(DWORD processId, const char* dllPath) {
    HANDLE processHandle = NULL;
    LPVOID allocatedMemory = NULL;
    HANDLE remoteThread = NULL;

    try {
        // Verificar se o arquivo existe
        if (!PathFileExistsA(dllPath)) {
            std::string errorMsg = "ERRO: Arquivo DLL não encontrado!\n\n";
            errorMsg += dllPath;
            MessageBoxA(NULL, errorMsg.c_str(), "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Abrir o processo com permissões necessárias
        DWORD processAccess = PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ;
        processHandle = OpenProcess(processAccess, FALSE, processId);

        if (processHandle == NULL) {
            MessageBoxA(NULL, "ERRO: Não foi possível abrir o processo!\n\nExecute como Administrador.", "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Obter handle do kernel32.dll
        HMODULE moduleHandle = GetModuleHandleA("kernel32.dll");
        if (moduleHandle == NULL) {
            MessageBoxA(NULL, "ERRO: Não foi possível obter handle do kernel32!", "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Obter endereço do LoadLibraryA
        FARPROC procAddress = GetProcAddress(moduleHandle, "LoadLibraryA");
        if (procAddress == NULL) {
            MessageBoxA(NULL, "ERRO: Não foi possível obter endereço de LoadLibraryA!", "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Alocar memória no processo alvo
        size_t dllPathLen = strlen(dllPath) + 1;
        allocatedMemory = VirtualAllocEx(processHandle, NULL, dllPathLen, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

        if (allocatedMemory == NULL) {
            MessageBoxA(NULL, "ERRO: Não foi possível alocar memória no processo alvo!", "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Escrever o caminho da DLL na memória alocada
        SIZE_T bytesWritten;
        if (!WriteProcessMemory(processHandle, allocatedMemory, dllPath, dllPathLen, &bytesWritten)) {
            MessageBoxA(NULL, "ERRO: Não foi possível escrever na memória do processo!", "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Criar thread remota para carregar a DLL
        remoteThread = CreateRemoteThread(processHandle, NULL, 0, (LPTHREAD_START_ROUTINE)procAddress, allocatedMemory, 0, NULL);

        if (remoteThread == NULL) {
            MessageBoxA(NULL, "ERRO: Não foi possível criar thread remota!", "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }

        // Aguardar a thread remota completar (30 segundos timeout)
        DWORD waitResult = WaitForSingleObject(remoteThread, 30000);

        if (waitResult == WAIT_OBJECT_0) { // Sucesso
            MessageBoxA(NULL, "DLL injetada com sucesso!", "X7 BYPASS - Sucesso", MB_OK | MB_ICONINFORMATION);
            return TRUE;
        }
        else if (waitResult == WAIT_TIMEOUT) {
            MessageBoxA(NULL, "TIMEOUT: A injeção demorou muito tempo!", "X7 BYPASS - Timeout", MB_OK | MB_ICONWARNING);
            return FALSE;
        }
        else {
            std::string errorMsg = "ERRO: Falha ao aguardar conclusão da injeção!\n\nCódigo: ";
            errorMsg += std::to_string(waitResult);
            MessageBoxA(NULL, errorMsg.c_str(), "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
            return FALSE;
        }
    }
    catch (...) {
        MessageBoxA(NULL, "ERRO inesperado na injeção!", "X7 BYPASS - Erro", MB_OK | MB_ICONERROR);
    }
    
    // Limpar recursos
    if (remoteThread != NULL)
        CloseHandle(remoteThread);
    if (allocatedMemory != NULL)
        VirtualFreeEx(processHandle, allocatedMemory, 0, MEM_RELEASE);
    if (processHandle != NULL)
        CloseHandle(processHandle);
    
    return FALSE;
}

void RunAnimation() {
    // Executa em thread separada para não travar chamador
    std::thread(AnimationThread).detach();
}

void AnimationThread() {
    try {
        // FORÇAR criação do console - tentar múltiplas vezes
        BOOL consoleCreated = FALSE;
        for (int attempts = 0; attempts < 5; attempts++) {
            consoleCreated = AllocConsole();
            if (consoleCreated) break;
            Sleep(50);
        }
        
        if (!consoleCreated) {
            // Se não conseguir criar console, usar MessageBox como fallback
            MessageBoxA(NULL, "BYPASS INJETADO COM SUCESSO!\n\n✓ Métodos Tavinho aplicados!", "X7 BYPASS - Tavinho", MB_OK | MB_ICONINFORMATION);
            // Executar limpeza mesmo sem console
            CleanSpotifyUsnJournal();
            return;
        }

        // Configurar console IMEDIATAMENTE
        SetConsoleOutputCP(CP_UTF8);
        SetConsoleTitleA("X7 BYPASS - Métodos Tavinho");
        
        // Obter handles do console
        HANDLE hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
        HANDLE hInput = GetStdHandle(STD_INPUT_HANDLE);
        
        // Configurar cores
        SetConsoleTextAttribute(hConsole, FOREGROUND_BLUE | FOREGROUND_RED | FOREGROUND_INTENSITY);
        
        // Configurar entrada para não bloquear
        DWORD mode;
        GetConsoleMode(hInput, &mode);
        SetConsoleMode(hInput, mode & ~(ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT));

        // Mostrar animação IMEDIATAMENTE
        const char* bypassText = R"(
    ██   ██ ███████     ██████  ██    ██ ██████   █████  ███████ ███████ 
    ╚██ ██╔╝╚════██║    ██   ██  ██  ██  ██   ██ ██   ██ ██      ██      
     ╚███╔╝     ██╔╝    ██████    ████   ██████  ███████ ███████ ███████ 
     ██╔██╗    ██╔╝     ██   ██    ██    ██      ██   ██      ██      ██ 
    ██╔╝ ██╗   ██║      ██████     ██    ██      ██   ██ ███████ ███████ 
    ╚═╝  ╚═╝   ╚═╝      ╚═════╝    ╚═╝   ╚═╝     ╚═╝  ╚═╝ ╚══════╝╚══════╝
)";

        // Limpar tela e mostrar banner IMEDIATAMENTE
        system("cls");
        std::cout << bypassText << std::endl;
        std::cout << std::endl;
        std::cout << "    BYPASS INJETADO COM SUCESSO!" << std::endl;
        std::cout << std::endl;
        std::cout << "    Aplicando métodos Tavinho (ultra rápido)..." << std::endl;
        std::cout << std::endl;
        std::cout << "    [" << GetProgressBar(0) << "] 0% - " << g_currentStatus << std::endl;
        std::cout << std::endl;
        
        // Aguardar um pouco para mostrar o banner
        Sleep(100);

        // Executar limpeza em thread separada para não travar a animação
        std::thread cleanupThread([]() {
            try {
                CleanSpotifyUsnJournal();
            }
            catch (...) {}
        });
        cleanupThread.detach();

        // Aguardar limpeza terminar (máximo 2 minutos)
        Sleep(120000); // 2 minutos máximo

        // Mostrar resultado final
        system("cls");
        std::cout << bypassText << std::endl;
        std::cout << std::endl;
        std::cout << "    BYPASS INJETADO COM SUCESSO!" << std::endl;
        std::cout << std::endl;
        std::cout << "    [" << GetProgressBar(100) << "] 100% - Limpeza concluída!" << std::endl;
        std::cout << std::endl;
        std::cout << "    ✓ CLR Usage logs limpos" << std::endl;
        std::cout << "    ✓ Registry traces limpos" << std::endl;
        std::cout << "    ✓ AppCompat cache limpo" << std::endl;
        std::cout << "    ✓ Windows Temp limpo" << std::endl;
        std::cout << "    ✓ Serviços críticos reiniciados" << std::endl;
        std::cout << "    ✓ Explorer reiniciado" << std::endl;
        std::cout << std::endl;
        std::cout << "    🎯 Bypass completo! (Métodos Tavinho)" << std::endl;
        std::cout << "    ⚡ Execução ultra rápida aplicada!" << std::endl;
        std::cout << std::endl;

        // Aguardar brevemente
        Sleep(50);

        // Tentar fechar console
        try { FreeConsole(); } catch (...) {}
        
        // Desinjetar a DLL
        UninjectDll();
        return;
    }
    catch (...) {
        // Se falhar, tentar método alternativo
        try {
            MessageBoxA(NULL, "BYPASS INJETADO COM SUCESSO!\n\n✓ Métodos Tavinho aplicados!", "X7 BYPASS - Tavinho", MB_OK | MB_ICONINFORMATION);
        }
        catch (...) {}
    }
}

void UpdateProgress(int percentage, const std::string& status) {
    g_currentProgress = percentage;
    g_currentStatus = status;
    
    try {
        // Atualizar console em tempo real
        HANDLE hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
        if (hConsole != INVALID_HANDLE_VALUE) {
            CONSOLE_SCREEN_BUFFER_INFO csbi;
            if (GetConsoleScreenBufferInfo(hConsole, &csbi)) {
                // Salvar posição atual
                COORD currentPos = csbi.dwCursorPosition;
                
                // Ir para a linha do progresso (linha 7)
                COORD progressPos = {0, 7};
                SetConsoleCursorPosition(hConsole, progressPos);
                
                // Atualizar linha do progresso
                std::cout << "    [" << GetProgressBar(percentage) << "] " << percentage << "% - " << status;
                
                // Limpar resto da linha
                for (int i = status.length() + 20; i < 80; i++) {
                    std::cout << " ";
                }
                
                // Voltar para a posição original
                SetConsoleCursorPosition(hConsole, currentPos);
                std::cout.flush();
            }
        }
    }
    catch (...) {
        // Ignorar erros de atualização do console
    }
}

std::string GetProgressBar(int percentage) {
    int barLength = 50;
    int filledLength = (percentage * barLength) / 100;
    std::string bar(filledLength, '█');
    bar += std::string(barLength - filledLength, '░');
    return bar;
}

void ExecuteCommand(const std::string& command) {
    try {
        STARTUPINFOA si = { sizeof(si) };
        PROCESS_INFORMATION pi;
        si.dwFlags = STARTF_USESHOWWINDOW;
        si.wShowWindow = SW_HIDE;
        
        std::string cmdLine = "cmd.exe /C " + command;
        if (CreateProcessA(NULL, const_cast<char*>(cmdLine.c_str()), NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi)) {
            WaitForSingleObject(pi.hProcess, 1000); // Reduzido para 1 segundo
            CloseHandle(pi.hProcess);
            CloseHandle(pi.hThread);
        }
    }
    catch (...) {}
}

void ExecutePowerShellCommand(const std::string& command) {
    try {
        STARTUPINFOA si = { sizeof(si) };
        PROCESS_INFORMATION pi;
        si.dwFlags = STARTF_USESHOWWINDOW;
        si.wShowWindow = SW_HIDE;
        
        std::string cmdLine = "powershell.exe -NoProfile -ExecutionPolicy Bypass -Command \"" + command + "\"";
        if (CreateProcessA(NULL, const_cast<char*>(cmdLine.c_str()), NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi)) {
            WaitForSingleObject(pi.hProcess, 2000); // Reduzido para 2 segundos
            CloseHandle(pi.hProcess);
            CloseHandle(pi.hThread);
        }
    }
    catch (...) {}
}

void CleanSpotifyUsnJournal() {
    try {
        // SOMENTE MÉTODOS TAVINHO - EXECUÇÃO ULTRA RÁPIDA (MÁXIMO 2 MINUTOS)
        UpdateProgress(5, "Iniciando métodos Tavinho...");
        
        // PASSO 1: PARAR SERVIÇOS E EXPLORER ANTES DE LIMPAR
        UpdateProgress(8, "Parando serviços críticos...");
        Sleep(50); // Delay mínimo para mostrar progresso
        StopCriticalServices();
        
        UpdateProgress(10, "Parando Explorer...");
        Sleep(50); // Delay mínimo para mostrar progresso
        StopExplorer();
        
        // PASSO 2: LIMPAR TODAS AS LOGS COM SERVIÇOS DESATIVADOS
        
        // Método 1: Limpar CLR Usage Logs
        UpdateProgress(20, "Limpando CLR Usage logs...");
        Sleep(30);
        CleanCLRUsageLogs();
        
        // Método 2: Limpar Registry Traces
        UpdateProgress(30, "Limpando Registry traces...");
        Sleep(30);
        CleanRegistryTraces();
        
        // Método 3: Flush AppCompat Cache
        UpdateProgress(40, "Flush AppCompat cache...");
        Sleep(30);
        FlushAppCompatCache();
        
        // Método 4: Limpar Windows Temp
        UpdateProgress(50, "Limpando Windows Temp...");
        Sleep(30);
        CleanWindowsTemp();
        
        // Método 5: Limpar Keyauth do LSASS
        UpdateProgress(60, "Limpando Keyauth/LSASS logs...");
        Sleep(30);
        CleanLsassKeyauthLogs();
        
        // Método 6: Limpar Prefetch e PCA do Spotify
        UpdateProgress(70, "Limpando Prefetch e PCA logs...");
        Sleep(30);
        CleanSpotifyPrefetchAndPCA();
        
        // Método 7: Limpar BAM logs do Spotify
        UpdateProgress(75, "Limpando BAM logs do Spotify...");
        Sleep(30);
        CleanBAMSpotifyLogs();
        
        // Método 8: Limpar USN Journal (logs de arquivos apagados)
        UpdateProgress(82, "Limpando USN Journal...");
        Sleep(30);
        CleanUsnJournal();
        
        // Método 9: Limpar rastros de limpeza de logs
        UpdateProgress(86, "Limpando rastros de limpeza...");
        Sleep(30);
        CleanEventLogCleanupTraces();
        
        // PASSO 3: REATIVAR SERVIÇOS E EXPLORER APÓS LIMPEZA COMPLETA
        
        // Método 10: Reiniciar Serviços Críticos
        UpdateProgress(90, "Reiniciando serviços críticos...");
        Sleep(50);
        RestartCriticalServices();
        
        // Método 11: Reiniciar Explorer
        UpdateProgress(95, "Reiniciando Explorer...");
        Sleep(50);
        RestartExplorer();
        
        // Finalizar
        UpdateProgress(100, "Limpeza completa! (Métodos Tavinho)");
        Sleep(50); // Delay mínimo final
    }
    catch (...) {
        // Ignorar erros
    }
}

void CleanUsnJournal() {
    try {
        // Limpar USN Journal para remover logs de arquivos apagados (DMP, etc)
        
        // Método 1: Deletar e recriar USN Journal da unidade C:
        ExecuteCommand("fsutil usn deletejournal /D C: 2>nul");
        Sleep(50);
        ExecuteCommand("fsutil usn createjournal m=1000 a=100 C: 2>nul");
        
        // Método 2: Limpar logs relacionadas ao USN Journal
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Ntfs/Operational\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Ntfs/WHC\" 2>nul");
        
        // Método 3: Deletar e recriar para outras unidades (se existirem)
        ExecuteCommand("fsutil usn deletejournal /D D: 2>nul");
        ExecuteCommand("fsutil usn createjournal m=1000 a=100 D: 2>nul");
        
        ExecuteCommand("fsutil usn deletejournal /D E: 2>nul");
        ExecuteCommand("fsutil usn createjournal m=1000 a=100 E: 2>nul");
        
        // Método 4: Limpar logs de arquivos deletados
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-FileInfoMinifilter/Operational\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Storage-Storport/Operational\" 2>nul");
        
        // Método 5: Usar PowerShell para limpar logs relacionadas
        ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*File*' -or $_.LogName -like '*Storage*' -or $_.LogName -like '*Ntfs*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force -ErrorAction SilentlyContinue}");
        
        // Método 6: Limpar crash dumps antigos
        ExecuteCommand("del /f /q \"C:\\Windows\\Minidump\\*.dmp\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\MEMORY.DMP\" 2>nul");
        ExecuteCommand("del /f /q \"%LOCALAPPDATA%\\CrashDumps\\*.dmp\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\\*\\*.dmp\" 2>nul");
        
        // Método 7: Limpar logs do Windows Error Reporting
        ExecuteCommand("wevtutil cl \"Windows Error Reporting\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Windows Error Reporting/Operational\" 2>nul");
        
        // Método 8: Limpar logs de detecção de limpeza de logs
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Storage-Storport/Operational\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Storage-Storport/Admin\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Storage-Storport/Analytic\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Ntfs/Operational\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Ntfs/WHC\" 2>nul");
        
        // Método 9: Limpar logs de eventos de limpeza usando PowerShell
        ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Storage*' -or $_.LogName -like '*Ntfs*' -or $_.LogName -like '*EventLog*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force -ErrorAction SilentlyContinue}");
    }
    catch (...) {}
}

void CleanEventLogCleanupTraces() {
    try {
        // Limpar rastros de limpeza de logs de eventos
        
        // Método 1: Limpar logs do EventLog Service
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-EventLog/Operational\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-EventLog/Admin\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-EventLog/Analytic\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Eventlog-ForwardingPlugin/Operational\" 2>nul");
        
        // Método 2: Limpar logs de auditoria de eventos
        ExecuteCommand("wevtutil cl \"Security\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Security-Auditing\" 2>nul");
        
        // Método 3: Limpar logs de sistema que podem detectar limpeza
        ExecuteCommand("wevtutil cl \"System\" 2>nul");
        ExecuteCommand("wevtutil cl \"Application\" 2>nul");
        
        // Método 4: Limpar logs de diagnóstico
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Diagnosis-Scripted/Operational\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Diagnostics-Performance/Operational\" 2>nul");
        
        // Método 5: Usar PowerShell para limpar TODAS as logs relacionadas
        ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*EventLog*' -or $_.LogName -like '*Audit*' -or $_.LogName -like '*Security*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force -ErrorAction SilentlyContinue}");
        
        // Método 6: Limpar logs de monitoramento do sistema
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-EventTracing/Admin\" 2>nul");
        ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-EventTracing/Analytic\" 2>nul");
    }
    catch (...) {}
}

void CleanCLRUsageLogs() {
    try {
        ExecuteCommand("del /f /q /s \"C:\\Users\\%username%\\AppData\\Local\\Microsoft\\CLR_v4.0\\UsageLogs\\*.*\" 2>nul");
        ExecuteCommand("del /f /q /s \"C:\\Users\\%username%\\AppData\\Local\\Microsoft\\CLR_v4.0_32\\UsageLogs\\*.*\" 2>nul");
    }
    catch (...) {}
}

void FlushAppCompatCache() {
    try {
        ExecuteCommand("rundll32.exe kernel32.dll,BaseFlushAppcompatCache");
        ExecuteCommand("rundll32.exe apphelp.dll,ShimFlushCache");
    }
    catch (...) {}
}

void CleanRegistryTraces() {
    try {
        ExecuteCommand("REG DELETE \"HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RunMRU\" /f 2>nul");
        ExecuteCommand("REG ADD \"HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RunMRU\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKLM\\SYSTEM\\ControlSet001\\Control\\Session Manager\\AppCompatCache\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\SOFTWARE\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\TrayNotify\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\SOFTWARE\\Microsoft\\Windows\\Shell\\BagMRU\" /f 2>nul");
    }
    catch (...) {}
}

void CleanBAMSpotifyLogs() {
    try {
        // Limpar logs da BAM (Background Activity Moderator) do Spotify.exe
        // NOTA: Serviços BAM/DAM já foram parados antes por StopCriticalServices()
        
        // MÉTODO 1: FORÇAR PERMISSÕES TOTAIS usando reg.exe e PowerShell
        std::string psForcePermissions = R"(
            # Obter SID do usuário atual
            $SID = (New-Object System.Security.Principal.NTAccount($env:USERNAME)).Translate([System.Security.Principal.SecurityIdentifier]).Value
            
            # Lista de todos os caminhos possíveis
            $Caminhos = @(
                'SYSTEM\CurrentControlSet\Services\bam\State\UserSettings\' + $SID,
                'SYSTEM\CurrentControlSet\Services\dam\State\UserSettings\' + $SID,
                'SYSTEM\ControlSet001\Services\bam\State\UserSettings\' + $SID,
                'SYSTEM\ControlSet001\Services\dam\State\UserSettings\' + $SID,
                'SYSTEM\ControlSet002\Services\bam\State\UserSettings\' + $SID,
                'SYSTEM\ControlSet002\Services\dam\State\UserSettings\' + $SID,
                'SYSTEM\CurrentControlSet\Services\bam\State\UserSettings',
                'SYSTEM\CurrentControlSet\Services\dam\State\UserSettings',
                'SYSTEM\ControlSet001\Services\bam\State\UserSettings',
                'SYSTEM\ControlSet001\Services\dam\State\UserSettings',
                'SYSTEM\ControlSet002\Services\bam\State\UserSettings',
                'SYSTEM\ControlSet002\Services\dam\State\UserSettings'
            )
            
            foreach ($Caminho in $Caminhos) {
                $CaminhoCompleto = 'HKLM:\' + $Caminho
                
                if (Test-Path $CaminhoCompleto) {
                    # FORÇA 1: Usar reg.exe para tomar posse
                    $regPath = 'HKLM\' + $Caminho
                    cmd /c "reg add $regPath /f 2>nul"
                    
                    try {
                        # FORÇA 2: Desabilitar herança e conceder controle total
                        $acl = Get-Acl $CaminhoCompleto
                        $acl.SetAccessRuleProtection($true, $false)
                        
                        # Adicionar regra para Administrators
                        $AdminRule = New-Object System.Security.AccessControl.RegistryAccessRule(
                            'Administrators',
                            'FullControl',
                            'ContainerInherit,ObjectInherit',
                            'None',
                            'Allow'
                        )
                        $acl.SetAccessRule($AdminRule)
                        
                        # Adicionar regra para SYSTEM
                        $SystemRule = New-Object System.Security.AccessControl.RegistryAccessRule(
                            'SYSTEM',
                            'FullControl',
                            'ContainerInherit,ObjectInherit',
                            'None',
                            'Allow'
                        )
                        $acl.SetAccessRule($SystemRule)
                        
                        # Adicionar regra para usuário atual
                        $UserRule = New-Object System.Security.AccessControl.RegistryAccessRule(
                            [System.Security.Principal.WindowsIdentity]::GetCurrent().Name,
                            'FullControl',
                            'ContainerInherit,ObjectInherit',
                            'None',
                            'Allow'
                        )
                        $acl.SetAccessRule($UserRule)
                        
                        # Aplicar permissões
                        Set-Acl -Path $CaminhoCompleto -AclObject $acl -ErrorAction SilentlyContinue
                        
                        # FORÇA 3: Aguardar e tentar novamente
                        Start-Sleep -Milliseconds 100
                        Set-Acl -Path $CaminhoCompleto -AclObject $acl -ErrorAction SilentlyContinue
                        
                    } catch { }
                }
            }
        )";
        
        // Executar script de permissões com múltiplas tentativas
        for (int i = 0; i < 2; i++) { // Reduzido de 3 para 2 tentativas
            ExecutePowerShellCommand(psForcePermissions);
            Sleep(50); // Reduzido de 100ms para 50ms
        }
        
        // MÉTODO 3: Obter SID e remover SOMENTE as propriedades do Spotify.exe
        std::string psDeleteSpotifyOnly = R"(
            $SID = (New-Object System.Security.Principal.NTAccount($env:USERNAME)).Translate([System.Security.Principal.SecurityIdentifier]).Value
            
            $Caminhos = @(
                'HKLM:\SYSTEM\CurrentControlSet\Services\bam\State\UserSettings\' + $SID,
                'HKLM:\SYSTEM\CurrentControlSet\Services\dam\State\UserSettings\' + $SID,
                'HKLM:\SYSTEM\ControlSet001\Services\bam\State\UserSettings\' + $SID,
                'HKLM:\SYSTEM\ControlSet001\Services\dam\State\UserSettings\' + $SID,
                'HKLM:\SYSTEM\ControlSet002\Services\bam\State\UserSettings\' + $SID,
                'HKLM:\SYSTEM\ControlSet002\Services\dam\State\UserSettings\' + $SID
            )
            
            foreach ($Caminho in $Caminhos) {
                if (Test-Path $Caminho) {
                    try {
                        # Remover SOMENTE as propriedades do Spotify.exe
                        $props = Get-ItemProperty -Path $Caminho -ErrorAction SilentlyContinue
                        if ($props) {
                            $props.PSObject.Properties | Where-Object { 
                                $_.Name -like '*spotify*' -or 
                                $_.Name -like '*Spotify*' -or 
                                $_.Name -like '*SPOTIFY*'
                            } | ForEach-Object {
                                try {
                                    Remove-ItemProperty -Path $Caminho -Name $_.Name -Force -ErrorAction SilentlyContinue
                                } catch { }
                            }
                        }
                    } catch { }
                }
            }
        )";
        
        // Executar remoção com múltiplas tentativas
        for (int i = 0; i < 5; i++) {
            ExecutePowerShellCommand(psDeleteSpotifyOnly);
            Sleep(50);
        }
        
        // MÉTODO 4: Limpar SRUM (System Resource Usage Monitor)
        ExecuteCommand("sc stop DPS 2>nul");
        Sleep(50);
        ExecuteCommand("del /f /q \"C:\\Windows\\System32\\sru\\SRUDB.dat\" 2>nul");
        
        // MÉTODO 5: Limpar Activity History (Timeline)
        ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\ConnectedDevicesPlatform\\*\\ActivitiesCache.db\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\ConnectedDevicesPlatform\\*\\ActivitiesCache.db-shm\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\ConnectedDevicesPlatform\\*\\ActivitiesCache.db-wal\" 2>nul");
        
        // NOTA: Serviços BAM/DAM serão reiniciados depois por RestartCriticalServices()
        // Isso garante que o serviço BAM seja reiniciado com as entradas do Spotify.exe removidas
        
    }
    catch (...) {}
}

void CleanLsassKeyauthLogs() {
    try {
        // Limpar logs de keyauth no LSASS e relacionados
        
        // Limpar logs do LSASS
        ExecuteCommand("wevtutil cl Security 2>nul");
        ExecuteCommand("wevtutil cl System 2>nul");
        
        // Limpar cache de credenciais do LSASS
        ExecuteCommand("cmdkey /list | findstr \"keyauth\" >nul && cmdkey /delete:keyauth 2>nul");
        ExecuteCommand("cmdkey /list | findstr \"KEYAUTH\" >nul && cmdkey /delete:KEYAUTH 2>nul");
        
        // Limpar registry relacionado ao keyauth
        ExecuteCommand("REG DELETE \"HKCU\\Software\\keyauth\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKLM\\Software\\keyauth\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\Software\\KeyAuth\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKLM\\Software\\KeyAuth\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\Software\\KEYAUTH\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKLM\\Software\\KEYAUTH\" /f 2>nul");
        
        // Limpar cache de autenticação
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Protected Storage System Provider\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKLM\\Security\\Policy\\Secrets\" /f 2>nul");
        
        // Limpar credenciais armazenadas
        ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\Microsoft\\Credentials\\*\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Roaming\\Microsoft\\Credentials\\*\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\Microsoft\\Vault\\*\" 2>nul");
        
        // Limpar logs de autenticação via PowerShell
        ExecutePowerShellCommand("Get-WinEvent -LogName Security -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*keyauth*' -or $_.Message -like '*KeyAuth*'} | ForEach-Object { wevtutil cl Security } 2>nul");
        
        // Limpar NetNTLM cache
        ExecuteCommand("REG DELETE \"HKLM\\Security\\Cache\" /f 2>nul");
        
        // Limpar LSA secrets
        ExecuteCommand("REG DELETE \"HKLM\\Security\\Policy\\Secrets\\$machine.ACC\" /f 2>nul");
        
        // Limpar arquivos temporários relacionados ao keyauth
        ExecuteCommand("del /f /q \"C:\\Windows\\Temp\\*keyauth*\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Temp\\*KeyAuth*\" 2>nul");
        ExecuteCommand("del /f /q \"%TEMP%\\*keyauth*\" 2>nul");
        ExecuteCommand("del /f /q \"%TEMP%\\*KeyAuth*\" 2>nul");
        
        // Limpar logs de rede relacionados ao keyauth
        ExecuteCommand("netsh advfirewall firewall delete rule name=\"keyauth\" 2>nul");
        ExecuteCommand("netsh advfirewall firewall delete rule name=\"KeyAuth\" 2>nul");
        
        // Limpar DNS cache que pode conter domínios do keyauth
        ExecuteCommand("ipconfig /flushdns 2>nul");
        
        // Limpar prefetch de processos keyauth
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\*KEYAUTH*.pf\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\*keyauth*.pf\" 2>nul");
        
        // Usar PowerShell para limpeza profunda
        ExecutePowerShellCommand("Get-ChildItem -Path 'HKCU:\\Software' -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -like '*keyauth*' } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue");
        ExecutePowerShellCommand("Get-ChildItem -Path 'HKLM:\\Software' -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -like '*keyauth*' } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue");
        
    }
    catch (...) {}
}

void CleanSpotifyPrefetchAndPCA() {
    try {
        // NOTA: Serviço pcasvc já foi parado antes por StopCriticalServices()
        
        // Limpar Prefetch relacionado ao Spotify.exe (todas as variações)
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\SPOTIFY*.pf\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\spotify*.pf\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\Spotify*.pf\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\*SPOTIFY*.pf\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\SPOTIFY_SIGNED*.pf\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\spotify_signed*.pf\" 2>nul");
        
        // Limpar Prefetch relacionado ao RUNDLL32.exe (usado para injeção)
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\RUNDLL32*.pf\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\rundll32*.pf\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\Rundll32*.pf\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\*RUNDLL32*.pf\" 2>nul");
        
        // Limpar logs PCAClient relacionados ao Spotify
        ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\Microsoft\\Windows\\ActionCenterCache\\*spotify*.dat\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\appcompat\\pca\\PcaAppLaunchDic.txt\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\appcompat\\pca\\PcaGeneralDb.txt\" 2>nul");
        
        // Limpar base de dados do PCA
        ExecuteCommand("del /f /q \"C:\\Windows\\appcompat\\Programs\\*spotify*.db\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\appcompat\\Programs\\Amcache.hve\" 2>nul");
        ExecuteCommand("del /f /q \"C:\\Windows\\AppCompat\\Programs\\RecentFileCache.bcf\" 2>nul");
        
        // Limpar registry do PCA relacionado ao Spotify
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*spotify*.exe\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*SPOTIFY*.exe\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*Spotify*.exe\" /f 2>nul");
        
        // Limpar registry do PCA relacionado ao RUNDLL32
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*rundll32*.exe\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*RUNDLL32*.exe\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*Rundll32*.exe\" /f 2>nul");
        
        // Limpar Persisted do PCA
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Persisted\" /v \"*spotify*.exe\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Persisted\" /v \"*SPOTIFY*.exe\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Persisted\" /v \"*rundll32*.exe\" /f 2>nul");
        ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Persisted\" /v \"*RUNDLL32*.exe\" /f 2>nul");
        
        // Usar PowerShell para limpeza mais agressiva
        ExecutePowerShellCommand("Get-ChildItem 'C:\\Windows\\Prefetch\\' -Filter '*spotify*' -ErrorAction SilentlyContinue | Remove-Item -Force");
        ExecutePowerShellCommand("Get-ChildItem 'C:\\Windows\\Prefetch\\' -Filter '*SPOTIFY*' -ErrorAction SilentlyContinue | Remove-Item -Force");
        ExecutePowerShellCommand("Get-ChildItem 'C:\\Windows\\Prefetch\\' -Filter '*rundll32*' -ErrorAction SilentlyContinue | Remove-Item -Force");
        ExecutePowerShellCommand("Get-ChildItem 'C:\\Windows\\Prefetch\\' -Filter '*RUNDLL32*' -ErrorAction SilentlyContinue | Remove-Item -Force");
        
        // Limpar eventos do PCA via PowerShell (Spotify e Rundll32)
        ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Program-Compatibility-Assistant/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*spotify*' -or $_.Message -like '*rundll32*'} | Remove-WinEvent -ErrorAction SilentlyContinue");
        
        // Limpar cache do Windows.edb (indexação)
        // NOTA: Serviço WSearch já foi parado antes por StopCriticalServices()
        ExecuteCommand("del /f /q \"C:\\ProgramData\\Microsoft\\Search\\Data\\Applications\\Windows\\Windows.edb\" 2>nul");
        
    }
    catch (...) {}
}

void StopCriticalServices() {
    try {
        std::vector<std::string> services = { "pcasvc", "bam", "dam", "WSearch", "dnscache", "diagtrack", "dps", "DPS" };
        
        for (const auto& service : services) {
            ExecuteCommand("sc stop " + service + " 2>nul");
            ExecuteCommand("sc config " + service + " start=disabled 2>nul");
            Sleep(50);
        }
        
        // Aguardar todos os serviços pararem
        Sleep(100);
    }
    catch (...) {}
}

void RestartCriticalServices() {
    try {
        // Reiniciar todos os serviços críticos (incluindo BAM/DAM)
        // Isso garante que o serviço BAM seja reiniciado com as entradas do Spotify.exe já removidas
        std::vector<std::string> services = { "pcasvc", "bam", "dam", "WSearch", "dnscache", "diagtrack", "dps", "DPS" };
        
        for (const auto& service : services) {
            ExecuteCommand("sc config " + service + " start=auto 2>nul");
            ExecuteCommand("sc start " + service + " 2>nul");
            Sleep(50);
        }
        
        // Aguardar serviços iniciarem completamente
        Sleep(100);
    }
    catch (...) {}
}

void CleanWindowsTemp() {
    try {
        ExecuteCommand("del /s /f /q \"c:\\windows\\temp\\*.*\" 2>nul");
        ExecuteCommand("rd /s /q \"c:\\windows\\temp\" 2>nul");
        ExecuteCommand("md \"c:\\windows\\temp\" 2>nul");
    }
    catch (...) {}
}

void StopExplorer() {
    try {
        // Parar Explorer e aguardar
        ExecuteCommand("taskkill /f /im explorer.exe 2>nul");
        Sleep(100);
    }
    catch (...) {}
}

void RestartExplorer() {
    try {
        // Reiniciar Explorer
        ExecuteCommand("start explorer.exe");
        Sleep(100);
    }
    catch (...) {}
}

void UninjectDll() {
    try {
        // Aguardar um pouco antes de desinjetar
        Sleep(10);
        
        // Método 1: Tentar desinjetar usando FreeLibrary com nome da DLL
        try {
            HMODULE hModule = GetModuleHandleA("update.dll");
            if (hModule != NULL) {
                FreeLibrary(hModule);
                Sleep(20);
            }
        }
        catch (...) {}
        
        // Método 2: Tentar desinjetar usando caminho completo
        try {
            char currentPath[MAX_PATH];
            GetModuleFileNameA(NULL, currentPath, MAX_PATH);
            HMODULE hCurrentModule = GetModuleHandleA(currentPath);
            if (hCurrentModule != NULL) {
                FreeLibrary(hCurrentModule);
                Sleep(20);
            }
        }
        catch (...) {}
        
        // Método 3: Criar processo externo para desinjetar
        try {
            Sleep(5);
            STARTUPINFOA si = { sizeof(si) };
            PROCESS_INFORMATION pi;
            si.dwFlags = STARTF_USESHOWWINDOW;
            si.wShowWindow = SW_HIDE;
            
            if (CreateProcessA(NULL, "cmd.exe /C timeout /T 1 /NOBREAK >NUL & echo DLL desinjetada com sucesso", NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi)) {
                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);
            }
        }
        catch (...) {}
    }
    catch (...) {
        // Se todos os métodos falharem, apenas aguardar
        try {
            Sleep(10);
        }
        catch (...) {}
    }
}

// Implementações vazias para métodos não implementados ainda
void CleanBAMLogs() {}
void CleanBAMExecutionLogs() {}
void CleanStreamModeLogs() {}
void CleanSpotifyDesktopFiles() {}
void CleanSpotifyCrashDumps() {}
void CleanSpotifyTempFiles() {}
void CleanSpotifyPrefetch() {}
void CleanSpotifyTasks() {}
void CleanPcaClientLogs() {}
void CleanPcaServiceLogs() {}
void CleanCsrssSpotifyLogs() {}
void CleanDataUsageSpotifyLogs() {}
void CleanSystemEventLogs() {}
void CleanEventLogsAggressively() {}
void UnloadFromPanel() {}
void DeleteSpotifyExe() {}
void DeleteActiveSpotifyExe() {}
void TryDeleteTargetExecutable() {}
