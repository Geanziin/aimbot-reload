#include <windows.h>
#include <iostream>
#include <string>
#include <vector>
#include <tlhelp32.h>
#include <psapi.h>
#include <shlwapi.h>

#pragma comment(lib, "shlwapi.lib")

// Constantes para injeção
#define PROCESS_CREATE_THREAD 0x0002
#define PROCESS_QUERY_INFORMATION 0x0400
#define PROCESS_VM_OPERATION 0x0008
#define PROCESS_VM_WRITE 0x0020
#define PROCESS_VM_READ 0x0010
#define MEM_COMMIT 0x1000
#define MEM_RESERVE 0x2000
#define PAGE_READWRITE 0x04

class DLLInjector {
private:
    std::string dllPath;
    
public:
    DLLInjector(const std::string& path) : dllPath(path) {}
    
    // Buscar processo pelo nome
    DWORD FindProcessByName(const std::string& processName) {
        HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (hSnapshot == INVALID_HANDLE_VALUE) {
            std::cout << "ERRO: Não foi possível criar snapshot de processos!" << std::endl;
            return 0;
        }

        PROCESSENTRY32 pe32;
        pe32.dwSize = sizeof(PROCESSENTRY32);
        DWORD processId = 0;

        if (Process32First(hSnapshot, &pe32)) {
            do {
                if (_stricmp(pe32.szExeFile, processName.c_str()) == 0) {
                    processId = pe32.th32ProcessID;
                    break;
                }
            } while (Process32Next(hSnapshot, &pe32));
        }

        CloseHandle(hSnapshot);
        return processId;
    }
    
    // Listar todos os processos disponíveis
    void ListProcesses() {
        HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (hSnapshot == INVALID_HANDLE_VALUE) {
            std::cout << "ERRO: Não foi possível criar snapshot de processos!" << std::endl;
            return;
        }

        PROCESSENTRY32 pe32;
        pe32.dwSize = sizeof(PROCESSENTRY32);
        
        std::cout << "\n=== PROCESSOS DISPONÍVEIS ===" << std::endl;
        std::cout << "PID\t\tNome do Processo" << std::endl;
        std::cout << "----------------------------------------" << std::endl;

        if (Process32First(hSnapshot, &pe32)) {
            do {
                std::cout << pe32.th32ProcessID << "\t\t" << pe32.szExeFile << std::endl;
            } while (Process32Next(hSnapshot, &pe32));
        }

        CloseHandle(hSnapshot);
    }
    
