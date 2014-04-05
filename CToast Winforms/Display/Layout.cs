using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CToast
{
    interface IVisualTree
    {
        IVisualTree Parent { get; }
        IVisualTree LeftTree { get; }
        IVisualTree RightTree { get; }
        Point Position { get; }
        Rectangle NodeBounds { get; }
        Rectangle TreeBounds { get; }
        int Depth { get; }
        int ChildNodeCount { get; }
        string Text { get; }
        void Move(int dx, int dy);
        int DeepestLevel { get; }
    }

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

    interface ITreeLayout
    {
        IVisualTree LayoutTree(Node root);
    }

    class VisualTreeNode<T> : IVisualTree
    {
        public string Text { get; set; }

        public VisualTreeNode<T> Parent { get; set; }
        public VisualTreeNode<T> LeftChild { get; set; }
        public VisualTreeNode<T> RightChild { get; set; }
        public Point Position { get; set; }
        public int Depth { get; set; }

        public int Index { get; set; }

        public int X { get { return Position.X; } set { Position = new Point(value, Position.Y); } }
        public int Y { get { return Position.Y; } set { Position = new Point(Position.X, value); } }

        public T Data { get; set; }

        public IEnumerable<VisualTreeNode<T>> Children
        {
            get
            {
                if (LeftChild != null) yield return LeftChild;
                if (RightChild != null) yield return RightChild;
            }
        }
        public Rectangle NodeBounds
        {
            get
            {
                return new Rectangle(this.X - 10, this.Y - 10, 20, 20);
            }
        }

        private Rectangle mBounds;
        public Rectangle TreeBounds
        {
            get
            {
                if (mBounds != Rectangle.Empty)
                    return mBounds;

                var ac = this.AllChildren.ToArray();
                var minX = ac.Min(p => p.NodeBounds.Left);
                var maxX = ac.Max(p => p.NodeBounds.Right);
                var minY = ac.Min(p => p.NodeBounds.Top);
                var maxY = ac.Max(p => p.NodeBounds.Bottom);

                mBounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
                return mBounds;
            }
        }

        private VisualTreeNode<T> mLeftBrother = null;
        private bool mDidLeftBrotherCalc;

        public VisualTreeNode<T> LeftBrother
        {
            get
            {
                if (mDidLeftBrotherCalc)
                    return mLeftBrother;


                if (this.Parent == null)
                    return null;

                if (Parent.RightChild == this)
                    mLeftBrother = Parent.LeftChild;

                mDidLeftBrotherCalc = true;

                return mLeftBrother;
            }
        }

      
        public IEnumerable<VisualTreeNode<T>> AllChildren
        {
            get
            {
                yield return this;
                foreach (var c in this.Children)
                {
                    foreach (var cc in c.AllChildren)
                        yield return cc;
                }
            }
        }

        public VisualTreeNode<T> LeftMostSibling
        {
            get
            {
                if (this.Parent != null && this != Parent.Children.First())
                    return Parent.Children.First();
                else
                    return null;
            }
        }

        public void Move(int x, int y)
        {
            this.Position = new Point(this.Position.X + x, this.Position.Y + y);

            foreach (var child in Children)
                child.Move(x, y);

            mBounds = Rectangle.Empty;
        }

        private int mCount = -1;

        public int ChildNodeCount
        {
            get
            {
                if (mCount > -1)
                    return mCount;

                mCount = 0;
                if (this.LeftTree != null)
                    mCount += this.LeftTree.ChildNodeCount;
                if (this.RightChild != null)
                    mCount += this.RightTree.ChildNodeCount;

                if (this.LeftTree == null && this.RightTree == null)
                    mCount = 1;

                return mCount;
            }
        }

        private int mTreeDepth = -1;
        public int TreeDepth { 
            get
            {
                if (mTreeDepth != -1)
                    return mTreeDepth;

                if (this.LeftChild != null && this.RightChild != null)
                    mTreeDepth = 1 + Math.Max(this.LeftChild.TreeDepth, this.RightChild.TreeDepth);
                else if (this.LeftChild != null)
                    mTreeDepth = 1 + this.LeftChild.TreeDepth;
                else if (this.RightChild != null)
                    mTreeDepth = 1 + this.RightChild.TreeDepth;
                else
                    mTreeDepth = 1;
               

                return mTreeDepth;
            }
        }

        public IEnumerable<VisualTreeNode<T>> BreadthFirstTraversal()
        {
            Queue<VisualTreeNode<T>> queue = new Queue<VisualTreeNode<T>>();
            queue.Enqueue(this);

            List<VisualTreeNode<T>> output = new List<VisualTreeNode<T>>();
            
            while(queue.FirstOrDefault() != null)
            {
                var node = queue.Dequeue();
                output.Add(node);

                if(node.LeftChild != null)
                    queue.Enqueue(node.LeftChild);
                if(node.RightChild != null)
                    queue.Enqueue(node.RightChild);
            }

            return output;
        }


        public IVisualTree LeftTree { get { return this.LeftChild; } }
        public IVisualTree RightTree { get { return this.RightChild; } }

        public int DeepestLevel { get;set;}

        IVisualTree IVisualTree.Parent
        {
            get { return this.Parent; }
        }

   
    }

    interface ITreeLayoutStep<T>
    {
         void DoLayout(VisualTreeNode<T> tree,Size layoutArea);
    }

    abstract class TreeLayout<T> : TreeRenderer<VisualTreeNode<T>>, ITreeLayout 
    {
        public abstract string Name { get; }
        private System.Windows.Forms.Control mControl;
        
        public TreeLayout(System.Windows.Forms.Control control)
        {
            mControl = control;
        }

        protected override VisualTreeNode<T> RenderNode(Node root)
        {
            var visualTree = CreateVisualTree(root, 0);

            var deepestLevel = visualTree.AllChildren.Max(p => p.Depth);
            foreach (var node in visualTree.AllChildren)
                node.DeepestLevel = deepestLevel;

         //   new DebugRelabelNodes<T>().DoLayout(visualTree, mControl.Size);

            foreach (var step in this.GetLayoutSteps())
                step.DoLayout(visualTree, mControl.Size);

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
