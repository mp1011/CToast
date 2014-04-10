using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CToast
{

    class VisualTreeRenderer : TreeRenderer<Bitmap>
    {
        private Control mControl;
        private ITreeLayout mLayout;

        private List<IRenderingStep> mSteps = new List<IRenderingStep>();

        public VisualTreeRenderer(Control control, ITreeLayout layout, IEnumerable<IRenderingStep> steps)
        {
            mControl = control;
            mLayout = layout;

            mSteps.AddRange(steps);
        }

        public Size LayoutSize { get { return mControl.Size; } }

        protected override Bitmap RenderNode(Node root)
        {
            var visualTree = mLayout.LayoutTree(root);

            var treeSize = AlignTree(visualTree, mControl.Size);
            var maxSize = new Size(mControl.Width * 4, mControl.Height * 4);

            Bitmap bmp;
            Graphics g;

            if (treeSize.Width > maxSize.Width || treeSize.Height > maxSize.Height)
            {
                bmp = new Bitmap(maxSize.Width, maxSize.Height);
                g = Graphics.FromImage(bmp);
                g.ScaleTransform(maxSize.Width / treeSize.Width, maxSize.Width / treeSize.Width);
            }
            else
            {
                bmp = new Bitmap(treeSize.Width, treeSize.Height);
                g = Graphics.FromImage(bmp);
            }
            

            foreach (var step in mSteps.OrderByDescending(p => p.DrawLayer))
                step.Render(g, visualTree, treeSize);

            g.Dispose();
            return bmp;

        }

        private Size AlignTree(IVisualTree tree, Size minimumSize)
        {

            tree.Move(-tree.TreeBounds.Left, -tree.TreeBounds.Top);
            int dx = 0, dy = 0;
            Size targetSize = Size.Empty;

            var centerToLeft = tree.Position.X;
            var centerToRight = tree.TreeBounds.Right - tree.Position.X;

            if (centerToRight > centerToLeft)
            {
                dx =(centerToRight - centerToLeft);
                centerToLeft = tree.Position.X + dx;
            }
            targetSize.Width = centerToLeft * 2;

            if (mLayout.Orientation == LayoutOrientation.Center)
            {
                var centerToTop = tree.Position.Y;
                var centerToBottom = tree.TreeBounds.Bottom - tree.Position.Y;

                if (centerToBottom > centerToTop)
                {
                    dy = (centerToBottom - centerToTop);
                    centerToTop = tree.Position.Y + dx;
                }

                targetSize.Height = centerToTop * 2;
            }
            else
            {
                dy = 10;
            }

            tree.Move(dx, dy);

            if (mLayout.Orientation == LayoutOrientation.TopDown)
                targetSize.Height = tree.TreeBounds.Bottom+20;

            dx = 0;
            dy = 0;
            if (targetSize.Width < minimumSize.Width)
            {
                dx = (minimumSize.Width / 2) - tree.Position.X;
                targetSize.Width = minimumSize.Width;
            }
                    
            dy = 0;
            if (mLayout.Orientation == LayoutOrientation.Center)
                dy = (minimumSize.Height / 2) - tree.Position.Y;
          
            tree.Move(dx, dy);

            return targetSize;
        }
        
        public static PathGradientBrush CreateCircularGradientBrush(Rectangle area, Color outer, Color inner)
        {
            var path = new GraphicsPath();
            path.AddEllipse(area);
            var brush = new PathGradientBrush(path);
            brush.SurroundColors = new Color[] { outer };
            brush.CenterColor = inner;
            return brush;
        }

        public Size CalculateNodeSize(Node node)
        {
            Size largest = Size.Empty;

            foreach (var step in mSteps)
            {
                var size = step.CalculateNodeSize(node);
                if (size.Width * size.Height > largest.Width * largest.Height)
                    largest = size;
            }

            if (largest.Width == 0 || largest.Height == 0)
                return new Size(8, 8);
            else
                return largest;

        }

        public static Color GetNodeInnerColor(IVisualTree node, bool isLeft)
        {
            if (node == null || String.IsNullOrEmpty(node.Text))
                return Color.White;

            int dummy; double dummy2;
            if (Int32.TryParse(node.Text, out dummy) || double.TryParse(node.Text, out dummy2))
            {
                if (isLeft)
                    return Color.LightBlue;
                else
                    return Color.LightYellow;
            }

            switch (node.Text)
            {
                case ",":
                case "&":
                case "[]":
                case "head":
                case "tail":
                    return Color.LightGreen;
                case "@":
                    return Color.Pink;
                case "+":
                case "-":
                case "*":
                case "/":
                case "%":
                    return Color.Gray;

                default:
                    return Color.White;
            }
        }

        public static Color GetNodeOuterColor(IVisualTree node, bool isLeft)
        {
            if (node == null || String.IsNullOrEmpty(node.Text))
                return Color.White;

            int dummy; double dummy2;
            if (Int32.TryParse(node.Text, out dummy) || double.TryParse(node.Text, out dummy2))
            {
                if (isLeft)
                    return Color.DarkBlue;
                else
                    return Color.Orange;
            }

            switch (node.Text)
            {
                case ",":
                case "&":
                case "[]":
                case "head":
                case "tail":
                    return Color.DarkGreen;
                case "@":
                    return Color.DarkRed;
                case "+":
                case "-":
                case "*":
                case "/":
                case "%":
                    return Color.DarkGray;

                default:
                    return Color.DarkGray;
            }
        }
    }

    interface IRenderingStep
    {
        string Name { get; }
        int DrawLayer { get; }

        void Render(Graphics g, IVisualTree tree, Size imageSize);
        Size CalculateNodeSize(Node node);
    }

    class RenderLines : IRenderingStep 
    {
        public string Name { get { return "Lines"; } }
        public int DrawLayer { get { return 2; } }

        public void Render( Graphics g, IVisualTree node, Size imageSize)
        {
            if (node == null)
                return;


            var pen1 = new Pen(Util.FadeColor(Color.LightBlue, Color.DarkBlue, node.DepthPercent()),  2f);
           
            if (node.LeftTree != null)
            {
                g.DrawLine(pen1, node.Position, node.LeftTree.Position);
            //    g.DrawLine(pen2, node.Position, node.LeftTree.Position);
            }

            if (node.RightTree != null)
            {
                g.DrawLine(pen1, node.Position, node.RightTree.Position);
              //  g.DrawLine(pen2, node.Position, node.RightTree.Position);
            }

            Render(g, node.LeftTree, imageSize);
            Render(g, node.RightTree, imageSize);
        }

        public Size CalculateNodeSize(Node node)
        {
            return Size.Empty;
        }
    }

    class RenderLinesAlt : IRenderingStep
    {
        public string Name { get { return "Lines (alt)"; } }
        public int DrawLayer { get { return 2; } }

        public void Render(Graphics g, IVisualTree node, Size imageSize)
        {
            if (node == null)
                return;

            var pen1 = new Pen(Util.FadeColor(Color.LightBlue, Color.DarkBlue, node.DepthPercent()), 2f);

            if (node.LeftTree != null)
            {                
                var yMid = node.Position.Y + ((node.LeftTree.Position.Y - node.Position.Y) / 2);
                g.DrawLine(pen1, node.Position, new Point(node.Position.X, yMid));
                g.DrawLine(pen1, new Point(node.Position.X,yMid), new Point(node.LeftTree.Position.X,yMid));
                g.DrawLine(pen1, new Point(node.LeftTree.Position.X, yMid), node.LeftTree.Position);
            }

            if (node.RightTree != null)
            {
                var yMid = node.Position.Y + ((node.RightTree.Position.Y - node.Position.Y) / 2);
                g.DrawLine(pen1, node.Position, new Point(node.Position.X, yMid));
                g.DrawLine(pen1, new Point(node.Position.X, yMid), new Point(node.RightTree.Position.X, yMid));
                g.DrawLine(pen1, new Point(node.RightTree.Position.X, yMid), node.RightTree.Position);
            }

            Render(g, node.LeftTree, imageSize);
            Render(g, node.RightTree, imageSize);
        }

        public Size CalculateNodeSize(Node node)
        {
            return Size.Empty;
        }
    }


    class RenderNodes : IRenderingStep
    {
        public virtual string Name { get { return "Nodes"; } }
        public int DrawLayer { get { return 1; } }


        public void Render( Graphics g, IVisualTree node, Size imageSize)
        {
            if (node == null)
                return;

            RenderNode(g, node, false);
        }

        protected void RenderNode(Graphics g, IVisualTree node, bool isLeft)
        {
            if (node == null)
                return;

            if (RenderNodeSpecial(g, node))
                return;

            var c1 = VisualTreeRenderer.GetNodeInnerColor(node, isLeft);
            var c2 = VisualTreeRenderer.GetNodeOuterColor(node, isLeft);

            c1 = Util.FadeColor(c1, c2, (float)node.Depth / (float)node.DeepestLevel);
            c2 = Util.FadeColor(c1, Color.Black, .2f);

            var pen = new Pen(Color.Black);

            if (node.Text.Length > 3)
            {
                var path = new GraphicsPath();
                path.AddRectangle(node.NodeBounds);
                var brush = new PathGradientBrush(path);
                brush.CenterColor = c1;
                brush.SurroundColors = new Color[] { c2 };
                g.FillRectangle(brush, node.NodeBounds);
                g.DrawRectangle(pen, node.NodeBounds);
            }
            else
            {
                var brush = VisualTreeRenderer.CreateCircularGradientBrush(node.NodeBounds, c2, c1);
                g.FillEllipse(brush, node.NodeBounds);
                g.DrawEllipse(pen, node.NodeBounds);
            }

            RenderNode(g,node.LeftTree, true);
            RenderNode(g,node.RightTree, false);
        }

        protected virtual bool RenderNodeSpecial(Graphics g, IVisualTree node)
        {
            return false;
        }

        public Size CalculateNodeSize(Node node)
        {
            return new Size(8, 8);
        }
    }

    class RenderTriangles : IRenderingStep
    {

        public string Name
        {
            get { return "Triangles"; }
        }

        public int DrawLayer
        {
            get { return 100; }
        }

        public void Render( Graphics g, IVisualTree tree, Size imageSize)
        {
            Render(g, tree, false);
        }

        private void Render(Graphics g, IVisualTree tree, bool isLeft)
        {
            if (tree == null)
                return;
             
            if (tree.LeftTree != null && tree.RightTree != null)
            {
                var path = new GraphicsPath();
                path.AddPolygon(new Point[] { tree.Position, tree.LeftTree.Position, tree.RightTree.Position });

                Color c = VisualTreeRenderer.GetNodeInnerColor(tree, isLeft);
                var brush = new SolidBrush(c);
                g.FillPath(brush, path);
            }

            Render(g, tree.LeftTree, true);
            Render(g, tree.RightTree, false);
        }

        public Size CalculateNodeSize(Node node)
        {
            return Size.Empty;
        }
    }

    class RenderCircles : IRenderingStep
    {
        public string Name
        {
            get { return "Circles"; }
        }

        public int DrawLayer
        {
            get { return 101; }
        }

        public void Render( Graphics g, IVisualTree root, Size imageSize)
        {

            foreach (var node in root.Traverse().Reverse())
            {
                Color c = (node.Depth % 2 == 0) ? Color.LightBlue : Color.LightSeaGreen;

                c = Util.FadeColor(c, Color.White, ((float)node.Depth / (float)node.DeepestLevel));
                int radius = Util.GetPointDistance(root.Position, node.Position);

                Rectangle circleArea = new Rectangle(root.Position.X - radius, root.Position.Y - radius, radius * 2, radius * 2);

                g.FillEllipse(new SolidBrush(c), circleArea);

                
            }
        }

        public Size CalculateNodeSize(Node node)
        {
            return Size.Empty;
        }
    }

    class RenderText : IRenderingStep
    {
        public virtual string Name { get { return "Text"; } }
        public int DrawLayer { get { return 0; } }

        public void Render( Graphics g, IVisualTree node, Size imageSize)
        {
            if (node == null)
                return;

            RenderNode(g, node, false);
        }

        private Font mFont = new Font("Arial", 10f, FontStyle.Regular);

        protected void RenderNode(Graphics g, IVisualTree node, bool isLeft)
        {
            if (node == null)
                return;

            StringFormat format = StringFormat.GenericTypographic;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;

        //    if (node.Depth < node.DeepestLevel / 2f)
            //    g.DrawString(node.Text, this.mFont, new SolidBrush(Color.Black), node.NodeBounds, format);
          //  else
                            g.DrawString(node.Text, this.mFont, new SolidBrush(Color.White), node.NodeBounds, format);

            RenderNode(g, node.LeftTree, true);
            RenderNode(g, node.RightTree, false);

        }


        public Size CalculateNodeSize(Node node)
        {
            if (node == null || node.Value == null)
                return Size.Empty;

            string text = node.Value.ToString();

            var textSize = System.Windows.Forms.TextRenderer.MeasureText(text, mFont, new Size(10000, 10000), TextFormatFlags.SingleLine);

            if (textSize.Width < 16)
                textSize = new Size(16, textSize.Height);

            if (text.Length < 3)
                return new Size(textSize.Width, textSize.Width);
            else
                return textSize;
        }


    }

    class RenderBackdrop : IRenderingStep
    {

        public string Name
        {
            get { return "Backdrop"; }
        }

        public int DrawLayer
        {
            get { return 10000; }
        }

        public void Render( Graphics g, IVisualTree tree, Size imageSize)
        {
             var  rec = new Rectangle(0, 0, imageSize.Width, imageSize.Height);
            LinearGradientBrush brush = new LinearGradientBrush(rec, Color.White, Color.AliceBlue ,
                                        LinearGradientMode.ForwardDiagonal);
            g.FillRectangle(brush, rec);
        }

        public Size CalculateNodeSize(Node node)
        {
            return Size.Empty;
        }
    }

    class RenderSyntax : IRenderingStep
    {

        public string Name
        {
            get { return "Expression"; }
        }

        public int DrawLayer
        {
            get { return 1; }
        }

        public void Render( Graphics g, IVisualTree tree, Size imageSize)
        {
            if (tree == null)
                return;

            var text = new SyntaxRenderer().Render(tree.OriginalNode);

            g.FillRectangle(new SolidBrush(Color.Black), tree.NodeBounds);
            g.DrawRectangle(new Pen(Color.Gray), tree.NodeBounds);

            StringFormat format = StringFormat.GenericTypographic;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;

            g.DrawString(text, mFont, new SolidBrush(Color.LightBlue), tree.NodeBounds, format);


            Render(g, tree.LeftTree, imageSize);
            Render(g, tree.RightTree, imageSize);
        }

        private Font mFont = new Font("Courier", 10f, FontStyle.Regular);

        public Size CalculateNodeSize(Node node)
        {

            if (node == null)
                return Size.Empty;

            var text = new SyntaxRenderer().Render(node);

            var textSize = System.Windows.Forms.TextRenderer.MeasureText(text, mFont, new Size(10000, 10000), TextFormatFlags.SingleLine);
            return textSize;
        }
    }


    abstract class TypedRenderingStep<T> : IRenderingStep
    {

        public string Name { get { return GetName(); } }


        protected abstract string GetName();

        public int  DrawLayer
        {
	        get { return 2000; }
        }

        public void Render( Graphics g, IVisualTree tree, Size imageSize)
        {
 	        var cast = tree as VisualTreeNode<T>;
            if(cast == null)
                return;

            RenderTyped(g,cast,imageSize);
        }

        protected abstract void RenderTyped(Graphics g, VisualTreeNode<T> node, Size imageSize);


        public Size  CalculateNodeSize(Node node)
        {
 	        return Size.Empty;
        }
    }

    class RenderSunburst : TypedRenderingStep<RadialTreeNodeData>
    {

        protected override string  GetName()
        {
         return "Sunburst (Radial Only)"; 
        }

        protected override void RenderTyped(Graphics g, VisualTreeNode<RadialTreeNodeData> tree, Size imageSize)
        {
            PaintNode(tree,tree,g,false);
        }

        private Rectangle GetNodeRec(IVisualTree tree, IVisualTree center)
        {
            if (tree == null)
                tree = center;

            int radius = Util.GetPointDistance(tree.Position, center.Position);
            if (radius == 0)
                radius = 32;
            else
                radius += 16;

            return new Rectangle(center.Position.X - radius, center.Position.Y - radius, radius * 2, radius * 2);
        }


        private void PaintNode(VisualTreeNode<RadialTreeNodeData> center, VisualTreeNode<RadialTreeNodeData> node, Graphics g, bool isLeft)
        {
            if (node == null)
                return;

            Color c;
            Color c2 = Color.Black;
            if (node.OriginalNode.IsAtomic)
            {
                c = Color.LightYellow;
                c2 = Color.Gold;
            }
            else if (isLeft)
            {
                c = Color.White;
                c2 = Color.LightGreen;
            }
            else
            {
                c = Color.White;
                c2 = Color.LightBlue;
            }

            float pct = .05f * node.Depth;
            while (pct > 1)
                pct -= 1f;

            c = Util.FadeColor(c, c2, pct);
            c2= Util.FadeColor(c2, Color.Black, pct);

            PaintArc(center, node, g, c,c2);

            PaintNode(center, node.LeftChild, g, true);
            PaintNode(center, node.RightChild, g, false);

        }

        private void PaintArc(VisualTreeNode<RadialTreeNodeData> center, VisualTreeNode<RadialTreeNodeData> node, Graphics g, Color c, Color c2)
        {
            int start = node.Data.RangeStart;
            int sweep = node.Data.RangeSweep;

            var rec = GetNodeRec(node, center);
            var rec2 = GetNodeRec(node.Parent, center);

            if (node.Parent == null)
            {
                g.FillEllipse(new SolidBrush(c), rec);
                return;
            }
          
            var path = new GraphicsPath();

            path.StartFigure();

            path.AddArc(rec, Util.FixAngle(-start), -sweep);

            path.AddArc(rec2, Util.FixAngle(-start-sweep), sweep);

            path.CloseFigure();

            var brush = VisualTreeRenderer.CreateCircularGradientBrush(rec, c2, c);
            g.FillPath(brush, path);              
            g.DrawPath(new Pen(Util.FadeColor(c2,Color.Black,.5f), 1f), path);

        }

    }
}