    // Injetar DLL em processo específico
    BOOL InjectDLL(DWORD processId) {
        HANDLE processHandle = NULL;
        LPVOID allocatedMemory = NULL;
        HANDLE remoteThread = NULL;

        try {
            // Verificar se o arquivo existe
            if (!PathFileExistsA(dllPath.c_str())) {
                std::cout << "ERRO: Arquivo DLL não encontrado: " << dllPath << std::endl;
                return FALSE;
            }

            // Abrir o processo com permissões necessárias
            DWORD processAccess = PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ;
            processHandle = OpenProcess(processAccess, FALSE, processId);

            if (processHandle == NULL) {
                std::cout << "ERRO: Não foi possível abrir o processo!" << std::endl;
                std::cout << "Execute como Administrador." << std::endl;
                return FALSE;
            }

            // Obter handle do kernel32.dll
            HMODULE moduleHandle = GetModuleHandleA("kernel32.dll");
            if (moduleHandle == NULL) {
                std::cout << "ERRO: Não foi possível obter handle do kernel32!" << std::endl;
                return FALSE;
            }

            // Obter endereço do LoadLibraryA
            FARPROC procAddress = GetProcAddress(moduleHandle, "LoadLibraryA");
            if (procAddress == NULL) {
                std::cout << "ERRO: Não foi possível obter endereço de LoadLibraryA!" << std::endl;
                return FALSE;
            }

            // Alocar memória no processo alvo
            size_t dllPathLen = dllPath.length() + 1;
            allocatedMemory = VirtualAllocEx(processHandle, NULL, dllPathLen, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            if (allocatedMemory == NULL) {
                std::cout << "ERRO: Não foi possível alocar memória no processo alvo!" << std::endl;
                return FALSE;
            }

            // Escrever o caminho da DLL na memória alocada
            SIZE_T bytesWritten;
            if (!WriteProcessMemory(processHandle, allocatedMemory, dllPath.c_str(), dllPathLen, &bytesWritten)) {
                std::cout << "ERRO: Não foi possível escrever na memória do processo!" << std::endl;
                return FALSE;
            }

            // Criar thread remota para carregar a DLL
            remoteThread = CreateRemoteThread(processHandle, NULL, 0, (LPTHREAD_START_ROUTINE)procAddress, allocatedMemory, 0, NULL);

            if (remoteThread == NULL) {
                std::cout << "ERRO: Não foi possível criar thread remota!" << std::endl;
                return FALSE;
            }

            std::cout << "Aguardando injeção..." << std::endl;

            // Aguardar a thread remota completar (30 segundos timeout)
            DWORD waitResult = WaitForSingleObject(remoteThread, 30000);

            if (waitResult == WAIT_OBJECT_0) { // Sucesso
                std::cout << "✓ DLL injetada com sucesso!" << std::endl;
                return TRUE;
            }
            else if (waitResult == WAIT_TIMEOUT) {
                std::cout << "TIMEOUT: A injeção demorou muito tempo!" << std::endl;
                return FALSE;
            }
            else {
                std::cout << "ERRO: Falha ao aguardar conclusão da injeção! Código: " << waitResult << std::endl;
                return FALSE;
            }
        }
        catch (...) {
            std::cout << "ERRO inesperado na injeção!" << std::endl;
            return FALSE;
        }
        finally {
            // Limpar recursos
            if (remoteThread != NULL)
                CloseHandle(remoteThread);
            if (allocatedMemory != NULL)
                VirtualFreeEx(processHandle, allocatedMemory, 0, MEM_RELEASE);
            if (processHandle != NULL)
                CloseHandle(processHandle);
        }
    }
    
    // Injetar por nome do processo
    BOOL InjectByName(const std::string& processName) {
        DWORD processId = FindProcessByName(processName);
        if (processId == 0) {
            std::cout << "ERRO: Processo '" << processName << "' não está em execução!" << std::endl;
            return FALSE;
        }
        
        std::cout << "Processo encontrado: " << processName << " (PID: " << processId << ")" << std::endl;
        return InjectDLL(processId);
    }
    
    // Injetar por PID
    BOOL InjectByPID(DWORD processId) {
        return InjectDLL(processId);
    }
};

void ShowBanner() {
    std::cout << R"(
    ██   ██ ███████     ██████  ██    ██ ██████   █████  ███████ ███████ 
    ╚██ ██╔╝╚════██║    ██   ██  ██  ██  ██   ██ ██   ██ ██      ██      
     ╚███╔╝     ██╔╝    ██████    ████   ██████  ███████ ███████ ███████ 
     ██╔██╗    ██╔╝     ██   ██    ██    ██      ██   ██      ██      ██ 
    ██╔╝ ██╗   ██║      ██████     ██    ██      ██   ██ ███████ ███████ 
    ╚═╝  ╚═╝   ╚═╝      ╚═════╝    ╚═╝   ╚═╝     ╚═╝  ╚═╝ ╚══════╝╚══════╝
)" << std::endl;
    std::cout << "\n    X7 BYPASS - Injetor Universal C++" << std::endl;
    std::cout << "    ======================================" << std::endl;
}

void ShowMenu() {
    std::cout << "\n=== MENU PRINCIPAL ===" << std::endl;
    std::cout << "1. Injetar por nome do processo" << std::endl;
    std::cout << "2. Injetar por PID" << std::endl;
    std::cout << "3. Listar processos disponíveis" << std::endl;
    std::cout << "4. Injeção rápida (Discord)" << std::endl;
    std::cout << "5. Injeção rápida (WinRAR)" << std::endl;
    std::cout << "6. Injeção rápida (Spotify)" << std::endl;
    std::cout << "7. Injeção rápida (Chrome)" << std::endl;
    std::cout << "8. Injeção rápida (Firefox)" << std::endl;
    std::cout << "9. Injeção rápida (Steam)" << std::endl;
    std::cout << "0. Sair" << std::endl;
    std::cout << "\nEscolha uma opção: ";
}

