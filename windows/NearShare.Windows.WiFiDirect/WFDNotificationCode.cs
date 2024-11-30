namespace NearShare.Windows.WiFiDirect;

public enum WFDNotificationCode : uint
{
    GroupRequest = 0x100000,
    PeerStateChanged = 0x100001,
    StateChanged = 0x100002,
    DiscoveryComplete = 0x100003,
    ConnectRequest = 0x100004,
    ConnectionFailed = 0x10000B,
    RoleChanged = 0x10000C,
    LegacyPeerConnectionFailed = 0x10000D,
}
