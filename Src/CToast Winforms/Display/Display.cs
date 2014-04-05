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

            var bmp = new Bitmap(mControl.Size.Width, mControl.Size.Height);
            var g = Graphics.FromImage(bmp);

            foreach (var step in mSteps.OrderByDescending(p => p.DrawLayer))
                step.Render(g, visualTree, mControl.Size);

            g.Dispose();
            return bmp;

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
        public int DrawLayer { get { return 1; } }

        public void Render(Graphics g, IVisualTree node, Size imageSize)
        {
            if (node == null)
                return;

            var pen1 = new Pen(Color.DarkBlue, 2f);
            var pen2 = new Pen(Color.Blue);
       
            if (node.LeftTree != null)
            {
                g.DrawLine(pen1, node.Position, node.LeftTree.Position);
                g.DrawLine(pen2, node.Position, node.LeftTree.Position);
            }

            if (node.RightTree != null)
            {
                g.DrawLine(pen1, node.Position, node.RightTree.Position);
                g.DrawLine(pen2, node.Position, node.RightTree.Position);
            }

            Render(g,node.LeftTree, imageSize);
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
        public int DrawLayer { get { return 0; } }

        private Font mFont = new Font("Arial", 10f, FontStyle.Regular);

        public void Render(Graphics g, IVisualTree node, Size imageSize)
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

            StringFormat format = StringFormat.GenericTypographic;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;
            format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;

            if (node.Depth < node.DeepestLevel / 2f)
                g.DrawString(node.Text, this.mFont, new SolidBrush(Color.Black), node.NodeBounds, format);
            else
                g.DrawString(node.Text, this.mFont, new SolidBrush(Color.White), node.NodeBounds, format);

            RenderNode(g,node.LeftTree, true);
            RenderNode(g,node.RightTree, false);
        }

        protected virtual bool RenderNodeSpecial(Graphics g, IVisualTree node)
        {
            return false;
        }

        public Size CalculateNodeSize(Node node)
        {
            var size = CalculateNodeSizeSpecial(node);
            if (!size.IsEmpty)
                return size;

            
            if(node == null || node.Value == null)
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

        protected virtual Size CalculateNodeSizeSpecial(Node node)
        {
            return Size.Empty;
        }
    }

    class RenderLeaves : RenderNodes
    {
        public override string Name
        {
            get
            {
                return "Leaves Only";
            }
        }
    
        protected override bool RenderNodeSpecial(Graphics g, IVisualTree node)
        {
            if (node.LeftTree != null || node.RightTree != null)
            {
                RenderNode(g, node.LeftTree, true);
                RenderNode(g, node.RightTree, false);
                return true; 
            }

            return false;

        }

        protected override Size CalculateNodeSizeSpecial(Node node)
        {
            if (!node.IsAtomic)
                return new Size(16, 16);
            else
                return Size.Empty;
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

        public void Render(Graphics g, IVisualTree tree, Size imageSize)
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

        public void Render(Graphics g, IVisualTree root, Size imageSize)
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

    class RenderNodesWithoutText : IRenderingStep
    {
        public virtual string Name { get { return "Nodes w/o Text"; } }
        public int DrawLayer { get { return 0; } }

        public void Render(Graphics g, IVisualTree node, Size imageSize)
        {
            if (node == null)
                return;

            RenderNode(g, node, false);
        }

        protected void RenderNode(Graphics g, IVisualTree node, bool isLeft)
        {
            if (node == null)
                return;

            var c1 = VisualTreeRenderer.GetNodeInnerColor(node, isLeft);
            var c2 = VisualTreeRenderer.GetNodeOuterColor(node, isLeft);

            c1 = Util.FadeColor(c1, c2, (float)node.Depth / (float)node.DeepestLevel);
            c2 = Util.FadeColor(c1, Color.Black, .2f);

            var pen = new Pen(Color.Black);
          
            var brush = VisualTreeRenderer.CreateCircularGradientBrush(node.NodeBounds, c2, c1);
            g.FillEllipse(brush, node.NodeBounds);
            g.DrawEllipse(pen, node.NodeBounds);
            
            RenderNode(g, node.LeftTree, true);
            RenderNode(g, node.RightTree, false);
        }


        public Size CalculateNodeSize(Node node)
        {
            return new Size(8, 8);
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

        public void Render(Graphics g, IVisualTree tree, Size imageSize)
        {
            var rec = new Rectangle(0,0,imageSize.Width,imageSize.Height);
            LinearGradientBrush brush = new LinearGradientBrush(rec, Color.White, Color.LightSkyBlue,
                                        LinearGradientMode.ForwardDiagonal);
            g.FillRectangle(brush, rec);
        }

        public Size CalculateNodeSize(Node node)
        {
            return Size.Empty;
        }
    }
}
