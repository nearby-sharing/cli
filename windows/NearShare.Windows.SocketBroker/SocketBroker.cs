using System.Runtime.InteropServices;

namespace NearShare.Windows;
public static partial class SocketBroker
{
    [StructLayout(LayoutKind.Sequential)]
    struct ServiceTriggerRetrieveSocketsResult
    {
        nint a;
        nint b;
        nint c;
    }

    [LibraryImport("sbservicetrigger.dll")]
    private static partial nint ServiceTriggerEnumerateTransferredSockets([MarshalAs(UnmanagedType.LPWStr)] string serviceName, out nint data);

    [LibraryImport("sbservicetrigger.dll")]
    private static partial nint ServiceTriggerRetrieveSockets([MarshalAs(UnmanagedType.LPWStr)] string serviceName, nint socket, out nint data);

    public static void FreeSockets()
    {
        ServiceTriggerEnumerateTransferredSockets("CDPSvc", out var socketEnumeration);
        ServiceTriggerRetrieveSockets("CDPSvc", socketEnumeration + 20, out _);
    }
}
