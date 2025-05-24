using System.Diagnostics;
using BLEFinder.Classes;
using static BLEFinder.Classes.RotaDefine;

namespace BLEFinder.Paginas;

public partial class Andar0 : ContentView
{
    // tamanho da imagem quando foi retirado as cordenadas das salas
    public static int andar = 0; 
    int larguraOriginal = 203;
    int alturaOriginal = 648;

    RectF rectF;

    public Action<string> _OnRoomSelected;

    TouchDraw drawable = new TouchDraw();
    private RotaDraw routeDrawable;

    public List<QrPoint> QrCodeList = new List<QrPoint>
    {
        new QrPoint{Name="0.1", X=189, Y=219},
        new QrPoint{Name="0.2", X=99, Y=260},
        new QrPoint{Name="0.3", X=162, Y=291},
        new QrPoint{Name="0.4", X=189, Y=396},
        new QrPoint{Name="0.5", X=163, Y=491},
        new QrPoint{Name="0.6", X=145, Y=428},
    };

    public Point BeaconPosition = new Point() { X = 155, Y = 239 };

    public List<Room> Rooms = new List<Room>
    {
        new Room { x1 = 96, x2 = 195, y1 = 49, y2 = 224, destino = [189, 220], name = "P0.3" },
        new Room { x1 = 96, x2 = 195, y1 = 49, y2 = 224, destino = [189, 220], name = "P/Foto" },
        new Room { x1 = 96, x2 = 174, y1 = 315, y2 = 475, destino = [145, 467], name = "P/Audio" },
        new Room { x1 = 96, x2 = 197, y1 = 507, y2 = 641, destino = [145, 513], name = "P/Vídeo" },
        new Room { x1 = 42, x2 = 68, y1 = 358, y2 = 387, destino = [57, 379], name = "P/Fem" },
        new Room { x1 = 42, x2 = 68, y1 = 392, y2 = 421, destino = [57, 400], name = "P/Masc" },
    };

    public Andar0(Action<string> RoomSelected)
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

            if(!string.IsNullOrEmpty(room.name))
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
                origem = new int[] {(int)BeaconPosition.X,  (int)BeaconPosition.Y};
            }else if(type.type == "QrCode")
            {
                QrPoint? qrCode = QrCodeList.Find((name) => name.Name == type.qrName);
                origem = new int[] {(int)qrCode.X,  (int)qrCode.Y};
            }
            else{
                origem = [0, 0];
            }

            loadingRoutes.IsVisible = true;
            Loading.IsRunning = true;
            Loading.IsVisible = true;

            Room room = Room.GetRoomByName(Rooms, roomName);
            setRectF(room);

            int[] destino = room.destino;

            await Task.Delay(150);

            await RotaDefine.tracer("predio_p1_0.png", size: [larguraOriginal, alturaOriginal], routeDrawable, origem, destino, graphicsView);

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
