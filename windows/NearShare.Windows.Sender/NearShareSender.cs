using Microsoft.CorrelationVector;
using System.Runtime.Versioning;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Internal.DataTransfer.NearShare;
using Windows.Storage;
using Windows.System.RemoteSystems;

namespace NearShare.Windows.Sender;

[SupportedOSPlatform("windows10.0.10240")]
public static class NearShareSender
{
    const string UriId = "UniformResourceLocatorW";

    public static async ValueTask SendAsync(RemoteSystem receiver, Uri uri, IProgress<SendDataProgress> progress, CancellationToken cancellationToken = default)
    {
        DataPackage package = new();
        package.SetUri(uri);

        CorrelationVector correlationVector = new();

        ShareSenderBroker broker = new();
        await broker.ShareDataWithProgressAsync(correlationVector.Value, receiver, package, UriId).AsTask(cancellationToken, progress);
    }

    const string FileId = "Shell IDList Array";
    public static async ValueTask SendAsync(RemoteSystem receiver, IEnumerable<IStorageItem> storageItems, IProgress<SendDataProgress> progress, CancellationToken cancellationToken = default)
    {
        DataPackage package = new();
        package.SetStorageItems(storageItems);

        CorrelationVector correlationVector = new();

        ShareSenderBroker broker = new();
        await broker.ShareDataWithProgressAsync(correlationVector.Value, receiver, package, FileId).AsTask(cancellationToken, progress);
    }
}
