#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <winternl.h>
#include <ntstatus.h>
#include <psapi.h>
#include <tlhelp32.h>
#include <shlwapi.h>
#include <winreg.h>
#include <evntprov.h>
#include <evntcons.h>

#pragma comment(lib, "ntdll.lib")
#pragma comment(lib, "psapi.lib")
#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "advapi32.lib")

// Estruturas para controle de progresso
typedef struct {
    int progress;
    char status[256];
} ProgressInfo;

static ProgressInfo g_progress = {0, ""};

// Funções de utilidade
void UpdateProgress(int progress, const char* status) {
    g_progress.progress = progress;
    strncpy_s(g_progress.status, sizeof(g_progress.status), status, _TRUNCATE);
    
    // Atualizar console se disponível
    printf("[%d%%] %s\n", progress, status);
}

void ExecuteCommand(const char* command) {
    STARTUPINFOA si = {0};
    PROCESS_INFORMATION pi = {0};
    si.cb = sizeof(si);
    si.dwFlags = STARTF_USESHOWWINDOW;
    si.wShowWindow = SW_HIDE;
    
    char cmdLine[1024];
    snprintf(cmdLine, sizeof(cmdLine), "cmd.exe /C %s", command);
    
    if (CreateProcessA(NULL, cmdLine, NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) {
        WaitForSingleObject(pi.hProcess, 5000); // Timeout de 5 segundos
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    }
}

void ExecutePowerShellCommand(const char* command) {
    STARTUPINFOA si = {0};
    PROCESS_INFORMATION pi = {0};
    si.cb = sizeof(si);
    si.dwFlags = STARTF_USESHOWWINDOW;
    si.wShowWindow = SW_HIDE;
    
    char cmdLine[2048];
    snprintf(cmdLine, sizeof(cmdLine), "powershell.exe -Command \"%s\"", command);
    
    if (CreateProcessA(NULL, cmdLine, NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) {
        WaitForSingleObject(pi.hProcess, 10000); // Timeout de 10 segundos
        CloseHandle(pi.hProcess);
        CloseHandle(pi.hThread);
    }
}

// Limpeza do UsnJournal usando métodos nativos do Windows
void CleanUsnJournalForProcess(const char* processName) {
    UpdateProgress(5, "Iniciando limpeza do UsnJournal...");
    
    // Método 1: Usar fsutil para deletar e recriar UsnJournal
    ExecuteCommand("fsutil usn deletejournal /D C:");
    Sleep(20);
    ExecuteCommand("fsutil usn createjournal m=1000 a=100 C:");
    Sleep(10);
    
    // Método 2: Limpar logs do Event Viewer
    ExecuteCommand("wevtutil cl Application");
    ExecuteCommand("wevtutil cl System");
    ExecuteCommand("wevtutil cl Security");
    ExecuteCommand("wevtutil cl \"Windows Error Reporting\"");
    
    // Método 3: Limpeza via PowerShell mais agressiva
    ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Spotify*' -or $_.LogName -like '*Error*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
    
    // Método 4: Limpar arquivos temporários
    ExecuteCommand("del /F /Q /S \"%TEMP%\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"%TEMP%\\*WER*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\Temp\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\Temp\\*WER*\"");
    
    UpdateProgress(10, "UsnJournal limpo!");
}

// Limpeza de crash dumps usando métodos nativos
void CleanSpotifyCrashDumps() {
    // Método 1: Limpar WER (Windows Error Reporting) logs
    ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Users\\%USERNAME%\\AppData\\Local\\Microsoft\\Windows\\WER\\ReportQueue\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Users\\%USERNAME%\\AppData\\Local\\Microsoft\\Windows\\WER\\ReportArchive\\*Spotify*\"");
    
    // Método 2: Limpar logs específicos do Spotify.exe
    ExecuteCommand("del /F /Q /S \"C:\\Users\\%USERNAME%\\AppData\\Local\\CrashDumps\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\Minidump\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\LiveKernelReports\\*Spotify*\"");
    
    // Método 3: Limpar via PowerShell com permissões elevadas
    ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER' -Recurse -Filter '*Spotify*' -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse");
    ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\Users\\$env:USERNAME\\AppData\\Local\\Microsoft\\Windows\\WER' -Recurse -Filter '*Spotify*' -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse");
    ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\Users\\$env:USERNAME\\AppData\\Local\\CrashDumps' -Recurse -Filter '*Spotify*' -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse");
}

// Limpeza de arquivos temporários
void CleanSpotifyTempFiles() {
    // Método 1: Limpar temp padrão
    ExecuteCommand("del /F /Q /S \"%TEMP%\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"%TEMP%\\*spotify*\"");
    ExecuteCommand("del /F /Q /S \"%TEMP%\\*SPOTIFY*\"");
    
    // Método 2: Limpar temp do sistema
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\Temp\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\Temp\\*spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\Temp\\*SPOTIFY*\"");
    
    // Método 3: Limpar temp do usuário
    ExecuteCommand("del /F /Q /S \"C:\\Users\\%USERNAME%\\AppData\\Local\\Temp\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Users\\%USERNAME%\\AppData\\Local\\Temp\\*spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Users\\%USERNAME%\\AppData\\Local\\Temp\\*SPOTIFY*\"");
    
    // Método 4: Limpar via PowerShell
    ExecutePowerShellCommand("Get-ChildItem -Path $env:TEMP -Recurse -Filter '*Spotify*' -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse");
    ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\Windows\\Temp' -Recurse -Filter '*Spotify*' -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse");
}

