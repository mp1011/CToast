using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{

    public abstract class Node
    {
     
        private Node mLeftNode, mRightNode;
        private object mValue;

        public ulong Id { get; private set; }

        public virtual object Value { get { return mValue; } protected set { mValue = value; } }
        public Node LeftNode { get { return mLeftNode; } protected set { mLeftNode = value; TreeTest.Run(this); } }
        public Node RightNode { get { return mRightNode; } protected set { mRightNode = value; TreeTest.Run(this); } }

        public bool IsAtomic { get { return mLeftNode == null && mRightNode == null; } }
        
        public bool IsEmptyList
        {
            get
            {
                var op = this.TypedValue<Operator>(null);
                if(op is ListOperator || op is OpenBracketOperator)
                    return ListHelper.GetNodeList(this).Length == 0;
                if (this is NullNode)
                    return true;

                return false;
            }
        }

        private int mDepthUnsafe = -1;
        /// <summary>
        /// Returns the depth of the tree at this node. This value is cached and not recalculated.
        /// </summary>
        public int DepthUnsafe
        {
            get
            {
                if (mDepthUnsafe == -1)
                {
                    int depth = 1;
                    if (this.LeftNode != null && this.RightNode != null)
                        depth += Math.Max(this.LeftNode.DepthUnsafe, this.RightNode.DepthUnsafe);
                    else if (this.LeftNode != null)
                        depth += this.LeftNode.DepthUnsafe;
                    else if (this.RightNode != null)
                        depth += this.RightNode.DepthUnsafe;

                    mDepthUnsafe = depth;
                }

                return mDepthUnsafe;
            }
        }

        private int mTreeeSizeUnsafe = -1;
        /// <summary>
        /// Returns the total number of child nodes. This value is cached and not recalculated so it will be wrong if this tree has changed.
        /// </summary>
        public int TreeeSizeUnsafe
        {
            get
            {
                if (mTreeeSizeUnsafe == -1)
                {
                    mTreeeSizeUnsafe = 1;
                    if (this.LeftNode != null)
                        mTreeeSizeUnsafe += this.LeftNode.TreeeSizeUnsafe;
                    if (this.RightNode != null)
                        mTreeeSizeUnsafe +=  this.RightNode.TreeeSizeUnsafe;
                }

                return mTreeeSizeUnsafe;
            }
        }

        protected Node() 
        {
            Id = IdGenerator.NextId();
        }

        protected Node(object value, Node left, Node right)
        {
            Id = IdGenerator.NextId();
            mValue = value;
            this.LeftNode = left;
            this.RightNode = right;
        }

        public override string ToString()
        {
            if (mValue == null)
                return "[]";
            else
                return mValue.ToString();
        }

        public bool ValueEquals(Node other, bool requireAtomic)
        {
            if (this.IsEmptyList)
                return other.IsEmptyList;
            if (other != null && other.IsEmptyList)
                return this.IsEmptyList;

            if (requireAtomic)
            {
                if (other == null || !other.IsAtomic || !this.IsAtomic || this.Value == null || other.Value == null)
                    return false;
            }
            else
            {
                if (other == null || this.Value == null || other.Value == null)
                    return false;
            }

            return this.Value.Equals(other.Value);           
        }

       /// <summary>
       /// Returns true if all nodes in the two trees have the same value. This does NOT mean that the two trees evaluate to the same value. 
       /// 2+2 is not "exactly equal" to 4, only 4 is exactly equal to 4.
       /// </summary>
       /// <param name="other"></param>
       /// <returns></returns>
        public bool TreeEqualsExact(Node other)
        {
            if (other == null)
                return false;

            if (!this.ValueEquals(other,false))
                return false;

            var leftMatch = (this.LeftNode == null && other.LeftNode == null) || (this.LeftNode != null && this.LeftNode.TreeEqualsExact(other.LeftNode));
            var rightMatch = (this.RightNode == null && other.RightNode == null) || (this.RightNode != null && this.RightNode.TreeEqualsExact(other.RightNode));

            return leftMatch && rightMatch;
        }

        public bool HasTypedValue<T>()
        {
            return this.Value is T;           
        }

        public T TypedValue<T>(T defaultValue)
        {
            try
            {
                if (this.Value is T)
                {
                    T thisVal = (T)this.Value;
                    return thisVal;
                }
                return defaultValue;
            }
            catch 
            {
                return defaultValue;
            }

        }

        protected virtual Evaluation PostEvaluate(Context context)
        {
            return Evaluation.Unchanged(this);
        }

        public Evaluation PostEvaluateNodeAndChildren(Context context)
        {
            Evaluation thisEval = null, leftEval = null, rightEval = null;

            thisEval = this.PostEvaluate(context);
            if(thisEval.Result == EvaluationResult.Changed)
                return thisEval;

            if (LeftNode != null)
            {
                leftEval = this.LeftNode.PostEvaluateNodeAndChildren(context);
                this.LeftNode = leftEval.NewNode;
            }
            else
                leftEval = Evaluation.Unchanged(null);

            if (RightNode != null)
            {
                rightEval = this.RightNode.PostEvaluateNodeAndChildren(context);
                this.RightNode = rightEval.NewNode;
            }
            else
                rightEval = Evaluation.Unchanged(null);

            if (leftEval.Result == EvaluationResult.Changed || rightEval.Result == EvaluationResult.Changed)
                return Evaluation.Changed(this);
            else
                return Evaluation.Unchanged(this);            
        }

         /// <summary>
        /// Performs a single step of evaluation. Returns null if no evaluation was possible.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Evaluation StepEvaluateNodeAndChildrenWithPostEvaluate(Context context)
        {
            var result = StepEvaluateNodeAndChildren(context);
            var node = result.NewNode;

            if (node == null)
                return Evaluation.Changed(new NullNode());

            Evaluation postResult = null;

            while (true)
            {
                postResult = node.PostEvaluateNodeAndChildren(context);
                if (postResult.Result == EvaluationResult.Unchanged)
                    break;

                node = postResult.NewNode;
            }
            return new Evaluation(result.Result, postResult.NewNode);
        }

        /// <summary>
        /// Performs a single step of evaluation. Returns null if no evaluation was possible.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Evaluation StepEvaluateNodeAndChildren(Context context)
        {
            var thisEval = this.StepEvaluateNode(context);

            if (thisEval.NewNode != null)
                DiagnosticUtil.Trace(context,"StepEvaluate", this.ToString(), thisEval.Result.ToString(), thisEval.NewNode.ToString());
            else
                DiagnosticUtil.Trace(context, "StepEvaluate", this.ToString(), thisEval.Result.ToString(), "{{null}}");

            if (thisEval.Result == EvaluationResult.ChangeContext)
                context = thisEval.NewContext;            
            else if (thisEval.Result != EvaluationResult.Unchanged)
                return thisEval;

            Evaluation leftEval=null, rightEval=null;

            if (this.LeftNode != null && !thisEval.SkipLeftNode)            
                leftEval = LeftNode.StepEvaluateNodeAndChildren(context);

            if (this.RightNode != null && !thisEval.SkipRightNode)            
                rightEval = RightNode.StepEvaluateNodeAndChildren(context);

            if ((leftEval != null && leftEval.Result == EvaluationResult.Changed) || (rightEval != null && rightEval.Result == EvaluationResult.Changed))
            {
                var left = this.LeftNode; var right = this.RightNode;

                if ((leftEval != null && leftEval.Result == EvaluationResult.Changed))
                    left = leftEval.NewNode;
                else if ((rightEval != null && rightEval.Result == EvaluationResult.Changed))
                    right = rightEval.NewNode;

                return Evaluation.Changed(this.Copy(left,right));

            }


            return Evaluation.Unchanged(this);
        }

        protected virtual Evaluation StepEvaluateNode(Context context)
        {
            return Evaluation.Unchanged(this);
        }

        public Node EvaluateFull(Context context)
        {
            var node = this;
            var eval = node.StepEvaluateNodeAndChildren(context);

            while (eval.Result == EvaluationResult.Changed)
            {
                node = eval.NewNode;
                eval = node.StepEvaluateNodeAndChildren(context);
            }

            return node;
        }

        public Node ReplaceNodes(Func<Node, Node> transformation)
        {
            var newRoot = transformation(this);
            
            if(this.LeftNode != null)
                newRoot.LeftNode = this.LeftNode.ReplaceNodes(transformation);

            if(this.RightNode != null)
                newRoot.RightNode = this.RightNode.ReplaceNodes(transformation);

            return newRoot;
        }

        //todo better naming or consolidate with function above
        public Node ReplaceNodes2(Func<Node, ReplaceResult> transformation)
        {
            var result = transformation(this);

            var newRoot = result.Result;
            if (result.ReplaceChildren)
            {
                if (this.LeftNode != null)
                    newRoot.LeftNode = newRoot.LeftNode.ReplaceNodes2(transformation);

                if (this.RightNode != null)
                    newRoot.RightNode = newRoot.RightNode.ReplaceNodes2(transformation);
            }

            return newRoot;
        }

        public IEnumerable<Node> Traverse(Traversal traversal, bool includeThis)
        {
            if (includeThis)
                yield return this;

            switch (traversal)
            {
                case Traversal.RightOnly:
                    yield return (this.RightNode);
                    if (this.RightNode != null)
                    {
                        foreach (var child in this.RightNode.Traverse(traversal, false))
                            yield return (child);
                    }
                    break;
                case Traversal.Preorder:
                    if (this.LeftNode != null)
                    {
                        yield return this.LeftNode;
                        foreach (var child in this.LeftNode.Traverse(traversal, false))
                            yield return (child);
                    }

                    if (this.RightNode != null)
                    {
                        yield return this.RightNode;
                        foreach (var child in this.RightNode.Traverse(traversal, false))
                            yield return (child);
                    }
                    break;
                case Traversal.LeftRoots:

                    if(this.LeftNode != null)
                        yield return (this.LeftNode);

                    if (this.RightNode != null)
                    {
                        foreach (var child in this.RightNode.Traverse(traversal, false))
                            yield return (child);
                    }
                    break;
            }
        }

        public Node Copy()
        {
            var copy = this.CopyInner();
            copy.mValue = this.CopyValue();

            if (this.LeftNode != null)
                copy.LeftNode = this.LeftNode.Copy();

            if (this.RightNode != null)
                copy.RightNode = this.RightNode.Copy();

            return copy;
        }

        public Node Copy(Node newLeft, Node newRight)
        {
            var copy = this.CopyInner();
            copy.mValue = this.CopyValue();

            if (this.LeftNode != null || newLeft != null)
                copy.LeftNode = newLeft ?? this.LeftNode.Copy();

            if (this.RightNode != null || newRight != null)
                copy.RightNode = newRight ?? this.RightNode.Copy();

            return copy;
        }

        public Node CopyAllowNull(Node newLeft, Node newRight)
        {
            var copy = this.CopyInner();
            copy.mValue = this.CopyValue();

            if (this.LeftNode != null)
                copy.LeftNode = newLeft;

            if (this.RightNode != null)
                copy.RightNode = newRight;

            return copy;
        }

        protected abstract Node CopyInner();

        protected virtual object CopyValue()
        {
            return this.Value;
        }

        public int ComputeTreeSize()
        {
            int l = 0, r = 0;
            if (LeftNode != null)
                l = LeftNode.ComputeTreeSize();
            if (RightNode != null)
                r = RightNode.ComputeTreeSize();

            return 1 + l + r;
        }
    }

    public class LiteralNode : Node
    {     
        public LiteralNode(object value)
        {
            this.Value = value;
        }

        protected override Evaluation StepEvaluateNode(Context context)
        {
            return Evaluation.Unchanged(this);
        }

        protected override Node CopyInner()
        {
            return new LiteralNode(Value);
        }
    }

    public class LiteralNode<T> : Node 
    {
        public T ValueStrict { get { return this.Value == null ? default(T) : (T)this.Value; } }

        public LiteralNode(T value)
        {
            this.Value = value;
        }

        protected override Evaluation StepEvaluateNode(Context context)
        {
            return Evaluation.Unchanged(this);
        }

        protected override Node CopyInner()
        {
            return new LiteralNode<T>((T)Value);
        }
    }

    class SystemMessageNode : Node
    {
        public SystemMessageNode(Exception ex)
        {
            this.Value = "ERROR: " + ex.Message;
        }

        public SystemMessageNode(string message)
        {
            this.Value = message;
        }

        protected override Evaluation StepEvaluateNode(Context context)
        {
            return Evaluation.Unchanged(this);
        }

        public override object Value { get { return "<<" + base.Value + ">>"; } }

        protected override Node CopyInner()
        {
            return new SystemMessageNode(this.Value.ToString());
        }
    }

    public class OperatorNode : Node
    {
        private OperatorOccurrence mOccurrence;

        public Operator Op
        {
            get
            {
                if (mOccurrence == null)
                {
                    return this.Value as Operator;
                }

                return mOccurrence.Operator;
            }
        }

        //todo, can we get rid of?
        public OperatorOccurrence Occ { get { return mOccurrence; } }


        public OperatorNode(OperatorOccurrence op, Node leftTree, Node rightTree) : base(op.Operator,leftTree,rightTree)
        {
            mOccurrence = op;
            
            var newNodes = op.Operator.InitializeNode(this);
            if (newNodes != null)
            {
                this.LeftNode = newNodes.Item1;
                this.RightNode = newNodes.Item2;
            }
        }

        public OperatorNode(Operator op, Node leftTree, Node rightTree)
            : base(op, leftTree, rightTree)
        {
            mOccurrence = null;
        }

        public ValidationResult Validate()
        {
            if (mOccurrence == null)
                return ValidationResult.Valid;
            else
                return mOccurrence.Operator.Validate(this);
        }

        protected override Evaluation PostEvaluate(Context context)
        {
            Operator op = this.Value as Operator;
            return op.PostEvaluate(this, context) ?? Evaluation.Unchanged(this);
        }

        protected override Evaluation StepEvaluateNode(Context context)
        {
            Operator op = this.Value as Operator;
            var result = op.StepEvaluate(this,context) ?? Evaluation.Unchanged(this);

            if (result.Result == EvaluationResult.IllegalOperation)            
                return Evaluation.Changed(this.Reparse(context));
            
            return result;
        }

        public Node Reparse(Context context)
        {
            if (mOccurrence == null)
                return new SystemMessageNode(new Exception("Illegal Operation"));

            context.AddIllegalOperation(mOccurrence);

            Node newNode = Parser.Parse(mOccurrence.OriginalExpression, context);
            return newNode;
        }

        //todo, don't particularly like this
        public void UpdateLeft(Node node)
        {
            this.LeftNode = node;
        }

        public override string ToString()
        {
            return (this.Value as Operator).DisplayLabel;
            //return this.GetSourceExpression();
        }

        public static OperatorNode TryCast<T>(Node node) where T : Operator
        {
            OperatorNode o = node as OperatorNode;
            if (o == null)
                return null;

            if ((o.Value as Operator).GetType() == typeof(T))
                return o;

            return null;
        }

        protected override Node CopyInner()
        {
            var copy = new OperatorNode(this.Value as Operator, null, null);
            copy.mOccurrence = this.mOccurrence;
            return copy;
        }
    }

    class NullNode : Node
    {
        public override string ToString()
        {
            return "[]";
        }

        protected override Node CopyInner()
        {
            return new NullNode();
        }
    }
   

}
