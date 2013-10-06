SigScan
=======

Old project used for Final Fantasy XI Windower project. Allows you to use byte signatures to dynamically locate memory addresses during program runtime.


This is no longer maintained. Please fork to contribute to it.

Readme is a paste of the index.html, it is recommended you view the HTML file at http://aikar.github.io/SigScan

===========


SigScan - Documentation

    About / Why you would use it.
    Function Details
        InitializeSigScan
        SigScan
        FinalizeSigScan
    Finding Signatures on your own.
    Examples in multiple languages.
        C++
        C#
        VB.NET
    Remarks - Remember to cleanup.


About / Why you would use it
Author: Aikar@Windower.net
Upon joining the windower team one of the first things I learned about was the use of byte signatures to find memory addresses. Cliff originally wrote a function to do it called GetAddress that was very basic. The function required actual byte arrays, so it required more work to 'use', and therefor I decided to write my own implementation with more features and make it faster using better techniques to find the destined address, and now we have SigScan, which I am now providing as a DLL to do scans from a 3rd party exe.

The most popular form of reading memory from the game is finding the modules base address then using a static offset to the desired memory location. The problem with this is when changes are made to the DLL, most offsets will 'shift' as the new code is put into the binary. Now all of those static offsets are changed and no longer work.

However there is a technique called "Byte Signature Scanning" that can help you find your desired memory location without using static memlocs by "scanning" for it. This The idea is to scan over the binaries actual CODE, and look for a specific pattern of code that references your desired memory location that doesnt occur anywhere else in the binary.

For Example:
05AB51D7 8338 60 CMP DWORD PTR DS:[EAX],60 05AB51DA 75 0E JNZ SHORT FFXiMain.05AB51EA 05AB51DC 6A 00 PUSH 0 05AB51DE E8 5DFA0C00 CALL FFXiMain.05B84C40 05AB51E3 83C4 04 ADD ESP,4 05AB51E6 84C0 TEST AL,AL 05AB51E8 74 07 JE SHORT FFXiMain.05AB51F1 05AB51EA C605 4C96E105 00 MOV BYTE PTR DS:[5E1964C],0
The very last line contains my desired Memory Address: 0x4C96E105

Every time the game loads, this same exact code will likely still be in the binary (unless the patch changed this specific code), and will be referencing the exact memory location i'm wanting to access. If you don't understand assembly, its ok, i'll give a brief overview of this data. The first column you can ignore, that was the memory addresses those instructions were at at the time of this paste. The 2nd column is the ACTUAL instructions/code in the binary. The 3rd column is just a textual representation of what those instructions are doing so you can read it easier if you program in assembly. On the last line you can see its moving a value of 0 into a byte, and the [5E1964C] is the address its storing to, and thats what I want. On the left hand side you can see that address is in the actual code too (note: hex is stored in reverse order in memory. This is what we are scanning for, the bytes in the middle column, up until the desired memory location (0x4C96E105). So instead of a static address, we can for the code and pull the memory address we want out of that data. From a 3rd party perspecive you can now use ReadProcessMemory on that address, or from an internal perspective, create a pointer to that address.
This greatly will increase the likelyhood your application will still work after the module is updated.
Function Details
The SigScan DLL relies on 3 functions to work.
InitializeSigScan( DWORD PID, const char* Module)
The first parameter is a 32bit unsigned integer, supplying the Process ID you are wanting to read, and the module to find a memory address in.

This function will connect to the process, find the base address, and if the process is a not the current process, copy the entire module to your programs local memory. This is required to achieve high speeds of scanning the memory, as doing it over ReadProcessMemory would be drastically slow.

SigScan(const char* Pattern, int Offset)
This is the primary function of the DLL, that does the actual scanning. You MUST call InitializeSigScan before using this function. It will automatically scan the last Process and Module initialized with InitializeSigScan(). This function accepts a string ascii based pattern of bytes. You must supply 2 characters per byte to check for as you normally would define as 0xXX. If its a single 0xF, you need "0f". So the strings length must be divisible by 2, where every 2 characters represents 1 byte.

Example: "83C408DFE0F6C4050F8A610100005FC605"
This will look for: 0x83 0xC4 0x08 0xDF 0xE0 0xF6 0xC4 0x05 0x0F 0x8A 0x61 0x01 0x00 0x00 0x5F 0xC6 0x05
By default, SigScan will use the next 4 bytes AFTER the end of the signature as the memory address you are wanting to find.
The Offset field is computed into this predefined location. So in most cases Offset should be 0. However, if you find a signature thats like 36 bytes before your desired memory location, you can set offset to 36 and it will look at 36 bytes AFTER the end of the signature. Offset will primarily be 0 in your scans, but is provided to you so you can use signatures a little more distant from the desired location without creating an insanely long signature or using a ton of wildcards.

There are 2 special characters that can be used in the pattern, one is address specification. As stated, by default the memory address it will return is at the end of the signature. But sometimes you may want to use a few bytes AFTER your desired memory location as part of your signature. If you wish to do this, you can specify WHERE in your signature your desired memory location is at with XXXXXXXX,
For example (not a real signature!): "3FBACD300200A1XXXXXXXXB1C4DA"
Now the position of the X's declares where the memory address is. Offset should be 0 when using this, as offset will modify the position, and will not returned your desired memory location!!!

