using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace CToast
{

    class TreeViewRenderer : TreeRenderer<TreeView>
    {
        private TreeView mTree;

        public TreeViewRenderer()
        {
            mTree = new TreeView();
        }

        public TreeViewRenderer(TreeView treeView)
        {
            mTree = treeView;
        }

        protected override TreeView RenderNode(Node root)
        {
            mTree.Nodes.Clear();
            AddNodes(mTree.Nodes, root);
            return mTree;
        }

        private void AddNodes(TreeNodeCollection treeNodes, Node node)
        {
            var treeNode = treeNodes.Add(node.ToString());
            treeNode.Tag = node.Id;
            
            if (node.LeftNode != null)
                AddNodes(treeNode.Nodes, node.LeftNode);
            if (node.RightNode != null)
                AddNodes(treeNode.Nodes, node.RightNode);

            treeNode.Expand();
        }
    }




    class HybridRenderer : TreeRenderer<Bitmap>
    {

        #region Factory

        public static HybridRenderer TypeA()
        {
            var hybrid = new HybridRenderer();

            var topLeftSize = 128;
            var totalWidth = 1024;
            var totalHeight = 768;

            hybrid.AddFrame(new RadialLayout(), new Rectangle(0, 0, topLeftSize, topLeftSize), new RenderLines(), new RenderNodes(), new RenderSunburst());
            hybrid.AddFrame(new RadialLayout(), new Rectangle(0, topLeftSize, topLeftSize, topLeftSize), new RenderLines());
            hybrid.AddFrame(new BuchheimLayout(), new Rectangle(0, topLeftSize*2, topLeftSize, topLeftSize), new RenderLines());
          
            
            hybrid.AddFrame(new BuchheimLayout(), new Rectangle(topLeftSize, 0, totalWidth - topLeftSize, totalHeight), new RenderLinesAlt(), new RenderNodes(), new RenderText(), new RenderExpression());

            return hybrid;
        }

        public static HybridRenderer TypeB()
        {
            var hybrid = new HybridRenderer();

            var totalWidth = 1024;
            var totalHeight = 768;
            var topHeight = 64;

            hybrid.AddFrame(new NullLayout(), new Rectangle(0, 0, totalWidth, topHeight), new RenderExpression());
            hybrid.AddFrame(new BuchheimLayout(), new Rectangle(0, topHeight, totalWidth / 2, totalHeight- topHeight), new RenderLinesAlt(), new RenderNodes(), new RenderText());
            hybrid.AddFrame(new RadialLayout(), new Rectangle(totalWidth / 2, topHeight, totalWidth / 2, totalHeight - topHeight), new RenderLines(), new RenderNodes(), new RenderSunburst());

            return hybrid;
        }


        #endregion

        class Frame
        {
            public TreeRenderer<Bitmap> Renderer;
            public LayoutOrientation Orientation;
            public Rectangle Location;
        }

        private List<Frame> mFrames = new List<Frame>();

        public void AddFrame(ITreeLayout layout, Rectangle area, params IRenderingStep[] renderSteps)
        {

            var renderer = new VisualTreeRenderer(() => area.Size, layout, renderSteps);
            layout.SetRenderer(renderer);
            mFrames.Add(new Frame { Renderer = renderer, Orientation = layout.Orientation, Location = area });
        }

        protected override Bitmap RenderNode(Node root)
        {

            int width = mFrames.Max(p => p.Location.Right);
            int height = mFrames.Max(p => p.Location.Bottom);

            var bmp = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(bmp);

            g.Clear(Color.White);

            foreach (var frame in mFrames)
                RenderFrame(frame, g, root);


            return bmp;
        }


        private void RenderFrame(Frame frame, Graphics g, Node root)
        {
            var frameBmp = frame.Renderer.Render(root);
            var aspectRatio = (float)frameBmp.Height / frameBmp.Width;

            var dest = frame.Location;
            dest.Size = new Size(dest.Width, (int)(dest.Width * aspectRatio));

            if (dest.Height > frame.Location.Height)
                dest.Size = new Size((int)(dest.Height / aspectRatio), dest.Height);


            if (dest.Width < frame.Location.Width)
                dest.X = dest.X + (frame.Location.Width - dest.Width) / 2;

            if (frame.Orientation == LayoutOrientation.Center && dest.Height < frame.Location.Height)
                dest.Y = dest.Y + (frame.Location.Height - dest.Height) / 2;

            g.DrawImage(frameBmp, dest);

        }
    }
}


