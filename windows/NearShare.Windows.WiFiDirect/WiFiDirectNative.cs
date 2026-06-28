using Microsoft.Win32.SafeHandles;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;
using Windows.Win32.NetworkManagement.WiFi;

namespace NearShare.Windows.WiFiDirect;

internal static partial class WiFiDirectNative
{
    const int PROPERTY_ADD_DEVICE_PROFILE = 16;
    const int PROPERTY_REMOVE_DEVICE_PROFILE = 17;

    [LibraryImport("wlanapi.dll", EntryPoint = "WFDStartUsingGroupInt")]
    public static partial uint StartUsingGroup(SafeFileHandle handle, nint a, out nint cookie);

    [LibraryImport("wlanapi.dll", EntryPoint = "WFDQueryPropertyInt")]
    public static partial uint QueryProperty(SafeFileHandle handle, nint propertyId, out uint size, out nint data);

    [LibraryImport("wlanapi.dll", EntryPoint = "WFDSetPropertyInt")]
    public static partial uint SetProperty(SafeFileHandle handle, nint propertyId, uint size, nint data);

    [LibraryImport("wlanapi.dll", EntryPoint = "WFDFreeMemoryInt")]
    public static partial uint FreeMemory(nint data);

    [LibraryImport("wlanapi.dll", EntryPoint = "WFDStopUsingGroupInt")]
    public static partial uint StopUsingGroup(SafeFileHandle handle, nint cookie);

    [LibraryImport("wlanapi.dll", EntryPoint = "WFDRegisterNotificationInt")]
    public static partial uint RegisterNotification(SafeFileHandle handle, uint sources, [MarshalAs(UnmanagedType.Bool)] bool ignoreDuplicates, NotificationCallback callback, nint context, nint reserved);

    [DllImport("wlanapi.dll", EntryPoint = "WFDStartOpenSessionInt")]
    public static extern unsafe uint StartOpenSession(SafeHandle hClientHandle, in byte* pDeviceAddress, void* pvContext, WFD_OPEN_SESSION_COMPLETE_CALLBACK pfnCallback, PCWSTR profileString, nint fireWallConfiguration, out SafeFileHandle phSessionHandle);

    public static unsafe uint StartOpenSession(SafeHandle hClientHandle, PhysicalAddress pDeviceAddress, nint pvContext, WFD_OPEN_SESSION_COMPLETE_CALLBACK pfnCallback, string profileString, nint fireWallConfiguration, out SafeFileHandle phSessionHandle)
    {
        ushort* pProfileString = Utf16StringMarshaller.ConvertToUnmanaged(profileString);
        try
        {
            fixed (byte* pAddress = pDeviceAddress.GetAddressBytes())
            {
                return StartOpenSession(hClientHandle, pAddress, (void*)pvContext, pfnCallback, (char*)pProfileString, fireWallConfiguration, out phSessionHandle);
            }
        }
        finally
        {
            Utf16StringMarshaller.Free(pProfileString);
        }
    }

    public static string GenerateProfile(string groupName, PhysicalAddress bssid, string ssid, bool isGroupOwner, PhysicalAddress deviceAddress, ReadOnlySpan<byte> psk)
    {
        return $"""
            <?xml version="1.0"?>
            <WFDProfile xmlns="http://www.microsoft.com/networking/WiFiDirect/profile/v1">
                <groupName>{groupName}</groupName>
                <groupID>
                    <deviceAddress>{ToStringFormatted(bssid)}</deviceAddress>
                    <SSID>{ssid}</SSID>
                </groupID>
                <persistent>true</persistent>
                <localSettings>
                    <role>{(isGroupOwner ? "GroupOwner" : "Client")}</role>
                    <deviceAddress>{ToStringFormatted(deviceAddress)}</deviceAddress>
                </localSettings>
                <security>
                    <groupKey>
                        <keyType>networkKey</keyType>
                        <protected>false</protected>
                        <keyMaterial>{Convert.ToHexString(psk)}</keyMaterial>
                    </groupKey>
                </security>
            </WFDProfile>
            """;

        static string ToStringFormatted(PhysicalAddress address)
            => string.Join(":", address.GetAddressBytes().Select(b => b.ToString("X2")));
    }

    public static string GenerateProfile(string groupName, PhysicalAddress bssid, string ssid, bool isGroupOwner, PhysicalAddress deviceAddress, string passphrase)
    {
        return $"""
            <?xml version="1.0"?>
            <WFDProfile xmlns="http://www.microsoft.com/networking/WiFiDirect/profile/v1">
                <groupName>{groupName}</groupName>
                <groupID>
                    <deviceAddress>{ToStringFormatted(bssid)}</deviceAddress>
                    <SSID>{ssid}</SSID>
                </groupID>
                <persistent>true</persistent>
                <localSettings>
                    <role>{(isGroupOwner ? "GroupOwner" : "Client")}</role>
                    <deviceAddress>{ToStringFormatted(deviceAddress)}</deviceAddress>
                </localSettings>
                <security>
                    <groupKey>
                        <keyType>passPhrase</keyType>
                        <protected>false</protected>
                        <keyMaterial>{passphrase}</keyMaterial>
                    </groupKey>
                </security>
            </WFDProfile>
            """;

        static string ToStringFormatted(PhysicalAddress address)
            => string.Join(":", address.GetAddressBytes().Select(b => b.ToString("X2")));
    }
}

public delegate void NotificationCallback(ref WFDNotificationData data, nint context);
