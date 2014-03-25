using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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

        public static System.Drawing.Color FadeColor(System.Drawing.Color start, System.Drawing.Color end, float percent)
        {
            System.Drawing.Color fade = System.Drawing.Color.FromArgb((byte)(start.R + ((end.R - start.R) * percent)), (byte)(start.G + ((end.G - start.G) * percent)), (byte)(start.B + ((end.B - start.B) * percent)));
            return fade;
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



    }
}
