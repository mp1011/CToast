/*
 * 
 *  TreePainter. Draws all the nodes of a TreeView in a Panel (with Labels).
 *  Copyright (C) 2010 Gustavo Antonio Parada Sarmiento gaps96@gmail.com
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 */ 

using System;
using System.Collections.Generic;
using System.Collections;           // For ArrayList
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;     // For Painting
using System.Text;
using System.Windows.Forms;

using System.Xml;					// For SVG
using System.IO;					// For SVG XML

/// <summary>
/// 
/// </summary>
/**
 * Notes about the version 3 are identified by the text: "Version 3 Feature"
 */
namespace TreePainter_v3_1
{
    public partial class Form1 : Form
    {      
        private bool scrolledAction = false;

        #region treePainter_vars
        private Color enabledNodeBackColor;
        private Color enabledNodeForeColor;
        private Color disabledNodeBackColor;
        private Color disabledNodeForeColor;
        private int nodeFontSize;
        private Font drawNodeFont;
        private Pen unionNodeLinesPen;
        #endregion

        public Form1()
        {
            InitializeComponent();
            
            enabledNodeBackColor = Color.Azure;
            enabledNodeForeColor = Color.Blue;
            disabledNodeBackColor = Color.LightGray;
            disabledNodeForeColor = Color.Snow;
            nodeFontSize = 8;
            drawNodeFont = new Font("Arial", nodeFontSize, FontStyle.Bold);
        }

        // pnlDrawBoard_Paint:
        // V3. Changed from: private void pnlDrawBoard_Paint(object sender, PaintEventArgs e)
        // V3. Add each (previously located) label to the panel.
        private void pnlDrawBoard_Paint()
        {
            Color initialColor = Color.Azure;
            Color finalColor = Color.RoyalBlue;

            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            // For a Nice Background.
            LinearGradientBrush brush =
                new LinearGradientBrush(rect, initialColor, finalColor,
                                        LinearGradientMode.ForwardDiagonal);
            pnlDrawBoard.CreateGraphics().FillRectangle(brush, rect);

            ArrayList labels = new ArrayList();

            if (!scrolledAction && chbDraw.Checked)
                locateNodes(pnlDrawBoard, trvBaseTree, trvTempTree, out labels);
            else
                return;
                        
            pnlDrawBoard.AutoScroll = true;
            if (!scrolledAction)
            for (int i = 0; i < labels.Count; i++)
                pnlDrawBoard.Controls.Add((Label)labels[i]);

            scrolledAction = false;
            // sender = null;
            // e = null;
            return;
        }

        private void trvBaseTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode tn = trvBaseTree.SelectedNode;
            lblStatus.Text = "Selected Node: \n"
                + tn.ToString()
                + "  Index " + tn.Index.ToString()
                + "  Level " + tn.Level.ToString() + "\n";

            if (tn.Parent != null)
                lblStatus.Text += "Parent" + tn.Parent.ToString()
                    + "  Index " + tn.Parent.Index.ToString()
                    + "  Level " + tn.Parent.Level.ToString();
            else
                lblStatus.Text += "No Parent";

            sender = null;
            e = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TreeNode tn = trvBaseTree.SelectedNode;

            if (tn != null)
                tn.Nodes.Insert(trvBaseTree.SelectedNode.Index + 1, textBox1.Text);
            else
                trvBaseTree.Nodes.Insert(trvBaseTree.GetNodeCount(false), textBox1.Text);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            fillTestingDataTree(1);
            pnlDrawBoard.Refresh();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            fillTestingDataTree(2);
            pnlDrawBoard.Refresh();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            fillTestingDataTree(3);
            pnlDrawBoard.Refresh();
        }

