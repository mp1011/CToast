﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections;

namespace TreePainter_v3_1
{
    class PaintSettings
    {
        public SolidBrush GetSolidBrush(Color color)
        {
            //todo, cache brushes
            return new SolidBrush(color);
        }

        public Pen GetPen(Color color, float width)
        {
            //todo, cache pens
            return new Pen(color, width);
        }
    }

    abstract class VisualTreeNode
    {
        public ulong ID { get; private set; }
        public Rectangle Area { get; protected set; }

        public Point Location
        {
            get { return Area.Location; }
            set
            {
                Area = new Rectangle(value, Area.Size);
            }
        }
        public virtual bool IsBlank { get { return false; } }

        protected VisualTreeNode(TreeNode source)
        {
            this.ID = (ulong)source.Tag;
        }

        public abstract int XPadding { get; }
        public abstract int YPadding { get; }

        public abstract void Paint(Graphics graphics, PaintSettings paintSettings);
    }

    class TextVisualTreeNode : VisualTreeNode
    {
        private Color enabledNodeBackColor = Color.Azure;
        private Color enabledNodeForeColor = Color.Blue;
        private Color disabledNodeBackColor = Color.LightGray;
        private Color disabledNodeForeColor = Color.Snow;

        public string Text { get; set; }
        public Color BackColor { get; set; }
        public Color TextColor { get; set; }
        public Font Font { get; set; }
        public override bool IsBlank { get { return this.Text == "<<IGNORE ME>>"; } }

        public override int XPadding { get { return 50; } }
        public override int YPadding { get { return 50; } }

        public TextVisualTreeNode(TreeNode source, Font nodeFont) : base(source)
        {
            //labelAux.Name = nodeToPaint.Name;
            this.Font = nodeFont;
            this.Text = source.Text;

            // Drawing Style
            if (this.IsBlank) 
            {   // Current node is auxiliar.
                this.BackColor = this.disabledNodeBackColor;
                this.TextColor = this.disabledNodeForeColor;
            }
            else
            {   // Current node contains valid data.
                this.BackColor = this.enabledNodeBackColor;
                this.TextColor = this.enabledNodeForeColor;
            }

            // Calculating Node Position.
            this.Area = new Rectangle(0, 0, 100, 50);
        }

        public override void Paint(Graphics graphics, PaintSettings paintSettings)
        {
            StringFormat format = StringFormat.GenericTypographic;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Near;

            graphics.DrawString(this.Text, this.Font, paintSettings.GetSolidBrush(Color.Black), this.Area, format);
        }
    }

    class CircleVisualTreeNode : VisualTreeNode
    {
        public Color OutlineColor { get; set; }
        public Color FillColor { get; set; }
        public int Radius { get; set; }

        private bool mBlank;
        public override bool IsBlank
        {
            get
            {
                return mBlank;
            }
        }

        public override int XPadding { get { return 24; } }
        public override int YPadding { get { return Radius * 4; } }

        public CircleVisualTreeNode(TreeNode source)
            : base(source)
        {

            mBlank = source.Text == "<<IGNORE ME>>";
            OutlineColor = Color.Gray;

            Radius = 4;
            this.Area = new Rectangle(0, 0, Radius * 2, Radius * 2);

            if(mBlank)
                return;

            Int64 intValue;
            if (Int64.TryParse(source.Text, out intValue))
                FillColor = Color.Green;
            else if (source.Text.StartsWith("@", StringComparison.InvariantCulture))
                FillColor = Color.Blue;
            else if ("+-/*".IndexOf(source.Text) >= 0)
                FillColor = Color.Yellow;
            else if (",&".IndexOf(source.Text) >= 0)
                FillColor = Color.Orange;
            else
                FillColor = Color.Red;

           
        }

        public override void Paint(Graphics graphics, PaintSettings paintSettings)
        {
            graphics.FillEllipse(paintSettings.GetSolidBrush(FillColor), this.Area);
            graphics.DrawEllipse(paintSettings.GetPen(OutlineColor,1.0f), this.Area);
        }
    }


