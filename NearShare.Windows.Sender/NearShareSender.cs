using Microsoft.CorrelationVector;
using System.Runtime.Versioning;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Internal.DataTransfer.NearShare;
using Windows.System.RemoteSystems;

namespace NearShare.Windows.Sender;

[SupportedOSPlatform("windows10.0.10240")]
public static class NearShareSender
{
    public static async ValueTask SendAsync(RemoteSystem receiver, Uri uri)
    {
        DataPackage package = new();
        package.SetUri(uri);
        
        CorrelationVector correlationVector = new();

        ShareSenderBroker broker = new();
        await broker.ShareDataWithProgressAsync(correlationVector.Value, receiver, package, "UniformResourceLocatorW");
    }
}
