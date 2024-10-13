namespace SoundBoard

open System
open System.Diagnostics
open System.Runtime.InteropServices
open System.Windows.Forms

module Keyboard =
    let private WH_KEYBOARD_LL = 13
    let private WM_KEYDOWN = IntPtr(0x0100)

    type private LowLevelKeyboardProc = delegate of nCode: int * wParam: IntPtr * lParam: IntPtr -> IntPtr

    [<DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)>]
    extern IntPtr private SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId)

    [<DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)>]
    extern [<MarshalAs(UnmanagedType.Bool)>] bool private UnhookWindowsHookEx(IntPtr hhk)

    [<DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)>]
    extern IntPtr private CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)

    [<DllImport("kernel32.dll",
                CallingConvention = CallingConvention.StdCall,
                CharSet = CharSet.Auto,
                SetLastError = true)>]
    extern IntPtr private GetModuleHandle(string lpModuleName)

    let private proc (callback: Keys -> unit) (nCode: int) (wParam: IntPtr) (lParam: IntPtr) =
        if nCode >= 0 && wParam = WM_KEYDOWN then
            let keyCode = Marshal.ReadInt32(lParam)
            callback (enum<Keys> (keyCode))

        CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam)

    let SetHook (callback: Keys -> unit) : unit =
        use curProcess = Process.GetCurrentProcess()
        use curModule = curProcess.MainModule

        SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc(proc callback), GetModuleHandle(curModule.ModuleName), 0u)
        |> ignore
