using System.Runtime.InteropServices;

namespace TotkRegistryToolkit.Win32;

public enum WindowMode : int { Hidden = 0, Visible = 5 }

public static partial class ConsoleInterop
{
    private static readonly IntPtr _handle = GetConsoleWindow();

    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetConsoleWindow();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr window_handle, int cmd_show_mode);

    public static void SetWindowMode(WindowMode mode)
    {
        ShowWindow(_handle, (int)mode);
    }
}