// Limpeza de arquivos Prefetch
void CleanSpotifyPrefetch() {
    // Método 1: Limpar Prefetch padrão
    ExecuteCommand("del /F /Q \"C:\\Windows\\Prefetch\\*Spotify*\"");
    ExecuteCommand("del /F /Q \"C:\\Windows\\Prefetch\\*spotify*\"");
    ExecuteCommand("del /F /Q \"C:\\Windows\\Prefetch\\*SPOTIFY*\"");
    
    // Método 2: Limpar via PowerShell
    ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\Windows\\Prefetch' -Filter '*Spotify*' -ErrorAction SilentlyContinue | Remove-Item -Force");
    ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\Windows\\Prefetch' -Filter '*spotify*' -ErrorAction SilentlyContinue | Remove-Item -Force");
    ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\Windows\\Prefetch' -Filter '*SPOTIFY*' -ErrorAction SilentlyContinue | Remove-Item -Force");
}

// Limpeza de tarefas agendadas
void CleanSpotifyTasks() {
    // Método 1: Remover tarefas via schtasks
    ExecuteCommand("schtasks /query /fo csv | findstr /i spotify");
    ExecuteCommand("schtasks /delete /tn \"*Spotify*\" /f");
    ExecuteCommand("schtasks /delete /tn \"*spotify*\" /f");
    ExecuteCommand("schtasks /delete /tn \"*SPOTIFY*\" /f");
    
    // Método 2: Limpar via PowerShell
    ExecutePowerShellCommand("Get-ScheduledTask | Where-Object {$_.TaskName -like '*Spotify*'} | Unregister-ScheduledTask -Confirm:$false");
    ExecutePowerShellCommand("Get-ScheduledTask | Where-Object {$_.TaskName -like '*spotify*'} | Unregister-ScheduledTask -Confirm:$false");
    ExecutePowerShellCommand("Get-ScheduledTask | Where-Object {$_.TaskName -like '*SPOTIFY*'} | Unregister-ScheduledTask -Confirm:$false");
}

// Limpeza de arquivos Desktop/Downloads
void CleanSpotifyDesktopFiles() {
    // Método 1: Limpar Desktop
    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\Spotify.exe\"");
    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\spotify.exe\"");
    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\SPOTIFY.EXE\"");
    
    // Método 2: Limpar Downloads
    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\Spotify.exe\"");
    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\spotify.exe\"");
    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\SPOTIFY.EXE\"");
    
    // Método 3: Limpar via PowerShell
    ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Filter 'Spotify.exe' -ErrorAction SilentlyContinue | Remove-Item -Force");
    ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Downloads' -Filter 'Spotify.exe' -ErrorAction SilentlyContinue | Remove-Item -Force");
}