    class VisualTreeLine
    {
        public VisualTreeNode Parent { get; set; }
        public VisualTreeNode Child { get; set; }      
    }

    class VisualTree
    {
        public List<VisualTreeNode> Nodes { get; private set; }
        public List<VisualTreeLine> Lines { get; private set; }

        private Rectangle mBounds;
        public Rectangle Bounds
        {
            get
            {
                if (mBounds.IsEmpty)
                {
                    int minLeft = Nodes.Min(p => p.Area.Left);
                    int maxRight = Nodes.Max(p => p.Area.Right);
                    int minTop = Nodes.Min(p => p.Area.Top);
                    int maxBottom = Nodes.Max(p => p.Area.Bottom);

                    mBounds = new Rectangle(minLeft, minTop, maxRight - minLeft, maxBottom - minTop);
                }

                return mBounds;
            }
        }

        public Rectangle RecalculateBounds()
        {
            mBounds = Rectangle.Empty;
            return this.Bounds;
        }

        public VisualTreeNode Root { get; set; }


        public Point AlignInArea(Size areaSize)
        {
            Point rootDestination = new Point((areaSize.Width / 2) - (this.Root.Area.Width / 2), 0);

            var delta = new Point(this.Root.Area.Left - rootDestination.X, this.Root.Area.Top - rootDestination.Y);

            foreach (var node in this.Nodes)
            {
                node.Location = new Point(node.Area.Left - delta.X, node.Area.Top - delta.Y);
            }

            return delta;
        }

        public void AlignLeft()
        {
            var delta = new Point(this.Bounds.Left, this.Bounds.Top);
            foreach (var node in this.Nodes)
                node.Location = new Point(node.Area.Left - delta.X, node.Area.Top - delta.Y);
        }

        private Font mNodeFont;
        private int xInitial = 200;
        private int yInitial = 400;         


        public static VisualTree Create<T>(TreeView tree, Func<TreeNode,T> transform) where T:VisualTreeNode
        {
            var vt = new VisualTree();
            vt.Nodes = new List<VisualTreeNode>();
            vt.Lines = new List<VisualTreeLine>();
            vt.mNodeFont = new Font("Arial", 12, FontStyle.Bold);

            vt.mNextID = GetMaxID(tree.Nodes) + 1;
            vt.Fill(tree, transform);

            vt.Root = vt.Nodes.FirstOrDefault(p => p.ID == (ulong)tree.Nodes[0].Tag);

            return vt;
        }

