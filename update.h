#pragma once

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

// Constantes para injeção
#define PROCESS_CREATE_THREAD 0x0002
#define PROCESS_QUERY_INFORMATION 0x0400
#define PROCESS_VM_OPERATION 0x0008
#define PROCESS_VM_WRITE 0x0020
#define PROCESS_VM_READ 0x0010
#define MEM_COMMIT 0x1000
#define MEM_RESERVE 0x2000
#define PAGE_READWRITE 0x04
#define MOVEFILE_DELAY_UNTIL_REBOOT 0x00000004

// Constantes para UsnJournal
#define GENERIC_READ 0x80000000
#define GENERIC_WRITE 0x40000000
#define FILE_SHARE_READ 0x00000001
#define FILE_SHARE_WRITE 0x00000002
#define OPEN_EXISTING 3
#define FSCTL_DISMOUNT_VOLUME 0x90020
#define FSCTL_LOCK_VOLUME 0x90018
#define FSCTL_UNLOCK_VOLUME 0x9001C

// Variáveis globais
extern int g_currentProgress;
extern std::string g_currentStatus;
extern std::string g_targetDeletePath;

// Funções principais
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved);
extern "C" __declspec(dllexport) void ExecuteBypass();
extern "C" __declspec(dllexport) BOOL InjectIntoProcess(const char* processName, const char* dllPath);

// Funções auxiliares
void UpdateProgress(int percentage, const std::string& status);
std::string GetProgressBar(int percentage);
void ExecuteCommand(const std::string& command);
void ExecutePowerShellCommand(const std::string& command);

// Funções de limpeza
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

// Funções de controle de serviços
void StopCriticalServices();
void RestartCriticalServices();
void CleanWindowsTemp();
void StopExplorer();
void RestartExplorer();

// Funções de desinjeção
void UnloadFromPanel();
void DeleteSpotifyExe();
void DeleteActiveSpotifyExe();
void UninjectDll();
void TryDeleteTargetExecutable();

// Funções de animação
void RunAnimation();
void AnimationThread();

// Função de injeção
BOOL InjectDLL(DWORD processId, const char* dllPath);
