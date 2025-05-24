using System.Diagnostics;
using BLEFinder.Classes;
using static BLEFinder.Classes.RotaDefine;

namespace BLEFinder.Paginas;

public partial class Andar1 : ContentView
{
    // tamanho da imagem quando foi retirado as cordenadas das salas
    public static int andar = 0;
    int larguraOriginal = 216;
    int alturaOriginal = 696;

    RectF rectF;

    public Action<string> _OnRoomSelected;

    TouchDraw drawable = new TouchDraw();
    private RotaDraw routeDrawable;

    private QrPoint? QrCurrent;

    public List<QrPoint> QrCodeList = new List<QrPoint>
    {
        new QrPoint{Name= "1.1", X=202, Y=135},
        new QrPoint{Name= "1.2", X=202, Y=280},
        new QrPoint{Name= "1.3", X=180, Y=233},
        new QrPoint{Name= "1.4", X=180, Y=345},
        new QrPoint{Name= "1.5", X=201, Y=388},
        new QrPoint{Name= "1.6", X=201, Y=552},
    };

    public Point BeaconPosition = new Point() { X = 177, Y = 258 };

    public List<Room> Rooms = new List<Room>
    {
        new Room { x1 = 102, x2 = 210, y1 = 540, y2 = 685, destino = [202, 550], name = "P1.1" },
        new Room { x1 = 102, x2 = 186, y1 = 335, y2 = 479, destino = [180, 344], name = "P1.2" },
        new Room { x1 = 102, x2 = 191, y1 = 146, y2 = 238, destino = [180, 234], name = "P1.3" },
        new Room { x1 = 103, x2 = 210, y1 = 53, y2 = 140, destino = [202, 134], name = "P1.4" },
    };

    public Andar1(Action<string> RoomSelected)
    {
        InitializeComponent();
        drawable = new TouchDraw();
        drawingCanvas.Drawable = drawable;

        routeDrawable = new RotaDraw();
        graphicsView.Drawable = routeDrawable;

        _OnRoomSelected = RoomSelected;
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

        MainThread.BeginInvokeOnMainThread(() =>
        {

            #pragma warning disable CS8629
            Point touchPoint = (Point)e.GetPosition(mapImage);
            #pragma warning restore CS8629

            //Debug.WriteLine($"{touchPoint.X} {touchPoint.Y}");

            double imageWidth = mapImage.Width;
            double imageHeight = mapImage.Height;

            double scaleX = larguraOriginal / imageWidth;
            double scaleY = alturaOriginal / imageHeight;

            double newCordX = touchPoint.X * scaleX;
            double newCordY = touchPoint.Y * scaleY;

            Room room = Room.GetRoom(Rooms, newCordX, newCordY);
            setRectF(room);
            //Debug.WriteLine($"Sala: {roomName.name}");
            if (!string.IsNullOrEmpty(room.name))
                _OnRoomSelected.Invoke(room.name);

        });
    }

    public async void defRota(string roomName, Andar type)
    {
        int[] origem;
        if (roomName != null && !string.IsNullOrEmpty(type.type))
        {

            if (type.type == "Beacon")
            {
                origem = new int[] { (int)BeaconPosition.X, (int)BeaconPosition.Y };
            }
            else if (type.type == "QrCode")
            {
                QrPoint? qrCode = QrCodeList.Find((name) => name.Name == type.qrName);
                origem = new int[] { (int)qrCode.X, (int)qrCode.Y };
            }
            else
            {
                origem = [0, 0];
            }

            loadingRoutes.IsVisible = true;
            Loading.IsRunning = true;
            Loading.IsVisible = true;

            Room room = Room.GetRoomByName(Rooms, roomName);
            setRectF(room);

            int[] destino = room.destino;

            await Task.Delay(150);

            await RotaDefine.tracer("predio_p1_1.png", size: [larguraOriginal, alturaOriginal], routeDrawable, origem, destino, graphicsView);

            await Task.Delay(200);

            loadingRoutes.IsVisible = false;
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
        else
        {
            //Debug.WriteLine("WTF Brow?");
        }
    }

    internal void setRectF(Room room)
    {
        double imageWidth = mapImage.Width;
        double imageHeight = mapImage.Height;

        double width = (room.x1 - room.x2) * (imageWidth / larguraOriginal);
        double height = (room.y1 - room.y2) * (imageHeight / alturaOriginal);

        double posX = (mapImage.X + border.X) + (room.x2 * (imageWidth / larguraOriginal));
        double posY = (mapImage.Y + border.Y) + (room.y2 * (imageHeight / alturaOriginal));

        rectF = new RectF(
            (float)posX,
            (float)posY,
            (float)width,
            (float)height
        );

        drawable.RectToDraw = rectF;
        drawingCanvas.Invalidate();
    }
    public void removeRectF()
    {
        drawable.RectToDraw = null;
        drawingCanvas.Invalidate();
    }
}
