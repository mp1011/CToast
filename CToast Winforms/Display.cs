using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using GraphSharp;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using TreePainter_v3_1;

namespace CToast
{

    static class FormRenderHelper
    {

        public static void InvokeAction<T>(this System.Windows.Forms.Form form, Action<T> action, T arg)
        {
            if (form.InvokeRequired)            
                form.Invoke(action, arg);            
            else
                action(arg);
        }

        public static void InvokeAction<T1, T2>(this System.Windows.Forms.Form form, Action<T1, T2> action, T1 arg1, T2 arg2)
        {
            if (form.InvokeRequired)
                form.Invoke(action, arg1,arg2);
            else
                action(arg1,arg2);
        }


    }
  
    class GraphicsRenderer : TreeRenderer<Image>
    {
        private List<Tuple<Rectangle, string>> mToolTips = new List<Tuple<Rectangle, string>>();
        private bool mShowSelectors = false;

        private Font mFont = new Font("Arial", 12);
        private Brush mFontBrush = new SolidBrush(Color.Black);

        private Font mExpFont = new Font("Arial", 9);
        private Brush mExpBrush = new SolidBrush(Color.DarkGray);


        private Dictionary<string, Brush> mSpecialBrushes = new Dictionary<string, Brush>();
      
        private Pen mPen = new Pen(Color.LightBlue);
        private Pen mLinePen = new Pen(Color.DarkBlue, 1.5f);

        public GraphicsRenderer(bool showSel)
        {
            mShowSelectors = showSel;
            mSpecialBrushes.Add(typeof(ArgNode).Name, new SolidBrush(Color.LightGreen));
            mSpecialBrushes.Add(typeof(FunctionSelectorNode).Name, new SolidBrush(Color.LightBlue));
            mSpecialBrushes.Add(typeof(FunctionPatternNode).Name, new SolidBrush(Color.LightCoral));

        }

        protected override Image RenderNode(Node root)
        {
            return Render(root, Color.Transparent);
        }

        public Image Render(Node root,Color c)
        {
            if(root == null)
                return null;

         
            if (!mShowSelectors)
            {
                if (root is FunctionSelectorNode)
                    return this.Render(root.LeftNode,Color.Red);
                else if (root is FunctionPatternNode)
                    return this.Render(root.RightNode, Color.Red);
            }

            Image leftChild;
            Image rightChild;

            if (root.LeftNode != null)
                leftChild = Render(root.LeftNode);
            else
                leftChild = NullImage();


            if (root.RightNode != null)
                rightChild = Render(root.RightNode);
            else
                rightChild = NullImage();
     
            //guess final size            
            Bitmap image = new Bitmap(leftChild.Width + rightChild.Width, Math.Max(leftChild.Height, rightChild.Height) + 32);
            Graphics g = Graphics.FromImage(image);
            g.Clear(Color.White);

            SizeF labelSize = g.MeasureString(root.ToString(), mFont);
            Point labelPosition = new Point((int)((image.Width -labelSize.Width) / 2), 0);
            int topHeight = (int)labelSize.Height + 20;

            if (labelSize.Width > image.Width || topHeight + Math.Max(leftChild.Height, rightChild.Height) > image.Height)
            {
                image = new Bitmap(Math.Max(image.Width,(int)labelSize.Width+8), Math.Max(leftChild.Height, rightChild.Height) + topHeight);
                g = Graphics.FromImage(image);
                g.Clear(Color.White);

                labelPosition = new Point((int)((image.Width - labelSize.Width) / 2), 0);
                topHeight = (int)labelSize.Height + 20;
            }

            if (labelPosition.X < 0)
                labelPosition.X = 0;

            if (!c.IsEmpty)
            {
                var b = new SolidBrush(c);
                g.FillRectangle(b, new Rectangle(labelPosition.X, 0, (int)labelSize.Width, (int)labelSize.Height));
            }
            else
                DrawSpecial(root, g, new Rectangle(labelPosition.X, 0, (int)labelSize.Width, (int)labelSize.Height));

            g.DrawRectangle(mPen, new Rectangle(labelPosition.X, 0, (int)labelSize.Width, (int)labelSize.Height));
            g.DrawString(root.ToString(), mFont, mFontBrush, labelPosition);

            if(leftChild.Height > 5)
                g.DrawLine(mLinePen, new Point(labelPosition.X + (int)(labelSize.Width / 2), labelPosition.Y + (int)labelSize.Height), new Point(leftChild.Width / 2, topHeight));

            if (rightChild.Height > 5)
                g.DrawLine(mLinePen, new Point(labelPosition.X + (int)(labelSize.Width / 2), labelPosition.Y + (int)labelSize.Height), new Point(leftChild.Width + (rightChild.Width / 2), topHeight));


            g.DrawImage(leftChild, new Point(0,topHeight));
            g.DrawImage(rightChild, new Point(leftChild.Width,topHeight));
            g.Dispose();

            return image;
        }

