using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using VNode = CToast.VisualTreeNode<CToast.TreePainterNodeData>;
using System.Collections;

namespace CToast
{
    //Adapted from https://code.google.com/p/tree-painter/

    class TreePainterNodeData
    {
        public ulong ID { get;  set; }        
        public bool IsBlank { get; set; }
        public int XPadding { get; set; }
        public int YPadding { get; set; }
    }


    class TreePainterLayout : TreeLayout<TreePainterNodeData>
    {
        public override string Name
        {
            get { return "TreePainter v3"; }
        }

        public TreePainterLayout(System.Windows.Forms.Control c)  :base(c)
        {
        }

        private Font mFont = new Font("Arial", 12, FontStyle.Bold);

        protected override VisualTreeNode<TreePainterNodeData> CreateNewNode(Node n, int depth)
        {
            var data = new TreePainterNodeData { ID = n.Id, IsBlank = n.TypedValue<string>("") == "<<IGNORE ME>>", XPadding = 20, YPadding = 20 };
            return new VisualTreeNode<TreePainterNodeData> { Data = data };
        }

        protected override IEnumerable<ITreeLayoutStep<TreePainterNodeData>> GetLayoutSteps()
        {
            yield return new TreePainterLayoutFirstPass { Layout = this };
            yield return new TreePainterLayoutSecondPass();
            yield return new CenterTree<TreePainterNodeData> { HorizontalOnly=true};
        }

        class TreePainterLayoutFirstPass : ITreeLayoutStep<TreePainterNodeData>
        {

            public TreePainterLayout Layout { get; set; }

