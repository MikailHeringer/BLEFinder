using BLEFinder.Classes;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static BLEFinder.Classes.ApiService;

namespace BLEFinder.Classes
{
    public class ApiService
    {
        private static readonly HttpClient _httpClient = new();

        public static async Task<List<string>> ObterSemestre(string s)
        {
            try
            {
                string url = $"https://ble-finder-api.vercel.app/semestre?sem={s}";
                var response = await _httpClient.GetStringAsync(url);

                List<string> data = JsonSerializer.Deserialize<List<string>>(response);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro geral: {ex.Message}");
                return new List<string>();
            }
        }

        public static async Task<List<List<string>>> ObterCursos()
        {
            try
            {
                string url = $"https://ble-finder-api.vercel.app/curso";
                var response = await _httpClient.GetStringAsync(url);

                List<List<string>>? data = JsonSerializer.Deserialize<List<List<string>>>(response);

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro geral: {ex.Message}");
                return new List<List<string>>();
            }
        }

        public async Task<List<GradeAula>?> ObterGradeAula(int tipo, int anosem, string turma)
        {
            try
            {
                string url = $"https://ble-finder-api.vercel.app/grade?tipo={tipo}&anosem={anosem}&turma={turma}";
                var response = await _httpClient.GetStringAsync(url);

                var data = JsonSerializer.Deserialize<List<List<string>>>(response);
                var aulas = new List<GradeAula>();

                foreach (var aula in data)
                {
                    string dia = aula[0];
                    string horario = aula[1];
                    string curso = aula[2];
                    string professor = aula[3];
                    string local = aula[4];

                    // Verificar se já existe uma aula com os mesmos dados (dia, curso, professor)
                    var aulaExistente = aulas.FirstOrDefault(a =>
                        a.Dia == dia &&
                        a.Curso == curso &&
                        a.Professor == professor);

                    if (aulaExistente != null)
                    {
                        // Ajustar o horário para unir os períodos
                        aulaExistente.Horario += " e " + horario;
                    }
                    else
                    {
                        // Criar uma nova aula
                        aulas.Add(new GradeAula
                        {
                            Dia = dia,
                            Horario = horario,
                            Curso = curso,
                            Professor = professor,
                            Local = local
                        });
                    }
                }

                return aulas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro geral: {ex.Message}");
                return new List<GradeAula>();
            }
        }
    }

    public class GradeAula
    {
        public string? Dia { get; set; }
        public string? Horario { get; set; }
        public string? Curso { get; set; }
        public string? Professor { get; set; }
        public string? Local { get; set; }

        // Variaveis para controle de interface
        public bool isVisible { get; set; } = false;
        public string? Dot { get; set; } = "";
    }

    public class GradeCursos
    {
        public string? Dia { get; set; }
        public string? Horario { get; set; }
        public string? Curso { get; set; }
        public string? Professor { get; set; }
        public string? Local { get; set; }

        // Variavel para controle de interface
        public string? Dot { get; set; } = "";
    }



}