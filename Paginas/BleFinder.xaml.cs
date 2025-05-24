using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Linq;

namespace BLEFinder.Paginas;

public partial class BleFinder : ContentPage
{
    private readonly IAdapter _adapter;
    private readonly IBluetoothLE _bluetooth;
    private const string DeviceName = "Holy-IOT";

    private Dictionary<string, string> DevicesIds = new Dictionary<string, string>();

    public BleFinder()
	{
		InitializeComponent();
        _bluetooth = CrossBluetoothLE.Current;
        _adapter = _bluetooth.Adapter;
        _adapter.DeviceDiscovered += OnDeviceDiscovered;

        DevicesIds.Clear();
        DevicesIds.Add("00000000-0000-0000-0000-fedd2e882daa", "A");
        DevicesIds.Add("00000000-0000-0000-0000-c9056da1f8ea", "B");
        DevicesIds.Add("00000000-0000-0000-0000-c84acea9b84f", "C");
        DevicesIds.Add("00000000-0000-0000-0000-d89e9b6749af", "D");
        DevicesIds.Add("00000000-0000-0000-0000-fd90106c4f78", "E");
        DevicesIds.Add("00000000-0000-0000-0000-f93042d505a4", "F");
    }

    private async void StartScan_Clicked(object sender, EventArgs e)
    {
        if (_bluetooth.State != BluetoothState.On)
        {
            await DisplayAlert("Erro", "Bluetooth desligado. Ative o Bluetooth!", "OK");
            return;
        }

        Status.Text = "Buscando dispositivo...";

        while (true)
        {
            Status.Text = "";
            await _adapter.StartScanningForDevicesAsync();
        }
    }

    private async void OnDeviceDiscovered(object sender, DeviceEventArgs e)
    {
        if (DevicesIds.ContainsKey(e.Device.Id.ToString()))
        {
            string deviceName = DevicesIds[e.Device.Id.ToString()];
            string newText = $"\t{deviceName}: {e.Device.Rssi.ToString()}";


            MainThread.BeginInvokeOnMainThread(() =>
            {
                switch (deviceName)
                {
                    case "A":
                        RssiA.Text = newText;
                        break;
                    case "B":
                        RssiB.Text = newText;
                        break;
                    case "C":
                        RssiC.Text = newText;
                        break;
                    case "D":
                        RssiD.Text = newText;
                        break;
                    case "E":
                        RssiE.Text = newText;
                        break;
                    case "F":
                        RssiF.Text = newText;
                        break;
                }
            });
        }
    }
}