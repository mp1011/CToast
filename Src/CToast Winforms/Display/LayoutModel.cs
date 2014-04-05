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

    interface ITreeLayout
    {
        IVisualTree LayoutTree(Node root);
        void SetRenderer(VisualTreeRenderer renderer);
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

        public Size NodeSize { get; set; }

        public Rectangle NodeBounds
        {
            get
            {
                return new Rectangle(this.X - (NodeSize.Width / 2), this.Y - (NodeSize.Height / 2), NodeSize.Width, NodeSize.Height);
            }
        }

        public Rectangle NodeBoundsWithPadding
        {
            get
            {
                int pad = 4;
                return new Rectangle(this.X - (NodeSize.Width / 2) - pad, this.Y - (NodeSize.Height / 2) - pad , NodeSize.Width + (pad*2), NodeSize.Height + (pad*2));
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
        public int TreeDepth
        {
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

            while (queue.FirstOrDefault() != null)
            {
                var node = queue.Dequeue();
                output.Add(node);

                if (node.LeftChild != null)
                    queue.Enqueue(node.LeftChild);
                if (node.RightChild != null)
                    queue.Enqueue(node.RightChild);
            }

            return output;
        }


        public IVisualTree LeftTree { get { return this.LeftChild; } }
        public IVisualTree RightTree { get { return this.RightChild; } }

        public int DeepestLevel { get; set; }

        IVisualTree IVisualTree.Parent
        {
            get { return this.Parent; }
        }


    }

    interface ITreeLayoutStep<T>
    {
        void DoLayout(VisualTreeNode<T> tree, Size layoutArea);
    }
}
