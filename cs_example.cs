using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
namespace CSSigScanTest
{
	class Program
	{
		enum ProcessAccessFlags
		{
			PROCESS_ALL_ACCESS = 0x1F0FFF,
			PROCESS_CREATE_THREAD = 0x2,
			PROCESS_DUP_HANDLE = 0x40,
			PROCESS_QUERY_INFORMATION = 0x400,
			PROCESS_SET_INFORMATION = 0x200,
			PROCESS_TERMINATE = 0x1,
			PROCESS_VM_OPERATION = 0x8,
			PROCESS_VM_READ = 0x10,
			PROCESS_VM_WRITE = 0x20,
			SYNCHRONIZE = 0x100000
		}
		[DllImport("kernel32.dll")]
		static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwProcessId);
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CloseHandle(IntPtr hObject);
		[DllImport("kernel32.dll", SetLastError = true)]
		static extern bool ReadProcessMemory(IntPtr hProcess, uint lpBaseAddress, ref uint lpBuffer, int dwSize, int lpNumberOfBytesRead);

		[DllImport("SigScan.dll", EntryPoint = "InitializeSigScan")]
		public static extern void InitializeSigScan(uint iPID,[MarshalAs(UnmanagedType.LPStr)] string Module);
		[DllImport("SigScan.dll", EntryPoint = "SigScan")]
		public static extern UInt32 SigScan([MarshalAs(UnmanagedType.LPStr)] string Pattern, int Offset);
		[DllImport("SigScan.dll", EntryPoint = "FinalizeSigScan")]
		public static extern void FinalizeSigScan();
		static void Main(string[] args)
		{
			uint iPID = 7784;
			IntPtr hProc = OpenProcess(ProcessAccessFlags.PROCESS_VM_READ, false, iPID);
			InitializeSigScan(iPID,"FFXiMain.dll");
			uint memloc = SigScan("b0015ec390518b4c24088d4424005068", 36) + 0x0C;
			FinalizeSigScan();
			uint Data = 0;
			ReadProcessMemory(hProc, memloc, ref Data, 4, 0);
			CloseHandle(hProc);
			Console.WriteLine(Data);
			Console.ReadLine();
		}
	}
}
