using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using Windows.Win32.Foundation;
using static Windows.Win32.PInvoke;

namespace NearShare.Windows.WiFiDirect;

public readonly record struct WiFiDirectHandle(SafeFileHandle Handle) : IDisposable
{
    public void Dispose()
        => WFDCloseHandle(Handle);

    public event NotificationCallback Notification
    {
        add
        {
            var hr = (HRESULT)WiFiDirectNative.RegisterNotification(
                Handle,
                (uint)WFDNotificationSource.WiFiDirectManager, ignoreDuplicates: false,
                value, context: 0,
                0
            );
            hr.ThrowOnFailure();
        }
        remove => throw new NotImplementedException();
    }

    public static WiFiDirectHandle Open()
    {
        var hr = WFDOpenHandle(WFD_API_VERSION, out _, out SafeFileHandle clientHandle);
        if (hr != 0)
            throw new Win32Exception((int)hr);

        return new WiFiDirectHandle(clientHandle);
    }
}