            public void DoLayout(VNode root, Size layoutArea)
            {
             int xInitial = 0;
             int yInitial = 0;
             List<VNode> Nodes = new List<VNode>();

            if (root.Children.Count() == 0)
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
            int totalNodes = Layout.getAllChildNodes(root, list);

            // Obtaining the deepest node.
            foreach (VNode n in list)
                if (n.Depth > maxDepht)
                    maxDepht = n.Depth; // Maximum Depth

            // ArrayList to group nodes by level.
            listByLevel = new ArrayList[maxDepht + 1];

            // TreeNodes from originalTree are *copied* to tempTree (work TreeView)            
            var tempTree = root; // CopyTree(root);

            // Temporary adjusts will be made only on the tempTree (work TreeView)
            list = new ArrayList();
            totalNodes = Layout.getAllChildNodes(tempTree, list);

            // The 'branches' of tempTree will be checked; each branch should have the same 'depth'.
            foreach (VNode n in list)
                if (n.Children.Count() == 0 && n.Depth < maxDepht)
                    Layout.addBlankNode(n, maxDepht);  // Recursive function

            // Process work is made only on the tempTree (work TreeView)
            // At this point, each branch have the same 'depht'. Containing 'auxiliary' nodes.
            list = new ArrayList();
            list.Add(root);
            totalNodes = Layout.getAllChildNodes(tempTree, list);

            // Nodes are grouped by its Level.
            foreach (VNode n in list)
            {
                if (listByLevel[n.Depth] == null)
                    listByLevel[n.Depth] = new ArrayList();
                listByLevel[n.Depth].Add(n);
            }

            // Initial position for the bottom left node (last level, first node)
            x = xInitial;
            y = yInitial;

            // Assigning Location of the nodes of the second tree, in hierarchical order.
            for (int z = maxDepht; z >= 0; z--)
            {
                for (int index = 0; index < listByLevel[z].Count; index++)
                {
                    VNode visualNode = (VNode)(listByLevel[z][index]);
               
                    visualNode.Position = new Point(x, y);
                    //nodeToPaint.Tag = new Rectangle(labelAux.Left,
                    //                                labelAux.Top,
                    //                                labelAux.PreferredWidth,
                    //                                labelAux.PreferredHeight);

                    // If the current node is not in the last level, then, 
                    // its position should be calculated based on its child nodes.
                    if (z < maxDepht)
                    {

                        var firstChild = visualNode.Children.First();
                        var lastChild = visualNode.Children.Last();

                        Point posFirstChild = firstChild.Position;
                        Point posLastChild = lastChild.Position;

                        Point relocatedPoint = visualNode.Position;
                        relocatedPoint.X = (posFirstChild.X + posLastChild.X) / 2;                
                        visualNode.Position = relocatedPoint;                     
                    }

                    // Return Located Labels
                    Nodes.Add(visualNode);

                    // The next sibling node will be to the right of the current one.
                    // Where this node finishes plus Margin.

                    // Note: Label.Right != Label.Left + labelAux.PreferredWidth                    
                    x = visualNode.NodeBounds.Right + visualNode.Data.XPadding;
                    //     System.Console.WriteLine("Calculated X:" + x.ToString());
                }
                // The total nodes of the current level had been drawn.
                // The previous (superior) level should be located above the current level.
                y -= Nodes.Last().NodeBounds.Height + Nodes.Last().Data.YPadding;
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

            }


        class TreePainterLayoutSecondPass : ITreeLayoutStep<TreePainterNodeData>
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

   
        private VNode CopyTree(VNode node)
        {
            if(node == null)
                return null;

            return new VNode
            {
                Text = node.Text,
                Depth = node.Depth,
                Index = node.Index,
                LeftChild = CopyTree(node.LeftChild),
                RightChild = CopyTree(node.RightChild),
                Position = node.Position,
                Data = new TreePainterNodeData { ID = node.Data.ID, IsBlank = node.Data.IsBlank, XPadding = node.Data.XPadding, YPadding = node.Data.YPadding }
            };
        }

      

        private ulong mNextID = 0;
        /// <summary>
        /// Function to iteratively add 'auxiliar' nodes to the current <c>t</c> node.
        /// </summary>
        /// <param name="t">A <c>TreeNode</c> contained in a TreeView.</param>
        /// <param name="untilLevel">Maximum depth. If the <c>t</c> node is not in this level, 
        /// this function will add one 'auxiliar' node to it, as child.
        /// </param>
        private void addBlankNode(VNode t, int untilLevel)
        {

            if (t.Depth < untilLevel)
            {
                VNode tnBlank = new VNode { Text = "<<IGNORE ME>>", Data = new TreePainterNodeData { IsBlank = true, XPadding = 0, YPadding = 0 } };
                tnBlank.Data.ID = ++mNextID;
                tnBlank.Depth = t.Depth + 1;

                if (t.LeftChild == null)
                    t.LeftChild = tnBlank;
                else if (t.RightChild == null)
                    t.RightChild = tnBlank;

                if (tnBlank.Depth < untilLevel)
                    addBlankNode(tnBlank, untilLevel);
            }
            return;
        }
       
        private static ulong GetMaxID(VNode node)
        {
            ulong max = 0;

        
            foreach (VNode child in node.Children)
            {
                var nodeValue = child.Data.ID;
                if (nodeValue > max)
                    max = nodeValue;

                var childMax = GetMaxID(child);
                if (childMax > max)
                    max = childMax;
            }

            return max;
        }

        /// <summary>
        /// Function that obtains children for each node in the <c>TreeNodeCollection</c>.
        /// The full 'family' will be contained in the <c>ArrayList</c>.
        /// </summary>
        /// <param name="nodes"><c>TreeNodeCollection</c> to read.</param>
        /// <param name="array">Destination of data.</param>
        /// <returns>A integer with the counter of all nodes.</returns>
        private int getAllChildNodes(VNode node, ArrayList array)
        {
            int totalNodes = node.Children.Count();
            foreach (VNode child in node.Children)
            {
                array.Add(child);
                totalNodes += this.getAllChildNodes(child, array);
            }
            return totalNodes;
        }







    }
}
