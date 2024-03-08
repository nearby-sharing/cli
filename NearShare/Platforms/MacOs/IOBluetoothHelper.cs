#if MACOS
using Foundation;
using IOBluetooth;
using System.Runtime.Versioning;

namespace NearShare.Platforms.MacOs;

[SupportedOSPlatform("macos")]
internal static class IOBluetoothHelper
{
    public static Task<SdpServiceRecord> QuerySdpService(this BluetoothDevice device, string serviceId)
        => device.QuerySdpService(Guid.Parse(serviceId));

    public static Task<SdpServiceRecord> QuerySdpService(this BluetoothDevice device, Guid serviceId)
    {
        var data = NSData.FromArray(serviceId.ToByteArray());
        SdpUuid uuid = SdpUuid.FromData(data);
        return device.QuerySdpService(uuid);
    }

    public static async Task<SdpServiceRecord> QuerySdpService(this BluetoothDevice device, SdpUuid serviceId)
    {
        TaskCompletionSource promise = new();

        var result = device.PerformSdpQuery(new SdpCallback(promise));

        await promise.Task;

        return device.GetServiceRecordForUuid(serviceId) ?? throw new IOException("Cloud not find sdp service");
    }

    sealed class SdpCallback(TaskCompletionSource promise) : DeviceAsyncCallbacks
    {
        public override void SdpQueryComplete(BluetoothDevice device, int status)
            => promise.TrySetResult();
    }
}

#endif