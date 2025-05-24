using System.Diagnostics;

namespace BLEFinder.Classes
{
    public class Room
    {
        public double x1 { get; set; }
        public double y1 { get; set; }
        public double x2 { get; set; }
        public double y2 { get; set; }
        public int[] destino { get; set; } = [0, 0];
        public string? name { get; set; }

        public static Dictionary<string, List<Dictionary<string, int>>> Labs = new ()
        {
            ["P"] = new()
            {
                new(){["Audio"] = 0},
                new(){["Vídeo"] = 0},
                new(){["Foto"] = 0},
                new(){["Fem"] = 0},
                new(){["Masc"] = 0}
            }
        };

        public static Dictionary<string, List<Dictionary<string, int>>> Beacon = new()
        {
            ["P"] = new()
            {
                new(){["A"] = 0},
                new(){["D"] = 1},
                new(){["F"] = 2}
            }
        };

        public static Room GetRoom(List<Room> rooms, double x, double y)
        {
            Room? room = rooms.Find((room) => room.x1 <= x && room.x2 >= x && room.y1 <= y && room.y2 >= y);

            if (room != null && !String.IsNullOrEmpty(room.name))
            {
                return room;
            }

            return new Room();
        }

        public static Room GetRoomByName(List<Room> rooms, string name)
        {
            Room? room = rooms.Find((room) => room.name == name);

            if (room != null && !String.IsNullOrEmpty(room.name))
            {
                return room;
            }

            return new Room();
        }

        public static Andar? Comparar(Andar a, Andar b)
        {
            if (a.data is null && b.data is null)
                return new Andar();

            if (a.data is null)
                return b;

            if (b.data is null)
                return a;

            //Debug.WriteLine($"{((a.data > b.data) ? a : b)} : {a.data} / {b.data}");
            return (a.data > b.data) ? a : b;
        }

    }

    public class fileInfo{
        public string name { get; set; }
        public List<int> size { get; set; }
    }

    public class Andar
    {
        public string? qrName { get; set; } = null;
        public int? andar { get; set; } = null;
        public DateTime? data { get; set; } = null;
        public string? type { get; set; } = null;
    }

}
