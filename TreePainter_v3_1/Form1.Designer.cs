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

namespace TreePainter_v3_1
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        	this.lblBaseTree = new System.Windows.Forms.Label();
        	this.lblTempTree = new System.Windows.Forms.Label();
        	this.trvBaseTree = new System.Windows.Forms.TreeView();
        	this.trvTempTree = new System.Windows.Forms.TreeView();
        	this.pnlDrawBoard = new System.Windows.Forms.Panel();
        	this.lblDrawTitle = new System.Windows.Forms.Label();
        	this.lblStatus = new System.Windows.Forms.Label();
        	this.groupBox1 = new System.Windows.Forms.GroupBox();
        	this.chbDraw = new System.Windows.Forms.CheckBox();
        	this.textBox1 = new System.Windows.Forms.TextBox();
        	this.button1 = new System.Windows.Forms.Button();
        	this.button5 = new System.Windows.Forms.Button();
        	this.button4 = new System.Windows.Forms.Button();
        	this.button3 = new System.Windows.Forms.Button();
        	this.yPadding = new System.Windows.Forms.NumericUpDown();
        	this.xPadding = new System.Windows.Forms.NumericUpDown();
        	this.fontSize = new System.Windows.Forms.NumericUpDown();
        	this.label3 = new System.Windows.Forms.Label();
        	this.label2 = new System.Windows.Forms.Label();
        	this.label1 = new System.Windows.Forms.Label();
        	this.groupBox2 = new System.Windows.Forms.GroupBox();
        	this.lblTest = new System.Windows.Forms.Label();
        	this.yStart = new System.Windows.Forms.NumericUpDown();
        	this.xStart = new System.Windows.Forms.NumericUpDown();
        	this.label5 = new System.Windows.Forms.Label();
        	this.txbBlankValue = new System.Windows.Forms.TextBox();
        	this.label4 = new System.Windows.Forms.Label();
        	this.btnExport = new System.Windows.Forms.Button();
        	this.groupBox1.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.yPadding)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.xPadding)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.fontSize)).BeginInit();
        	this.groupBox2.SuspendLayout();
        	((System.ComponentModel.ISupportInitialize)(this.yStart)).BeginInit();
        	((System.ComponentModel.ISupportInitialize)(this.xStart)).BeginInit();
        	this.SuspendLayout();
        	// 
        	// lblBaseTree
        	// 
        	this.lblBaseTree.AutoSize = true;
        	this.lblBaseTree.Location = new System.Drawing.Point(12, 9);
        	this.lblBaseTree.Name = "lblBaseTree";
        	this.lblBaseTree.Size = new System.Drawing.Size(79, 13);
        	this.lblBaseTree.TabIndex = 0;
        	this.lblBaseTree.Text = "Base TreeView";
        	// 
        	// lblTempTree
        	// 
        	this.lblTempTree.AutoSize = true;
        	this.lblTempTree.Location = new System.Drawing.Point(223, 9);
        	this.lblTempTree.Name = "lblTempTree";
        	this.lblTempTree.Size = new System.Drawing.Size(134, 13);
        	this.lblTempTree.TabIndex = 1;
        	this.lblTempTree.Text = "Temporary Work TreeView";
        	// 
        	// trvBaseTree
        	// 
        	this.trvBaseTree.Location = new System.Drawing.Point(15, 25);
        	this.trvBaseTree.Name = "trvBaseTree";
        	this.trvBaseTree.Size = new System.Drawing.Size(205, 227);
        	this.trvBaseTree.TabIndex = 3;
        	this.trvBaseTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvBaseTree_AfterSelect);
        	// 
        	// trvTempTree
        	// 
        	this.trvTempTree.BackColor = System.Drawing.Color.WhiteSmoke;
        	this.trvTempTree.Location = new System.Drawing.Point(226, 25);
        	this.trvTempTree.Name = "trvTempTree";
        	this.trvTempTree.Size = new System.Drawing.Size(205, 227);
        	this.trvTempTree.TabIndex = 4;
        	// 
        	// pnlDrawBoard
        	// 
        	this.pnlDrawBoard.BackColor = System.Drawing.Color.LightBlue;
        	this.pnlDrawBoard.Location = new System.Drawing.Point(442, 25);
        	this.pnlDrawBoard.Name = "pnlDrawBoard";
        	this.pnlDrawBoard.Size = new System.Drawing.Size(576, 432);
        	this.pnlDrawBoard.TabIndex = 5;
        	this.pnlDrawBoard.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pnlDrawBoard_MouseClick);
        	// 
        	// lblDrawTitle
        	// 
        	this.lblDrawTitle.AutoSize = true;
        	this.lblDrawTitle.Location = new System.Drawing.Point(439, 9);
        	this.lblDrawTitle.Name = "lblDrawTitle";
        	this.lblDrawTitle.Size = new System.Drawing.Size(76, 13);
        	this.lblDrawTitle.TabIndex = 2;
        	this.lblDrawTitle.Text = "Drawing Panel";
        	// 
        	// lblStatus
        	// 
        	this.lblStatus.Location = new System.Drawing.Point(12, 255);
        	this.lblStatus.Name = "lblStatus";
        	this.lblStatus.Size = new System.Drawing.Size(187, 65);
        	this.lblStatus.TabIndex = 6;
        	this.lblStatus.Text = "Status Message";
        	// 
        	// groupBox1
        	// 
        	this.groupBox1.Controls.Add(this.chbDraw);
        	this.groupBox1.Controls.Add(this.textBox1);
        	this.groupBox1.Controls.Add(this.button1);
        	this.groupBox1.Controls.Add(this.button5);
        	this.groupBox1.Controls.Add(this.button4);
        	this.groupBox1.Controls.Add(this.button3);
        	this.groupBox1.Location = new System.Drawing.Point(226, 255);
        	this.groupBox1.Name = "groupBox1";
        	this.groupBox1.Size = new System.Drawing.Size(213, 113);
        	this.groupBox1.TabIndex = 18;
        	this.groupBox1.TabStop = false;
        	this.groupBox1.Text = "Testing Tasks";
        	// 
        	// chbDraw
        	// 
        	this.chbDraw.AutoSize = true;
        	this.chbDraw.Location = new System.Drawing.Point(98, 91);
        	this.chbDraw.Name = "chbDraw";
        	this.chbDraw.Size = new System.Drawing.Size(92, 17);
        	this.chbDraw.TabIndex = 20;
        	this.chbDraw.Text = "Draw in Panel";
        	this.chbDraw.UseVisualStyleBackColor = true;
        	this.chbDraw.CheckedChanged += new System.EventHandler(this.chbDraw_CheckedChanged);
        	// 
        	// textBox1
        	// 
        	this.textBox1.Location = new System.Drawing.Point(6, 19);
        	this.textBox1.Name = "textBox1";
        	this.textBox1.Size = new System.Drawing.Size(86, 20);
        	this.textBox1.TabIndex = 19;
        	// 
        	// button1
        	// 
        	this.button1.Location = new System.Drawing.Point(6, 45);
        	this.button1.Name = "button1";
        	this.button1.Size = new System.Drawing.Size(86, 50);
        	this.button1.TabIndex = 18;
        	this.button1.Text = "add childNode to selected on BaseTree";
        	this.button1.UseVisualStyleBackColor = true;
        	this.button1.Click += new System.EventHandler(this.button1_Click);
        	// 
        	// button5
        	// 
        	this.button5.Location = new System.Drawing.Point(98, 62);
        	this.button5.Name = "button5";
        	this.button5.Size = new System.Drawing.Size(112, 23);
        	this.button5.TabIndex = 16;
        	this.button5.Text = "Fill Base with test 3";
        	this.button5.UseVisualStyleBackColor = true;
        	this.button5.Click += new System.EventHandler(this.button5_Click);
        	// 
        	// button4
        	// 
        	this.button4.Location = new System.Drawing.Point(98, 39);
        	this.button4.Name = "button4";
        	this.button4.Size = new System.Drawing.Size(112, 23);
        	this.button4.TabIndex = 15;
        	this.button4.Text = "Fill Base with test 2";
        	this.button4.UseVisualStyleBackColor = true;
        	this.button4.Click += new System.EventHandler(this.button4_Click);
        	// 
        	// button3
        	// 
        	this.button3.Location = new System.Drawing.Point(98, 16);
        	this.button3.Name = "button3";
        	this.button3.Size = new System.Drawing.Size(112, 23);
        	this.button3.TabIndex = 14;
        	this.button3.Text = "Fill Base with test 1";
        	this.button3.UseVisualStyleBackColor = true;
        	this.button3.Click += new System.EventHandler(this.button3_Click);
        	// 
        	// yPadding
        	// 
        	this.yPadding.Location = new System.Drawing.Point(68, 60);
        	this.yPadding.Name = "yPadding";
        	this.yPadding.Size = new System.Drawing.Size(37, 20);
        	this.yPadding.TabIndex = 26;
        	this.yPadding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.yPadding.Value = new decimal(new int[] {
        	        	        	30,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.yPadding.ValueChanged += new System.EventHandler(this.chbDraw_CheckedChanged);
        	// 
        	// xPadding
        	// 
        	this.xPadding.Location = new System.Drawing.Point(68, 36);
        	this.xPadding.Name = "xPadding";
        	this.xPadding.Size = new System.Drawing.Size(37, 20);
        	this.xPadding.TabIndex = 25;
        	this.xPadding.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.xPadding.Value = new decimal(new int[] {
        	        	        	10,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.xPadding.ValueChanged += new System.EventHandler(this.chbDraw_CheckedChanged);
        	// 
        	// fontSize
        	// 
        	this.fontSize.Location = new System.Drawing.Point(68, 13);
        	this.fontSize.Name = "fontSize";
        	this.fontSize.Size = new System.Drawing.Size(37, 20);
        	this.fontSize.TabIndex = 24;
        	this.fontSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.fontSize.Value = new decimal(new int[] {
        	        	        	8,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.fontSize.ValueChanged += new System.EventHandler(this.chbDraw_CheckedChanged);
        	// 
        	// label3
        	// 
        	this.label3.AutoSize = true;
        	this.label3.Location = new System.Drawing.Point(6, 60);
        	this.label3.Name = "label3";
        	this.label3.Size = new System.Drawing.Size(55, 13);
        	this.label3.TabIndex = 23;
        	this.label3.Text = "Y padding";
        	// 
        	// label2
        	// 
        	this.label2.AutoSize = true;
        	this.label2.Location = new System.Drawing.Point(6, 36);
        	this.label2.Name = "label2";
        	this.label2.Size = new System.Drawing.Size(55, 13);
        	this.label2.TabIndex = 22;
        	this.label2.Text = "X padding";
        	// 
        	// label1
        	// 
        	this.label1.AutoSize = true;
        	this.label1.Location = new System.Drawing.Point(6, 16);
        	this.label1.Name = "label1";
        	this.label1.Size = new System.Drawing.Size(51, 13);
        	this.label1.TabIndex = 21;
        	this.label1.Text = "Text Size";
        	// 
        	// groupBox2
        	// 
        	this.groupBox2.Controls.Add(this.btnExport);
        	this.groupBox2.Controls.Add(this.lblTest);
        	this.groupBox2.Controls.Add(this.yStart);
        	this.groupBox2.Controls.Add(this.xStart);
        	this.groupBox2.Controls.Add(this.label5);
        	this.groupBox2.Controls.Add(this.txbBlankValue);
        	this.groupBox2.Controls.Add(this.label4);
        	this.groupBox2.Controls.Add(this.yPadding);
        	this.groupBox2.Controls.Add(this.label1);
        	this.groupBox2.Controls.Add(this.xPadding);
        	this.groupBox2.Controls.Add(this.label2);
        	this.groupBox2.Controls.Add(this.fontSize);
        	this.groupBox2.Controls.Add(this.label3);
        	this.groupBox2.Location = new System.Drawing.Point(12, 371);
        	this.groupBox2.Name = "groupBox2";
        	this.groupBox2.Size = new System.Drawing.Size(419, 86);
        	this.groupBox2.TabIndex = 27;
        	this.groupBox2.TabStop = false;
        	this.groupBox2.Text = "Drawing Vars";
        	// 
        	// lblTest
        	// 
        	this.lblTest.AutoSize = true;
        	this.lblTest.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        	this.lblTest.ForeColor = System.Drawing.SystemColors.ControlText;
        	this.lblTest.Location = new System.Drawing.Point(312, 12);
        	this.lblTest.Name = "lblTest";
        	this.lblTest.Size = new System.Drawing.Size(73, 15);
        	this.lblTest.TabIndex = 32;
        	this.lblTest.Text = "Testing Label";
        	this.lblTest.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        	this.lblTest.MouseLeave += new System.EventHandler(this.lblTest_MouseLeave);
        	this.lblTest.MouseEnter += new System.EventHandler(this.lblTest_MouseEnter);
        	// 
        	// yStart
        	// 
        	this.yStart.Location = new System.Drawing.Point(248, 36);
        	this.yStart.Maximum = new decimal(new int[] {
        	        	        	10000,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.yStart.Name = "yStart";
        	this.yStart.Size = new System.Drawing.Size(37, 20);
        	this.yStart.TabIndex = 31;
        	this.yStart.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.yStart.ValueChanged += new System.EventHandler(this.chbDraw_CheckedChanged);
        	// 
        	// xStart
        	// 
        	this.xStart.Location = new System.Drawing.Point(207, 36);
        	this.xStart.Maximum = new decimal(new int[] {
        	        	        	10000,
        	        	        	0,
        	        	        	0,
        	        	        	0});
        	this.xStart.Name = "xStart";
        	this.xStart.Size = new System.Drawing.Size(37, 20);
        	this.xStart.TabIndex = 30;
        	this.xStart.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
        	this.xStart.ValueChanged += new System.EventHandler(this.chbDraw_CheckedChanged);
        	// 
        	// label5
        	// 
        	this.label5.AutoSize = true;
        	this.label5.Location = new System.Drawing.Point(111, 36);
        	this.label5.Name = "label5";
        	this.label5.Size = new System.Drawing.Size(92, 26);
        	this.label5.TabIndex = 29;
        	this.label5.Text = "Starting Point (x,y)\r\nOr Click on Panel";
        	// 
        	// txbBlankValue
        	// 
        	this.txbBlankValue.Location = new System.Drawing.Point(207, 13);
        	this.txbBlankValue.Name = "txbBlankValue";
        	this.txbBlankValue.Size = new System.Drawing.Size(64, 20);
        	this.txbBlankValue.TabIndex = 28;
        	this.txbBlankValue.Text = "---";
        	// 
        	// label4
        	// 
        	this.label4.AutoSize = true;
        	this.label4.Location = new System.Drawing.Point(140, 13);
        	this.label4.Name = "label4";
        	this.label4.Size = new System.Drawing.Size(63, 13);
        	this.label4.TabIndex = 27;
        	this.label4.Text = "blank Value";
        	// 
        	// btnExport
        	// 
        	this.btnExport.Location = new System.Drawing.Point(312, 36);
        	this.btnExport.Name = "btnExport";
        	this.btnExport.Size = new System.Drawing.Size(92, 23);
        	this.btnExport.TabIndex = 33;
        	this.btnExport.Text = "Export to SVG";
        	this.btnExport.UseVisualStyleBackColor = true;
        	this.btnExport.Click += new System.EventHandler(this.BtnExportClick);
        	// 
        	// Form1
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(1030, 465);
        	this.Controls.Add(this.groupBox2);
        	this.Controls.Add(this.groupBox1);
        	this.Controls.Add(this.lblStatus);
        	this.Controls.Add(this.lblDrawTitle);
        	this.Controls.Add(this.pnlDrawBoard);
        	this.Controls.Add(this.trvTempTree);
        	this.Controls.Add(this.trvBaseTree);
        	this.Controls.Add(this.lblTempTree);
        	this.Controls.Add(this.lblBaseTree);
        	this.Name = "Form1";
        	this.Text = "Tree Painter v3_1";
        	this.Load += new System.EventHandler(this.Form1_Load);
        	this.groupBox1.ResumeLayout(false);
        	this.groupBox1.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.yPadding)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.xPadding)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.fontSize)).EndInit();
        	this.groupBox2.ResumeLayout(false);
        	this.groupBox2.PerformLayout();
        	((System.ComponentModel.ISupportInitialize)(this.yStart)).EndInit();
        	((System.ComponentModel.ISupportInitialize)(this.xStart)).EndInit();
        	this.ResumeLayout(false);
        	this.PerformLayout();
        }
        private System.Windows.Forms.Button btnExport;

        #endregion

        private System.Windows.Forms.Label lblBaseTree;
        private System.Windows.Forms.Label lblTempTree;
        private System.Windows.Forms.TreeView trvBaseTree;
        private System.Windows.Forms.TreeView trvTempTree;
        private System.Windows.Forms.Panel pnlDrawBoard;
        private System.Windows.Forms.Label lblDrawTitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox chbDraw;
        private System.Windows.Forms.NumericUpDown yPadding;
        private System.Windows.Forms.NumericUpDown xPadding;
        private System.Windows.Forms.NumericUpDown fontSize;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txbBlankValue;
        private System.Windows.Forms.NumericUpDown yStart;
        private System.Windows.Forms.NumericUpDown xStart;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblTest;
    }
}

