using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    public static class ListHelper
    {
        public static Node CreateList(IEnumerable<Node> items)
        {
            if(items.FirstOrDefault() == null)
                return new OperatorNode(new CommaOperator(), null,null);
            else if(items.Count() == 1)
                return new OperatorNode(new CommaOperator(), items.FirstOrDefault(), null);
            else 
                return new OperatorNode(new CommaOperator(), items.FirstOrDefault(), CreateList(items.Skip(1)));
        }

        public static Node EnsureList2(Node node, bool assumeListCompleted)
        {
            return EnsureList(node, assumeListCompleted).NewNode;
        }

        public static Evaluation EnsureList(Node node, bool assumeListCompleted)
        {
            if (node == null)
                return Evaluation.Changed(CreateList(new Node[] { }));

            Evaluation list = null;

            var op = node.TypedValue<Operator>(null);
            if (op == null || !(op is CommaOperator))
                list = Evaluation.Changed(CreateList(new Node[] { node }));
            else
                list = Evaluation.Unchanged(node);

            if (assumeListCompleted)
            {
                var listEval = VerifyList(list.NewNode);

                if (list.Result == EvaluationResult.Changed)
                    return Evaluation.Changed(listEval.NewNode);
                else
                    return listEval;
            }
            else
                return list;

        }

        private static Evaluation VerifyList(Node root)
        {
            var op = root.RightNode.NotNull().TypedValue<CommaOperator>(null);
            if (op != null)
            {
                var rightEval = VerifyList(root.RightNode);

                if (rightEval.Result == EvaluationResult.Changed)
                    return Evaluation.Changed(root.Copy(root.LeftNode, rightEval.NewNode));
                else
                    return Evaluation.Unchanged(root);
            }
            else if (root.RightNode == null)
                return Evaluation.Unchanged(root);

            var rightEval2 = EnsureList(root.RightNode, false);
            if (rightEval2.Result == EvaluationResult.Changed)
                return Evaluation.Changed(root.Copy(root.LeftNode, rightEval2.NewNode));
            else
                return Evaluation.Unchanged(root);

        }

        public static bool IsValidList(Node node)
        {
            if(IsEmptyList(node))
                return true;

            var op = node.TypedValue<Operator>(null);
            if (op is ListOperator || op is OpenBracketOperator)
                return IsValidList(node.RightNode);
            else
                return false;
        }

        public static bool IsEmptyList(Node node)
        {
            return node == null || node.IsEmptyList;
        }

        public static IEnumerable<Node> GetNodeList(OperatorNode root)
        {
            if (root != null && (root.LeftNode != null || root.RightNode != null))
            {
                yield return root.LeftNode;

                var opNode = root.RightNode as OperatorNode;
                if (opNode != null && opNode.Op is CommaOperator)
                {
                    foreach (var item in GetNodeList(opNode))
                        yield return item;
                }
                else if (root.RightNode != null)
                    yield return root.RightNode;
            }
        }

        public static Node[] GetNodeList(Node root)
        {
            return GetNodeList(root as OperatorNode).ToArray();
        }
    }

    public abstract class ListOperator : BinaryOperator 
    {
        protected ListOperator(string symbol) : base(symbol, Associativity.Right, Precedence.Comma) { }
      
        public override ValidationResult Validate(Node node)
        {
            // left side can not be a function declaration, if so it probably should have been a function call
            if (node.LeftNode != null)
            {
                var leftOp = node.LeftNode.TypedValue<Operator>(null);
                if (leftOp != null && leftOp is FunctionDeclarationOperator)
                    return ValidationResult.RedoLeft;
            }

            // this doesn't seem right
            if (node.RightNode != null)
            {
                var rightOp = node.RightNode.TypedValue<Operator>(null);
                if (rightOp != null && rightOp is FunctionDeclarationOperator)
                    return ValidationResult.RedoRight;
            }


            return base.Validate(node);
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            if (!IsValidNode(node))
            {
                if (node.RightNode.IsEmptyList)
                    return Evaluation.Changed(node.CopyAllowNull(node.LeftNode, null));

                var commaOp = new OperatorNode(new CommaOperator(), node.RightNode, null);
                OperatorNode replacement = new OperatorNode(new CommaOperator(), node.LeftNode, commaOp);
                return Evaluation.Changed(replacement);
            }

            return StepEvaluateEx(node, context);
        }

        protected bool IsValidNode(Node node)
        {
            // right node must be either another comma, a function, or nothing
            if (node == null)
                return true;

            Node rightTree = node.RightNode;
            if (rightTree == null)
                return true;

            OperatorNode commaOp = OperatorNode.TryCast<CommaOperator>(rightTree);
            if (commaOp != null)
                return true;

            OperatorNode concatOp = OperatorNode.TryCast<ConcatOperator>(rightTree);
            if (concatOp != null)
                return true;
      
            OperatorNode tailOp = OperatorNode.TryCast<TailOperator>(rightTree);
            if (tailOp != null)
                return true;

            FunctionSelectorNode funcSel = rightTree as FunctionSelectorNode;
            if (funcSel != null)
                return true;

            return IsValidNodeEx(node);
        }

        protected virtual bool IsValidNodeEx(Node node)
        {
            return false;
        }

        protected virtual Evaluation StepEvaluateEx(Node node, Context context)
        {
            return Evaluation.Unchanged(node);
        }
    }

    public class CommaOperator : ListOperator
    {
        public CommaOperator()
            : base(",")
        {

        }

        internal override Tuple<Node, Node> InitializeNode(Node root)
        {
            if (root.RightNode == null)
                return null;

            Operator o = root.RightNode.Value as Operator;
            if (o != null && o.Precedence == Precedence.Comma)
                return null;

            if (o != null && o is FunctionCallOperator)
                return null;

            if (o != null && o is TailOperator)
                return null;

            var newRight = new OperatorNode(new CommaOperator(), root.RightNode, null);
            return new Tuple<Node, Node>(root.LeftNode, newRight);

        }
    }

    public class ConcatOperator : ListOperator
    {
        public ConcatOperator()
            : base("&")
        {

        }

        internal override Tuple<Node, Node> InitializeNode(Node root)
        {
            return new Tuple<Node, Node>(root.LeftNode, root.RightNode);

        }

        protected override Evaluation StepEvaluateEx(Node node, Context context)
        {
            bool leftList = ListHelper.IsValidList(node.LeftNode);
            bool rightList = ListHelper.IsValidList(node.RightNode);

            bool leftEmpty = ListHelper.IsEmptyList(node.LeftNode);
            bool rightEmpty = ListHelper.IsEmptyList(node.RightNode);

            if (leftEmpty && rightEmpty)
                return Evaluation.Unchanged(node);
            else if (leftEmpty && rightList)
                return Evaluation.Changed(node.RightNode);
            else if (leftList && rightEmpty)
                return Evaluation.Changed(node.LeftNode);
            else if (leftList && rightList)
                return Evaluation.Changed(AppendList(node.LeftNode, node.RightNode));
            else
            {
                if (rightList && node.LeftNode.IsAtomic)
                    return Evaluation.Changed(new OperatorNode(new CommaOperator(), node.LeftNode, node.RightNode));

                Node newLeft=null,newRight=null;
                if (node.LeftNode.TypedValue<Operator>(null) is OpenBracketOperator)
                    newLeft = node.LeftNode.RightNode;
                if (node.RightNode.TypedValue<Operator>(null) is OpenBracketOperator)
                    newRight = node.RightNode.RightNode;

                if(newLeft != null || newRight != null)
                    return Evaluation.Changed(node.Copy(newLeft,newRight));
                else                  
                    return Evaluation.Unchanged(node);
            }
        }

        private Node AppendList(Node leftList, Node rightList)
        {           
            if (leftList.RightNode == null)
                return leftList.Copy(leftList.LeftNode, rightList);
            else
                return leftList.Copy(leftList.LeftNode, AppendList(leftList.RightNode, rightList));
           
        }

        protected override bool IsValidNodeEx(Node node)
        {
            Node rightTree = node.RightNode;
            if (rightTree == null)
                return true;

            OperatorNode funcOp = OperatorNode.TryCast<FunctionCallOperator>(rightTree);
            if (funcOp != null)
                return true;

            if (rightTree.TypedValue<Operator>(null) is OpenBracketOperator)
                return true;

            return false;
        }
    }

    public abstract class ListManipulationOperator : Operator
    {

        protected ListManipulationOperator(string label, string displayLabel, Precedence precedence) : base(label, displayLabel, Associativity.Right, precedence) { }


        protected override OperatorOccurrence CreateOccurrence(string expression, int expressionPosition, int index)
        {
            return new OperatorOccurrence(expression, index, expressionPosition, this);
        }

        //public override Evaluation PostEvaluate(Node node, Context context)
        //{
        //    //return StepEvaluate(node, context);
        //} 

        public sealed override Evaluation StepEvaluate(Node node, Context context)
        {
            if (node.RightNode == null)
                return Evaluation.Changed(new NullNode());

            var unpackResult = OpenBracketOperator.TryUnpack(node.RightNode, context);
            if (unpackResult.Result == EvaluationResult.Changed)
                return Evaluation.Changed(node.Copy(node.LeftNode, unpackResult.NewNode));

            return StepEvaluateOperator(node, context);
        }

        protected abstract Evaluation StepEvaluateOperator(Node node, Context context);
    }

    public class HeadOperator : ListManipulationOperator
    {
        public HeadOperator() : base("h:", "head", Precedence.Head) { }

        protected override Evaluation StepEvaluateOperator(Node node, Context context)
        {
            var list = node.RightNode.TypedValue<ListOperator>(null);
            if (list == null && node.RightNode.IsAtomic)
                return Evaluation.Changed(node.RightNode);
            else if (list == null)
                return Evaluation.Unchanged(node);
            else
                return Evaluation.Changed(node.RightNode.LeftNode);
        }
    }

    public class TailOperator : ListManipulationOperator
    {
        public TailOperator() : base("t:", "tail", Precedence.Head) { }

        protected override Evaluation StepEvaluateOperator(Node node, Context context)
        {
            var list = node.RightNode.TypedValue<ListOperator>(null);
            if (list == null && node.RightNode.IsAtomic)
                return Evaluation.Changed(new NullNode());
            else if (list == null)
                return Evaluation.Unchanged(node);
            else if (node.RightNode.RightNode == null)
                return Evaluation.Changed(new NullNode());
            else
                return Evaluation.Changed(node.RightNode.RightNode);
        }
    }

}
