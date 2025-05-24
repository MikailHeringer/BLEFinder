using System.Diagnostics;
using BLEFinder.Classes;
using static BLEFinder.Classes.RotaDefine;

namespace BLEFinder.Paginas;

public partial class Andar2 : ContentView
{
    // tamanho da imagem quando foi retirado as cordenadas das salas
    public static int andar = 0;
    int larguraOriginal = 159;
    int alturaOriginal = 648;

    RectF rectF;

    public Action<string> _OnRoomSelected;

    TouchDraw drawable = new TouchDraw();
    private RotaDraw routeDrawable;

    private QrPoint? QrCurrent;

    public List<QrPoint> QrCodeList = new List<QrPoint>
    {
        new QrPoint{Name= "2.1", X=145, Y=86},
        new QrPoint{Name= "2.2", X=126, Y=187},
        new QrPoint{Name= "2.3", X=145, Y=247},
        new QrPoint{Name= "2.4", X=124, Y=298},
        new QrPoint{Name= "2.5", X=144, Y=388},
        new QrPoint{Name= "2.6", X=145, Y=500},
    };

    public Point BeaconPosition = new Point() { X = 115, Y = 231 };

    public List<Room> Rooms = new List<Room>
    {
        new Room { x1 = 44, x2 = 152, y1 = 496, y2 = 641, destino = [145, 501], name = "P2.1" },
        new Room { x1 = 44, x2 = 129, y1 = 291, y2 = 435, destino = [124, 297], name = "P2.2" },
        new Room { x1 = 44, x2 = 133, y1 = 97, y2 = 192, destino = [124, 187], name = "P2.3" },
        new Room { x1 = 44, x2 = 153, y1 = 5, y2 = 94, destino = [145, 85], name = "P2.4" },
    };

    public Andar2(Action<string> RoomSelected)
    {
        InitializeComponent();
        drawable = new TouchDraw();
        drawingCanvas.Drawable = drawable;

        routeDrawable = new RotaDraw();
        graphicsView.Drawable = routeDrawable;

        _OnRoomSelected = RoomSelected;

        //this.QrCurrent = QrCodeList.Find((qrCodePoint) => qrCodePoint.Name == name);
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

        MainThread.BeginInvokeOnMainThread(() =>
        {

            #pragma warning disable CS8629
            Point touchPoint = (Point)e.GetPosition(mapImage);
            #pragma warning restore CS8629

            

            double imageWidth = mapImage.Width;
            double imageHeight = mapImage.Height;

            double scaleX = larguraOriginal / imageWidth;
            double scaleY = alturaOriginal / imageHeight;

            double newCordX = touchPoint.X * scaleX;
            double newCordY = touchPoint.Y * scaleY;

            Room room = Room.GetRoom(Rooms, newCordX, newCordY);

            //Debug.WriteLine($"{touchPoint.X} {touchPoint.Y} <-> {room.name}");
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

            await RotaDefine.tracer("predio_p1_2.png", size: [larguraOriginal, alturaOriginal], routeDrawable, origem, destino, graphicsView);

            await Task.Delay(200);

            loadingRoutes.IsVisible = false;
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
        else
        {
            //Debug.WriteLine("WTF Brow?")
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