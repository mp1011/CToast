using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CToast
{
    public partial class MainForm : Form
    {
        private EvaluationChain mCurrentEvaluation = new EvaluationChain();
        private Context mContext;

        public MainForm()
        {
            InitializeComponent();
            
            if(System.IO.File.Exists("input.txt"))
                txtInput.Text = System.IO.File.ReadAllText("input.txt");
            
            pgeGraph.Tag = new GraphSharpRenderer(graphControl1);
            pgeImage.Tag = new ImageRenderer((n, img) => { mRenderedBitmap = img; pnlImage.Refresh(); });
            pgeTree.Tag = new TreeViewRenderer(treeView1);
            pgeText.Tag = new SyntaxRenderer((n, t) => textBox1.Text = t);
            pgeFile.Tag = new NullRenderer();
            pgeColorTree.Tag = new ColorTreeRenderer((n, img) => { mRenderedBitmap = img; pnlImgColorTree.Refresh(); });
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

            this.RenderTree();
            this.Refresh();
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
            lblProgress.Text = "Loading libraries...";
            this.Refresh();

            System.IO.File.WriteAllText("input.txt", txtInput.Text);
            var ctx = new Context(Environment.CommandLine);
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

        #region Rendering

        private Image mRenderedBitmap;

        private ITreeRenderer ActiveRenderer
        {
            get
            {
                return tabDisplay.SelectedTab.Tag as ITreeRenderer;
            }
        }

        private void RenderTree()
        {
            if (mCurrentEvaluation.TotalSteps == 0)
                return;

            ActiveRenderer.Render(mCurrentEvaluation.DisplayedNode);
        }

        private void pnlImgColorTree_Paint(object sender, PaintEventArgs e)
        {
            PaintPanel(pnlImgColorTree,e.Graphics);
        }

        private void pnlImage_Paint(object sender, PaintEventArgs e)
        {
            PaintPanel(pnlImage, e.Graphics);
        }

        private void PaintPanel(Panel panel, Graphics g)
        {
            if(mRenderedBitmap == null)
                return;

            var ratio = (float)mRenderedBitmap.Width / (float)mRenderedBitmap.Height ;
            var destRec = new Rectangle(0, 0, (int)(pnlImage.Height * ratio), pnlImage.Height);
            destRec.X = (pnlImage.Width - destRec.Width) / 2;

            if (destRec.Width > pnlImage.Width)
            {
                ratio = (float)mRenderedBitmap.Height / (float)mRenderedBitmap.Width;
                destRec = new Rectangle(0, 0, pnlImage.Width, (int)(pnlImage.Width * ratio));             
            }

            g.DrawImage(mRenderedBitmap, destRec);
        }

        private void btnRenderColorTrees_Click(object sender, EventArgs e)
        {
            RenderToFiles(TreePainter_v3_1.TreeRender.RenderTreesAsColorTrees);
        }

        private void btnRenderTextTrees_Click(object sender, EventArgs e)
        {
            RenderToFiles(TreePainter_v3_1.TreeRender.RenderTreesAsTextTrees);
        }

        private void RenderToFiles(Func<TreeView[],IEnumerable<Bitmap>> paintFunction)
        {
            string folder = PathHelper.CreateOutputFolder();
            System.IO.Directory.CreateDirectory(folder);

            var treeRenderer = new TreeViewRendererAlternate(null);
            var trees = mCurrentEvaluation.Steps.Select(p=> treeRenderer.Render(p)).ToArray();

            int i = 0;
            foreach (var image in paintFunction(trees))
                image.Save(folder + "\\" + (++i).ToString("00000") + ".png", System.Drawing.Imaging.ImageFormat.Png);

        }


        #endregion

     
 





    }
}
