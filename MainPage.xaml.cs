using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Collections.ObjectModel;

namespace BLEFinder
{
    public partial class MainPage : ContentPage
    {
        private readonly IAdapter _adapter;
        private readonly IBluetoothLE _bluetooth;
        public List<BleDevice> Devices; //{ get; set; }

        private CustomDrawable drawable = new();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            _bluetooth = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;

            Devices = new List<BleDevice>();

            _adapter.DeviceDiscovered -= OnDeviceDiscovered;
            _adapter.DeviceDiscovered += OnDeviceDiscovered;

            var graphicsView = new GraphicsView
            {
                Drawable = drawable,
                HeightRequest = 400,
                WidthRequest = 400
            };

            graphicsView.StartInteraction += (s, e) =>
            {
                drawable.AddPoint(new PointF((float)e.Touches[0].X, (float)e.Touches[0].Y));
                graphicsView.Invalidate();
            };

            // Content = new VerticalStackLayout
            // {
            //     Children = { graphicsView }
            // };
        }

        private async void OnScanClicked(object sender, EventArgs e)
        {
            if (_bluetooth.State != BluetoothState.On)
            {
                await DisplayAlert("Erro", "Bluetooth desligado. Ative o Bluetooth!", "OK");
                return;
            }

            if (_adapter.IsScanning)
            {
                await DisplayAlert("Atenção", "O escaneamento já está em andamento!", "OK");
                return;
            }

            StatusLabel.Text = "Buscando dispositivos...";
            Devices.Clear();

            try
            {
                await _adapter.StartScanningForDevicesAsync();
                StatusLabel.Text = Devices.Count > 0 ? "Scan finalizado!" : "Nenhum dispositivo encontrado.";

                DevicesList.ItemsSource = Devices;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao iniciar o scan: {ex.Message}", "OK");
            }
        }

        private void OnDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            if (e.Device.Name == null) return;

            double distance = CalculateDistance(e.Device.Rssi);
            var device = new BleDevice
            {
                Name = e.Device.Name,
                Distance = $"{distance:F2} metros"
            };

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Devices.Add(device);
                StatusLabel.Text = $"{Devices.Count} dispositivos encontrados!";
            });

            Console.WriteLine($"Encontrado: {device.Name} - Distância: {device.Distance}");
        }

        private double CalculateDistance(int rssi)
        {
            int txPower = -59;
            return Math.Pow(10, (txPower - rssi) / (10 * 2));
        }
    }

    public class BleDevice
    {
        public string Name { get; set; }
        public string Distance { get; set; }
    }

    public class CustomDrawable : IDrawable
    {
        private List<PointF> ClickedPoints = new();

        public void AddPoint(PointF point)
        {
            ClickedPoints.Add(point);
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.LightGray;
            canvas.FillRectangle(dirtyRect);

            canvas.StrokeSize = 4;
            canvas.StrokeColor = Colors.Black;

            canvas.DrawLine(50, 50, 300, 500);
            canvas.DrawLine(50, 100, 300, 100);
            canvas.DrawLine(50, 50, 50, 200);

            foreach (var point in ClickedPoints)
            {
                canvas.FillColor = Colors.Red;
                canvas.FillCircle(point.X, point.Y, 10);
            }
        }
    }
}