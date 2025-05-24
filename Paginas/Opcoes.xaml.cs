using System.ComponentModel;
using System.Text.Json;
using BLEFinder.Classes;
using Microsoft.Maui.Storage;

namespace BLEFinder.Paginas;

public partial class Opcoes : ContentPage, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    internal DbCurso dbCurso = new();
    internal DbSemestre dbSemestre = new();

    private List<string> _cursos;
    public List<string> Cursos
    {
        get => _cursos;
        set
        {
            _cursos = value;
            OnPropertyChanged(nameof(Cursos));
        }
    }

    private List<string> _listSem;
    public List<string> ListSem
    {
        get => _listSem;
        set
        {
            _listSem = value;
            OnPropertyChanged(nameof(ListSem));
        }
    }

    public List<List<string>> Semestres;

    public Opcoes()
	{
		InitializeComponent();
    }

    // Pega o curso selecionado e insere no campo semestre nos semestres disponiveis
    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        Picker picker = (Picker)sender;
        int SelcIndex = picker.SelectedIndex;

        if (SelcIndex >= 0)
        {
            ListSem = new List<string>(Semestres[SelcIndex]);
        }
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Salva as preferencias do usuario
    private void Save_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("SEM", SmPicker.SelectedItem.ToString());
        Preferences.Set("Curso", CursoPicker.SelectedItem.ToString());
        DisplayAlert("", "Configuração salva", "OK");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        List<List<string>>? jsonCurso, jsonSemestres;


        // Verifica o campo de aluno e busca valores caso não exista registro
        if (!String.IsNullOrEmpty(Preferences.Get("Cursos", string.Empty)))
        {    
            jsonCurso = await dbCurso.GetCursos();
            Cursos = jsonCurso[0];
        }
        else
        {
            starStopLoad(true);
            jsonCurso = await ApiService.ObterCursos();
            
            await dbCurso.ClearItens();

            for (int id=0; id < jsonCurso[0].Count; id++)
            {
                Curso curso = new Curso
                {
                    Name = jsonCurso[0][id],
                    Sigla = jsonCurso[1][id]
                };
                await dbCurso.SaveItem(curso);
            }
            Preferences.Set("Cursos", "true");
            Preferences.Set("AttDiaria", DateTime.Now.ToString());

            Cursos = jsonCurso[0];

        }

        // Verifica o campo de semestres e busca valores caso não exista registro
        if (!String.IsNullOrEmpty(Preferences.Get("Semestres", string.Empty)))
        {
            jsonSemestres = await dbSemestre.GetSemestre();
            Semestres = jsonSemestres;
        }
        else
        {
            starStopLoad(true);
            List<List<string>> semestres = new();
            await dbSemestre.ClearItens();

            for (int id = 0; id < jsonCurso[1].Count; id++)
            {
                List<string> listSem = await ApiService.ObterSemestre(jsonCurso[1][id]);

                foreach (var item in listSem)
                {
                    Semestre semestre = new Semestre
                    {
                        Name = item,
                        CursoId = id
                    };
                    
                    await dbSemestre.SaveItem(semestre);
                }
                semestres.Add(listSem);
            }

            Preferences.Set("Semestres", "true");
            Semestres = semestres;

        }

        BindingContext = this;
        starStopLoad(false);

        // Retorna os picker as configuração anterior
        string cursoSalvo = Preferences.Get("Curso", string.Empty);
        if (!string.IsNullOrEmpty(cursoSalvo) && Cursos.Contains(cursoSalvo))
        {
            CursoPicker.SelectedItem = cursoSalvo;
        }

        string semestreSalvo = Preferences.Get("SEM", string.Empty);
        if (!string.IsNullOrEmpty(semestreSalvo))
        {
            SmPicker.SelectedItem = semestreSalvo;
        }
    }

    private async void AtualizarLista_Clicked(object sender, EventArgs e)
    {
        List<List<string>>? jsonCurso;
        List<List<string>> semestres = new();

        if (!String.IsNullOrEmpty(Preferences.Get("AttDiaria", string.Empty)))
        {
            string dateString = Preferences.Get("AttDiaria", string.Empty);
            DateTime date = DateTime.Parse(dateString);
            TimeSpan intervalo = DateTime.Now - date;
            if (intervalo.Days > 1)
            {
                starStopLoad(true);
                jsonCurso = await ApiService.ObterCursos();
                
                // Atualizando lista de cursos
                await dbCurso.ClearItens();
                for (int id = 0; id < jsonCurso[0].Count; id++)
                {
                    Curso curso = new Curso
                    {
                        Name = jsonCurso[0][id],
                        Sigla = jsonCurso[1][id]
                    };
                    await dbCurso.SaveItem(curso);
                }

                // Atualizando lista de semestres
                await dbSemestre.ClearItens();
                for (int id = 0; id < jsonCurso[1].Count; id++)
                {
                    List<string> listSem = await ApiService.ObterSemestre(jsonCurso[1][id]);

                    foreach (var item in listSem)
                    {

                        Semestre semestre = new Semestre
                        {
                            Name = item,
                            CursoId = id
                        };

                        await dbSemestre.SaveItem(semestre);
                    }
                    semestres.Add(listSem);
                }

                Preferences.Set("Cursos", "true");
                Preferences.Set("Semestres", "true");
                Preferences.Set("AttDiaria", DateTime.Now.ToString());

                Semestres = semestres;
                Cursos = jsonCurso[0];

                BindingContext = this;
                starStopLoad(false);
                await DisplayAlert("", $"Lista atualizada ás {DateTime.Now:HH:mm:ss}", "OK");

            }
            else
            {
                _ = DisplayAlert("", "É permitido apenas uma atualização a cada 24h", "OK");
            }
        }
    }

    private void starStopLoad(bool s)
    {
        if (s)
        {
            ConfigPage.IsVisible = false;
            LoadingPage.IsVisible = true;
            Loading.IsRunning = true;
        }
        else
        {
            ConfigPage.IsVisible = true;
            LoadingPage.IsVisible = false;
            Loading.IsRunning = false;
        }
    }

}