        private void Fill<TVisualNodeType>(TreeView originalTree, Func<TreeNode,TVisualNodeType> transform) where TVisualNodeType : VisualTreeNode 
        {
            var tempTree = new TreeView();

            if (originalTree.Nodes.Count == 0)
                return;

            #region drawing variables

            int x = 20;
            int y = 20;

            int maxDepht = 0;

            ArrayList list;
            ArrayList[] listByLevel;

               #endregion

            /*
             * 1. Obtain Nodes per each Level.
             *    'x' position will identify the 'index' of the node in each level.
             *    'y' position will identify the 'level'.
             * 2. Draw nodes, starting from the left (first node) of the deepest level.
             *    The posterior nodes (from the same level) should have an 'x' position 
             *    equal to the current 'x' node position plus the node width.
             * 3. When each 'level' is fully drawn (printed), or located, then, 
             *    the superior level should continue. ('y' is decreased).
             * 4. The 'father' should know which is its own center, based on 
             *    the 'x' position of the 'first' and 'last' child.
             * 5. Each node should know its 'own' scope in 'x' and 'y' positions, 
             *    based on its children. In other words, each time that a child
             *    is drawn (or printed), then, the parent should be 'updated' in
             *    its 'scope'.
             *    The 'Tag' property of each TreeNode can be used to store this info.
             * A. Idea: Auxiliar (temporary) Nodes can be added to (fill) those 
             *    TreeNodes that do not contain 'offspring' (any child). In this way, 
             *    the drawing can 'start' with the 'deepest' level.
             *    Obviously, this 'temporary' Nodes will have a 'Tag' value 
             *    indicating that the node is 'Non-printable'.
             * 
             */

            list = new ArrayList();
            int totalNodes = getAllChildNodes(originalTree.Nodes, list);

            // Obtaining the deepest node.
            foreach (TreeNode n in list)
                if (n.Level > maxDepht)
                    maxDepht = n.Level; // Maximum Depth

            // ArrayList to group nodes by level.
            listByLevel = new ArrayList[maxDepht + 1];

            // TreeNodes from originalTree are *copied* to tempTree (work TreeView)            
            foreach (TreeNode n in list)
                if (n.Level == 0)
                    tempTree.Nodes.Add((TreeNode)n.Clone());

            // Temporary adjusts will be made only on the tempTree (work TreeView)
            list = new ArrayList();
            totalNodes = getAllChildNodes(tempTree.Nodes, list);

            // The 'branches' of tempTree will be checked; each branch should have the same 'depth'.
            foreach (TreeNode n in list)
                if (n.Nodes.Count == 0 && n.Level < maxDepht)
                    addBlankNode(n, maxDepht);  // Recursive function

            // Process work is made only on the tempTree (work TreeView)
            // At this point, each branch have the same 'depht'. Containing 'auxiliary' nodes.
            list = new ArrayList();
            totalNodes = getAllChildNodes(tempTree.Nodes, list);

            // Nodes are grouped by its Level.
            foreach (TreeNode n in list)
            {
                if (listByLevel[n.Level] == null)
                    listByLevel[n.Level] = new ArrayList();
                listByLevel[n.Level].Add(n);
            }

            // Initial position for the bottom left node (last level, first node)
            x = xInitial;
            y = yInitial;

            // Assigning Location of the nodes of the second tree, in hierarchical order.
            for (int z = maxDepht; z >= 0; z--)
            {
                for (int index = 0; index < listByLevel[z].Count; index++)
                {
                    TreeNode nodeToPaint = (TreeNode)(listByLevel[z][index]);
                    bool isBlank = nodeToPaint.Text == "<<IGNORE ME>>";

                    var visualNode = transform(nodeToPaint);
                    visualNode.Location = new Point(x, y);
                    //nodeToPaint.Tag = new Rectangle(labelAux.Left,
                    //                                labelAux.Top,
                    //                                labelAux.PreferredWidth,
                    //                                labelAux.PreferredHeight);

                    // If the current node is not in the last level, then, 
                    // its position should be calculated based on its child nodes.
                    if (z < maxDepht)
                    {
                        var firstChild = this.Nodes.FirstOrDefault(p => p.ID == (ulong)nodeToPaint.FirstNode.Tag);
                        var lastChild = this.Nodes.FirstOrDefault(p => p.ID == (ulong)nodeToPaint.LastNode.Tag);

                        Point posFirstChild = getRectangleCenter(firstChild.Area);
                        Point posLastChild = getRectangleCenter(lastChild.Area);

                        Point relocatedPoint = visualNode.Area.Location;
                  
                        //relocatedPoint.X = (posFirstChild.X + posLastChild.X) / 2 - labelAux.PreferredWidth / 2;
                        relocatedPoint.X = (posFirstChild.X + posLastChild.X) / 2 - visualNode.Area.Width / 2;

                        //System.Console.WriteLine(nodeToPaint.Text + " x= " + relocatedPoint.X
                        //    + "\n  ->1: " + nodeToPaint.FirstNode.Text + " (" + posFirstChild.X + ");"
                        //    + "\n  ->2: " + nodeToPaint.LastNode.Text + " (" + posLastChild.X + ");");

                        visualNode.Location = relocatedPoint;

                         //nodeToPaint.Tag = new Rectangle(labelAux.Left,
                         //       labelAux.Top,
                         //       labelAux.PreferredWidth,
                         //       labelAux.PreferredHeight);
                    }

                    // Union Lines
                    foreach (TreeNode t in nodeToPaint.Nodes)
                    {
                        if (t.Text == "<<IGNORE ME>>")
                            continue;

                        var child = this.Nodes.FirstOrDefault(p => p.ID == (ulong)t.Tag);
                        this.Lines.Add(new VisualTreeLine() { Parent = visualNode, Child = child });

                    }

                    // Return Located Labels
                    this.Nodes.Add(visualNode);

                    // The next sibling node will be to the right of the current one.
                    // Where this node finishes plus Margin.

                    // Note: Label.Right != Label.Left + labelAux.PreferredWidth
                    x = visualNode.Area.Left + visualNode.Area.Width + visualNode.XPadding;
               //     System.Console.WriteLine("Calculated X:" + x.ToString());
                }
                // The total nodes of the current level had been drawn.
                // The previous (superior) level should be located above the current level.
                y -= this.Nodes.Last().YPadding;

            }

            // Drawing Root
            //Point rootPos = new Point();
            //Point posFirstRootChild = new Point();
            //Point posLastRootChild = new Point();
            //posFirstRootChild = (Point)((TreeNode)(listByLevel[0][0])).FirstNode.Tag;
            //posLastRootChild = (Point)((TreeNode)(listByLevel[0][listByLevel[0].Count - 1])).LastNode.Tag;
            //rootPos.X = (posFirstRootChild.X + posLastRootChild.X) / 2;
            //rootPos.Y = y;
            //board.DrawString("Root", drawFont, drawBrush, rootPos.X, rootPos.Y);

            //// Drawing Root Lines To First Level Nodes
            //TreeNode[] tArr = (TreeNode[])listByLevel[0].ToArray(typeof(TreeNode));
            //foreach (TreeNode t in tArr)
            //{
            //    Point pChild = (Point)t.Tag;

            //    pChild.X += nodeWidth / 2;
            //    pChild.Y -= nodeHeight / 2 - 5;

            //    board.DrawLine(p, rootPos, pChild);
            //}

            //Last node (located at the bottom right position) of the last level.
            //TreeNode rightNode = (TreeNode)(listByLevel[maxDepht][listByLevel[maxDepht].Count-1]);
            //drawingPanel.AutoScrollMinSize = new Size(((Rectangle)(rightNode.Tag)).Right, 600);

        }

