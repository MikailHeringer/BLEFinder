using BLEFinder.Classes;
using System.Collections.ObjectModel;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;
using System.Security;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Diagnostics;

namespace BLEFinder.Paginas;

public partial class Favorito : ContentPage
{
	public ObservableCollection<GradeAula> ListarAulas { get; set; }
    internal string diaSemana = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(DateTime.Now.ToString("dddd", new CultureInfo("pt-BR")));
    string prefSem;
    bool firstLoad = true;
    List<string> SalasMapeadas;

    public Favorito()
	{
		InitializeComponent();

        SalasMapeadas = new List<string>
        {
            "P"
            //"A/Pascal",
            //"A2.3",
            //"E3.2",
            //"E3.5"
        };
       
        prefSem = Preferences.Get("SEM", string.Empty);
        ListarAulas = new ObservableCollection<GradeAula>();
        Loading.IsVisible = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (firstLoad)
        {
            firstLoad = false;

            Loading.IsRunning = true;
            List.IsVisible = false;
            Loading.IsVisible = true;

            List<GradeAula>? gradeAula = null;
            string Grade = Preferences.Get("GradeAula", string.Empty);

            if (!string.IsNullOrEmpty(Grade) && Grade != "[]") {
                gradeAula = JsonSerializer.Deserialize<List<GradeAula>>(Grade);
            }

            await CarregarSalas(gradeAula);

            Loading.IsRunning = false;
            Loading.IsVisible = false;
            List.IsVisible = true;
        }
        else if(prefSem != Preferences.Get("SEM", string.Empty))
        {
            Loading.IsRunning = true;
            List.IsVisible = false;
            Loading.IsVisible = true;

            prefSem = Preferences.Get("SEM", string.Empty);
            await CarregarSalas();

            Loading.IsRunning = false;
            Loading.IsVisible = false;
            List.IsVisible = true;
        }
    }

    private async Task CarregarSalas(List<GradeAula>? grade = null)
    {
        
        try
        {
            ApiService apiService = new ApiService();
            string sem = Preferences.Get("SEM", string.Empty);
            if (sem == "None" || string.IsNullOrEmpty(sem))
            {
                ConfigMSG.IsVisible = true;
                return;
            }
            ConfigMSG.IsVisible = false;
            UndefinedMSG.IsVisible = false;
            grade = grade ?? await apiService.ObterGradeAula(1, 20251, sem);
            if (grade != null)
            {

                ListarAulas.Clear();
                Preferences.Set("GradeAula", JsonSerializer.Serialize(grade));
                foreach (var aula in grade)
                {
                    if (aula.Dia == diaSemana)
                        aula.Dot = "•";

                    if (SalasMapeadas.Contains(aula.Local.Split("/")[0]))
                        aula.isVisible = true;

                    ListarAulas.Add(aula);
                }

                Loading.IsRunning = false;
                Loading.IsVisible = false;
                BindingContext = this;

                if (grade.Count == 0)
                    UndefinedMSG.IsVisible = true;
            }
        }
        catch (Exception)
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
            ErroMsg.IsVisible = true;
        }
    }

    private async void room_Tapped(object sender, TappedEventArgs e)
    {
        if(e.Parameter is string local && !string.IsNullOrWhiteSpace(local))
        {
            var localEnb = Uri.EscapeDataString(local);
            await Shell.Current.GoToAsync($"//Mapa?local={localEnb}");
        }
    }
}