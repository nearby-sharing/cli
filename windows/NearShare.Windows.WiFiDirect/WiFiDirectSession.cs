using System.ComponentModel;
using System.Net.NetworkInformation;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace NearShare.Windows.WiFiDirect;

public readonly record struct WiFiDirectSession(WiFiDirectHandle Handle, HANDLE SessionHandle)
{
    public static unsafe Task<WiFiDirectSession> ConnectAsync(WiFiDirectHandle handle, PhysicalAddress address)
    {
        TaskCompletionSource<WiFiDirectSession> promise = new();

        fixed (byte* pDeviceAddress = address.GetAddressBytes())
        {
            var hr = WFDStartOpenSession(
                handle.Handle, pDeviceAddress,
                pvContext: null,
                (HANDLE hSessionHandle, void* pvContext, Guid guidSessionInterface, uint dwError, uint dwReasonCode) =>
                {
                    if (dwError != 0)
                    {
                        promise.TrySetException(new Win32Exception((int)dwError));
                        return;
                    }

                    promise.TrySetResult(new WiFiDirectSession(handle, hSessionHandle));
                },
                out _
            );
        }

        return promise.Task;
    }
}