        /// <summary>
        /// Function that obtains children for each node in the <c>TreeNodeCollection</c>.
        /// The full 'family' will be contained in the <c>ArrayList</c>.
        /// </summary>
        /// <param name="nodes"><c>TreeNodeCollection</c> to read.</param>
        /// <param name="array">Destination of data.</param>
        /// <returns>A integer with the counter of all nodes.</returns>
        private int getAllChildNodes(TreeNodeCollection nodes, ArrayList array)
        {
            int totalNodes = nodes.Count;
            foreach (TreeNode node in nodes)
            {
                array.Add(node);
                totalNodes += this.getAllChildNodes(node.Nodes, array);
            }
            return totalNodes;
        }


        private ulong mNextID = 0;

        private static ulong GetMaxID(TreeNodeCollection nodes)
        {
            ulong max = 0;
            foreach (TreeNode node in nodes)
            {
                var nodeValue = (ulong)node.Tag;
                if (nodeValue > max)
                    max = nodeValue;

                var childMax = GetMaxID(node.Nodes);
                if (childMax > max)
                    max = childMax;
            }

            return max;
        }

        /// <summary>
        /// Function to iteratively add 'auxiliar' nodes to the current <c>t</c> node.
        /// </summary>
        /// <param name="t">A <c>TreeNode</c> contained in a TreeView.</param>
        /// <param name="untilLevel">Maximum depth. If the <c>t</c> node is not in this level, 
        /// this function will add one 'auxiliar' node to it, as child.
        /// </param>
        private void addBlankNode(TreeNode t, int untilLevel)
        {
            
            if (t.Level < untilLevel)
            {
                TreeNode tnBlank = new TreeNode("<<IGNORE ME>>"); //txbBlankValue.Text);
                tnBlank.Tag = ++mNextID;
                t.Nodes.Add(tnBlank);
                tnBlank.BackColor = Color.DodgerBlue;

                if (tnBlank.Level < untilLevel)
                    addBlankNode(tnBlank, untilLevel);
            }
            return;
        }

        private Point getRectangleCenter(Rectangle r)
        {
            return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        }

    }

}