        private void fillTestingDataTree(int testingScenario)
        {
            // Clear Previous Data
            trvBaseTree.Nodes.Clear();
            trvTempTree.Nodes.Clear();

            // Fill Tree with Testing Data
            for (int x = 0; x < 6; x++)
            {
                TreeNode tn = new TreeNode(x.ToString());
                trvBaseTree.Nodes.Insert(x, tn);
                switch (x)
                {
                    case 1:
                        tn.Nodes.Add("11");
                        tn.FirstNode.Nodes.Add("111");
                        break;
                    case 2:
                        tn.Nodes.Add("21");
                        tn.Nodes.Add("22");
                        if (testingScenario >= 2)
                        {
                            tn.LastNode.Nodes.Add("211");
                            tn.LastNode.Nodes.Add("212");
                            tn.LastNode.Nodes.Add("213");
                        }
                        break;
                    case 3:
                        tn.Nodes.Add("31");
                        tn.FirstNode.Nodes.Add("311");
                        tn.FirstNode.Nodes.Add("312");
                        if (testingScenario >= 3)
                        {
                            tn.FirstNode.FirstNode.Nodes.Add("3111");
                            tn.FirstNode.FirstNode.Nodes.Add("3112");
                            tn.FirstNode.FirstNode.Nodes.Add("3113");
                        }
                        break;
                    case 5:
                        if (testingScenario >= 3)
                        {
                        	tn.Nodes.Add("Text 5");
                        	tn.FirstNode.Nodes.Add("*51*");
                        	tn.FirstNode.FirstNode.Nodes.Add("5-1-1-1");
                        	tn.FirstNode.FirstNode.FirstNode.Nodes.Add("51111");
                        }
                        break;
                }
            }
            trvBaseTree.ExpandAll();
        }

        private void chbDraw_CheckedChanged(object sender, EventArgs e)
        {   
        	nodeFontSize = int.Parse(fontSize.Value.ToString());
        	
            pnlDrawBoard.Controls.Clear();
            pnlDrawBoard.Refresh();
            pnlDrawBoard_Paint();
            return;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            xStart.Value = 15;
            yStart.Value = pnlDrawBoard.Height - 30;
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
                TreeNode tnBlank = new TreeNode(txbBlankValue.Text);
                t.Nodes.Add(tnBlank);
                tnBlank.BackColor = Color.DodgerBlue;

                if (tnBlank.Level < untilLevel)
                    addBlankNode(tnBlank, untilLevel);
            }
            return;
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

