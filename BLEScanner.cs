using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;

namespace BLEFinder
{
    public class BLEScanner
    {
        private readonly IAdapter _adapter;
        private readonly IBluetoothLE _bluetooth;
        public ObservableCollection<string> Devices { get; } = new();

        public BLEScanner()
        {
            _bluetooth = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;

            _adapter.DeviceDiscovered += OnDeviceDiscovered;
        }

        public async Task StartScanning()
        {
            if (!_bluetooth.IsAvailable || !_bluetooth.IsOn)
            {
                Console.WriteLine("Bluetooth não está disponível ou está desligado.");
                return;
            }

            Devices.Clear();
            await _adapter.StartScanningForDevicesAsync();
        }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            var device = e.Device;
            if (device.Name != null)
            {
                double distance = CalculateDistance(device.Rssi);
                string deviceInfo = $"{device.Name} - Distância: {distance:F2}m";

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Devices.Add(deviceInfo);
                });

                Console.WriteLine(deviceInfo);
            }
        }


        private double CalculateDistance(int rssi, int txPower = -59)
        {
            return Math.Pow(10, (txPower - rssi) / (10 * 2.0));
        }
    }
}
