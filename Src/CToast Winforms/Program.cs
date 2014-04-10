using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace CToast
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }



    static class Util
    {

        public static int FixAngle(int angle)
        {
            while (angle < 0)
                angle += 360;
            while (angle >= 360)
                angle -= 360;

            return angle;
        }

        public static System.Drawing.Color FadeColor(System.Drawing.Color start, System.Drawing.Color end, float percent)
        {
            System.Drawing.Color fade = System.Drawing.Color.FromArgb((byte)(start.R + ((end.R - start.R) * percent)), (byte)(start.G + ((end.G - start.G) * percent)), (byte)(start.B + ((end.B - start.B) * percent)));
            return fade;
        }

        public static Point GetCenter(this Rectangle r)
        {       
            return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        }

            /// <summary>
        /// Converts an angle in radians to a point
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static System.Drawing.Point AngleToXY(int angleI, float length)
        {
            double angle = angleI * (Math.PI / 180.0);
            double a, b;

            double t = Math.Tan(angle);

            double c2 = length * length;
            double t2 = t * t;

            a = Math.Sqrt(c2 / (t2 + 1));
            b = Math.Sqrt(c2 - a * a);

            if (angle < (Math.PI / 2))
            {
                b *= -1;
            }
            if (angle >= (Math.PI / 2) && angle < Math.PI)
            {
                a *= -1;
                b *= -1;
            }
            else if (angle >= Math.PI && angle < ((Math.PI / 2) + Math.PI))
            {
                a *= -1;
            }
            else if (angle >= ((Math.PI / 2) + Math.PI))
            {
                ;
            }

            var f = 1f;
            if (length < 0)
                f = -1f;


            return new System.Drawing.Point((int)(Math.Round(a, 4) * f), (int)(Math.Round(b, 4) * f));
        }


        public static int GetPointDistance(Point pt1, Point pt2)
        {
            var x = pt1.X - pt2.X;
            var y = pt1.Y - pt2.Y;

            return (int)Math.Sqrt((x * x) + (y * y));
        }

        public static int GetLineAngle(Point source, Point target)
        {
            var a = (int)(GetLineAngleR(new PointF((float)source.X,(float)source.Y), new PointF((float)target.X,(float)target.Y)) * (180f / Math.PI));
            while (a < 0)
                a += 360;
            while (a >= 360)
                a -= 360;
            return a;
        }

        /// <summary>
        /// Returns the angle between the two points in radians
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static double GetLineAngleR(PointF source, PointF target)
        {
            double a, b;
            double angle;

            a = target.X - source.X;
            b = target.Y - source.Y;

            if (a == 0 && b == 0)
                return 0;

            angle = Math.Atan(Math.Abs(b) / Math.Abs(a));

            if (a >= 0 && b >= 0)
            {
                angle *= -1;
            }
            else if (a < 0 && b >= 0)
            {
                angle += Math.PI;
            }
            else if (a >= 0 && b < 0)
            {

            }
            else if (a < 0 && b < 0)
            {
                angle = Math.PI - angle;
            }
            return angle;

        }

    }
}
