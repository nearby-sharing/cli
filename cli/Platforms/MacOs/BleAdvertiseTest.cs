#if MACOS
using Foundation;
using CoreBluetooth;
using ShortDev.Microsoft.ConnectedDevices;
using ShortDev.Microsoft.ConnectedDevices.Transports;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;

namespace NearShare.Platforms.MacOs;

[SupportedOSPlatform("macos")]
internal static class BleAdvertiseTest
{
    public static async ValueTask Test()
    {
        var cdp = await CdpUtils.CreatePlatformAsync("MacOs Test");
        BLeBeacon beacon = new(cdp.DeviceInfo.Type, PhysicalAddress.None, cdp.DeviceInfo.Name);
        var beaconData = beacon.ToArray();

        Console.WriteLine("Created beacon data");

        BleDelegate @delegate = new();
        @delegate.Advertise(beaconData);

        Console.WriteLine("Running message loop...");

        var runLoop = NSRunLoop.Current;
        while (runLoop.RunUntil(NSRunLoopMode.Default, NSDate.DistantFuture)) { }

        Console.WriteLine("Finished message loop");
    }

    sealed class BleDelegate : NSObject, ICBPeripheralManagerDelegate
    {
        readonly CBPeripheralManager _peripheralManager;
        public BleDelegate()
        {
            _peripheralManager = new(this, queue: null);
            _peripheralManager.AdvertisingStarted += OnAdvertisingStarted;
        }

        public void Advertise(byte[] beaconData)
        {
            Console.WriteLine("Starting advertising...");
            try
            {
                NSDictionary options = new();
                options[CBAdvertisement.DataManufacturerDataKey] = NSData.FromArray([0, Constants.BLeBeaconManufacturerId, .. beaconData]);
                _peripheralManager.StartAdvertising(options);

                Console.WriteLine("Started advertising");
            }
            catch
            {
                Console.WriteLine("Failed advertising!");

                throw;
            }
        }

        private void OnAdvertisingStarted(object? sender, NSErrorEventArgs e)
        {
            if (e.Error is null)
            {
                Console.WriteLine("No error");
                return;
            }

            Console.WriteLine("Error");
            Console.WriteLine(e.Error.LocalizedFailureReason);
            Console.WriteLine(e.Error.LocalizedDescription);
            Console.WriteLine(e.Error.LocalizedRecoverySuggestion);
        }

        public void StateUpdated(CBPeripheralManager peripheral) { }
    }
}
#endif