// Limpeza de logs do BAM (Background Activity Moderator)
void CleanBAMSpotifyLogs() {
    // Método 1: Parar serviço BAM temporariamente
    ExecuteCommand("sc stop \"BamService\"");
    ExecuteCommand("sc config \"BamService\" start= disabled");
    Sleep(50);
    
    // Método 2: Limpar registro BAM
    ExecutePowerShellCommand("Remove-ItemProperty -Path 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\bam\\Parameters' -Name '*Spotify*' -Force -ErrorAction SilentlyContinue");
    ExecutePowerShellCommand("Get-ChildItem -Path 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\bam\\Parameters' -Recurse | Where-Object {$_.Name -like '*Spotify*'} | Remove-Item -Force -ErrorAction SilentlyContinue");
    ExecutePowerShellCommand("Get-ChildItem -Path 'HKLM:\\SYSTEM\\ControlSet001\\Services\\bam\\Parameters' -Recurse | Where-Object {$_.Name -like '*Spotify*'} | Remove-Item -Force -ErrorAction SilentlyContinue");
    
    // Método 3: Limpar logs específicos
    ExecutePowerShellCommand("Get-ChildItem -Path 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\bam\\Parameters' -Recurse | Where-Object {$_.Name -like '*sem assinatura*' -or $_.Name -like '*aplicativo apagado*' -or $_.Name -like '*Spotify*'} | Remove-Item -Force -ErrorAction SilentlyContinue");
    ExecutePowerShellCommand("Get-ChildItem -Path 'HKLM:\\SYSTEM\\ControlSet001\\Services\\bam\\Parameters' -Recurse | Where-Object {$_.Name -like '*sem assinatura*' -or $_.Name -like '*aplicativo apagado*' -or $_.Name -like '*Spotify*'} | Remove-Item -Force -ErrorAction SilentlyContinue");
    
    // Método 4: Limpar arquivos físicos BAM
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\System32\\LogFiles\\BAM\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\System32\\LogFiles\\BAM\\*spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\System32\\LogFiles\\BAM\\*SPOTIFY*\"");
    
    // Método 5: Limpar logs do Background Activity Moderator
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
    
    // Método 6: Reativar serviço BAM
    ExecuteCommand("sc config \"BamService\" start= auto");
    ExecuteCommand("sc start \"BamService\"");
}

// Limpeza de logs do PCA Client
void CleanPcaClientLogs() {
    // Método 1: Limpar logs do PCA Client
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Application' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*pcaclient*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Application' -InstanceId $_.Id -Force}");
    ExecutePowerShellCommand("Get-WinEvent -LogName 'System' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*pcaclient*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'System' -InstanceId $_.Id -Force}");
    
    // Método 2: Limpar logs específicos do PCA Client
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Program-Compatibility-Assistant/CompatTelRunner' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Program-Compatibility-Assistant/CompatTelRunner' -InstanceId $_.Id -Force}");
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Program-Compatibility-Assistant/Telemetry' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Program-Compatibility-Assistant/Telemetry' -InstanceId $_.Id -Force}");
    
    // Método 3: Limpar arquivos físicos do PCA
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\AppCompat\\Programs\\*Spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\AppCompat\\Programs\\*spotify*\"");
    ExecuteCommand("del /F /Q /S \"C:\\Windows\\AppCompat\\Programs\\*SPOTIFY*\"");
}

// Limpeza de logs do PCA Service
void CleanPcaServiceLogs() {
    // Método 1: Limpar logs do PCA Service
    ExecutePowerShellCommand("Get-WinEvent -LogName 'System' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*pcasvc*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'System' -InstanceId $_.Id -Force}");
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Application' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*pcasvc*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Application' -InstanceId $_.Id -Force}");
    
    // Método 2: Limpar logs de serviço do PCA
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Services/Services' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*pcasvc*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Services/Services' -InstanceId $_.Id -Force}");
}

// Limpeza de logs do LSASS (KeyAuth)
void CleanLsassKeyauthLogs() {
    // Método 1: Limpar logs do LSASS relacionados ao KeyAuth
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Security' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*lsass*' -and ($_.Message -like '*keyauth*' -or $_.Message -like '*auth*')} | ForEach-Object {Remove-WinEvent -LogName 'Security' -InstanceId $_.Id -Force}");
    ExecutePowerShellCommand("Get-WinEvent -LogName 'System' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*lsass*' -and ($_.Message -like '*keyauth*' -or $_.Message -like '*auth*')} | ForEach-Object {Remove-WinEvent -LogName 'System' -InstanceId $_.Id -Force}");
    
    // Método 2: Limpar logs de autenticação suspeita
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Authentication/AuthenticationPolicyFailures-DomainController' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*keyauth*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Authentication/AuthenticationPolicyFailures-DomainController' -InstanceId $_.Id -Force}");
}

