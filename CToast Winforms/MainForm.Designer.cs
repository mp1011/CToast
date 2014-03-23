namespace CToast
{
    partial class MainForm
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
            this.tabDisplay = new System.Windows.Forms.TabControl();
            this.pgeGraph = new System.Windows.Forms.TabPage();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.graphControl1 = new CToast.GraphControl();
            this.pgeImage = new System.Windows.Forms.TabPage();
            this.pnlImage = new System.Windows.Forms.Panel();
            this.pgeTree = new System.Windows.Forms.TabPage();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.pgeText = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.pgeColorTree = new System.Windows.Forms.TabPage();
            this.pnlImgColorTree = new System.Windows.Forms.Panel();
            this.pgeFile = new System.Windows.Forms.TabPage();
            this.btnRenderTextTrees = new System.Windows.Forms.Button();
            this.btnRenderColorTrees = new System.Windows.Forms.Button();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.pnlTop = new System.Windows.Forms.Panel();
            this.chkShowSelectors = new System.Windows.Forms.CheckBox();
            this.btnRestart = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.nbSteps = new System.Windows.Forms.NumericUpDown();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.chkPause = new System.Windows.Forms.CheckBox();
            this.btnEvaluate = new System.Windows.Forms.Button();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.tbDisplayedStep = new System.Windows.Forms.TrackBar();
            this.tabDisplay.SuspendLayout();
            this.pgeGraph.SuspendLayout();
            this.pgeImage.SuspendLayout();
            this.pgeTree.SuspendLayout();
            this.pgeText.SuspendLayout();
            this.pgeColorTree.SuspendLayout();
            this.pgeFile.SuspendLayout();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nbSteps)).BeginInit();
            this.pnlBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbDisplayedStep)).BeginInit();
            this.SuspendLayout();
            // 
            // tabDisplay
            // 
            this.tabDisplay.Controls.Add(this.pgeGraph);
            this.tabDisplay.Controls.Add(this.pgeImage);
            this.tabDisplay.Controls.Add(this.pgeTree);
            this.tabDisplay.Controls.Add(this.pgeText);
            this.tabDisplay.Controls.Add(this.pgeColorTree);
            this.tabDisplay.Controls.Add(this.pgeFile);
            this.tabDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabDisplay.Location = new System.Drawing.Point(0, 104);
            this.tabDisplay.Name = "tabDisplay";
            this.tabDisplay.SelectedIndex = 0;
            this.tabDisplay.Size = new System.Drawing.Size(983, 364);
            this.tabDisplay.TabIndex = 0;
            this.tabDisplay.SelectedIndexChanged += new System.EventHandler(this.tabDisplay_SelectedIndexChanged);
            this.tabDisplay.TabIndexChanged += new System.EventHandler(this.tabDisplay_TabIndexChanged);
            // 
            // pgeGraph
            // 
            this.pgeGraph.Controls.Add(this.elementHost1);
            this.pgeGraph.Location = new System.Drawing.Point(4, 22);
            this.pgeGraph.Name = "pgeGraph";
            this.pgeGraph.Padding = new System.Windows.Forms.Padding(3);
            this.pgeGraph.Size = new System.Drawing.Size(975, 338);
            this.pgeGraph.TabIndex = 0;
            this.pgeGraph.Text = "Graph";
            this.pgeGraph.UseVisualStyleBackColor = true;
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(3, 3);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(969, 332);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.graphControl1;
            // 
            // pgeImage
            // 
            this.pgeImage.Controls.Add(this.pnlImage);
            this.pgeImage.Location = new System.Drawing.Point(4, 22);
            this.pgeImage.Name = "pgeImage";
            this.pgeImage.Padding = new System.Windows.Forms.Padding(3);
            this.pgeImage.Size = new System.Drawing.Size(975, 338);
            this.pgeImage.TabIndex = 1;
            this.pgeImage.Text = "Image";
            this.pgeImage.UseVisualStyleBackColor = true;
            // 
            // pnlImage
            // 
            this.pnlImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlImage.Location = new System.Drawing.Point(3, 3);
            this.pnlImage.Name = "pnlImage";
            this.pnlImage.Size = new System.Drawing.Size(969, 332);
            this.pnlImage.TabIndex = 0;
            this.pnlImage.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlImage_Paint);
            // 
            // pgeTree
            // 
            this.pgeTree.Controls.Add(this.treeView1);
            this.pgeTree.Location = new System.Drawing.Point(4, 22);
            this.pgeTree.Name = "pgeTree";
            this.pgeTree.Size = new System.Drawing.Size(975, 338);
            this.pgeTree.TabIndex = 2;
            this.pgeTree.Text = "Tree";
            this.pgeTree.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(975, 338);
            this.treeView1.TabIndex = 0;
            // 
            // pgeText
            // 
            this.pgeText.Controls.Add(this.textBox1);
            this.pgeText.Location = new System.Drawing.Point(4, 22);
            this.pgeText.Name = "pgeText";
            this.pgeText.Size = new System.Drawing.Size(975, 338);
            this.pgeText.TabIndex = 3;
            this.pgeText.Text = "Text";
            this.pgeText.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(975, 338);
            this.textBox1.TabIndex = 0;
            // 
            // pgeColorTree
            // 
            this.pgeColorTree.Controls.Add(this.pnlImgColorTree);
            this.pgeColorTree.Location = new System.Drawing.Point(4, 22);
            this.pgeColorTree.Name = "pgeColorTree";
            this.pgeColorTree.Padding = new System.Windows.Forms.Padding(3);
            this.pgeColorTree.Size = new System.Drawing.Size(975, 338);
            this.pgeColorTree.TabIndex = 5;
            this.pgeColorTree.Text = "Color Tree";
            this.pgeColorTree.UseVisualStyleBackColor = true;
            // 
            // pnlImgColorTree
            // 
            this.pnlImgColorTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlImgColorTree.Location = new System.Drawing.Point(3, 3);
            this.pnlImgColorTree.Name = "pnlImgColorTree";
            this.pnlImgColorTree.Size = new System.Drawing.Size(969, 332);
            this.pnlImgColorTree.TabIndex = 1;
            this.pnlImgColorTree.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlImgColorTree_Paint);
            // 
            // pgeFile
            // 
            this.pgeFile.Controls.Add(this.btnRenderTextTrees);
            this.pgeFile.Controls.Add(this.btnRenderColorTrees);
            this.pgeFile.Location = new System.Drawing.Point(4, 22);
            this.pgeFile.Name = "pgeFile";
            this.pgeFile.Padding = new System.Windows.Forms.Padding(3);
            this.pgeFile.Size = new System.Drawing.Size(975, 338);
            this.pgeFile.TabIndex = 4;
            this.pgeFile.Text = "File";
            this.pgeFile.UseVisualStyleBackColor = true;
            // 
            // btnRenderTextTrees
            // 
            this.btnRenderTextTrees.Location = new System.Drawing.Point(11, 57);
            this.btnRenderTextTrees.Name = "btnRenderTextTrees";
            this.btnRenderTextTrees.Size = new System.Drawing.Size(158, 32);
            this.btnRenderTextTrees.TabIndex = 1;
            this.btnRenderTextTrees.Text = "Render as Text Trees";
            this.btnRenderTextTrees.UseVisualStyleBackColor = true;
            this.btnRenderTextTrees.Click += new System.EventHandler(this.btnRenderTextTrees_Click);
            // 
            // btnRenderColorTrees
            // 
            this.btnRenderColorTrees.Location = new System.Drawing.Point(11, 19);
            this.btnRenderColorTrees.Name = "btnRenderColorTrees";
            this.btnRenderColorTrees.Size = new System.Drawing.Size(158, 32);
            this.btnRenderColorTrees.TabIndex = 0;
            this.btnRenderColorTrees.Text = "Render as Color Trees";
            this.btnRenderColorTrees.UseVisualStyleBackColor = true;
            this.btnRenderColorTrees.Click += new System.EventHandler(this.btnRenderColorTrees_Click);
            // 
            // txtInput
            // 
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInput.Location = new System.Drawing.Point(14, 17);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(846, 26);
            this.txtInput.TabIndex = 1;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.chkShowSelectors);
            this.pnlTop.Controls.Add(this.btnRestart);
            this.pnlTop.Controls.Add(this.label1);
            this.pnlTop.Controls.Add(this.nbSteps);
            this.pnlTop.Controls.Add(this.progressBar);
            this.pnlTop.Controls.Add(this.lblProgress);
            this.pnlTop.Controls.Add(this.chkPause);
            this.pnlTop.Controls.Add(this.btnEvaluate);
            this.pnlTop.Controls.Add(this.txtInput);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(983, 104);
            this.pnlTop.TabIndex = 2;
            // 
            // chkShowSelectors
            // 
            this.chkShowSelectors.AutoSize = true;
            this.chkShowSelectors.Location = new System.Drawing.Point(423, 67);
            this.chkShowSelectors.Name = "chkShowSelectors";
            this.chkShowSelectors.Size = new System.Drawing.Size(100, 17);
            this.chkShowSelectors.TabIndex = 10;
            this.chkShowSelectors.Text = "Show Selectors";
            this.chkShowSelectors.UseVisualStyleBackColor = true;
            // 
            // btnRestart
            // 
            this.btnRestart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestart.Location = new System.Drawing.Point(866, 43);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(105, 20);
            this.btnRestart.TabIndex = 9;
            this.btnRestart.Text = "Restart";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(757, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Steps per Batch:";
            // 
            // nbSteps
            // 
            this.nbSteps.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.nbSteps.Location = new System.Drawing.Point(849, 72);
            this.nbSteps.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.nbSteps.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nbSteps.Name = "nbSteps";
            this.nbSteps.Size = new System.Drawing.Size(122, 20);
            this.nbSteps.TabIndex = 7;
            this.nbSteps.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(14, 49);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(843, 11);
            this.progressBar.TabIndex = 6;
            this.progressBar.Value = 1;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(129, 67);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(79, 13);
            this.lblProgress.TabIndex = 5;
            this.lblProgress.Text = "Total Steps = 0";
            // 
            // chkPause
            // 
            this.chkPause.AutoSize = true;
            this.chkPause.Location = new System.Drawing.Point(15, 66);
            this.chkPause.Name = "chkPause";
            this.chkPause.Size = new System.Drawing.Size(109, 17);
            this.chkPause.TabIndex = 4;
            this.chkPause.Text = "Pause Evaluation";
            this.chkPause.UseVisualStyleBackColor = true;
            this.chkPause.CheckedChanged += new System.EventHandler(this.chkPause_CheckedChanged);
            // 
            // btnEvaluate
            // 
            this.btnEvaluate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEvaluate.Location = new System.Drawing.Point(866, 17);
            this.btnEvaluate.Name = "btnEvaluate";
            this.btnEvaluate.Size = new System.Drawing.Size(105, 20);
            this.btnEvaluate.TabIndex = 2;
            this.btnEvaluate.Text = "Evaluate";
            this.btnEvaluate.UseVisualStyleBackColor = true;
            this.btnEvaluate.Click += new System.EventHandler(this.btnEvaluate_Click);
            // 
            // worker
            // 
            this.worker.WorkerReportsProgress = true;
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.worker_DoWork);
            this.worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.worker_ProgressChanged);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.worker_RunWorkerCompleted);
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.tbDisplayedStep);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 468);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(983, 67);
            this.pnlBottom.TabIndex = 10;
            // 
            // tbDisplayedStep
            // 
            this.tbDisplayedStep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tbDisplayedStep.Location = new System.Drawing.Point(7, 6);
            this.tbDisplayedStep.Name = "tbDisplayedStep";
            this.tbDisplayedStep.Size = new System.Drawing.Size(976, 45);
            this.tbDisplayedStep.TabIndex = 0;
            this.tbDisplayedStep.Scroll += new System.EventHandler(this.tbDisplayedStep_Scroll);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(983, 535);
            this.Controls.Add(this.tabDisplay);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlBottom);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.tabDisplay.ResumeLayout(false);
            this.pgeGraph.ResumeLayout(false);
            this.pgeImage.ResumeLayout(false);
            this.pgeTree.ResumeLayout(false);
            this.pgeText.ResumeLayout(false);
            this.pgeText.PerformLayout();
            this.pgeColorTree.ResumeLayout(false);
            this.pgeFile.ResumeLayout(false);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nbSteps)).EndInit();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbDisplayedStep)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabDisplay;
        private System.Windows.Forms.TabPage pgeGraph;
        private System.Windows.Forms.TabPage pgeImage;
        private System.Windows.Forms.TabPage pgeTree;
        private System.Windows.Forms.TabPage pgeText;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Button btnEvaluate;
        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.CheckBox chkPause;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private GraphControl graphControl1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nbSteps;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.TrackBar tbDisplayedStep;
        private System.Windows.Forms.CheckBox chkShowSelectors;
        private System.Windows.Forms.Panel pnlImage;
        private System.Windows.Forms.TabPage pgeFile;
        private System.Windows.Forms.TabPage pgeColorTree;
        private System.Windows.Forms.Panel pnlImgColorTree;
        private System.Windows.Forms.Button btnRenderColorTrees;
        private System.Windows.Forms.Button btnRenderTextTrees;
    }
}