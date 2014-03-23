using System;
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
    public partial class TreeViewer : UserControl
    {

        public void SetTree(TreeView tree)
        {
          //  mTree = VisualTree.Create(tree);
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            pnlDrawBoard.Controls.Clear();
            pnlDrawBoard.Refresh();
            pnlDrawBoard_Paint();
            return;
        }

        private VisualTree mTree;

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

        public TreeViewer()
        {
            InitializeComponent();            
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

            if (!scrolledAction)
                PaintTree();
            else
                return;
                        
            pnlDrawBoard.AutoScroll = true;
            scrolledAction = false;
            // sender = null;
            // e = null;
            return;
        }

        private void RepositionTree()
        {
            mTree.AlignInArea(pnlDrawBoard.Size);
        }

        private void PaintTree()
        {
            if (mTree.Root == null)
                return;

            RepositionTree();

            Graphics board = pnlDrawBoard.CreateGraphics();

            foreach (var node in mTree.Nodes.OfType<TextVisualTreeNode>())
            {
                if (node.IsBlank)
                    continue;

                // Version 3 Feature
                var labelAux = new Label();                    
                labelAux.Name = node.ID.ToString();
                labelAux.Font = node.Font;
                labelAux.Text = node.Text;
                labelAux.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                labelAux.AutoSize = true;
                labelAux.BorderStyle = BorderStyle.FixedSingle;
           
                // labelAux.MouseLeave += new System.EventHandler(this.lblTest_MouseLeave);
              //  labelAux.MouseEnter += new System.EventHandler(this.lblTest_MouseEnter);
                    
                labelAux.Tag = node;

                labelAux.BackColor = node.BackColor;
                labelAux.ForeColor = node.TextColor;

                unionNodeLinesPen = new Pen(labelAux.BackColor);
                    
                // Calculating Node Position.
                labelAux.Left = node.Area.Left;
                labelAux.Top = node.Area.Top;
                labelAux.Width = node.Area.Width;
                labelAux.Height = node.Area.Height;

                //node.Area = new Rectangle(labelAux.Left, labelAux.Top, labelAux.PreferredWidth, labelAux.PreferredHeight);
                node.Location = new Point(labelAux.Left, labelAux.Top);

                pnlDrawBoard.Controls.Add(labelAux);
            }

            foreach (var line in mTree.Lines)
            {
                if (line.Child.IsBlank)
                    continue;

                Rectangle r = line.Parent.Area;
                // Parent Center
                Point parentCenterPos = getRectangleCenter(r);

                // Child Center
                Point childCenterPos = getRectangleCenter(line.Child.Area);
                childCenterPos.Y = line.Child.Area.Top;

                board.DrawLine(unionNodeLinesPen, parentCenterPos, childCenterPos);                
            }
            
        }

        private Point getRectangleCenter(Rectangle r)
        {
            return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        }
     
    }
    
}