int main() {
    // Configurar console para UTF-8
    SetConsoleOutputCP(CP_UTF8);
    SetConsoleTitleA("X7 BYPASS - Injetor Universal");
    
    // Definir caminho da DLL
    char userName[256];
    DWORD userNameLen = sizeof(userName);
    GetUserNameA(userName, &userNameLen);
    
    std::string dllPath = "C:\\Users\\" + std::string(userName) + "\\AppData\\Local\\Discord\\update.dll";
    
    // Verificar se a DLL existe
    if (!PathFileExistsA(dllPath.c_str())) {
        std::cout << "AVISO: DLL não encontrada no caminho padrão: " << dllPath << std::endl;
        std::cout << "Digite o caminho completo da DLL: ";
        std::getline(std::cin, dllPath);
        
        if (!PathFileExistsA(dllPath.c_str())) {
            std::cout << "ERRO: DLL não encontrada!" << std::endl;
            system("pause");
            return 1;
        }
    }
    
    DLLInjector injector(dllPath);
    
    ShowBanner();
    std::cout << "DLL encontrada: " << dllPath << std::endl;
    
    int choice;
    std::string processName;
    DWORD processId;
    
    while (true) {
        ShowMenu();
        std::cin >> choice;
        
        switch (choice) {
            case 1: {
                std::cout << "\nDigite o nome do processo (ex: discord.exe): ";
                std::cin.ignore();
                std::getline(std::cin, processName);
                
                if (injector.InjectByName(processName)) {
                    std::cout << "\n✓ Injeção realizada com sucesso!" << std::endl;
                    std::cout << "✓ Métodos Tavinho aplicados!" << std::endl;
                } else {
                    std::cout << "\n✗ Falha na injeção!" << std::endl;
                }
                break;
            }
            
            case 2: {
                std::cout << "\nDigite o PID do processo: ";
                std::cin >> processId;
                
                if (injector.InjectByPID(processId)) {
                    std::cout << "\n✓ Injeção realizada com sucesso!" << std::endl;
                    std::cout << "✓ Métodos Tavinho aplicados!" << std::endl;
                } else {
                    std::cout << "\n✗ Falha na injeção!" << std::endl;
                }
                break;
            }
            
            case 3: {
                injector.ListProcesses();
                break;
            }
            
            case 4: {
                std::cout << "\nInjetando no Discord..." << std::endl;
                if (injector.InjectByName("Discord.exe")) {
                    std::cout << "\n✓ Injeção realizada com sucesso!" << std::endl;
                    std::cout << "✓ Métodos Tavinho aplicados!" << std::endl;
                } else {
                    std::cout << "\n✗ Falha na injeção!" << std::endl;
                }
                break;
            }
            
            case 5: {
                std::cout << "\nInjetando no WinRAR..." << std::endl;
                if (injector.InjectByName("WinRAR.exe")) {
                    std::cout << "\n✓ Injeção realizada com sucesso!" << std::endl;
                    std::cout << "✓ Métodos Tavinho aplicados!" << std::endl;
                } else {
                    std::cout << "\n✗ Falha na injeção!" << std::endl;
                }
                break;
            }
            
            case 6: {
                std::cout << "\nInjetando no Spotify..." << std::endl;
                if (injector.InjectByName("Spotify.exe")) {
                    std::cout << "\n✓ Injeção realizada com sucesso!" << std::endl;
                    std::cout << "✓ Métodos Tavinho aplicados!" << std::endl;
                } else {
                    std::cout << "\n✗ Falha na injeção!" << std::endl;
                }
                break;
            }
            
            case 7: {
                std::cout << "\nInjetando no Chrome..." << std::endl;
                if (injector.InjectByName("chrome.exe")) {
                    std::cout << "\n✓ Injeção realizada com sucesso!" << std::endl;
                    std::cout << "✓ Métodos Tavinho aplicados!" << std::endl;
                } else {
                    std::cout << "\n✗ Falha na injeção!" << std::endl;
                }
                break;
            }
            
            case 8: {
                std::cout << "\nInjetando no Firefox..." << std::endl;
                if (injector.InjectByName("firefox.exe")) {
                    std::cout << "\n✓ Injeção realizada com sucesso!" << std::endl;
                    std::cout << "✓ Métodos Tavinho aplicados!" << std::endl;
                } else {
                    std::cout << "\n✗ Falha na injeção!" << std::endl;
                }
                break;
            }
            
            case 9: {
                std::cout << "\nInjetando no Steam..." << std::endl;
                if (injector.InjectByName("steam.exe")) {
                    std::cout << "\n✓ Injeção realizada com sucesso!" << std::endl;
                    std::cout << "✓ Métodos Tavinho aplicados!" << std::endl;
                } else {
                    std::cout << "\n✗ Falha na injeção!" << std::endl;
                }
                break;
            }
            
            case 0: {
                std::cout << "\nSaindo..." << std::endl;
                return 0;
            }
            
            default: {
                std::cout << "\nOpção inválida!" << std::endl;
                break;
            }
        }
        
        std::cout << "\nPressione Enter para continuar...";
        std::cin.ignore();
        std::cin.get();
        system("cls");
        ShowBanner();
    }
    
    return 0;
}
