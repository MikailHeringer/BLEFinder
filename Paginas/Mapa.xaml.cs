using BLEFinder.Classes;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BLEFinder.Paginas;

public partial class Mapa : ContentPage, IQueryAttributable
{
    internal string posicao = null;

    internal Andar andarQR = new() { type="QrCode" };
    internal int? antigoQR = null;
    internal bool _verificando = false;

    //Salas
    Andar0? andar0 = null;
    Andar1? andar1 = null;
    Andar2? andar2 = null;

    QrCodeScanner? ScannerScreen = null;

    public Mapa()
	{
		InitializeComponent();
        contentScan.Content = null;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if ((andar0 != null || andar1 != null || andar2 != null) && !closeButton.IsVisible)
        {
            //returnScanButton.IsVisible = true;
        }

        if (ScannerScreen != null) {
            ScannerScreen = null;
            returnScanButton.IsVisible = false;
            contentScan.Content = null;
        }
    }

    // Recebe o dados de Sala, Andar e Predio que o usuario pretende ir (valor é recebido da guia Favoritos)
    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        string[]? info;
        if (query.TryGetValue("local", out var localParam))
        {
            string local = Uri.UnescapeDataString(localParam?.ToString() ?? "");
            _verificando = true;

            BlackBackgroundBox.IsVisible = true;
            BackNavScreen.IsVisible = true;
            closeButton.IsVisible = true;

            await ultraNavegatorBlaster3000(local);
        }
    }


    // Faz um scan de um qrcode recebendo o valor do qrcode
    private string? NameQr;
    private void scanButton_Clicked(object sender, EventArgs e)
    {
        returnScanButton.IsVisible = true;
        contentScan.Content = ScannerScreen = new QrCodeScanner(qrCode =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    andarQR.qrName = qrCode;
                    andarQR.andar = Convert.ToInt32(qrCode.Split(".")[0]);
                    andarQR.data = DateTime.Now;
                    BleScanner.andarAntigo = 5;
                }
                catch (Exception)
                {
                    DisplayAlert("", "QrCode Inválido", "OK");
                }

                ScannerScreen = null;
                contentScan.Content = null;
                returnScanButton.IsVisible = false;
            });
        });
    }

    private async void defReturn(string sala)
    {
        _verificando = true;
        BlackBackgroundBox.IsVisible = true;
        BackNavScreen.IsVisible = true;
        closeButton.IsVisible = true;

        await ultraNavegatorBlaster3000(sala);
    }
     
    // Botões (Image button) para abrir a tela com o mapa
    private void terreoButton_Clicked(object sender, EventArgs e)
    {
        buttons.IsVisible = false;
        returnButton.IsVisible = true;

        contentView.Content = andar0 = new Andar0(sala =>
        {
            MainThread.BeginInvokeOnMainThread(() => defReturn(sala));
        });
    }

    private void secondButton_Clicked(object sender, EventArgs e)
    {
        buttons.IsVisible = false;
        returnButton.IsVisible = true;

        contentView.Content = andar1 = new Andar1(sala =>
        {
            MainThread.BeginInvokeOnMainThread(() => defReturn(sala));
        });
    }

    private void thirdButton_Clicked(object sender, EventArgs e)
    {
        buttons.IsVisible = false;
        returnButton.IsVisible = true;

        contentView.Content = andar2 = new Andar2(sala =>
        {
            MainThread.BeginInvokeOnMainThread(() => defReturn(sala));
        });
    }

    // Botão da tela do mapa, retorna para tela inicial da guia Mapa
    private void returnButton_Clicked(object sender, EventArgs e)
    {
        // Volta para o estado inicial
        returnButton.IsVisible = false;
        contentView.Content = null;
        buttons.IsVisible = true;
        BleScanner.StopScan();

        andarQR = new() { type = "QrCode" };
        BleScanner.Beacon = new() { type = "Beacon" };

        lbPosicao.Text = "Desconhecida";
        lbPosicao.TextColor = Color.FromArgb("#F00");
        lbOrientacao.Text = "Para iniciar, escaneie um QR Code ou aproxime-se de uma escada.";

        _verificando = false;
    }


    // Botão da tela de scan do qrCode, volta para tela inicial da guia Mapa
    private void returnScanButton_Clicked(object sender, EventArgs e)
    {
        // Volta para o estado inicial
        contentScan.Content = null;
        //buttons.IsVisible = true;
        returnScanButton.IsVisible = false;
    }


    // Botão da tela de navegação, retorna para tela do mapa em aberto ou para tela inicial da  guia Mapa quando nenhuma mapa esta aberto 
    private void closeButton_Clicked(object sender, EventArgs e)
    {
        BlackBackgroundBox.IsVisible = false;
        BackNavScreen.IsVisible = false;
        closeButton.IsVisible = false;
        BleScanner.StopScan();

        if(andar0 != null)
            andar0.removeRectF();
        
        if(andar1 != null)
            andar1.removeRectF();
          
        if(andar2 != null)
            andar2.removeRectF();

        andarQR = new() { type = "QrCode" };
        BleScanner.Beacon = new() { type = "Beacon" };
        _verificando = false;

        lbPosicao.Text = "Desconhecida";
        lbPosicao.TextColor = Color.FromArgb("#F00");
        lbOrientacao.Text = "Para iniciar, escaneie um QR Code ou aproxime-se de uma escada.";
    }

    public async Task ultraNavegatorBlaster3000(string local = "none")
    {
        if (local == "none") return;

        await BleScanner.StartScan();
        string[] infoDestino = new string[3];
        int CicleCounter = 0;
        
        if (local.Contains("."))
        {
            var match = Regex.Match(local, @"^([A-Z])(\d)\.(\d{1,2})$");
            infoDestino = match.Success
            ? new string[] { match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value }
            : new string[3];
        }
        else if(local.Contains("/"))
        {
            string[] partes = local.Split("/");
            string? andar = Room.Labs.ContainsKey(partes[0])
                    ? Room.Labs[partes[0]].FirstOrDefault(d => d.ContainsKey(partes[1]))[partes[1]].ToString()
                    : null;
            infoDestino = new string[3]
            {
                partes[0], // Predio
                andar, // Andar
                partes[1], // Sala
            };
        }
        else { _ = DisplayAlert("Erro", "Valor de sala invalido", "OK"); }

        lbDestino.Text = 
            (infoDestino != null) ? 
            (Convert.ToInt32(infoDestino[1]) < 1) ? 
                $"Térreo" : 
                $"{infoDestino[1]}º andar" : 
                "";

        await Task.Delay(1500);
        while (_verificando)
        {
            Andar? infoAtual = Room.Comparar(andarQR, BleScanner.Beacon);
            Debug.WriteLine($"{infoAtual.type} : {infoAtual.data.ToString()}");

            if(BleScanner.uuid.Any(v => v != 999))
            {
                beaconAlert.Text = "Beacon detectado";
                beaconAlert.TextColor = Color.FromArgb("#0F0");
            }

            if (infoAtual.andar != null)
            {
                string texto;
                int andarDestino = Convert.ToInt32(infoDestino[1]);
                if (infoAtual.andar > andarDestino)
                {
                    if (Convert.ToInt32(infoDestino[1]) == 0)
                        texto = $"Desca para o térro";
                    else
                        texto = $"Desca para {andarDestino}º andar";
                }
                else if (infoAtual.andar < andarDestino)
                {
                    texto = $"Suba para {andarDestino}º andar";
                }
                else
                {
                    texto = "Está no andar correto";

                    _verificando = false;
                    BleScanner.StopScan();
                    lbOrientacao.Text = texto;

                    lbPosicao.Text = (infoAtual.andar > 0) ? $" {infoAtual.andar}º andar" : " Térreo";
                    lbPosicao.TextColor = Color.FromArgb("#16A085");

                    await Task.Delay(2000);

                    BlackBackgroundBox.IsVisible = false;
                    BackNavScreen.IsVisible = false;
                    closeButton.IsVisible = false;
                    buttons.IsVisible = false;

                    andarQR = new() { type = "QrCode" };
                    BleScanner.Beacon = new() { type = "Beacon" };
                    BleScanner.uuid = Enumerable.Repeat(999, 3).ToArray();

                    beaconAlert.Text = "Beacon não localizado";
                    beaconAlert.TextColor = Color.FromArgb("#F00");

                    lbPosicao.Text = "Desconhecida";
                    lbPosicao.TextColor = Color.FromArgb("#F00");
                    lbOrientacao.Text = "Para iniciar, escaneie um QR Code ou aproxime-se de uma escada.";


                    switch (andarDestino)
                    {
                        case 0:
                            contentView.Content = andar0 = new Andar0(sala =>
                                        MainThread.BeginInvokeOnMainThread(() => defReturn(sala)));
                            andar0.defRota(local, infoAtual);
                            returnButton.IsVisible = true; // Teste
                            break;
                        case 1:
                            contentView.Content = andar1 = new Andar1(sala =>
                                        MainThread.BeginInvokeOnMainThread(() => defReturn(sala)));
                            andar1.defRota(local, infoAtual);
                            returnButton.IsVisible = true; // Teste
                            break;
                        case 2:
                            contentView.Content = andar2 = new Andar2(sala =>
                                        MainThread.BeginInvokeOnMainThread(() => defReturn(sala)));
                            andar2.defRota(local, infoAtual);
                            returnButton.IsVisible = true; // Teste
                            break;
                    }

                    return;
                }

                lbPosicao.Text = (infoAtual.andar > 0) ? $" {infoAtual.andar}º andar" : " Térreo";
                lbPosicao.TextColor = Color.FromArgb("#16A085");

                lbOrientacao.Text = texto;
            }
            else
            {
                Debug.WriteLine("Erro de Scan");

                if (infoAtual.andar == null && ++CicleCounter > 4)
                {
                    BleScanner.StopScan();
                    await Task.Delay(1000);
                    await BleScanner.StartScan();
                    CicleCounter = 0;
                }

                //BleScanner.StopScan();


                await Task.Delay(1000);
            }


            await Task.Delay(100);
        }

        
    }
}