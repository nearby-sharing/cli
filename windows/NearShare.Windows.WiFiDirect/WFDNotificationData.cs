using System.Runtime.InteropServices;

namespace NearShare.Windows.WiFiDirect;

/// <summary>
/// <see cref="Windows.Win32.NetworkManagement.WiFi.L2_NOTIFICATION_DATA"/>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct WFDNotificationData
{
    public WFDNotificationSource NotificationSource;
    public WFDNotificationCode NotificationCode;
    public Guid InterfaceGuid;
    public uint dwDataSize;
    public unsafe void* pData;
}

public enum WFDPeerState
{
    Connected = 1,
    Disconnected = 2,
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct WFD_PEER_CHANGED_NOTIFICATION_DATA()
{
    public readonly uint magic = 0x1100101;
    public readonly WFDPeerState state;
    public readonly Guid someId;
    public readonly uint authAlgorithm;
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct WFD_GROUP_REQUEST_NOTIFICATION_DATA()
{
    public readonly uint magic = 0x1400101;
    public readonly uint type; // 1 = Reinvoke, 2 = GOUnsolicited, 3 = Provision
}

[StructLayout(LayoutKind.Sequential)]
public readonly struct WFD_CONNECT_REQUEST_NOTIFICATION_DATA
{

}
