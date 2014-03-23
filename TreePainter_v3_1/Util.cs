using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TreePainter_v3_1
{
    static class Util
    {

        public static void DrawRoundedRectangle(this Graphics g, Pen pen, Rectangle area, int radius)
        {
            var path = CreateRoundedRectanglePath(area.X, area.Y, area.Width, area.Height, radius);
            g.DrawPath(pen, path);
        }

        public static void FillRoundedRectangle(this Graphics g, Brush brush, Rectangle area, int radius)
        {
            var path = CreateRoundedRectanglePath(area.X, area.Y, area.Width, area.Height, radius);
            g.FillPath(brush, path);
        }

        /// <summary>
        /// http://tech.pro/tutorial/656/csharp-creating-rounded-rectangles-using-a-graphics-path
        /// </summary>
        /// <param name="area"></param>
        /// <param name="radius"></param>
        private static GraphicsPath CreateRoundedRectanglePath(int x, int y, int width, int height, int radius)
        {
            int xw = x + width;
            int yh = y + height;
            int xwr = xw - radius;
            int yhr = yh - radius;
            int xr = x + radius;
            int yr = y + radius;
            int r2 = radius * 2;
            int xwr2 = xw - r2;
            int yhr2 = yh - r2;

            GraphicsPath p = new GraphicsPath();
            p.StartFigure();

            //Top Left Corner
            p.AddArc(x, y, r2, r2, 180, 90);
            

            //Top Edge
            p.AddLine(xr, y, xwr, y);

            //Top Right Corner
            p.AddArc(xwr2, y, r2, r2, 270, 90);
           

            //Right Edge
            p.AddLine(xw, yr, xw, yhr);

            //Bottom Right Corner           
            p.AddArc(xwr2, yhr2, r2, r2, 0, 90);
            
            //Bottom Edge
            p.AddLine(xwr, yh, xr, yh);

            //Bottom Left Corner
            p.AddArc(x, yhr2, r2, r2, 90, 90);
          
            //Left Edge
            p.AddLine(x, yhr, x, yr);

            p.CloseFigure();
            return p;
        }
    }
}