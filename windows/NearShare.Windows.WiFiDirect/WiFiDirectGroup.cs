using System.ComponentModel;

namespace NearShare.Windows.WiFiDirect;
public readonly record struct WiFiDirectGroup(WiFiDirectHandle Handle, nint Cookie) : IDisposable
{
    public readonly void Dispose()
        => WiFiDirectNative.StopUsingGroup(Handle.Handle, Cookie);

    public static WiFiDirectGroup Start(WiFiDirectHandle handle)
    {
        var hr = WiFiDirectNative.StartUsingGroup(handle.Handle, 0, out var cookie);
        if (hr != 0)
            throw new Win32Exception((int)hr);

        return new WiFiDirectGroup(handle, cookie);
    }
}
