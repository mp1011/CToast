using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    public class EvaluationChain 
    {
        private LinkedList<Node> mEvaluations = new LinkedList<Node>();
        private Context mContext;

        public string StarterExpression { get; private set; }

        private int mDisplayedStep;
        public int DisplayedStep
        {
            get
            {
                if (mDisplayedStep < 1)
                    return 1;
                if (mDisplayedStep > mEvaluations.Count)
                    return mEvaluations.Count;
                return mDisplayedStep;
            }
            set
            {
                mDisplayedStep = value;
            }
        }

        public int TotalSteps { get { return mEvaluations.Count; } }

        public Node[] Steps
        {
            get
            {
                return mEvaluations.ToArray();
            }
        }

        public Node DisplayedNode
        {
            get
            {
                try
                {
                    int d = this.DisplayedStep;
                    var temp = mEvaluations.ToArray();
                    if (d >= temp.Length)
                        return temp[temp.Length - 1];
                    else
                        return temp[d - 1];
                }
                catch (Exception ex)
                {
                    return mEvaluations.Last.Value;
                }
            }
        }

        public int StepSize { get; set; }

        public EvaluationChain() { }

        public void Reset(Context context, Node first, string starterExpression)
        {
            this.StarterExpression = starterExpression;
            this.DisplayedStep = 0;
            this.mContext = context;
            mEvaluations.Clear();
            mEvaluations.AddLast(first);
        }


        public Node[] AddSteps(int maxSteps)
        {
            return AddStepsEx(maxSteps).ToArray();
        }

        private IEnumerable<Node> AddStepsEx(int maxSteps)
        {
            if (mEvaluations.Count > 0)
            {
                while (--maxSteps >= 0)
                {
                    var result =  mEvaluations.Last.Value.Copy().StepEvaluateNodeAndChildrenWithPostEvaluate(this.mContext);
                    if (result.NewNode == null)
                        break;

                    if(result.Result == EvaluationResult.Changed)
                        mEvaluations.AddLast(result.NewNode.Copy());

                    yield return result.NewNode;

                    if (result.Result == EvaluationResult.Unchanged)
                        break;
                }

                this.DisplayedStep = mEvaluations.Count;
            }
        }

    }
}
