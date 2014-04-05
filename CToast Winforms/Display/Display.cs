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
        
        protected override Bitmap RenderNode(Node root)
        {
            var visualTree = mLayout.LayoutTree(root);

            var bmp = new Bitmap(mControl.Size.Width, mControl.Size.Height);
            var g = Graphics.FromImage(bmp);

            foreach (var step in mSteps.OrderByDescending(p => p.DrawLayer))
                step.Render(g, visualTree);

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
    }

    interface IRenderingStep
    {
        string Name { get; }
        int DrawLayer { get; }
        void Render(Graphics g, IVisualTree tree);
    }

    class RenderLines : IRenderingStep 
    {
        public string Name { get { return "Lines"; } }
        public int DrawLayer { get { return 1; } }

        public void Render(Graphics g, IVisualTree node)
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

            Render(g,node.LeftTree);
            Render(g,node.RightTree);
        } 
    }

    class RenderNodes : IRenderingStep
    {
        public virtual string Name { get { return "Nodes"; } }
        public int DrawLayer { get { return 0; } }

        private Font mFont = new Font("Arial", 10f, FontStyle.Regular);
       
        public void Render(Graphics g, IVisualTree node)
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

            var c1 = isLeft ? Color.LightBlue : Color.Yellow;
            var c2 = isLeft ? Color.DarkBlue : Color.DarkRed;

            c1 = Util.FadeColor(c1, c2, (float)node.Depth / (float)node.DeepestLevel);
            c2 = Util.FadeColor(c1, Color.Black, .2f);

            var brush = VisualTreeRenderer.CreateCircularGradientBrush(node.NodeBounds, c2, c1);
            g.FillEllipse(brush, node.NodeBounds);

            var pen = new Pen(Color.Black);
            g.DrawEllipse(pen, node.NodeBounds);

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

        public void Render(Graphics g, IVisualTree tree)
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

                Color c = isLeft ? Color.LightGreen : Color.LightBlue;
                var brush = new SolidBrush(c);
                g.FillPath(brush, path);
            }

            Render(g, tree.LeftTree, true);
            Render(g, tree.RightTree, false);
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

        public void Render(Graphics g, IVisualTree root)
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
    }

}
