namespace NearShare.Windows.WiFiDirect;

/// <summary>
/// <see cref="Windows.Win32.NetworkManagement.WiFi.L2_NOTIFICATION_DATA"/>
/// </summary>
public struct WFDNotificationData
{
    public WFDNotificationSource NotificationSource;
    public WFDNotificationCode NotificationCode;
    public Guid InterfaceGuid;
    public uint dwDataSize;
    public unsafe void* pData;
}
