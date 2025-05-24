using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using ResizeMode = SixLabors.ImageSharp.Processing.ResizeMode;
using Size = SixLabors.ImageSharp.Size;

namespace BLEFinder.Classes
{
    public class GeradorDeMapa
    {
        public static bool[,] GerarMatriz(Stream caminhoImagem, int largura, int altura)
        {
            using var imagem = Image.Load<Rgba32>(caminhoImagem);

            imagem.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(largura, altura),
                Mode = ResizeMode.Stretch
            }));

            bool[,] mapa = new bool[largura, altura];

            for (int y = 0; y < altura; y++)
            {
                for (int x = 0; x < largura; x++)
                {
                    var pixel = imagem[x, y];

                    bool ehVerde = pixel.R < 100 && pixel.G > 200 && pixel.B < 100;

                    mapa[x, y] = ehVerde;
                }
            }

            return mapa;
        }

        public static void SalvarMatriz(string nomeMatrix, bool[,] matriz)
        {
            string caminho = Path.Combine(FileSystem.AppDataDirectory, nomeMatrix);

            int largura = matriz.GetLength(0);
            int altura = matriz.GetLength(1);

            using var fs = new FileStream(caminho, FileMode.Create);
            using var bw = new BinaryWriter(fs);

            bw.Write(largura);
            bw.Write(altura);

            for (int y = 0; y < altura; y++)
                for (int x = 0; x < largura; x++)
                    bw.Write(matriz[x, y]);
        }

        public static bool[,] CarregarMatriz(string nomeMatrix)
        {
            string caminho = Path.Combine(FileSystem.AppDataDirectory, nomeMatrix);
            using var fs = new FileStream(caminho, FileMode.Open);
            using var br = new BinaryReader(fs);
            int largura = br.ReadInt32();
            int altura = br.ReadInt32();

            bool[,] matriz = new bool[largura, altura];

            for (int y = 0; y < altura; y++)
                for (int x = 0; x < largura; x++)
                    matriz[x, y] = br.ReadBoolean();

            return matriz;
        }
    }
}
