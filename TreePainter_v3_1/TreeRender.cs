using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO;

namespace TreePainter_v3_1
{
    public class BitmapWithOrigin
    {
        public Bitmap Bitmap { get; set; }
        public Point Origin { get; set; }

        public BitmapWithOrigin() { }

        public BitmapWithOrigin(string file)
        {
            Bitmap = new Bitmap(file);

            file = file.Substring(file.LastIndexOf(Path.DirectorySeparatorChar));
            file = file.Substring(file.IndexOf("_")+1).Replace(".png", "");
            var xy = file.Split('_');
            Origin = new Point(Int32.Parse(xy[0]), Int32.Parse(xy[1]));
        }
    }
    public class TreeRender
    {

        internal static IEnumerable<BitmapWithOrigin> RenderTrees<T>(IEnumerable<TreeView> trees, Func<TreeNode,T> transform, PaintSettings paintSettings) where T:VisualTreeNode
        {         
            foreach (var tree in trees.Select(t => VisualTree.Create(t,transform)))
                yield return PaintTree(tree,paintSettings,false);
        }

        public static Bitmap RenderSingleTree(TreeView tree)
        {
            var paintSettings = new PaintSettings();
            var nodeFont = new Font("Arial", 12, FontStyle.Bold);
            return PaintTree(VisualTree.Create<TextVisualTreeNode>(tree, t => new TextVisualTreeNode(t, nodeFont)), paintSettings, true).Bitmap;
        }

        public static Bitmap RenderSingleTreeAsColorTree(TreeView tree)
        {
            var paintSettings = new PaintSettings();
            return PaintTree(VisualTree.Create<CircleVisualTreeNode>(tree, t => new CircleVisualTreeNode(t)),paintSettings,true).Bitmap;
        }

        public static IEnumerable<Bitmap> RenderTreesAsColorTrees(IEnumerable<TreeView> trees)
        {
            var paintSettings = new PaintSettings();
            var images = RenderTrees<CircleVisualTreeNode>(trees, t => new CircleVisualTreeNode(t), paintSettings);
            return RenderFinal(images.ToArray(), new Size(640,480));
        }

        public static IEnumerable<Bitmap> RenderTreesAsTextTrees(IEnumerable<TreeView> trees)
        {
            var paintSettings = new PaintSettings();
            var nodeFont = new Font("Arial", 12, FontStyle.Bold);
            var images = RenderTrees<TextVisualTreeNode>(trees, t => new TextVisualTreeNode(t,nodeFont), paintSettings);
            return RenderFinal(images.ToArray(), new Size(320, 240));
        }

        private static BitmapWithOrigin PaintTree(VisualTree tree, PaintSettings paintSettings, bool alignCenter) 
        {
            if (alignCenter)
                tree.AlignInArea(tree.Bounds.Size);
            else
                tree.AlignLeft();

            var bounds = tree.RecalculateBounds();
          
            Rectangle rect = new Rectangle(0, 0, bounds.Right, bounds.Bottom);

            var bmp = new Bitmap(bounds.Width, bounds.Height);
            
            Graphics g = Graphics.FromImage(bmp);
            foreach (var line in tree.Lines)
            {
                if (line.Child.IsBlank)
                    continue;

                Rectangle r = line.Parent.Area;
                // Parent Center
                Point parentCenterPos = getRectangleCenter(r);

                // Child Center
                Point childCenterPos = getRectangleCenter(line.Child.Area);             
                g.DrawLine(paintSettings.GetPen(Color.Black, 2.0f), parentCenterPos, childCenterPos);
            }

            foreach (var node in tree.Nodes)
            {
                if (node.IsBlank)
                    continue;

                node.Paint(g, paintSettings);
            }
           
            g.Dispose();
            return new BitmapWithOrigin { Bitmap = bmp, Origin = getRectangleCenter(tree.Root.Area) };

        }

        private static Point getRectangleCenter(Rectangle r)
        {
            return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        }

        
        internal static IEnumerable<Bitmap> RenderFinal(BitmapWithOrigin[] bitmaps, Size frameSize)
        {
            var origin = new Point(frameSize.Width / 2, 50);
      
            foreach (var img in bitmaps)
            {
   
                float scale = 1.0f;
                var destRec = ScaleDestRec(img, origin, scale);

                while (destRec.Left < 0 || destRec.Right > frameSize.Width || destRec.Y < 0 || destRec.Bottom > frameSize.Height)
                {
                    destRec = ScaleDestRec(img, origin, scale);

                    scale -= .05f;
                    if (scale < .1f)
                        break;               
                }

                var output = new Bitmap(frameSize.Width, frameSize.Height);
                var g = Graphics.FromImage(output);
                g.Clear(Color.White);
                g.DrawImage(img.Bitmap, destRec, new Rectangle(0, 0, img.Bitmap.Width, img.Bitmap.Height), GraphicsUnit.Pixel);
                g.Dispose();
                yield return output;
            }
            
        }

