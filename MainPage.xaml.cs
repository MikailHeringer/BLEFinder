using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System.Collections.ObjectModel;
using System.Threading;

namespace BLEFinder
{
    public partial class MainPage : ContentPage
    {
        private readonly IAdapter _adapter;
        private readonly IBluetoothLE _bluetooth;
        public List<IDevice> Devices = new();

        private CustomDrawable drawable = new();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            _bluetooth = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;

            Devices.Clear();

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
            StackDevices.Children.Clear();

            try
            {
                await _adapter.StartScanningForDevicesAsync();
                StatusLabel.Text = Devices.Count > 0 ? "Scan finalizado!" : "Nenhum dispositivo encontrado.";

                foreach (var device in Devices)
                {
                    Button button = new Button
                    {
                        Text = device.Name + " - " + device.Id,
                        
                        //Command = new Command(() => CalculateDistance(device.Rssi)) // Passa o ID no método
                        Command = new Command(() => AtualizarRssi()) // Passa o ID no método
                    };

                    StackDevices.Children.Add(button);
                }

                //DevicesList.ItemsSource = Devices;

                //await DisplayAlert("Aviso", "Começando a atualizar Rssi", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao iniciar o scan: {ex.Message}", "OK");
            }
        }

        private async void OnDeviceDiscovered(object sender, DeviceEventArgs e)
        {
            if (!e.Device.Name.Contains("iTAG")) return;

            Console.WriteLine(e.Device);

            //double distance = CalculateDistance(e.Device.Rssi);

            var device = new BleDevice
            {
                Name = e.Device.Name,
                Rssi = e.Device.Rssi,
                Id = e.Device.Id,
                //Distance = $"{distance:F2} metros"
            };

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Devices.Add(e.Device);
                StatusLabel.Text = $"{Devices.Count} dispositivos encontrados!";
            });

            //Console.WriteLine($"Encontrado: {device.Name} - Distância: {device.Distance}");

            await _adapter.StopScanningForDevicesAsync();
        }

        private double CalculateDistance(int rssi)
        {
            int txPower = -59;
            double distance = Math.Pow(10, (txPower - rssi) / (10 * 2));

            int A = -59;
            double n = 2.0;
            double distanciaTeste = Math.Pow(10, (A - rssi) / (10 * n));

            DisplayAlert($"Distancia", $"Distancia calculada: {distance}; rssi: {rssi}; distancia: {distanciaTeste}", "OK");
            return distance;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("batata");
        }

        public void AtualizarRssi()
        {
            Devices.ForEach(async device => {
                bool teste = await device.UpdateRssiAsync();
                if (teste)
                {
                    StatusLabel.Text = $"Nova RSSI: {CalculateDistance(device.Rssi)}";
                } else
                {
                    StatusLabel.Text = $"Não deu, antiga: RSSI: {CalculateDistance(device.Rssi)}";
                }
            });
        }
    }

    public class BleDevice
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Rssi { get; set; }
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