        private void DrawSpecial(Node node, Graphics g, Rectangle area)
        {
            string typeName = node.GetType().Name;

            Brush b= mSpecialBrushes.TryGet(typeName, null);
            if(b != null)
                g.FillRectangle(b, area);          
                //mToolTips.Add(new Pair<Rectangle,string>(area,o.GetSourceExpression()));            
        }

        private Image NullImage()
        {
            var bmp = new Bitmap(20,2);
//            var g = Graphics.FromImage(bmp);
 //           g.Clear(Color.Blue);
  //          g.Dispose();
            return bmp;
        }

        public string GetToolTip(Point position)
        {
            return null;

            var i = mToolTips.FirstOrDefault(p => p.Item1.Contains(position));
            if (i == null)
                return null;
            return i.Item2;
        }
    }

    class XMLRenderer : TreeRenderer<string>
    {
        private int mTabDepth = 0;

        private string CurrentIndent
        {
            get
            {
                return "".PadRight(mTabDepth * 5, ' ');
            }
        }

        protected override string RenderNode(Node root)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}<node value='{1}'>", this.CurrentIndent, root.ToString()); sb.AppendLine();

            mTabDepth++;

            if (root.LeftNode != null)
                sb.Append(this.Render(root.LeftNode));

            if (root.RightNode != null)
                sb.Append(this.Render(root.RightNode));

            mTabDepth--;
            sb.AppendFormat("{0}</node>", this.CurrentIndent); sb.AppendLine();

