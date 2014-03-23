namespace TreePainter_v3_1
{
    partial class TreeViewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlDrawBoard = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // pnlDrawBoard
            // 
            this.pnlDrawBoard.BackColor = System.Drawing.Color.LightBlue;
            this.pnlDrawBoard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDrawBoard.Location = new System.Drawing.Point(0, 0);
            this.pnlDrawBoard.Name = "pnlDrawBoard";
            this.pnlDrawBoard.Size = new System.Drawing.Size(594, 484);
            this.pnlDrawBoard.TabIndex = 6;
            // 
            // TreeViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnlDrawBoard);
            this.Name = "TreeViewer";
            this.Size = new System.Drawing.Size(594, 484);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlDrawBoard;
    }
}
