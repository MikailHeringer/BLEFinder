using Microsoft.Maui.Graphics;

namespace BLEFinder.Classes
{
    public class TouchDraw : IDrawable
    {
        public RectF? RectToDraw { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (RectToDraw != null)
            {
                canvas.StrokeColor = Colors.Red;
                canvas.StrokeSize = 2;
                canvas.DrawRectangle(RectToDraw.Value);
            }
        }
    }
}