            return sb.ToString();
        }
    }

    class TextRenderer : TreeRenderer<string>
    {
        private int mTabDepth = 0;

        private string CurrentIndent
        {
            get
            {
                return "".PadRight(mTabDepth * 5, ' ');
            }
        }

        protected override string RenderNode(Node root)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}{1}", this.CurrentIndent, root.ToString()); sb.AppendLine();

            mTabDepth++;

            if (root.LeftNode != null)
                sb.Append(this.Render(root.LeftNode));

            if (root.RightNode != null)
                sb.Append(this.Render(root.RightNode));

            mTabDepth--;
            return sb.ToString();
        }
    }

    class GraphSharpEdge : GraphSharp.TypedEdge<Node>
    {
        public GraphSharpEdge(Node parent, Node child) : base(parent, child, GraphSharp.EdgeTypes.Hierarchical) { }
    }

    class GraphSharpGraph : GraphSharp.HierarchicalGraph<Node, GraphSharpEdge>
    {
        public GraphSharpGraph() :base()
        {
        }

        public GraphSharpGraph(bool a)
            : base(a)
        {
        }

        public GraphSharpGraph(bool a, int i) : base(a, i) { }
    }

    class GraphSharpRenderer : TreeRenderer<GraphSharpViewModel>
    {
        protected override GraphSharpViewModel RenderNode(Node root)
        {
            return new GraphSharpViewModel(RenderGraph(root));
        }
      
        private GraphSharpGraph RenderGraph(Node root)
        {
            var graph = new GraphSharpGraph();

            graph.AddVertexRange(root.Traverse(Traversal.Preorder, true));
            graph.AddEdgeRange(root.Traverse(Traversal.Preorder,true).SelectMany(p => this.CreateEdges(p)));
            return graph;
        }

        private IEnumerable<GraphSharpEdge> CreateEdges(Node root)
        {               
                if (root.LeftNode != null)
                    yield return new GraphSharpEdge(root, root.LeftNode);
              
                if (root.RightNode != null)
                    yield return new GraphSharpEdge(root, root.RightNode);
                         
        }
    }

    class NodeGraphLayout : GraphSharp.Controls.GraphLayout<Node, GraphSharpEdge, GraphSharpGraph> { }

    class GraphSharpViewModel : INotifyPropertyChanged
    {
        #region Data

        private string layoutAlgorithmType;
        private GraphSharpGraph graph;
        private List<String> layoutAlgorithmTypes = new List<string>();
        private int count;
        #endregion

        #region Ctor
        public GraphSharpViewModel(GraphSharpGraph g)
        {
            graph = g;

            //layoutAlgorithmTypes.Add("BoundedFR");
            //layoutAlgorithmTypes.Add("Circular");
            //layoutAlgorithmTypes.Add("CompoundFDP");
            //layoutAlgorithmTypes.Add("EfficientSugiyama");
            //layoutAlgorithmTypes.Add("FR");
            //layoutAlgorithmTypes.Add("ISOM");
            //layoutAlgorithmTypes.Add("KK");
            //layoutAlgorithmTypes.Add("LinLog");
            //layoutAlgorithmTypes.Add("Tree");

            LayoutAlgorithmType = "Tree";
        }
        #endregion


        #region Public Properties     

        public string LayoutAlgorithmType
        {
            get { return layoutAlgorithmType; }
            set
            {
                layoutAlgorithmType = value;
                NotifyPropertyChanged("LayoutAlgorithmType");
            }
        }

        public GraphSharpGraph Graph
        {
            get { return graph; }
            set
            {
                graph = value;
                NotifyPropertyChanged("Graph");
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }

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

    class TreeViewRendererAlternate : TreeRenderer<TreeView>
    {
        public TreeViewRendererAlternate()
        {
        }
     
        protected override TreeView RenderNode(Node root)
        {
            var tree = new TreeView();
            AddNodes(tree.Nodes, root);
            return tree;
        }

        private void AddNodes(TreeNodeCollection treeNodes, Node node)
        {
            if (node == null)
                return;

            var op = node.TypedValue<Operator>(null);

            if (op is CommaOperator || op is OpenBracketOperator || op is ConcatOperator)
            {
                var nodes = treeNodes;
                foreach (var itemNode in ListHelper.GetNodeList(node))
                {
                    var t = AddNodesNormal(nodes, itemNode);
                    if (t == null)
                        break;
                    nodes = t.Nodes;
                }
            }
            else if (op is FunctionCallOperator)
            {
                var t = new TreeNode("@" + node.LeftNode.Value);
                t.Tag = node.LeftNode.Id;

                treeNodes.Add(t);
                treeNodes = t.Nodes;

                foreach (var argNode in ListHelper.GetNodeList(node.RightNode))
                {
                    AddNodes(treeNodes, argNode);
                }

            }
            else
                AddNodesNormal(treeNodes, node);
        }

        private TreeNode AddNodesNormal(TreeNodeCollection treeNodes, Node node)
        {
            if (node == null || node.Value == null)
                return null;

            var treeNode = new TreeNode(node.Value.ToString());
            treeNode.Tag = node.Id;

            treeNodes.Add(treeNode);

            AddNodes(treeNode.Nodes, node.LeftNode);
            AddNodes(treeNode.Nodes, node.RightNode);

            return treeNode;
        }
    }

    class ImageRenderer : TreeRenderer<BitmapWithOrigin>
    {
        protected override BitmapWithOrigin RenderNode(Node root)
        {
            TreeRenderer<TreeView> treeView = new TreeViewRendererAlternate();
            return TreePainter_v3_1.TreeRender.RenderSingleTree(treeView.Render(root));            
        }

    }

    class ColorTreeRenderer : TreeRenderer<BitmapWithOrigin>
    {
        protected override BitmapWithOrigin RenderNode(Node root)
        {
            TreeRenderer<TreeView> treeView = new TreeViewRendererAlternate();
            return TreePainter_v3_1.TreeRender.RenderSingleTreeAsColorTree(treeView.Render(root));            
        }
    }

    class SunburstRenderer : TreeRenderer<Bitmap>
    {
        private Control mControl;

        public SunburstRenderer(Control ctl) 
        {
            mControl = ctl;
        }

        private float mPenWidth = 5f;
        protected override Bitmap RenderNode(Node root)
        {

            var bmp = new Bitmap((int)(mControl.Width * .7), (int)(mControl.Height * .7));
            var center = new Point(bmp.Width / 2, bmp.Height / 2);
            var rec = new Rectangle(center.X - 10, center.Y - 10, 20, 20);
            var graphics = Graphics.FromImage(bmp);

            PaintNode(0, root, graphics, 1f, 0, true,0, 360, rec);

            return bmp;
        }

        private Rectangle GetNextRec(Rectangle rec)
        {
           return new Rectangle((int)(rec.X - mPenWidth), (int)(rec.Y - mPenWidth), (int)(rec.Width + (mPenWidth * 2)), (int)(rec.Height + (mPenWidth * 2)));    
        }

       
        private void PaintNode(int level, Node node, Graphics g, float firstPercent, int nodeIndex, bool isLeft, int parentAngleStart, int parentAngleSweep, Rectangle parentRec)
        {            
            var rec = GetNextRec(parentRec);

            int nodeStart, nodeSweep;

            if (nodeIndex == 0)
            {
                nodeStart = parentAngleStart;
                nodeSweep = (int)(parentAngleSweep * firstPercent);
            }
            else
            {
                nodeStart = parentAngleStart + ((int)(parentAngleSweep * firstPercent));
                nodeSweep = (int)(parentAngleSweep * (1-firstPercent));
            }

        
            Color c = Color.White;
            Color c2 = Color.Black; 

            var op = node.TypedValue<Operator>(null);

            if (node.IsAtomic)
            {
                c = Color.Gold;
            }
            else if (isLeft)
            {
                c = Color.Pink;
                c2 = Color.DarkRed;
            }
            else
            {
                c = Color.LightBlue;
                c2 = Color.DarkBlue;
            }

            float pct = .05f * level;
            while (pct > 1)
                pct -= 1f;

            c = Util.FadeColor(c, c2, pct);

            PaintArc(g, c, rec, nodeStart, nodeSweep);
     

            if (node.LeftNode != null && node.RightNode != null)
            {
                var count1 = node.LeftNode.TreeeSizeUnsafe;
                var count2 = node.RightNode.TreeeSizeUnsafe;

                var pct1 = (float)count1 / (float)(count1 + count2);

                float limit = .1f;
                if (pct < limit)
                    pct = limit;
                if (pct > 1 - limit)
                    pct = 1 - limit;
                
                PaintNode(level+1, node.LeftNode, g, pct1,0,true, nodeStart, nodeSweep, rec);
                PaintNode(level + 1, node.RightNode, g, pct1, 1,false, nodeStart, nodeSweep, rec);
            }
            else if (node.LeftNode != null)
            {
                PaintNode(level + 1, node.LeftNode, g, 1, 0,true, nodeStart, nodeSweep, rec);
            }
            else if (node.RightNode != null)
            {
                PaintNode(level + 1, node.RightNode, g, 1, 0, false, nodeStart, nodeSweep, rec);
            }

        }

        private void PaintArc(Graphics g, Color c, Rectangle rec, int start, int sweep)
        {
            var rec2 = new Rectangle((int)(rec.X - mPenWidth),(int)(rec.Y -mPenWidth), (int)(rec.Width + (mPenWidth *2)),(int)(rec.Height + (mPenWidth *2)));
    
            var path = new GraphicsPath();

            path.StartFigure();

            path.AddArc(rec, start, sweep);

            int start2 = start + sweep;
            while (start2 >= 360)
                start2 -= 360;

            path.AddArc(rec2, start2, -sweep);
            path.CloseFigure();
            g.FillPath(new SolidBrush(c), path);
            g.DrawPath(new Pen(Color.Black,1f), path);
        }
       
    }

    class RadialTreeRenderer : TreeRenderer<Bitmap>
    {
       private Control mControl;

       public RadialTreeRenderer(Control ctl) 
        {
            mControl = ctl;
        }


         protected override Bitmap RenderNode(Node root)
         {

             List<int> levelRadiuses = new List<int>();
             levelRadiuses.Add(30);
             levelRadiuses.Add(20);
             levelRadiuses.Add(10);
             levelRadiuses.Add(8);
             while(levelRadiuses.Count < root.DepthUnsafe)
                 levelRadiuses.Add(8);


             var bmp = new Bitmap(mControl.Width, mControl.Height);
             var g=  Graphics.FromImage(bmp);

             Point center = new Point(mControl.Width / 2, mControl.Height / 2);

             for (int i = root.DepthUnsafe; i > 0; i--)
             {
                 var r = levelRadiuses[0] + (levelRadiuses.Skip(1).Take(i).Sum() * 2);

                 var rec = new Rectangle(center.X - r, center.Y - r, (r * 2),(r * 2));
                 Color clr;
                 if ((i % 2) == 0)
                     clr = Util.FadeColor(Color.Red, Color.White, (float)i / (float)root.DepthUnsafe);
                 else
                     clr = Util.FadeColor(Color.Pink, Color.White, (float)i / (float)root.DepthUnsafe);

                 g.FillEllipse(new SolidBrush(clr), rec);
             }

             DrawNode(g, center,center, 0, levelRadiuses, 0, true, root);
             DrawChildNodes(g, center,center, 0, levelRadiuses, 0, 360, root,true);

             DrawNode(g, center,center, 0, levelRadiuses, 0, false, root);
             DrawChildNodes(g, center, center, 0, levelRadiuses, 0, 360, root, false);


             return bmp;
         }

         private void DrawChildNodes(Graphics g, Point center, Point thisNodeLocation, int level, List<int> levelRadiuses, int parentRangeStart, int parentRangeSweep, Node node, bool drawLine)
         {

             if (node.LeftNode != null && node.RightNode != null)
             {
                 var count1 = node.LeftNode.TreeeSizeUnsafe;
                 var count2 = node.RightNode.TreeeSizeUnsafe;

                 var pct1 = (float)count1 / (float)(count1 + count2);

                 var limit = .1f;
                 if (pct1 < limit)
                     pct1 = limit;
                 else if (pct1 > 1 - limit)
                     pct1 = 1 - limit;

                 int leftStart = parentRangeStart;
                 int leftSweep = (int)(parentRangeSweep * pct1);

                 int rightStart = leftStart + leftSweep;
                 int rightSweep = (int)(parentRangeSweep * (1 - pct1));

                 var leftLoc = DrawNode(g, center,thisNodeLocation, level + 1, levelRadiuses, leftStart + (leftSweep / 2), drawLine, node.LeftNode);
                 DrawChildNodes(g, center, leftLoc, level + 1, levelRadiuses, leftStart, leftSweep, node.LeftNode, drawLine);

                 var rightLoc = DrawNode(g, center, thisNodeLocation, level + 1, levelRadiuses, rightStart + (rightSweep / 2), drawLine, node.RightNode);
                 DrawChildNodes(g, center, rightLoc, level + 1, levelRadiuses, rightStart, rightSweep, node.RightNode, drawLine);             
             }
             else if (node.LeftNode != null)
             {
                 int leftStart = parentRangeStart;
                 int leftSweep =parentRangeSweep;

                 var leftLoc = DrawNode(g, center,thisNodeLocation, level + 1, levelRadiuses, leftStart + (leftSweep / 2), drawLine, node.LeftNode);
                 DrawChildNodes(g, center, leftLoc, level + 1, levelRadiuses, leftStart, leftSweep, node.LeftNode, drawLine);
             }
             else if (node.RightNode != null)
             {
                 int rightStart = parentRangeStart;
                 int rightSweep = parentRangeSweep;

                 var rightLoc = DrawNode(g, center, thisNodeLocation, level + 1, levelRadiuses, rightStart + (rightSweep / 2), drawLine, node.RightNode);
                 DrawChildNodes(g, center, rightLoc, level + 1, levelRadiuses, rightStart, rightSweep, node.RightNode, drawLine);
             }
         }

         private Point DrawNode(Graphics g, Point center, Point parentLocation, int level, List<int> levelRadiuses, int degree, bool drawLine, Node node)
         {

             while (degree >= 360)
                 degree -= 360;

             int distanceFromCenter = 0;
             if (level > 0)
                 distanceFromCenter = levelRadiuses[0] + (levelRadiuses.Skip(1).Take(level - 1).Sum() * 2) + levelRadiuses[level];

             var radius = levelRadiuses[level];

             Point location = center;
             Point offset = Util.AngleToXY(degree, distanceFromCenter);
             var textRec = new Rectangle((location.X + offset.X) - radius, (location.Y + offset.Y) - radius, radius * 2, radius * 2);

             var brush = new SolidBrush(Color.DarkBlue);
              var font = new Font("Arial", 9f, FontStyle.Regular);
             var format = StringFormat.GenericTypographic;
             format.Alignment = StringAlignment.Center;
             format.LineAlignment = StringAlignment.Center;
             format.FormatFlags = StringFormatFlags.NoWrap;
             
             var thisCenter = new Point(textRec.X + (textRec.Width / 2), textRec.Y + (textRec.Height / 2));

             if (drawLine)
             {
                 g.DrawLine(new Pen(Color.Black, 2f), thisCenter, parentLocation);
             }
             else
             {
                 var color = Util.FadeColor(Color.LightBlue, Color.DarkBlue, (float)level / (float)levelRadiuses.Count);
                 g.FillEllipse(new SolidBrush(color), textRec);

                 color = Util.FadeColor(Color.LightGreen, Color.DarkGreen,  (float)level / (float)levelRadiuses.Count);
                 g.DrawEllipse(new Pen(color,2f), textRec);
                 g.DrawEllipse(new Pen(Color.Black,1f), textRec);

                 if(node.Value != null)
                     g.DrawString(node.Value.ToString(), font, new SolidBrush(Color.White), textRec, format);
                
             }
             return thisCenter;
     
         }


           

    }


    class TriangleTreeRenderer : TreeRenderer<Bitmap>
    {

        public TriangleTreeRenderer() { }


        class RelativePoint
        {
            public RelativePoint Parent { get; set; }

            private int mAngle;
            public int Angle
            {
                get
                {
                    return mAngle;
                }
                set
                {
                    int newAngle = value;
                    while (newAngle < 0)
                        newAngle += 360;
                    while (newAngle >= 360)
                        newAngle -= 360;

                    mAngle = newAngle;
                }
            }
            public int Distance { get; set; }

            public Point Location
            {
                get; private set;
            }

            public void Move(int dx, int dy)
            {
                this.Location = new Point(this.Location.X + dx, this.Location.Y + dy);
            }

            public void CalcLocation()
            {
                if (Parent == null)
                    return;

                var offset = Util.AngleToXY(Angle, Distance);
                this.Location = new Point(Parent.Location.X + (offset.X), Parent.Location.Y + (offset.Y));                
            }
        }

        class TriangleGroup
        {
            public RelativePoint Top { get; set; }
            public Node Node { get; set; }

            public TriangleGroup LeftChild { get; set; }
            public TriangleGroup RightChild { get; set; }

            private RelativePoint mBottomLeft;
            public RelativePoint BottomLeft
            {
                get
                {
                    if (LeftChild == null)
                    {
                        if (mBottomLeft == null)
                        {
                            mBottomLeft = new RelativePoint { Parent = this.Top, Distance = 4, Angle = 240 };
                        }
                        return mBottomLeft;
                    }
                    else
                        return LeftChild.BottomLeft;
                }
            }

            public Point TextPoint
            {
                get
                {
                    var pt = this.Top.Location;
                    var angle = this.LeftAngle + (this.RightAngle - this.LeftAngle) / 2;

                    while (angle > 360)
                        angle -= 360;

                    var offset = Util.AngleToXY(angle, 10);

                    return new Point(pt.X + offset.X, pt.Y + offset.Y);
                }
            }

            private RelativePoint mBottomRight;
            public RelativePoint BottomRight
            {
                get
                {
                    if (RightChild == null)
                    {
                        if (mBottomRight == null)
                        {
                            mBottomRight = new RelativePoint { Parent = this.Top, Distance = 4, Angle = 300 };
                        }
                        return mBottomRight;
                    }
                    else
                        return RightChild.BottomRight;
                }
            }

            public void CalcLocation()
            {
                Top.CalcLocation();
                if (LeftChild != null)
                    LeftChild.CalcLocation();
                else
                    BottomLeft.CalcLocation();

                if (RightChild != null)
                    RightChild.CalcLocation();
                else
                    BottomRight.CalcLocation();
            }

            public void Move(int dx, int dy)
            {
                this.Top.Move(dx, dy);

                this.CalcLocation();
            }

            private IEnumerable<RelativePoint> GetAllNodes()
            {
                yield return this.Top;
                if (this.LeftChild != null)
                {
                    foreach (var node in LeftChild.GetAllNodes())
                        yield return node;
                }
                else
                    yield return this.BottomLeft;

                if (this.RightChild != null)
                {
                    foreach (var node in RightChild.GetAllNodes())
                        yield return node;
                }
                else
                    yield return this.BottomRight;
            }

            public void Rotate(int angle, bool rotateHead)
            {
                if(rotateHead)
                    this.Top.Angle += angle;

                if (this.LeftChild != null)
                    this.LeftChild.Rotate(angle,true);
                else
                    this.BottomLeft.Angle += angle;


                if (this.RightChild != null)
                    this.RightChild.Rotate(angle,true);
                else
                    this.BottomRight.Angle += angle;

            }

            public Rectangle Bounds
            {
                get
                {
                    var nodes = this.GetAllNodes().ToArray();

                    var minX = nodes.Min(p => p.Location.X);
                    var maxX = nodes.Max(p => p.Location.X);
                    var minY = nodes.Min(p => p.Location.Y);
                    var maxY = nodes.Max(p => p.Location.Y);

                    return new Rectangle(minX, minY, maxX - minX, maxY - minY);

                    //var bottomLeft = new Point(this.BottomLeftNode.Area.Left, this.BottomLeftNode.Area.Bottom);
                    //var bottomRight = new Point(this.BottomRightNode.Area.Right, this.BottomRightNode.Area.Bottom);
                    //var topY = this.Root.Area.Y;

                    //return new Rectangle(bottomLeft.X, topY, (bottomRight.X - bottomLeft.X), Math.Max(bottomLeft.Y,bottomRight.Y) - topY);
                }
            }


            public int LeftAngle
            {
                get
                {
                    return Util.GetLineAngle(Top.Location, BottomLeft.Location);
                }
            }

            public int RightAngle
            {
                get
                {
                    return Util.GetLineAngle(Top.Location, BottomRight.Location);
                }
            }


        }

        protected override Bitmap RenderNode(Node root)
        {

            int depth = root.DepthUnsafe;
            var rootTriangle = NodeToTriangle(root, 1, depth);

            FinalizePosition(rootTriangle);

            var b = rootTriangle.Bounds;
            rootTriangle.Move(100, 100);

            if (b.Width <= 0 || b.Height <= 0)
            {
                var blank = new Bitmap(8, 8);
                return blank;
            }

            var bmp = new Bitmap(rootTriangle.Bounds.Right + 50, rootTriangle.Bounds.Bottom + 50);
            var g = Graphics.FromImage(bmp);

            DrawTriangles(rootTriangle, g,false,0);
          //  DrawLines(rootTriangle, g);
          //  DrawText(rootTriangle, g, 0);

            g.Dispose();

            return bmp;
        }

        private Font mFont = new Font("Arial", 8f, FontStyle.Regular);

        private void FinalizePosition(TriangleGroup triangle)
        {
            if (triangle.LeftChild != null)
                FinalizePosition(triangle.LeftChild);

            if (triangle.RightChild != null)
                FinalizePosition(triangle.RightChild);

            if (triangle.LeftChild != null && triangle.RightChild != null)
            {
                var angle = triangle.Top.Angle;

                triangle.LeftChild.Rotate(angle - triangle.LeftChild.RightAngle, false);
                triangle.RightChild.Rotate(angle - triangle.RightChild.LeftAngle, false);
            }

            triangle.CalcLocation();
          
        }

        private TriangleGroup NodeToTriangle(Node n, int depth, int maxDepth)
        {

            var triangle = new TriangleGroup { Node = n };

         //   var textSize = System.Windows.Forms.TextRenderer.MeasureText(n.Value.ToString(), mFont, new Size(1000, 50), TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);

        //    triangle.Root = new VisualNode { Node = n, Radius = (int)(textSize.Width * .5f) };
            triangle.Top = new RelativePoint { Angle = 270, Distance = 20 };

            TriangleGroup leftTriangle = null, rightTriangle = null;
      
            if (n.LeftNode != null)
            {
                leftTriangle = NodeToTriangle(n.LeftNode, depth+1,maxDepth);
                leftTriangle.Top.Parent = triangle.Top;
            }
            else if (depth < maxDepth)
            {
                leftTriangle = PadTree(depth + 1, maxDepth);
                leftTriangle.Top.Parent = triangle.Top;
            }

            if (n.RightNode != null)
            {
                rightTriangle = NodeToTriangle(n.RightNode,depth + 1, maxDepth);
                rightTriangle.Top.Parent = triangle.Top;
            }
            else if (depth < maxDepth)
            {
                rightTriangle = PadTree(depth + 1, maxDepth);
                rightTriangle.Top.Parent = triangle.Top;
            }


            triangle.LeftChild = leftTriangle;
            triangle.RightChild = rightTriangle;

            triangle.CalcLocation();

            return triangle;
        }

        private TriangleGroup PadTree(int depth, int maxDepth)
        {
            var triangle = new TriangleGroup();
            triangle.Top = new RelativePoint { Angle = 270, Distance = 20 };

            TriangleGroup leftTriangle = null, rightTriangle = null;

            if (depth < maxDepth)
            {
                leftTriangle = PadTree(depth + 1, maxDepth);
                leftTriangle.Top.Parent = triangle.Top;
            }

            if (depth < maxDepth)
            {
                rightTriangle = PadTree(depth + 1, maxDepth);
                rightTriangle.Top.Parent = triangle.Top;
            }

            triangle.LeftChild = leftTriangle;
            triangle.RightChild = rightTriangle;

            triangle.CalcLocation();

            if(leftTriangle != null)
                leftTriangle.Rotate(270 - leftTriangle.RightAngle, false);

            if(rightTriangle != null)
                rightTriangle.Rotate(270 - rightTriangle.LeftAngle, false);

            return triangle;
        }

        private void DrawLines(TriangleGroup triangle, Graphics g)
        {

            var pen = new Pen(Color.Black);

            if (triangle.LeftChild != null && triangle.LeftChild.Node != null)
            {
                g.DrawLine(pen, triangle.Top.Location, triangle.LeftChild.Top.Location);
                g.DrawLine(pen, triangle.LeftChild.Top.Location, triangle.LeftChild.TextPoint);
                DrawLines(triangle.LeftChild, g);
            }

            if (triangle.RightChild != null && triangle.RightChild.Node != null)
            {
                g.DrawLine(pen, triangle.Top.Location, triangle.RightChild.Top.Location);
                g.DrawLine(pen, triangle.RightChild.Top.Location, triangle.RightChild.TextPoint);

                DrawLines(triangle.RightChild, g);
            }

        }

        private void DrawText(TriangleGroup triangle, Graphics g, int level)
        {
            if (triangle == null)
                return;

            var pen = new Pen(Color.Black);
            var textBrush = new SolidBrush(Color.DarkBlue);

            if (triangle.Node != null && triangle.Node.Value != null)
            {
                if (triangle.Node.IsAtomic)
                {
                    StringFormat format = StringFormat.GenericTypographic;
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
                  //  g.DrawString(triangle.Node.Value.ToString(), this.mFont, textBrush, triangle.Top.Location, format);
                }
            }

            DrawText(triangle.LeftChild, g, level + 1);
            DrawText(triangle.RightChild, g, level + 1);
        }

        public static int DebugValue = -1;
        private void DrawTriangles(TriangleGroup triangle, Graphics g, bool isLeft, int level)
        {
            if (triangle == null)
                return;

            var pen = new Pen(Color.Black);
            var textBrush = new SolidBrush(Color.Blue);
          
            if (triangle.Node == null)
            {
              //  var brush = new SolidBrush(Util.FadeColor(Color.LightBlue, Color.DarkBlue, (level / 10f)));
               // g.FillPolygon(brush, new Point[] { triangle.Top.Location, triangle.BottomLeft.Location, triangle.BottomRight.Location });
               //   g.DrawPolygon(pen, new Point[] { triangle.Top.Location, triangle.BottomLeft.Location, triangle.BottomRight.Location });
            }
            else if (false)
            {
                Color[] c1 = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Orange, Color.Purple, Color.ForestGreen,Color.Turquoise };
                while (level >= c1.Length)
                    level -= c1.Length;
                var clr = c1[level];

                if (DebugValue < 0 || DebugValue == level)
                {
                    var brush = new SolidBrush(Util.FadeColor(clr, clr, (level / 10f)));
                    g.FillPolygon(brush, new Point[] { triangle.Top.Location, triangle.BottomLeft.Location, triangle.BottomRight.Location });
                }
                g.DrawPolygon(pen, new Point[] { triangle.Top.Location, triangle.BottomLeft.Location, triangle.BottomRight.Location });

            }
            else if (isLeft)
            {
                var brush = new SolidBrush(Util.FadeColor(Color.Gold, Color.DarkRed, (level / 10f)));
                g.FillPolygon(brush, new Point[] { triangle.Top.Location, triangle.BottomLeft.Location, triangle.BottomRight.Location });
                g.DrawPolygon(pen, new Point[] { triangle.Top.Location, triangle.BottomLeft.Location, triangle.BottomRight.Location });
            }
            else
            {
                var brush = new SolidBrush(Util.FadeColor(Color.LightGreen, Color.DarkGreen, (level / 10f)));
                g.FillPolygon(brush, new Point[] { triangle.Top.Location, triangle.BottomLeft.Location, triangle.BottomRight.Location });
                g.DrawPolygon(pen, new Point[] { triangle.Top.Location, triangle.BottomLeft.Location, triangle.BottomRight.Location });
            }


            DrawTriangles(triangle.LeftChild, g,true, level+1);
            DrawTriangles(triangle.RightChild, g,false, level + 1);
        }

    }
}