Any unkown character such as ? and invalid hex characters are treated as wildcards, but still must be matched up in 2's.
For example (not a real signature!): "3AB2DFAB????????DEA1FA"
Sometimes the middle of your signature may have other unwanted memory addresses in them, and you need to wildcard those out to skip them. Wildcard anything that will potentially be different every time the game opens - that includes ALL memory addresses that you are not trying to retrieve, otherwise your signature will not be valid the next time the game opens and starts at a different base address.
This function also has special characters that can be used at the start of the pattern to change how it functions.

    ##: If the pattern starts with a ## (eg: "##ABCCDDEE"). This resets the default "return area" to the start of the pattern, and returns the start of the pattern itself (NOT THE VALUE OF THOSE 4 BYTES!). This will return the memory address of the first byte in the signature. This is useful for finding actual functions themselves in the module.
    @@: Similiar to ##, this will return the address to the signature itself, but retails its default position at the end of the signature. This will return the memory address of the next byte after the signature. This is useful for finding actual functions themselves in the module.

Those however will not help much unless you are using a native language with a DLL loaded into the targets process to access those functions.
FinalizeSigScan()
This is a fairly simple function. It simply cleans up the memory used for the scanning when you are done. Always call this when your done scanning a module. Calling InitializeSigScan again will automatically call FinalizeSigScan so if you need to scan multiple modules you do not need to call it until you finish the last.
Finding Signatures on your own
Examples in multiple languages
C++
C++ has 2 different versions you can use. One is static library, so you may link the code directly into your program and not require a DLL to distribute with your application. This is recommended for C++ developers. However you may dynamically link still with SigScan.dll if you wish to use it over multiple applications.
Code for Static Linking (Header file sigscan.h or place in your own header file)
DWORD SigScan(const char* szPattern, int offset = 0); void InitializeSigScan(DWORD ProcessID, const char* Module); void FinalizeSigScan(); #pragma comment(lib,"SigScanStatic.lib")
Code for Dynamic Linking (Header file sigscan.h or place in your own header file)
__declspec(dllimport) DWORD SigScan(const char* szPattern, int offset = 0); __declspec(dllimport) void InitializeSigScan(DWORD ProcessID, const char* Module); __declspec(dllimport) void FinalizeSigScan(); #pragma comment(lib,"SigScan.lib")
Once you have your headers set up, you may utilize the functions as so.
DWORD dwPID = 7784; HANDLE hProc = OpenProcess(PROCESS_VM_READ,FALSE, dwPID); DWORD data = NULL; InitializeSigScan(dwPID,"FFXiMain.dll"); DWORD memloc = ((unsigned int)SigScan("b0015ec390518b4c24088d4424005068",36) + 0xC); FinalizeSigScan(); ReadProcessMemory(hProc,(LPCVOID) memloc,&data,sizeof(data),NULL); printf("time: %u\n",data); CloseHandle(hProc);


C#
One pitfall to .NET is not being able to statically link code, so you will have to import from the DLL and distribute it.
First you will need
using System.Runtime.InteropServices;
Then you need these imports in your main class
[DllImport("SigScan.dll", EntryPoint = "InitializeSigScan")] public static extern void InitializeSigScan(uint iPID,[MarshalAs(UnmanagedType.LPStr)] string szModule); [DllImport("SigScan.dll", EntryPoint = "SigScan")] public static extern UInt32 SigScan([MarshalAs(UnmanagedType.LPStr)] string Pattern, int Offset); [DllImport("SigScan.dll", EntryPoint = "FinalizeSigScan")] public static extern void FinalizeSigScan();
Then you can use the DLL like so
uint iPID = 7784; IntPtr hProc = OpenProcess(ProcessAccessFlags.PROCESS_VM_READ, false, iPID); InitializeSigScan(iPID,"FFXiMain.dll"); uint memloc = SigScan("b0015ec390518b4c24088d4424005068", 36) + 0x0C; FinalizeSigScan(); uint Data = 0; ReadProcessMemory(hProc, memloc, ref Data, 4, 0); CloseHandle(hProc); Console.WriteLine(Data);
VB.NET
One pitfall to .NET is not being able to statically link code, so you will have to import from the DLL and distribute it.
Note: VB.NET is not a real programming language, and needs to die in a fire. If you're using this, stop learning how to use signatures and learn another language please :(
First you will need
Imports System.Runtime.InteropServices
Then you need these imports in your main class
Declare Sub InitializeSigScan Lib "SigScan.dll" (ByVal PID As UInt32,<MarshalAs(UnmanagedType.LPStr)> ByVal szModule As String) Declare Function SigScan Lib "SigScan.dll" (<MarshalAs(UnmanagedType.LPStr)> ByVal Buffer As String, ByVal Offset As Int32) As UInt32 Declare Sub FinalizeSigScan Lib "SigScan.dll" ()
Then you can use the DLL like so
Dim iPID As Integer Dim hProc As IntPtr Dim memloc As UInt32 Dim Data As UInt32 iPID = 7784 hProc = OpenProcess(PROCESS_ACCESS.PROCESS_VM_READ, False, iPID) InitializeSigScan(iPID,"FFXiMain.dll") memloc = SigScan("b0015ec390518b4c24088d4424005068", 36) + &HC FinalizeSigScan() ReadProcessMemory(hProc, memloc, Data, 4, 0) CloseHandle(hProc) MsgBox(Data)
Remarks - Remember to cleanup
It's important you remember to call FinalizeSigScan() after finding all the signatures you need to find. If you do not do so, around 2.5mb of memory will be sitting there unused and wasted. You should clean up after you are done and save your users memory! This is automatically done when the DLL is unloaded, so if you do forgot itll clean up properly on application close, but you should do this when you are done always.

Also, note that this method of finding memory addresses is not 100% fullproof. If Square Enix changes some of the physical code in the area you built the signature off of... it will break! This does not happen much, but note you may need to repair/find a new signature after a patch one day. Don't expect there to never be problems ever again.
