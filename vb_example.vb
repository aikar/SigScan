Imports System.Runtime.InteropServices

Public Class Form1
	<Flags()> _
   Public Enum PROCESS_ACCESS As Integer
		' Specifies all possible access flags for the process object.
		PROCESS_ALL_ACCESS = &H1F0FFF
		PROCESS_CREATE_THREAD = &H2
		PROCESS_DUP_HANDLE = &H40
		PROCESS_QUERY_INFORMATION = &H400
		PROCESS_SET_INFORMATION = &H200
		PROCESS_TERMINATE = &H1
		PROCESS_VM_OPERATION = &H8
		PROCESS_VM_READ = &H10
		PROCESS_VM_WRITE = &H20
		SYNCHRONIZE = &H100000
	End Enum
	Declare Function OpenProcess Lib "kernel32.dll" (ByVal dwDesiredAccess As PROCESS_ACCESS, ByVal bInheritHandle As Boolean, ByVal dwProcessId As Long) As IntPtr
	Declare Function CloseHandle Lib "kernel32.dll" (ByVal hObject As IntPtr) As Boolean
	Private Declare Function ReadProcessMemory Lib "kernel32" (ByVal hProcess As Integer, ByVal lpBaseAddress As Integer, ByRef lpBuffer As Integer, ByVal nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer


	Declare Sub InitializeSigScan Lib "SigScan.dll" (ByVal PID As UInt32, <MarshalAs(UnmanagedType.LPStr)> ByVal szModule)
	Declare Function SigScan Lib "SigScan.dll" (<MarshalAs(UnmanagedType.LPStr)> ByVal lpBuffer As String, ByVal Offset As Int32) As UInt32
	Declare Sub FinalizeSigScan Lib "SigScan.dll" ()


	Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		Dim iPID As Integer
		Dim hProc As IntPtr
		Dim memloc As UInt32
		Dim Data As UInt32
		iPID = 7784
		hProc = OpenProcess(PROCESS_ACCESS.PROCESS_VM_READ, False, iPID)
		InitializeSigScan(iPID,"FFXiMain.dll")
		memloc = SigScan("b0015ec390518b4c24088d4424005068", 36) + &HC
		FinalizeSigScan()

		ReadProcessMemory(hProc, memloc, Data, 4, 0)
		CloseHandle(hProc)
		MsgBox(Data)
		End
	End Sub
End Class