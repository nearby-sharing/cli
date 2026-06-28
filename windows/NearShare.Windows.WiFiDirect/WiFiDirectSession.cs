using System.ComponentModel;
using System.Net.NetworkInformation;
using Windows.Win32.Foundation;

namespace NearShare.Windows.WiFiDirect;

public readonly record struct WiFiDirectSession(WiFiDirectHandle Handle, HANDLE SessionHandle)
{
    public static unsafe Task<WiFiDirectSession> ConnectAsync(WiFiDirectHandle handle, PhysicalAddress address, string ssid, string passphrase)
    {
        TaskCompletionSource<WiFiDirectSession> promise = new();

        var profile = WiFiDirectNative.GenerateProfile(ssid, address, ssid, isGroupOwner: false, PhysicalAddress.Parse("28:A0:6B:C8:BC:B7"), passphrase);
        Console.WriteLine(profile.ToString());
        var hr = WiFiDirectNative.StartOpenSession(
            handle.Handle,
            address,
            pvContext: 0,
            (hSessionHandle, pvContext, guidSessionInterface, dwError, dwReasonCode) =>
            {
                Console.WriteLine($"interface={guidSessionInterface} error={dwError} reason={dwReasonCode}");
                if (dwError != 0)
                {
                    promise.TrySetException(new Win32Exception((int)dwError));
                    return;
                }

                promise.TrySetResult(new WiFiDirectSession(handle, hSessionHandle));
            },
            profile,
            fireWallConfiguration: 0,
            out var hSession
        );

        Console.WriteLine($"hr={hr}");

        return promise.Task;
    }
}