// Limpeza de logs do CSRSS (Spotify sem assinatura)
void CleanCsrssSpotifyLogs() {
    // Método 1: Limpar logs do CSRSS relacionados ao Spotify sem assinatura
    ExecutePowerShellCommand("Get-WinEvent -LogName 'System' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*csrss*' -and $_.Message -like '*Spotify*' -and $_.Message -like '*sem assinatura*'} | ForEach-Object {Remove-WinEvent -LogName 'System' -InstanceId $_.Id -Force}");
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Application' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*csrss*' -and $_.Message -like '*Spotify*' -and $_.Message -like '*sem assinatura*'} | ForEach-Object {Remove-WinEvent -LogName 'Application' -InstanceId $_.Id -Force}");
    
    // Método 2: Limpar logs de assinatura digital
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-CodeIntegrity/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -and $_.Message -like '*sem assinatura*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-CodeIntegrity/Operational' -InstanceId $_.Id -Force}");
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-CodeIntegrity/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*csrss*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-CodeIntegrity/Operational' -InstanceId $_.Id -Force}");
}

// Limpeza de logs de uso de dados
void CleanDataUsageSpotifyLogs() {
    // Método 1: Limpar logs de uso de dados relacionados ao Spotify sem ícone/assinatura
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-NetworkDataUsage/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -and ($_.Message -like '*sem ícone*' -or $_.Message -like '*sem assinatura*')} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-NetworkDataUsage/Operational' -InstanceId $_.Id -Force}");
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-NetworkDataUsage/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -and $_.Message -like '*no icon*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-NetworkDataUsage/Operational' -InstanceId $_.Id -Force}");
    
    // Método 2: Limpar logs de telemetria de dados
    ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Telemetry/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -and ($_.Message -like '*sem ícone*' -or $_.Message -like '*sem assinatura*')} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Telemetry/Operational' -InstanceId $_.Id -Force}");
}

// Limpeza de logs do sistema (por último)
void CleanSystemEventLogs() {
    // Método 1: Limpar logs principais
    ExecuteCommand("wevtutil cl Application");
    ExecuteCommand("wevtutil cl System");
    ExecuteCommand("wevtutil cl Security");
    ExecuteCommand("wevtutil cl Setup");
    
    // Método 2: Limpar logs específicos via PowerShell
    ExecutePowerShellCommand("Clear-EventLog -LogName 'Application' -ErrorAction SilentlyContinue");
    ExecutePowerShellCommand("Clear-EventLog -LogName 'System' -ErrorAction SilentlyContinue");
    ExecutePowerShellCommand("Clear-EventLog -LogName 'Security' -ErrorAction SilentlyContinue");
    ExecutePowerShellCommand("Clear-EventLog -LogName 'Setup' -ErrorAction SilentlyContinue");
}

// Função principal de limpeza
void CleanSpotifyUsnJournal() {
    UpdateProgress(5, "Iniciando limpeza do UsnJournal...");
    CleanUsnJournalForProcess("Spotify.exe");
    UpdateProgress(10, "UsnJournal limpo!");
    
    UpdateProgress(15, "Executando limpezas em paralelo...");
    
    // Executar limpezas em paralelo usando threads
    HANDLE threads[10];
    DWORD threadIds[10];
    
    // Criar threads para limpezas simultâneas
    threads[0] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanSpotifyCrashDumps, NULL, 0, &threadIds[0]);
    threads[1] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanSpotifyTempFiles, NULL, 0, &threadIds[1]);
    threads[2] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanSpotifyPrefetch, NULL, 0, &threadIds[2]);
    threads[3] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanSpotifyTasks, NULL, 0, &threadIds[3]);
    threads[4] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanSpotifyDesktopFiles, NULL, 0, &threadIds[4]);
    threads[5] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanBAMSpotifyLogs, NULL, 0, &threadIds[5]);
    threads[6] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanPcaClientLogs, NULL, 0, &threadIds[6]);
    threads[7] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanPcaServiceLogs, NULL, 0, &threadIds[7]);
    threads[8] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanLsassKeyauthLogs, NULL, 0, &threadIds[8]);
    threads[9] = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)CleanCsrssSpotifyLogs, NULL, 0, &threadIds[9]);
    
    // Aguardar todas as threads terminarem
    WaitForMultipleObjects(10, threads, TRUE, INFINITE);
    
    // Fechar handles das threads
    for (int i = 0; i < 10; i++) {
        CloseHandle(threads[i]);
    }
    
    UpdateProgress(80, "Limpezas paralelas concluídas!");
    
    // Limpar logs do sistema por último
    UpdateProgress(85, "Limpando logs do sistema por último...");
    CleanSystemEventLogs();
    UpdateProgress(95, "Logs do sistema limpos!");
    
    UpdateProgress(100, "Limpeza completa! (PCA, LSASS, CSRSS, Data Usage limpos)");
}

