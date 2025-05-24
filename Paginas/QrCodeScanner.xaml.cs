using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace BLEFinder.Paginas;

public partial class QrCodeScanner : ContentView
{

    private readonly Action<string> _QrCodeDetector;
    private bool _hasRead = false;

	public QrCodeScanner(Action<string> OnBarcodeDetected)
	{
        InitializeComponent();
        _QrCodeDetector = OnBarcodeDetected;
        cameraView.IsDetecting = true;
    }

    private void OnBarcodeDetected(object sender, BarcodeDetectionEventArgs e)
    {

        if (_hasRead) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var resultado = e.Results.FirstOrDefault()?.Value;

            if (!string.IsNullOrEmpty(resultado))
            {
                _hasRead = true;
                cameraView.IsDetecting = false;
                _QrCodeDetector?.Invoke(resultado);
            }
        });
    }

}