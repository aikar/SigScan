#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

#ifdef SIGSCANDLL_EXPORTS
#define SIGSCAN_API __declspec(dllexport)
#else
#define SIGSCAN_API __declspec(dllimport)
#endif


SIGSCAN_API DWORD SigScan(const char* szPattern, int offset = 0);
SIGSCAN_API void InitializeSigScan(DWORD ProcessID, const char* szModule);
SIGSCAN_API void FinalizeSigScan();
