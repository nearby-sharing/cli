using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace NearShare.Windows.WiFiDirect;

internal static partial class WiFiDirectNative
{
    [LibraryImport("wlanapi.dll", EntryPoint = "WFDStartUsingGroupInt")]
    public static partial uint StartUsingGroup(SafeFileHandle handle, nint a, out nint cookie);

    [LibraryImport("wlanapi.dll", EntryPoint = "WFDStopUsingGroupInt")]
    public static partial uint StopUsingGroup(SafeFileHandle handle, nint cookie);

    [LibraryImport("wlanapi.dll", EntryPoint = "WFDRegisterNotificationInt")]
    public static partial uint RegisterNotification(SafeFileHandle handle, uint sources, [MarshalAs(UnmanagedType.Bool)] bool ignoreDuplicates, NotificationCallback callback, nint context, nint reserved);
}

public delegate void NotificationCallback(ref WFDNotificationData data, nint context);
