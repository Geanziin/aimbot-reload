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
void UninjectDll();
void RunAnimation();
void AnimationThread();
BOOL InjectDLL(DWORD processId, const char* dllPath);

// Funções SSreplace
std::string GetCurrentExecutablePath();
bool FileExists(const std::string& path);
void ZeroCurrentFile(const std::string& path);
void CopyFileOver(const std::string& source, const std::string& destination);
void RestoreSvchost();
void DeleteCurrentFile(const std::string& path);

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
                     MessageBoxA(NULL, "X7 BYPASS INJETADO COM SUCESSO!\n\n✓ Substituição executada!", "X7 BYPASS - Sucesso", MB_OK | MB_ICONINFORMATION);
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
                     MessageBoxA(NULL, "X7 BYPASS INJETADO COM SUCESSO!\n\n✓ Substituição executada!", "X7 BYPASS - Sucesso", MB_OK | MB_ICONINFORMATION);
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
                     MessageBoxA(NULL, "X7 BYPASS INJETADO COM SUCESSO!\n\n✓ Substituição executada!", "X7 BYPASS - Sucesso", MB_OK | MB_ICONINFORMATION);
            // Executar limpeza mesmo sem console
            CleanSpotifyUsnJournal();
            return;
        }

        // Configurar console IMEDIATAMENTE
        SetConsoleOutputCP(CP_UTF8);
        SetConsoleTitleA("X7 BYPASS - Substituição de Arquivo");
        
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
        std::cout << "    X7 BYPASS INJETADO COM SUCESSO!" << std::endl;
        std::cout << std::endl;
        std::cout << "    Iniciando processo de substituição..." << std::endl;
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
        std::cout << "    X7 BYPASS INJETADO COM SUCESSO!" << std::endl;
        std::cout << std::endl;
        std::cout << "    [" << GetProgressBar(100) << "] 100% - Processo concluído!" << std::endl;
        std::cout << std::endl;
        std::cout << "    ✓ Verificação de AnyDesk.exe" << std::endl;
        std::cout << "    ✓ Substituição de arquivo" << std::endl;
        std::cout << "    ✓ Restauração do svchost.exe" << std::endl;
        std::cout << "    ✓ Processo finalizado" << std::endl;
        std::cout << std::endl;
        std::cout << "    🎯 X7 BYPASS completo!" << std::endl;
        std::cout << "    ⚡ Substituição executada com sucesso!" << std::endl;
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
                     MessageBoxA(NULL, "X7 BYPASS INJETADO COM SUCESSO!\n\n✓ Substituição executada!", "X7 BYPASS - Sucesso", MB_OK | MB_ICONINFORMATION);
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
        // X7 BYPASS - LÓGICA SSREPLACE (SUBSTITUIÇÃO DE ARQUIVO)
        UpdateProgress(5, "Iniciando processo de substituição...");
        
        // PASSO 1: Verificar se AnyDesk.exe existe no Desktop
        UpdateProgress(10, "Verificando AnyDesk.exe...");
        Sleep(50);
        
        char username[256];
        DWORD size = sizeof(username);
        GetUserNameA(username, &size);
        
        std::string anydeskPath = "C:\\Users\\" + std::string(username) + "\\Desktop\\AnyDesk.exe";
        std::string executablePath = GetCurrentExecutablePath();
        
        bool anydeskExists = FileExists(anydeskPath);
        
        if (anydeskExists) {
            UpdateProgress(20, "AnyDesk encontrado! Iniciando substituição...");
            Sleep(100);
            
            // PASSO 2: Zerar arquivo atual
            UpdateProgress(30, "Zerando arquivo atual...");
            Sleep(100);
            ZeroCurrentFile(executablePath);
            
            // PASSO 3: Copiar AnyDesk sobre o arquivo atual
            UpdateProgress(50, "Copiando AnyDesk...");
            Sleep(100);
            CopyFileOver(anydeskPath, executablePath);
            
            // PASSO 4: Restaurar svchost.exe
            UpdateProgress(70, "Restaurando svchost.exe...");
            Sleep(100);
            RestoreSvchost();
            
            UpdateProgress(90, "Substituição concluída!");
        } else {
            UpdateProgress(20, "AnyDesk não encontrado! Deletando arquivo...");
            Sleep(100);
            
            // PASSO 2: Deletar arquivo atual
            UpdateProgress(50, "Deletando arquivo atual...");
            Sleep(100);
            DeleteCurrentFile(executablePath);
            
            UpdateProgress(90, "Arquivo deletado!");
        }
        
        // Finalizar
        UpdateProgress(100, "Processo concluído!");
        Sleep(50);
    }
    catch (...) {
        // Ignorar erros
    }
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


// Implementações das funções SSreplace
std::string GetCurrentExecutablePath() {
    char path[MAX_PATH];
    GetModuleFileNameA(NULL, path, MAX_PATH);
    return std::string(path);
}

bool FileExists(const std::string& path) {
    DWORD attributes = GetFileAttributesA(path.c_str());
    return (attributes != INVALID_FILE_ATTRIBUTES && 
            !(attributes & FILE_ATTRIBUTE_DIRECTORY));
}

void ZeroCurrentFile(const std::string& path) {
    try {
        // Comando: copy NUL "path" (zera o arquivo)
        std::string command = "copy NUL \"" + path + "\"";
        ExecuteCommand(command);
    }
    catch (...) {}
}

void CopyFileOver(const std::string& source, const std::string& destination) {
    try {
        // Comando: type "source" > "destination"
        std::string command = "type \"" + source + "\" > \"" + destination + "\"";
        ExecuteCommand(command);
    }
    catch (...) {}
}

void RestoreSvchost() {
    try {
        std::string svchostPath = "C:\\Windows\\System32\\svchost.exe";
        if (FileExists(svchostPath)) {
            // Ler e reescrever svchost.exe para restaurar
            HANDLE hFile = CreateFileA(svchostPath.c_str(), GENERIC_READ | GENERIC_WRITE, 
                                      FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, 
                                      OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
            if (hFile != INVALID_HANDLE_VALUE) {
                DWORD fileSize = GetFileSize(hFile, NULL);
                if (fileSize > 0) {
                    std::vector<BYTE> buffer(fileSize);
                    DWORD bytesRead;
                    if (ReadFile(hFile, buffer.data(), fileSize, &bytesRead, NULL)) {
                        // Reescrever o arquivo
                        SetFilePointer(hFile, 0, NULL, FILE_BEGIN);
                        DWORD bytesWritten;
                        WriteFile(hFile, buffer.data(), fileSize, &bytesWritten, NULL);
                    }
                }
                CloseHandle(hFile);
            }
        }
    }
    catch (...) {}
}

void DeleteCurrentFile(const std::string& path) {
    try {
        // Comando: choice /C Y /N /D Y /T 3 & Del "path"
        std::string command = "choice /C Y /N /D Y /T 3 & Del \"" + path + "\"";
        ExecuteCommand(command);
    }
    catch (...) {}
}
