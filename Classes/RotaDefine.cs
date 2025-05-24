using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLEFinder.Classes
{
    public class RotaDefine
    {
        public record Ponto(int X, int Y);

        public async static Task tracer(string file, int[] size, RotaDraw routeDraw, int[] pOrigem, int[] pDestino, GraphicsView graphicsView)
        {
            string mapColor = file.Substring(0, file.Length - 4) + "_color.png";
            string fileBin = file.Substring(0, file.Length - 4) + ".bin";

            string tempFolder = FileSystem.CacheDirectory;

            //string caminho = Path.Combine(tempFolder, file);
            Stream mapColorDir;
            try
            {
                mapColorDir = await FileSystem.OpenAppPackageFileAsync(mapColor);
            }
            catch (Exception e)
            {
                //Debug.WriteLine($"Erro ao carregar imagem {e.Message}");
                return;
            }

            bool[,] matriz;

            var mapaBin = Path.Combine(FileSystem.AppDataDirectory, fileBin);

            if (File.Exists(mapaBin))
            {
                matriz = GeradorDeMapa.CarregarMatriz(fileBin);
                //Debug.WriteLine("Mapa Carregado");
            }
            else
            {
                matriz = GeradorDeMapa.GerarMatriz(mapColorDir, size[0], size[1]);
                GeradorDeMapa.SalvarMatriz(fileBin, matriz);
                //Debug.WriteLine("Mapa Criado");
            }

            // Criando rota
            Ponto origem = new Ponto(pOrigem[0], pOrigem[1]);
            Ponto destino = new Ponto(pDestino[0], pDestino[1]);

            var matrizGorda = InflarParedes(matriz, 3);
            var rota = GerarRota(matrizGorda, origem, destino);

            routeDraw.rota = rota;
            routeDraw.mapa = matriz;
            //routeDraw.i = origem;
            //routeDraw.f = destino;

            graphicsView.Invalidate();

        }

        public static List<Ponto> GerarRota(bool[,] mapa, Ponto origem, Ponto destino)
        {
            int largura = mapa.GetLength(0);
            int altura = mapa.GetLength(1);

            var direcoes = new Ponto[]
            {
                new Ponto(0, -1), // cima
                new Ponto(1, 0),  // direita
                new Ponto(0, 1),  // baixo
                new Ponto(-1, 0), // esquerda
            };

            var fila = new Queue<Ponto>();
            var visitado = new bool[largura, altura];
            var veioDe = new Dictionary<Ponto, Ponto?>();

            fila.Enqueue(origem);
            visitado[origem.X, origem.Y] = true;
            veioDe[origem] = null;

            while (fila.Count > 0)
            {
                var atual = fila.Dequeue();

                if (atual.Equals(destino))
                    break;

                foreach (var dir in direcoes)
                {
                    var vizinho = new Ponto(atual.X + dir.X, atual.Y + dir.Y);

                    if (vizinho.X < 0 || vizinho.X >= largura || vizinho.Y < 0 || vizinho.Y >= altura)
                        continue;

                    if (!mapa[vizinho.X, vizinho.Y] || visitado[vizinho.X, vizinho.Y])
                        continue;

                    fila.Enqueue(vizinho);
                    visitado[vizinho.X, vizinho.Y] = true;
                    veioDe[vizinho] = atual;
                }
            }

            if (!veioDe.ContainsKey(destino))
                return new List<Ponto>();

            var caminho = new List<Ponto>();
            var atualPonto = destino;

            while (atualPonto != null)
            {
                caminho.Add(atualPonto);
                atualPonto = veioDe[atualPonto] ?? default!;
            }

            caminho.Reverse();
            return caminho;
        }

        public class RotaDraw : IDrawable
        {
            public List<Ponto>? rota { get; set; }

            public bool[,] mapa { get; set; }

            //public Ponto i { get; set; }
            //public Ponto f { get; set; }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                if (mapa == null)
                    return;                     

                int largura = mapa.GetLength(0);
                int altura = mapa.GetLength(1);

                float canvasWidth = dirtyRect.Width;
                float canvasHeight = dirtyRect.Height;

                float cellWidth, cellHeight;
                float offsetX = 0, offsetY = 0;

                float ratioX = canvasWidth / largura;
                float ratioY = canvasHeight / altura;
                float scale = Math.Min(ratioX, ratioY);

                cellWidth = cellHeight = scale;

                offsetX = (canvasWidth - (largura * cellWidth)) / 2;
                offsetY = (canvasHeight - (altura * cellHeight)) / 2;

                //Exibe a matrix sobre o mapa
                //for (int y = 0; y < altura; y++)
                //{
                //    for (int x = 0; x < largura; x++)
                //    {
                //        canvas.FillColor = mapa[x, y] ? Colors.LightGreen : Colors.Red;
                //        canvas.FillRectangle(
                //            offsetX + x * cellWidth,
                //            offsetY + y * cellHeight,
                //            cellWidth,
                //            cellHeight);
                //    }
                //}

                /*
                // Ponto de Origem
                var inicio = i;
                float inicioX = offsetX + inicio.X * cellWidth + cellWidth / 2;
                float inicioY = offsetY + inicio.Y * cellHeight + cellHeight / 2;

                canvas.FillColor = Color.FromArgb("#00F");
                canvas.FillCircle(inicioX, inicioY, 5);

                // Ponto de Destino
                var fim = f;
                float fimX = offsetX + fim.X * cellWidth + cellWidth / 2;
                float fimY = offsetY + fim.Y * cellHeight + cellHeight / 2;

                canvas.FillColor = Color.FromArgb("#F00");
                canvas.FillCircle(fimX, fimY, 5);
                */


                //Desenhar a rota

                if (rota != null && rota.Count > 1)
                {
                    canvas.StrokeSize = 2;

                    for (int i = 0; i < rota.Count - 1; i++)
                    {
                        var p1 = rota[i];
                        var p2 = rota[i + 1];

                        float x1 = offsetX + p1.X * cellWidth + cellWidth / 2;
                        float y1 = offsetY + p1.Y * cellHeight + cellHeight / 2;
                        float x2 = offsetX + p2.X * cellWidth + cellWidth / 2;
                        float y2 = offsetY + p2.Y * cellHeight + cellHeight / 2;

                        float t = (float)i / (rota.Count - 2);

                        byte r = (byte)(255 * t);
                        byte g = 0;
                        byte b = (byte)(255 * (1 - t));

                        canvas.StrokeColor = Color.FromRgb(r, g, b);

                        canvas.DrawLine(x1, y1, x2, y2);
                    }

                    // Ponto de Origem
                    var inicio = rota.First();
                    float inicioX = offsetX + inicio.X * cellWidth + cellWidth / 2;
                    float inicioY = offsetY + inicio.Y * cellHeight + cellHeight / 2;

                    canvas.FillColor = Color.FromArgb("#00F");
                    canvas.FillCircle(inicioX, inicioY, 5);

                    // Ponto de Destino
                    var fim = rota.Last();
                    float fimX = offsetX + fim.X * cellWidth + cellWidth / 2;
                    float fimY = offsetY + fim.Y * cellHeight + cellHeight / 2;

                    canvas.FillColor = Color.FromArgb("#F00");
                    canvas.FillCircle(fimX, fimY, 5);
                }
                else
                {
                    _ = Application.Current.MainPage.DisplayAlert("", "Erro ao definir rota", "OK");
                }

            }
        }

        public static bool[,] InflarParedes(bool[,] mapa, int margem)
        {
            int largura = mapa.GetLength(0);
            int altura = mapa.GetLength(1);
            var novoMapa = (bool[,])mapa.Clone();

            for (int x = 0; x < largura; x++)
            {
                for (int y = 0; y < altura; y++)
                {
                    if (!mapa[x, y])
                    {
                        for (int dx = -margem; dx <= margem; dx++)
                        {
                            for (int dy = -margem; dy <= margem; dy++)
                            {
                                int nx = x + dx;
                                int ny = y + dy;

                                if (nx >= 0 && nx < largura && ny >= 0 && ny < altura)
                                {
                                    novoMapa[nx, ny] = false;
                                }
                            }
                        }
                    }
                }
            }

            return novoMapa;
        }


    }
}
