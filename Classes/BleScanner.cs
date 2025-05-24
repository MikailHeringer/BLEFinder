using Shiny;
using Shiny.BluetoothLE;
using System.Diagnostics;
using System.Reactive.Linq;


//Holy-A | 00000000-0000-0000-0000-c84acea9b84f *
//Holy-B | 00000000-0000-0000-0000-d89e9b6749af
//Holy-D | 00000000-0000-0000-0000-f93042d505a4 *
//Holy-C | 00000000-0000-0000-0000-c9056da1f8ea
//Holy-E | 00000000-0000-0000-0000-fd90106c4f78
//Holy-F | 00000000-0000-0000-0000-fedd2e882daa *

namespace BLEFinder.Classes
{
    public static class BleScanner
    {
        public static IBleManager bleManager;
        private static IDisposable scan;
        private static bool isScanning = false;

        public static Andar Beacon = new() { type = "Beacon" };
        public static int? andarAntigo = null;

        public static int[] uuid = Enumerable.Repeat(999, 3).ToArray();
        public static double[] uuidRssi = new double[3];
        internal static mediaMovel? vA = new(), vD = new(), vF = new();

        private static readonly string[] bleIds = new[]
        {
            "00000000-0000-0000-0000-c84acea9b84f", // A
            "00000000-0000-0000-0000-f93042d505a4", // D
            "00000000-0000-0000-0000-fedd2e882daa", // F
        };

        private static readonly int[] bleCicle = new[]
        {
            0, // A
            0, // D
            0, // F
        };

        public async static Task StartScan()
        {
            if (bleManager == null) return;
            if (isScanning) return;

            var status = bleManager.RequestAccess().GetAwaiter().GetResult();

            if(status != AccessState.Available)
            {

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Bluetooth Desativado",
                        "O Bluetooth está desabilitado. Ative-o para usar a os beacons.",
                        "OK");
                });
                return;
            }

            isScanning = true;

            scan = bleManager
                .Scan()
                .Where(scanResult => bleIds.Contains(scanResult.Peripheral.Uuid.ToString()))
                .Subscribe(async scanResult =>
                {
                    int rssi = Math.Abs(scanResult.Rssi);

                    if (rssi < 80) // Não registra beacons com mais de 80 no sinal rssi
                    {
                        switch (scanResult.Peripheral.Uuid.ToString())
                        {
                            case "00000000-0000-0000-0000-c84acea9b84f":
                                uuid[0] = vA.calc(rssi);

                                bleCicle[0] = 0;
                                bleCicle[1]++;
                                bleCicle[2]++;

                                break;
                            case "00000000-0000-0000-0000-f93042d505a4":
                                uuid[1] = vD.calc(rssi);

                                bleCicle[0]++;
                                bleCicle[1] = 0;
                                bleCicle[2]++;

                                break;
                            case "00000000-0000-0000-0000-fedd2e882daa":
                                uuid[2] = vF.calc(rssi);

                                bleCicle[0]++;
                                bleCicle[1]++;
                                bleCicle[2]=0;

                                break;
                        }


                        for(int i=0; i < bleCicle.Length; i++)
                        {
                            if(bleCicle[i] >= 15)
                            {
                                uuid[i] = 999;
                            }
                        }

                        if(Array.IndexOf(uuid, uuid.Min()) != 999)
                        {
                            Beacon.andar = Array.IndexOf(uuid, uuid.Min());
                        }

                        Debug.WriteLine($"A:{uuid[0]} - {bleCicle[0]}  | D:{uuid[1]} - {bleCicle[1]} | F:{uuid[2]} - {bleCicle[2]} | Atual:{Beacon.andar}");


                        if (andarAntigo != Beacon.andar)
                        {
                            andarAntigo = Beacon.andar;
                            Beacon.data = DateTime.Now;
                        }

                        if(Beacon.data == null)
                            Beacon.data = DateTime.Now;

                    }
                    else
                    {
                        //Debug.WriteLine($"{scanResult.Peripheral.Name} - {rssi}: Valor do RSSI execede o limite");
                    }
                });

            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(318));
                if (isScanning)
                {
                    //Debug.WriteLine("Reiniciando BLE Scan para evitar timeout...");
                    StopScan();
                    await Task.Delay(3000);
                    StartScan();
                }
            });
        }

        public static void StopScan()
        {
            if (isScanning)
            {
                scan?.Dispose();
                isScanning = false;
            }
        }

    }

    internal class mediaMovel()
    {
        private readonly Queue<double> _values = new Queue<double>();
        private readonly int _size = 5;

        public int calc(int x)
        {
            _values.Enqueue(x);
            if (_values.Count > _size)
            {
                _values.Dequeue();
            }
            return Convert.ToInt32(Math.Round(_values.Average(), 2));
        }
    }

}
