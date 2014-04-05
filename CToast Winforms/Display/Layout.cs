using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CToast
{
    static class IVisualTreeUtil
    {
        public static IEnumerable<IVisualTree> GetChildren(this IVisualTree tree)
        {
            if (tree.LeftTree != null)
                yield return tree.LeftTree;

            if (tree.RightTree != null)
                yield return tree.RightTree;
        }

        public static IEnumerable<IVisualTree> Traverse(this IVisualTree tree)
        {
            yield return tree;
            if(tree.LeftTree != null)
                foreach(var n in tree.LeftTree.Traverse())
                    yield return n;

            if(tree.RightTree != null)
                foreach(var n in tree.RightTree.Traverse())
                    yield return n;
        }
    }

    abstract class TreeLayout<T> : TreeRenderer<VisualTreeNode<T>>, ITreeLayout 
    {
        public abstract string Name { get; }
        private VisualTreeRenderer mRenderer;

        public void SetRenderer(VisualTreeRenderer renderer)
        {
            mRenderer = renderer;
        }

        protected override VisualTreeNode<T> RenderNode(Node root)
        {
            if (root == null)
                return new VisualTreeNode<T>();

            var visualTree = CreateVisualTree(root, 0);

            var deepestLevel = visualTree.AllChildren.Max(p => p.Depth);
            foreach (var node in visualTree.AllChildren)
                node.DeepestLevel = deepestLevel;

         //   new DebugRelabelNodes<T>().DoLayout(visualTree, mControl.Size);

            foreach (var step in this.GetLayoutSteps())
                step.DoLayout(visualTree, mRenderer.LayoutSize);

            return visualTree;
        }

        protected abstract IEnumerable<ITreeLayoutStep<T>> GetLayoutSteps();
        protected abstract VisualTreeNode<T> CreateNewNode(Node n, int depth);

        private VisualTreeNode<T> CreateVisualTree(Node n, int depth)
        {
            if (n == null || n.Value == null)
                return null;

            var vt = CreateNewNode(n, depth);

            if (n.Value != null)
                vt.Text = n.Value.ToString();

            vt.LeftChild = CreateVisualTree(n.LeftNode, depth + 1);
            vt.RightChild = CreateVisualTree(n.RightNode, depth + 1);
            vt.Depth = depth;
            vt.Index = 1;
            vt.NodeSize = mRenderer.CalculateNodeSize(n);

            int index = 1;
            foreach (var child in vt.Children)
            {
                child.Parent = vt;
                child.Index = index;
                index++;
            }


            return vt;
        }

        public IVisualTree LayoutTree(Node n)
        {
            return RenderNode(n);
        }

    }

    class CenterTree<T> : ITreeLayoutStep<T>
    {
        public bool HorizontalOnly { get; set; }

        public void DoLayout(VisualTreeNode<T> tree,Size layoutArea)
        {
            var rootPos = tree.Position;

            var destinationPos = new Point((int)(layoutArea.Width / 2f),(int)(HorizontalOnly ? 10 : (layoutArea.Height /2f)));

            int dx = destinationPos.X - rootPos.X;
            int dy = destinationPos.Y - rootPos.Y;

            tree.Move(dx,dy);
        }
    }

    class SplitCollidingChildren<T> : ITreeLayoutStep<T>
    {

        public void DoLayout(VisualTreeNode<T> tree, Size layoutArea)
        {
            DoLayout(tree, 0);
        }

        private void DoLayout(VisualTreeNode<T> tree, int shift)
        {
            tree.X += shift;

            if (tree.LeftTree == null || tree.RightTree == null)            
            {
                if (tree.LeftChild != null)
                    DoLayout(tree.LeftChild, shift);
                else if (tree.RightChild != null)
                    DoLayout(tree.RightChild, shift);

                return;
            }

            int overlap = Math.Max(0, tree.LeftChild.NodeBoundsWithPadding.Right - tree.RightChild.NodeBoundsWithPadding.Left);          
            DoLayout(tree.LeftChild, shift - (overlap/2));
            DoLayout(tree.RightChild, shift + (overlap/2));

        }
    }


    class DebugRelabelNodes<T> : ITreeLayoutStep<T>
    {

        public void DoLayout(VisualTreeNode<T> tree, Size layoutArea)
        {
            int val = 0;
            foreach (var node in tree.BreadthFirstTraversal())
                node.Text = (++val).ToString();
        }


    }
}