        private static Rectangle ScaleDestRec(BitmapWithOrigin source, Point origin, float scale)
        {
            var scaledSourceOrigin = new Point((int)(source.Origin.X * scale), (int)(source.Origin.Y * scale));
            var offset = new Point(origin.X - scaledSourceOrigin.X, origin.Y - scaledSourceOrigin.Y);

            return new Rectangle(offset.X, offset.Y, (int)(source.Bitmap.Width * scale), (int)(source.Bitmap.Height * scale));

        }

        private static Bitmap RenderFinal(LinkedListNode<BitmapWithOrigin> imageNode, Size imageSize)
        {

            var bitmap = new Bitmap(imageSize.Width, imageSize.Height);
            var g = Graphics.FromImage(bitmap);

            // For a Nice Background.
            Color initialColor = Color.Azure;
            Color finalColor = Color.RoyalBlue;
            LinearGradientBrush brush =
                new LinearGradientBrush(new Rectangle(0, 0, imageSize.Width, imageSize.Height), initialColor, finalColor,
                                        LinearGradientMode.ForwardDiagonal);
            g.FillRectangle(brush, new Rectangle(0,0,imageSize.Width,imageSize.Height));


            var prev = imageNode.Previous;
            if (prev != null)
                RenderTree(prev.Value, g, imageSize, (i, p) => i.GetPixel(p.X, p.Y).Adjust(-200, 100, -20, -20));
        
            //var next = imageNode.Next;
            //if (next != null)
            //    RenderTree(next.Value, g, imageSize, c => c.Adjust(-200, -20, 100, -20));

            RenderTree(imageNode.Value, g, imageSize, (i, p) =>
            {
                var pixel = i.GetPixel(p.X, p.Y);
                int spread = 2;
                int count = 0, ta=0, tr = 0, tb=0, tg=0;

                for(int yy = p.Y-spread;yy < p.Y+spread;yy++)
                    for (int xx = p.X - spread; xx < p.X + spread; xx++)
                    {
                        if (xx >= 0 && yy >= 0 && xx < i.Width && yy < i.Height)
                        {
                            var pixel1 = i.GetPixel(xx,yy);
                            if(xx == p.X && yy == p.Y)
                                pixel1 = pixel1.Adjust(0, 200, 200, 255);
                            count++;

                            tr += pixel1.R;
                            tb += pixel1.B;
                            tg += pixel1.G;
                            ta += pixel1.A;
                        }
                    }
            
                if (count == 0)
                    return Color.FromArgb(0,255,255,255);

                var newPixel = Color.FromArgb((byte)(ta / count), (byte)(tr / count), (byte)(tg / count), (byte)(tb / count));
                return newPixel;
            });



            RenderTree(imageNode.Value, g, imageSize, (i,p) =>i.GetPixel(p.X,p.Y) );

            return bitmap;
        }

        private static void RenderTree(BitmapWithOrigin image, Graphics graphics, Size imageSize, Func<Bitmap,Point,Color> adjust)
        {
            Bitmap temp = new Bitmap(image.Bitmap.Width, image.Bitmap.Height);
            Graphics g2 = Graphics.FromImage(temp);
            g2.DrawImage(image.Bitmap, Point.Empty);

            for(int y = 0; y < temp.Height;y++)
                for (int x = 0; x < temp.Width; x++)
                    temp.SetPixel(x, y, adjust(temp, new Point(x,y)));


            var destOrigin = new Point((imageSize.Width / 2), 50);

            graphics.DrawImage(temp, new Point(destOrigin.X - image.Origin.X, 0));
        }
    }

    public static class ColorUtil
    {
        public static Color Adjust(this Color c, int a, int r, int g, int b)
        {
            int aa = c.A + a;
            int rr = c.R + r;
            int gg = c.G + g;
            int bb = c.B + b;

            if (aa < 0) aa = 0;
            if (rr < 0) rr = 0;
            if (gg < 0) gg = 0;
            if (bb < 0) bb = 0;

            if (aa > 255) aa = 255;
            if (rr > 255) rr = 255;
            if (gg > 255) gg = 255;
            if (bb > 255) bb = 255;

            return Color.FromArgb((byte)aa, (byte)rr, (byte)gg, (byte)bb);
        }
    }
}
