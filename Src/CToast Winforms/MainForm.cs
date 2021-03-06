﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace CToast
{
    public partial class MainForm : Form
    {
        private EvaluationChain mCurrentEvaluation = new EvaluationChain();
        private Context mContext;

        public MainForm()
        {
            InitializeComponent();

            if (System.IO.File.Exists("input.txt"))
            {
                foreach (var line in System.IO.File.ReadAllLines("input.txt"))
                    txtInput.Items.Add(line);

                if(txtInput.Items.Count > 0)
                    txtInput.Text = txtInput.Items[txtInput.Items.Count - 1].ToString();
            }

            mCustomTreeRenderer = new CustomTreeRenderer();
            mHybridRenderer = HybridRenderer.TypeB();

            pgeGraph.Tag = new FormRendererHelper<GraphSharpViewModel> { Renderer = new GraphSharpRenderer(), AfterRenderAction = (a, n) => graphControl1.DataContext = a };

            pgeImage.Tag = new FormRendererHelper<Bitmap> { Renderer = mCustomTreeRenderer, AfterRenderAction = (a, n) => {

                imgTree.Image = a;
              //  imgTree.Zoom = (int)(((float)imgTree.PanelWidth / (float)imgTree.Width) * 100f);

            } };
            pgeText.Tag = new FormRendererHelper<string> { Renderer = new SyntaxRenderer(), AfterRenderAction = (a, n) => { textBox1.Text = a; } };
            pgeTree.Tag = new FormRendererHelper<TreeView> {Renderer = new TreeViewRenderer(treeView1), NoAsync=true};
            pgeHybrid.Tag = new FormRendererHelper<Bitmap> { Renderer = mHybridRenderer, AfterRenderAction = (a,n) => { imgHybrid.Image = a;}};

            cboLayout.Items.Add(new BuchheimLayout());
            cboLayout.Items.Add(new RadialLayout());
            cboLayout.Items.Add(new BottomUpLayout());
            cboLayout.DisplayMember = "Name";

            clDrawStyle.Items.Add(new RenderLines());
            clDrawStyle.Items.Add(new RenderNodes());
            clDrawStyle.Items.Add(new RenderText());
            clDrawStyle.Items.Add(new RenderLinesAlt());        
            clDrawStyle.Items.Add(new RenderSyntax());
            clDrawStyle.Items.Add(new RenderTriangles());
            clDrawStyle.Items.Add(new RenderCircles());
            clDrawStyle.Items.Add(new RenderBackdrop());
            clDrawStyle.Items.Add(new RenderSunburst());
            clDrawStyle.Items.Add(new RenderExpression());

            clDrawStyle.DisplayMember = "Name";

            cboLayout.SelectedIndex = 0;
            clDrawStyle.SetItemChecked(0, true);
            clDrawStyle.SetItemChecked(1, true);
            clDrawStyle.SetItemChecked(2, true);

            mCustomTreeRenderer.Update(()=> new Size(imgTree.PanelWidth,imgTree.PanelHeight), cboLayout, clDrawStyle);
     
        }

        #region Controls

        private void btnEvaluate_Click(object sender, EventArgs e)
        {
            btnEvaluate.Enabled = false;
            btnRestart.Enabled = false;

            if (mCurrentEvaluation == null || mCurrentEvaluation.StarterExpression != txtInput.Text)
                BeginEvaluate();
            else
                ContinueEvaluate();
        }

        private void chkPause_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPause.Checked)
                worker.CancelAsync();
            else
                ContinueEvaluate();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            BeginEvaluate();
        }

        private void tabDisplay_TabIndexChanged(object sender, EventArgs e)
        {
            RenderTree();
        }

        private void tabDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            RenderTree();
        }

        private void tbDisplayedStep_Scroll(object sender, EventArgs e)
        {
            mCurrentEvaluation.DisplayedStep = tbDisplayedStep.Value;
            this.RenderTree();
        }

        #endregion

        #region Worker
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            EvaluationChain evaluation = e.Argument as EvaluationChain;

            int lastStepCount = evaluation.TotalSteps - 1;
            int startingStepCount = evaluation.TotalSteps;

            while(lastStepCount < evaluation.TotalSteps && evaluation.TotalSteps < (startingStepCount + evaluation.StepSize))
            {
                lastStepCount = evaluation.TotalSteps;
                evaluation.AddSteps(1);
                if (worker.CancellationPending)
                    break;

                var progress = ((float)(evaluation.TotalSteps - startingStepCount) / (float)evaluation.StepSize) * 100f;
                worker.ReportProgress((int)progress, evaluation);
            }

            e.Result = evaluation;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var eval = e.UserState as EvaluationChain;
            lblProgress.Text = "Total Steps: " + eval.TotalSteps.ToString();
            progressBar.Value = e.ProgressPercentage;

            tbDisplayedStep.Maximum = eval.TotalSteps;
            tbDisplayedStep.Value = tbDisplayedStep.Maximum;

            RenderTree();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var eval = e.Result as EvaluationChain;
            lblProgress.Text = "Total Steps: " + eval.TotalSteps.ToString();
            progressBar.Value = 100;
            btnEvaluate.Enabled = true;
            btnRestart.Enabled = true;
        }
        #endregion

        #region Evaluation

        private void BeginEvaluate()
        {
            timer1.Enabled = false;
            lblProgress.Text = "Loading libraries...";
            this.Refresh();

            var listItems = new List<string>();
            foreach (var item in txtInput.Items)
                listItems.Add(item.ToString());

            if (!listItems.Contains(txtInput.Text))
            {
                txtInput.Items.Add(txtInput.Text);
                listItems.Add(txtInput.Text);

                System.IO.File.WriteAllLines("input.txt", listItems.Reverse<string>().Take(25).Reverse().ToArray());
            }

            var ctx = new Context(Environment.GetCommandLineArgs().Skip(1).LastOrDefault());
            mContext = ctx;

            ctx.IsImportingLibraries = true;
            ctx.SkipFunctionSelectors = !chkShowSelectors.Checked;
       
            ctx.IsImportingLibraries = false;
            var starterNode = Parser.Parse(txtInput.Text, ctx);            
            mCurrentEvaluation.Reset(ctx, starterNode, txtInput.Text);
            RenderTree();
            btnEvaluate.Enabled = true;
            btnRestart.Enabled = true;
            lblProgress.Text = "Ready";

        }

        private void ContinueEvaluate()
        {
            if (!worker.IsBusy)
            {
                mCurrentEvaluation.StepSize = (int)nbSteps.Value;
                mContext.SkipFunctionSelectors = !chkShowSelectors.Checked;
                worker.RunWorkerAsync(mCurrentEvaluation);
            }
        }

        #endregion

        #region "Tree Rendering"

        private CustomTreeRenderer mCustomTreeRenderer;
        private HybridRenderer mHybridRenderer;

        private void cboLayout_SelectedIndexChanged(object sender, EventArgs e)
        {
            mCustomTreeRenderer.Update(() => new Size(imgTree.PanelWidth, imgTree.PanelHeight), cboLayout, clDrawStyle);
            RenderTree();
        }

        private void clDrawStyle_ItemCheck(object sender, ItemCheckEventArgs e)
        {
          //  lblProgress.Text += "a";
            //mCustomTreeRenderer.Update(imgTree, cboLayout, clDrawStyle);
            //RenderTree();
        }

        private void clDrawStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            mCustomTreeRenderer.Update(() => new Size(imgTree.PanelWidth, imgTree.PanelHeight), cboLayout, clDrawStyle);
            RenderTree();
        }


        #endregion

        #region Rendering


        private IFormRendererHelper ActiveRenderer
        {
            get
            {
                return tabDisplay.SelectedTab.Tag as IFormRendererHelper;
            }
        }

        private bool mAlreadyRendering;
        private void RenderTree()
        {
            if (mCurrentEvaluation.TotalSteps == 0)
                return;

            if (mAlreadyRendering)
                return;

            mAlreadyRendering = true;

            ActiveRenderer.Tree = mCurrentEvaluation.DisplayedNode;

            if (ActiveRenderer.NoAsync)
            {
                ActiveRenderer.CreateRendering();
                ActiveRenderer.DisplayRendering();
                mAlreadyRendering = false;
                return;
            }

            Task<IFormRendererHelper>.Factory.StartNew((rendererObj) =>
            {
                IFormRendererHelper rdr = rendererObj as IFormRendererHelper;
                rdr.CreateRendering();
                return rdr;
            }, this.ActiveRenderer).ContinueWith(task =>
                {
                    task.Result.DisplayRendering();
                    mAlreadyRendering = false;
                }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void RenderTreeNoAsync()
        {
            if (mCurrentEvaluation.TotalSteps == 0)
                return;

            ActiveRenderer.Tree = mCurrentEvaluation.DisplayedNode;

            ActiveRenderer.CreateRendering();
            ActiveRenderer.DisplayRendering();
            mAlreadyRendering = false;
            return;
        }

        #endregion

        #region Render to Disk

        private void btnSaveToDisk_Click(object sender, EventArgs e)
        {            
            btnSaveToDisk.Enabled = false;

            if (tabDisplay.SelectedTab == pgeHybrid)
                saveWorker.RunWorkerAsync(mHybridRenderer);
            else
                saveWorker.RunWorkerAsync(mCustomTreeRenderer);
        }

        private void saveWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string folder = PathHelper.CreateOutputFolder();
            System.IO.Directory.CreateDirectory(folder);

            var renderer = e.Argument as TreeRenderer<Bitmap>;

            for (int i = 0; i < mCurrentEvaluation.TotalSteps; i++)
            {
                var image = renderer.Render(mCurrentEvaluation.Steps[i]);
                image.Save(folder + "\\" + i.ToString("00000") + ".png", System.Drawing.Imaging.ImageFormat.Png);

                saveWorker.ReportProgress((int)(((float)i / (float)mCurrentEvaluation.TotalSteps) * 100));
            }

            e.Result = "Images saved to " + folder; 
        }

        private void saveWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbSaveToDisk.Value = e.ProgressPercentage;
        }

        private void saveWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pbSaveToDisk.Value = 0;
            MessageBox.Show(e.Result.ToString());
            btnSaveToDisk.Enabled = true;   
        }

        #endregion

        private void btnAnimate_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
                timer1.Enabled = false;
            else
            {
                tbDisplayedStep.Value = 0;
                timer1.Interval = Convert.ToInt32(txtAnimFrameDuration.Text);
                timer1.Tag = Convert.ToInt32(txtAnimStep.Text);
                timer1.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tbDisplayedStep.Value >= tbDisplayedStep.Maximum)
                timer1.Enabled = false;
            else
            {
                tbDisplayedStep.Value = Math.Min(tbDisplayedStep.Maximum, tbDisplayedStep.Value + (int)timer1.Tag);
                mCurrentEvaluation.DisplayedStep = tbDisplayedStep.Value;
                RenderTreeNoAsync();
            }
        }

       



    }

    interface IFormRendererHelper
    {
        void CreateRendering();
        void DisplayRendering();
        Node Tree { get;set;}
        bool NoAsync { get; set; }
    }

    class FormRendererHelper<T> : IFormRendererHelper
    {
        private T mRendering;     
        public Node Tree { get; set; }
        public TreeRenderer<T> Renderer;
        public Action<T, Node> AfterRenderAction;
        public bool NoAsync { get; set; }

        public void CreateRendering()
        {
            mRendering = Renderer.Render(Tree);            
        }

        public void DisplayRendering()
        {
            if(AfterRenderAction != null)
                AfterRenderAction(mRendering, Tree);
        }
    }

    class CustomTreeRenderer : TreeRenderer<Bitmap>
    {
        private VisualTreeRenderer mRenderer;

        public CustomTreeRenderer()
        {
        }

        public void Update(Func<Size> getSize, ComboBox mLayoutsDropdown, CheckedListBox mDrawStyleCheckList)
        {
            var layout = mLayoutsDropdown.SelectedItem as ITreeLayout;
            if (layout == null)
                return;

            var steps = mDrawStyleCheckList.CheckedItems.OfType<IRenderingStep>().ToArray();
            mRenderer = new VisualTreeRenderer(getSize, layout, steps);

            layout.SetRenderer(mRenderer);

        }

        protected override Bitmap RenderNode(Node root)
        {
            return mRenderer.Render(root);
        }
    }

}