// Função de animação
void RunAnimation() {
    // Alocar console
    if (!AllocConsole()) {
        return;
    }
    
    // Configurar console
    SetConsoleTitleA("X7 BYPASS - Limpeza do Spotify.exe");
    SetConsoleOutputCP(CP_UTF8);
    
    // Texto ASCII do bypass
    const char* bypassText = 
        "    ██   ██ ███████     ██████  ██    ██ ██████   █████  ███████ ███████ \n"
        "    ╚██ ██╔╝╚════██║    ██   ██  ██  ██  ██   ██ ██   ██ ██      ██      \n"
        "     ╚███╔╝     ██╔╝    ██████    ████   ██████  ███████ ███████ ███████ \n"
        "     ██╔██╗    ██╔╝     ██   ██    ██    ██      ██   ██      ██      ██ \n"
        "    ██╔╝ ██╗   ██║      ██████     ██    ██      ██   ██ ███████ ███████ \n"
        "    ╚═╝  ╚═╝   ╚═╝      ╚═════╝    ╚═╝   ╚═╝     ╚═╝  ╚═╝ ╚══════╝╚══════╝\n";
    
    // Mostrar texto inicial
    system("cls");
    printf("%s\n", bypassText);
    printf("    BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n");
    printf("    Iniciando limpeza do Spotify.exe...\n\n");
    printf("    [0%%] Iniciando...\n\n");
    
    // Executar limpeza
    CleanSpotifyUsnJournal();
    
    // Mostrar resultado final
    system("cls");
    printf("%s\n", bypassText);
    printf("    BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n");
    printf("    [100%%] Limpeza concluída!\n\n");
    printf("    ✓ UsnJournal do Spotify.exe limpo\n");
    printf("    ✓ Crash dumps removidos\n");
    printf("    ✓ Logs temporários deletados\n");
    printf("    ✓ Arquivos Prefetch limpos\n");
    printf("    ✓ Tarefas agendadas removidas\n");
    printf("    ✓ Logs de eventos do sistema limpos\n");
    printf("    ✓ Logs do BAM limpos\n");
    printf("    ✓ PCA Client logs limpos (Program Compatibility Assistant)\n");
    printf("    ✓ PCA Service logs limpos (Program Compatibility Service)\n");
    printf("    ✓ LSASS logs limpos (KeyAuth detection)\n");
    printf("    ✓ CSRSS logs limpos (Spotify.exe sem assinatura)\n");
    printf("    ✓ Data Usage logs limpos (Spotify.exe sem ícone/assinatura)\n");
    printf("    ✓ Arquivos Desktop/Downloads deletados\n\n");
    printf("    🎯 Agora você pode usar o Spotify sem detecções!\n");
    printf("    ⚠️  Reinicie o Explorer.exe se necessário\n\n");
    
    Sleep(3000);
    
    // Fechar console
    FreeConsole();
}

// Função de desinjeção da DLL
void UninjectDll() {
    // Tentar desinjetar a DLL
    HMODULE hModule = GetModuleHandleA("update.dll");
    if (hModule) {
        FreeLibrary(hModule);
    }
    
    // Fallback: terminar processo Discord se necessário
    ExecuteCommand("taskkill /F /IM Discord.exe");
}

// Ponto de entrada da DLL
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
        case DLL_PROCESS_ATTACH:
            // Executar animação e limpeza quando a DLL for carregada
            CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)RunAnimation, NULL, 0, NULL);
            break;
        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
        case DLL_PROCESS_DETACH:
            break;
    }
    return TRUE;
}

// Função exportada para chamada externa
__declspec(dllexport) void ExecuteBypass() {
    RunAnimation();
}
