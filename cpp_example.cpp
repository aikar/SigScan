#include <windows.h>
#include <stdio.h>

DWORD SigScan(const char* szPattern, int offset = 0);
void InitializeSigScan(DWORD ProcessID, const char* Module);
void FinalizeSigScan();
#pragma comment(lib,"SigScanStatic.lib")

int _tmain(int argc, _TCHAR* argv[])
{
	DWORD dwPID = 7784;
	HANDLE hProc = OpenProcess(PROCESS_VM_READ,FALSE, dwPID);
	BYTE data = NULL;
	InitializeSigScan(dwPID, "FFXiMain.dll");
	DWORD memloc = SigScan("83C408DFE0F6C4050F8A610100005FC605");
	FinalizeSigScan();
	ReadProcessMemory(hProc,(LPCVOID) memloc,&data,sizeof(data),NULL);
	printf("memloc: %X - isincombat: %u\n", memloc, data);
	CloseHandle(hProc);
	system("pause");
	return 0;
}