        /// <summary>
        /// Main Function to Locate and Draw Nodes based on a TreeView.
        /// V2 Named "drawNodes"
        /// V3 Renames to "locateNodes"
        /// </summary>
        /// <param name="drawingPanel">Panel that will be used as Drawing Board.</param>
        /// <param name="originalTree">TreeView containing the Nodes to draw.</param>
        /// <param name="tempTree">Temporary (Work) variable.</param>
        private void locateNodes(Panel drawingPanel, TreeView originalTree, TreeView tempTree, out ArrayList labeledNodes)
        {
            tempTree.Nodes.Clear();
            // Version 3 Feature
            // ArrayList labeledNodes;      // ArrayList which will contain Label objects.
            Label labelAux;                 // Temporary Label wich will be added to the labeledNodes arrayList.

            labeledNodes = new ArrayList();

            if (originalTree.Nodes.Count == 0)
                return;

            #region drawing variables

            Graphics board = drawingPanel.CreateGraphics();

            int x = 20;
            int y = 20;

            int maxDepht = 0;

            ArrayList list;
            ArrayList[] listByLevel;

            int xInitial = int.Parse(xStart.Value.ToString());
            int yInitial = int.Parse(yStart.Value.ToString());            
			
			// this.drawNodeFont = new Font(System.Windows.Forms.TextBox.DefaultFont.FontFamily, 
			this.drawNodeFont = new Font("Arial", 
			                             float.Parse(fontSize.Value.ToString()),FontStyle.Bold);
			
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

                    // Version 3 Feature
                    labelAux = new Label();                    
                    labelAux.Name = nodeToPaint.Name;                    
                    labelAux.Font = drawNodeFont;
                    labelAux.Text = nodeToPaint.Text;
                    labelAux.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    labelAux.AutoSize = true;
                    labelAux.BorderStyle = BorderStyle.FixedSingle;
                    labelAux.MouseLeave += new System.EventHandler(this.lblTest_MouseLeave);
                    labelAux.MouseEnter += new System.EventHandler(this.lblTest_MouseEnter);
                    
                    labelAux.Tag = nodeToPaint;

                    // Drawing Style
                    if (nodeToPaint.Text == txbBlankValue.Text)
                    {   // Current node is auxiliar.
                        labelAux.BackColor = this.disabledNodeBackColor;
                        labelAux.ForeColor = this.disabledNodeForeColor;
                    }
                    else
                    {   // Current node contains valid data.
                        labelAux.BackColor = this.enabledNodeBackColor;
                        labelAux.ForeColor = this.enabledNodeForeColor;
                    }
                    unionNodeLinesPen = new Pen(labelAux.BackColor);
                    
                    // Calculating Node Position.
                    labelAux.Location = new Point(x, y);

                    // nodeToPaint.Tag = labelAux.ClientRectangle;
                    nodeToPaint.Tag = new Rectangle(labelAux.Left,
                                                    labelAux.Top,
                                                    labelAux.PreferredWidth,
                                                    labelAux.PreferredHeight);

                    // If the current node is not in the last level, then, 
                    // its position should be calculated based on its child nodes.
                    if (z < maxDepht)
                    {
                        Point posFirstChild = 
                        	getRectangleCenter((Rectangle)(nodeToPaint.FirstNode.Tag));
                        
                        Point posLastChild =
                        	getRectangleCenter((Rectangle)(nodeToPaint.LastNode.Tag));
                        
                        Point relocatedPoint = labelAux.Location;
                        relocatedPoint.X = (posFirstChild.X + posLastChild.X) / 2 - labelAux.PreferredWidth / 2;
                        System.Console.WriteLine(nodeToPaint.Text + " x= " + relocatedPoint.X 
                            + "\n  ->1: " + nodeToPaint.FirstNode.Text + " (" + posFirstChild.X + ");"
                            + "\n  ->2: " + nodeToPaint.LastNode.Text + " (" + posLastChild.X + ");");
						
                        labelAux.Location = relocatedPoint;
                        // nodeToPaint.Tag = labelAux.ClientRectangle;
                        nodeToPaint.Tag = new Rectangle(labelAux.Left,
                                labelAux.Top,
                                labelAux.PreferredWidth,
                                labelAux.PreferredHeight);

                        
                    }

                    // Union Lines
                    foreach (TreeNode t in nodeToPaint.Nodes)
                    {
                    	Rectangle r = new Rectangle(labelAux.Left,
                                                    labelAux.Top,
                                                    labelAux.PreferredWidth,
                                                    labelAux.PreferredHeight);
                        // Parent Center
                        Point parentCenterPos = getRectangleCenter(r);

                        // Child Center
                        Point childCenterPos = getRectangleCenter((Rectangle)t.Tag);
                        childCenterPos.Y = ((Rectangle)t.Tag).Top;

                        board.DrawLine(unionNodeLinesPen, parentCenterPos, childCenterPos);
                    }
                    
                    // Return Located Labels
                    labeledNodes.Add(labelAux);

                    // The next sibling node will be to the right of the current one.
                    // Where this node finishes plus Margin.
                    
                    // Note: Label.Right != Label.Left + labelAux.PreferredWidth
                    x = labelAux.Left + labelAux.PreferredWidth +
                    	int.Parse(xPadding.Value.ToString());
                    System.Console.WriteLine("Calculated X:" + x.ToString());
                }
                // The total nodes of the current level had been drawn.
                // The previous (superior) level should be located above the current level.
                y -= int.Parse(yPadding.Value.ToString());

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

        private Point getRectangleCenter(Rectangle r)
        {        	
            return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        }

        private void pnlDrawBoard_MouseClick(object sender, MouseEventArgs e)
        {
            xStart.Value = e.X;
            yStart.Value = e.Y;
        }

        private void lblTest_MouseEnter(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Color.Snow;
            // ((TreeNode) ((Label)sender).Tag).TreeView.ExpandAll();
            // ((TreeNode) ((Label)sender).Tag).BackColor = Color.Yellow;
        }

        private void lblTest_MouseLeave(object sender, EventArgs e)
        {
            ((Label)sender).BackColor = Color.CadetBlue;
            // ((Label)sender).BorderStyle = System.Windows.Forms.BorderStyle.None;
            // lblTest.BackColor = Color.CadetBlue;
        }


        
        void BtnExportClick(object sender, EventArgs e)
        {
        	Form x = new Form();
        	PropertyGrid pg = new PropertyGrid();        	
        	TextBox tb = new TextBox();
        	// string xmlStr = "";
        	
        	pg.Size = new Size(370,200);
        	tb.Size = new Size(370,400);
        	x.Size = new Size(400,620);
        	
        	tb.Multiline = true;
        	tb.ScrollBars = ScrollBars.Both;
        	
        	tb.Text = ExportPanelToSvg(this.pnlDrawBoard);
        	
        	x.Controls.Add(pg);
        	x.Controls.Add(tb);
        	tb.Location = new Point(pg.Location.X, pg.Bottom);        		        	
        	
        	// pg.SelectedObject = pnlDrawBoard.HorizontalScroll;
        	// pg.Refresh();        	
        	       
        	x.Show();        	
        }
        
