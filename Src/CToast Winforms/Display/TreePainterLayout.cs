using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using VNode = CToast.VisualTreeNode<CToast.BottomUpNodeData>;
using System.Collections;

namespace CToast
{

    class BottomUpNodeData
    {   
        public bool IsBlank { get; set; }
        public int XPadding { get; set; }
        public int YPadding { get; set; }
    }


    class BottomUpLayout : TreeLayout<BottomUpNodeData>
    {
        public override string Name
        {
            get { return "Bottom Up"; }
        }
        public override LayoutOrientation Orientation
        {
            get { return LayoutOrientation.TopDown; }
        }

        private Font mFont = new Font("Arial", 12, FontStyle.Bold);

        protected override VNode CreateNewNode(Node n, int depth)
        {
            var data = new BottomUpNodeData { IsBlank =false, XPadding = 20, YPadding = 20 };
            return new VisualTreeNode<BottomUpNodeData>(n) { Data = data };
        }

        protected override IEnumerable<ITreeLayoutStep<BottomUpNodeData>> GetLayoutSteps()
        {
            yield return new FillBlanksStep();
            yield return new BottomUpLayoutStep();
            yield return new RemoveBlanksStep();
            yield return new SplitCollidingChildren<BottomUpNodeData>();
        }

        class FillBlanksStep : ITreeLayoutStep<BottomUpNodeData>
        {

            public void DoLayout(VNode tree, Size layoutArea)
            {
                int targetLevel = tree.DeepestLevel;
                FillBlanks(tree, null, false, targetLevel);
            }

            private void FillBlanks(VNode tree, VNode parent, bool isLeft, int deepestLevel)
            {
                if (tree == null && parent.Depth < deepestLevel)
                {
                    VNode tnBlank = new VNode(new CToast.LiteralNode<string>("")) { Text = "X", Data = new BottomUpNodeData { IsBlank = true, XPadding = 0, YPadding = 0 } };
                    tnBlank.Depth = parent.Depth + 1;

                    if (isLeft)
                        parent.LeftChild = tnBlank;
                    else
                        parent.RightChild = tnBlank;

                    tree = tnBlank;
                }

                if (tree != null)
                {
                    FillBlanks(tree.LeftChild, tree, true, deepestLevel);
                    FillBlanks(tree.RightChild, tree, false, deepestLevel);
                }
            }
        }

        class BottomUpLayoutStep : ITreeLayoutStep<BottomUpNodeData>
        {

            public void DoLayout(VNode tree, Size layoutArea)
            {

                int level = tree.DeepestLevel;

                int x = 0;
                int y = 0;

                var allNodes = tree.AllChildren.ToArray();
                int yStep = 0;

                foreach(var node in allNodes.Where(p=>p.Depth == level))
                {
                    node.X = x + node.NodeBounds.Width/2;
                    node.Y = 0;
                    x += node.NodeBounds.Width/2 + node.Data.XPadding;
                    yStep = Math.Max(yStep, node.Data.YPadding);
                }

                while (--level >= 0)
                {
                    y -= yStep;
                    yStep = 0;
                    foreach (var node in allNodes.Where(p => p.Depth == level))
                    {
                        int leftPos = node.Children.First().Position.X;
                        int rightPos = node.Children.Last().Position.X;

                        node.X = leftPos + (rightPos - leftPos) / 2;
                        node.Y = y;

                        yStep = Math.Max(yStep, node.Data.YPadding);
                    }
                }
            }
        }

        class RemoveBlanksStep : ITreeLayoutStep<BottomUpNodeData>
        {
            public void DoLayout(VNode node, Size layoutArea)
            {
                if (node == null)
                    return;

                if (node.LeftChild != null && node.LeftChild.Data.IsBlank)
                    node.LeftChild = null;

                if (node.RightChild != null && node.RightChild.Data.IsBlank)
                    node.RightChild = null;

                DoLayout(node.LeftChild, layoutArea);
                DoLayout(node.RightChild, layoutArea);
            }
        }

    }
}
