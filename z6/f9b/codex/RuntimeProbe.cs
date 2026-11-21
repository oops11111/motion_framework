// External dev-only diagnostic helper. No process memory contents are read.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

public sealed class F9bPoint { public int X; public int Y; }
public sealed class F9bRect { public int Left; public int Top; public int Right; public int Bottom; }
public sealed class F9bGeometry {
    public bool Exists; public bool Visible; public bool Enabled;
    public double Left; public double Top; public double Right; public double Bottom;
    public uint Dpi; public F9bRect Window;
}
public sealed class F9bMappings { public bool Complete; public string[] Paths; public int Queries; public int TerminalError; }
public static class F9bProbe {
    [StructLayout(LayoutKind.Sequential)] struct POINT { public int X; public int Y; }
    [StructLayout(LayoutKind.Sequential)] struct RECT { public int Left,Top,Right,Bottom; }
    [StructLayout(LayoutKind.Sequential)] struct SYSINFO { public ushort Arch,Reserved; public uint Page; public IntPtr Min,Max,Mask; public uint Count,Type,Granularity; public ushort Level,Revision; }
    [StructLayout(LayoutKind.Sequential)] struct MBI { public IntPtr Base,Allocation; public uint Protect0; public ushort Partition; public UIntPtr Size; public uint State,Protect,Type; }
    [StructLayout(LayoutKind.Sequential)] struct MOUSEINPUT { public int X,Y; public uint Data,Flags,Time; public UIntPtr Extra; }
    [StructLayout(LayoutKind.Explicit)] struct INPUT { [FieldOffset(0)]public uint Type; [FieldOffset(8)]public MOUSEINPUT Mouse; }
    [DllImport("kernel32.dll",SetLastError=true)] static extern IntPtr OpenProcess(uint access,bool inherit,int pid);
    [DllImport("kernel32.dll")] static extern bool CloseHandle(IntPtr handle);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool QueryFullProcessImageName(IntPtr handle,uint flags,StringBuilder name,ref uint size);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] static extern int GetPackageFullName(IntPtr handle,ref uint size,StringBuilder name);
    [DllImport("kernel32.dll")] static extern void GetNativeSystemInfo(out SYSINFO info);
    [DllImport("kernel32.dll",SetLastError=true)] static extern UIntPtr VirtualQueryEx(IntPtr handle,IntPtr address,out MBI info,UIntPtr size);
    [DllImport("psapi.dll",CharSet=CharSet.Unicode)] static extern uint GetMappedFileNameW(IntPtr handle,IntPtr address,StringBuilder name,uint size);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] static extern uint QueryDosDevice(string drive,StringBuilder target,int size);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hwnd,out RECT rect);
    [DllImport("user32.dll")] static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
    [DllImport("user32.dll")] static extern bool GetCursorPos(out POINT point);
    [DllImport("user32.dll")] static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll",SetLastError=true)] static extern uint SendInput(uint count,INPUT[] inputs,int size);
    static IntPtr Dpi() { IntPtr previous=SetThreadDpiAwarenessContext(new IntPtr(-4)); if(previous==IntPtr.Zero)throw new InvalidOperationException("Thread DPI context unavailable");return previous; }
    static IntPtr Guard(int pid,string exe,string package,long hwnd) {
        if(!Environment.Is64BitProcess)throw new InvalidOperationException("x64 helper required");
        // Query information and synchronization only: no PROCESS_VM_READ, ReadProcessMemory or debug attach.
        IntPtr h=OpenProcess(0x0400|0x1000|0x100000,false,pid);
        if(h==IntPtr.Zero)throw new InvalidOperationException("Owned process query unavailable");
        try {
            uint n=32768;StringBuilder name=new StringBuilder((int)n);
            if(!QueryFullProcessImageName(h,0,name,ref n)||!String.Equals(Path.GetFullPath(name.ToString()),Path.GetFullPath(exe),StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Process image mismatch");
            n=0;if(GetPackageFullName(h,ref n,null)!=122)throw new InvalidOperationException("Package query unavailable");
            name=new StringBuilder((int)n);if(GetPackageFullName(h,ref n,name)!=0||name.ToString()!=package)throw new InvalidOperationException("Process package mismatch");
            if(hwnd!=0){uint owner;GetWindowThreadProcessId(new IntPtr(hwnd),out owner);if(owner!=(uint)pid||!IsWindowVisible(new IntPtr(hwnd)))throw new InvalidOperationException("Owned visible HWND required");}
            return h;
        } catch {CloseHandle(h);throw;}
    }
    static string DosPath(string value) {
        foreach(string root in Directory.GetLogicalDrives()) {
            string drive=root.Substring(0,2);StringBuilder target=new StringBuilder(1024);
            if(QueryDosDevice(drive,target,target.Capacity)>0 && value.StartsWith(target.ToString()+"\\",StringComparison.OrdinalIgnoreCase))return drive+value.Substring(target.Length);
        }
        throw new InvalidOperationException("Mapped device path cannot be resolved");
    }
    public static F9bMappings ModuleMetadata(int pid,string exe,string package) {
        IntPtr h=Guard(pid,exe,package,0);
        try {
            HashSet<string> names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);ulong address=0;int count=0;int terminalError=0;SYSINFO system;GetNativeSystemInfo(out system);ulong maximum=(ulong)system.Max.ToInt64();
            while(address<=maximum) {
                if(++count>200000)throw new InvalidOperationException("Mapped-region query bound reached");
                MBI info;UIntPtr returned=VirtualQueryEx(h,new IntPtr((long)address),out info,(UIntPtr)Marshal.SizeOf(typeof(MBI)));
                if(returned==UIntPtr.Zero){terminalError=Marshal.GetLastWin32Error();break;}
                if(info.State==0x1000 && (info.Type==0x1000000 || info.Type==0x40000)) {
                    StringBuilder name=new StringBuilder(32768);
                    if(GetMappedFileNameW(h,info.Base,name,(uint)name.Capacity)>0) {
                        string leaf=Path.GetFileName(name.ToString());
                        if(String.Equals(leaf,"FluidMotion.L1.dll",StringComparison.OrdinalIgnoreCase)||String.Equals(leaf,"fluidmotion.dll",StringComparison.OrdinalIgnoreCase))names.Add(DosPath(name.ToString()));
                    }
                }
                ulong next=unchecked((ulong)info.Base.ToInt64())+info.Size.ToUInt64();if(next<=address)throw new InvalidOperationException("Mapped-region address overflow");address=next;
            }
            return new F9bMappings {Complete=address>maximum,Paths=new List<string>(names).ToArray(),Queries=count,TerminalError=terminalError};
        } finally {CloseHandle(h);}
    }
    public static F9bGeometry Geometry(int pid,long hwnd,string exe,string package) {
        IntPtr h=Guard(pid,exe,package,hwnd);IntPtr prior=Dpi();
        try {
            AutomationElement root=AutomationElement.FromHandle(new IntPtr(hwnd));
            if(root==null||root.Current.ProcessId!=pid)throw new InvalidOperationException("UIA root mismatch");
            var condition=new AndCondition(new PropertyCondition(AutomationElement.AutomationIdProperty,"SettingsButton"),new PropertyCondition(AutomationElement.ProcessIdProperty,pid));
            AutomationElement marker=root.FindFirst(TreeScope.Descendants,condition);
            if(marker==null)throw new InvalidOperationException("MainPage SettingsButton missing");
            var bounds=marker.Current.BoundingRectangle;
            if(marker.Current.IsOffscreen||!marker.Current.IsEnabled||bounds.Width<=0||bounds.Height<=0)throw new InvalidOperationException("Settings marker not visible/enabled/positive bounds");
            RECT window;
            if(!GetWindowRect(new IntPtr(hwnd),out window))throw new InvalidOperationException("Window bounds unavailable");
            return new F9bGeometry {Exists=true,Visible=!marker.Current.IsOffscreen,Enabled=marker.Current.IsEnabled,Left=bounds.Left,Top=bounds.Top,Right=bounds.Right,Bottom=bounds.Bottom,Dpi=GetDpiForWindow(new IntPtr(hwnd)),Window=new F9bRect{Left=window.Left,Top=window.Top,Right=window.Right,Bottom=window.Bottom}};
        } finally {SetThreadDpiAwarenessContext(prior);CloseHandle(h);}
    }
    public static void Capture(int pid,long hwnd,string exe,string package,string path,int left,int top,int right,int bottom) {
        IntPtr h=Guard(pid,exe,package,hwnd);IntPtr prior=Dpi();
        try {
            RECT window;if(GetForegroundWindow()!=new IntPtr(hwnd))throw new InvalidOperationException("Owned window is not foreground");if(!GetWindowRect(new IntPtr(hwnd),out window)||left<window.Left||top<window.Top||right>window.Right||bottom>window.Bottom||right<=left||bottom<=top)throw new InvalidOperationException("Crop outside owned window");
            using(Bitmap image=new Bitmap(right-left,bottom-top,PixelFormat.Format32bppArgb)) {
                using(Graphics graphics=Graphics.FromImage(image))graphics.CopyFromScreen(left,top,0,0,image.Size,CopyPixelOperation.SourceCopy);
                image.Save(path,ImageFormat.Png);
            }
        } finally {SetThreadDpiAwarenessContext(prior);CloseHandle(h);}
    }
    public static F9bGeometry[] EntryBurst(int pid,long hwnd,string exe,string package,string directory,int left,int top,int right,int bottom) {
        SignalEntry(pid,exe,package);
        List<F9bGeometry> frames=new List<F9bGeometry>();
        for(int i=0;i<12;i++) {
            // Orchestration only: no timestamps, frame intervals or timing statistics are collected.
            System.Threading.Thread.Sleep(15);
            string path=Path.Combine(directory,"entry-"+i.ToString("D2")+".png");
            if(File.Exists(path))throw new InvalidOperationException("Refuse to overwrite visual evidence");
            Capture(pid,hwnd,exe,package,path,left,top,right,bottom);
            frames.Add(Geometry(pid,hwnd,exe,package));
        }
        return frames.ToArray();
    }
    public static F9bPoint Cursor() {
        IntPtr prior=Dpi();try {POINT p;if(!GetCursorPos(out p))throw new InvalidOperationException("Cursor query failed");return new F9bPoint{X=p.X,Y=p.Y};}finally {SetThreadDpiAwarenessContext(prior);}
    }
    static void Move(int x,int y) {
        IntPtr prior=Dpi();try {
            int vx=GetSystemMetrics(76),vy=GetSystemMetrics(77),w=GetSystemMetrics(78),h=GetSystemMetrics(79);
            if(x<vx||y<vy||x>=vx+w||y>=vy+h)throw new InvalidOperationException("Pointer position outside desktop bounds");
            INPUT value=new INPUT();value.Type=0;value.Mouse.X=(int)((long)(x-vx)*65535/(w-1));value.Mouse.Y=(int)((long)(y-vy)*65535/(h-1));value.Mouse.Flags=0x0001|0x8000|0x4000;
            if(SendInput(1,new INPUT[]{value},Marshal.SizeOf(typeof(INPUT)))!=1)throw new InvalidOperationException("Pointer-only SendInput failed");
        } finally {SetThreadDpiAwarenessContext(prior);}
    }
    public static void MoveWithinWindow(int pid,long hwnd,string exe,string package,int x,int y) {
        IntPtr h=Guard(pid,exe,package,hwnd);IntPtr prior=Dpi();
        try {if(GetForegroundWindow()!=new IntPtr(hwnd))throw new InvalidOperationException("Owned window is not foreground");RECT r;if(!GetWindowRect(new IntPtr(hwnd),out r)||x<r.Left||y<r.Top||x>=r.Right||y>=r.Bottom)throw new InvalidOperationException("Pointer target outside owned window");Move(x,y);}finally {SetThreadDpiAwarenessContext(prior);CloseHandle(h);}
    }
    public static void RestoreOriginalPointer(int x,int y) { Move(x,y); } // Caller requires bound original-pointer receipt.
    public static void SignalEntry(int pid,string exe,string package) {
        IntPtr h=Guard(pid,exe,package,0);try {using(var signal=System.Threading.EventWaitHandle.OpenExisting("Local\\FM_PILOT_F9B_ENTRY_"+pid))signal.Set();}finally {CloseHandle(h);}
    }
}