        public string ExportPanelToSvg(Panel p)
        {       	        	
        	int totalWidth;
        	int totalHeight;
        	string svgNamespace = "http://www.w3.org/2000/svg";
        	Point pointLabel;
        	
        	if (p.Width > p.HorizontalScroll.Maximum)
        		totalWidth = p.Width;
        	else
        		totalWidth = p.HorizontalScroll.Maximum;

        	if (p.Height > p.VerticalScroll.Maximum)
        		totalHeight = p.Height;
        	else
        		totalHeight = p.VerticalScroll.Maximum;
        	
			XmlDocument doc = new XmlDocument();

			XmlElement root = doc.CreateElement("svg",svgNamespace);
			root.SetAttribute("xmlns:xlink","http://www.w3.org/1999/xlink");
			root.SetAttribute("width",totalWidth.ToString());
			root.SetAttribute("height",totalHeight.ToString());

			foreach (Control c in p.Controls)
			{
				if (c.GetType() != typeof(Label))
					continue;
				
				Label l = (Label)c;
				TreeNode tn = (TreeNode)l.Tag;
				pointLabel = getRectangleCenter((Rectangle)tn.Tag);
				
				XmlElement rectLabel = doc.CreateElement("rect",svgNamespace);
				XmlElement textLabel = doc.CreateElement("text",svgNamespace);				
				
				rectLabel.SetAttribute("width",l.PreferredWidth.ToString());
				rectLabel.SetAttribute("height",l.PreferredHeight.ToString());
				rectLabel.SetAttribute("rx","5");
				rectLabel.SetAttribute("x",l.Location.X.ToString());
				rectLabel.SetAttribute("y",l.Location.Y.ToString());
				rectLabel.SetAttribute("fill","white");
				rectLabel.SetAttribute("stroke","#707070");
				  	      
				textLabel.SetAttribute("x",pointLabel.X.ToString());
				textLabel.SetAttribute("y",pointLabel.Y.ToString());
				textLabel.SetAttribute("text-anchor","middle");
				textLabel.SetAttribute("font-family",l.Font.FontFamily.Name);
				textLabel.SetAttribute("font-size",l.Font.Size.ToString());
				textLabel.SetAttribute("style","dominant-baseline: central;");
				textLabel.InnerText = l.Text;

				foreach (TreeNode tnSon in tn.Nodes)
				{
					XmlElement line = doc.CreateElement("line",svgNamespace);
				
					Rectangle rectSon = (Rectangle)tnSon.Tag;
					Point pointSon = getRectangleCenter(rectSon);
					
					line.SetAttribute("x1",pointLabel.X.ToString());
					line.SetAttribute("y1",l.Bottom.ToString());
					
					line.SetAttribute("x2",pointSon.X.ToString());
					line.SetAttribute("y2",rectSon.Top.ToString());
					
					line.SetAttribute("stroke-width","1");
					line.SetAttribute("stroke","#707070");
					
					root.AppendChild(line);
				}
				root.AppendChild(rectLabel);
				root.AppendChild(textLabel);
				
			}
               	
			doc.AppendChild(root);
 
			return FormatXml(doc);
        }
        
        /// <summary>
		/// Formats the provided XML so it's indented and humanly-readable.
		/// </summary>
		/// <param name="inputXml">The input XML to format.</param>
		/// <returns></returns>	
		/** 
		 * References:
		 * http://www.shrinkrays.net/code-snippets/csharp/format-xml-in-csharp.aspx	 
		 **/
		public static string FormatXml(System.Xml.XmlDocument xmlDoc)
		{			
			StringBuilder strBuilder = new StringBuilder();												
			XmlTextWriter xtWriter = new XmlTextWriter(new StringWriter(strBuilder));
			xtWriter.Formatting = System.Xml.Formatting.Indented;
			xmlDoc.Save(xtWriter);
			return strBuilder.ToString().Replace("encoding=\"utf-16\"","encoding=\"utf-8\"");
		}

    }
}