using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    class EqualityOperator : BinaryOperator
    {

        public EqualityOperator()
            : base("=", "==", Associativity.Left, Precedence.Equals)
        {
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            Node leftTree = node.LeftNode;
            Node rightTree = node.RightNode;

            var equality = NodesEqual(leftTree, rightTree);
            if (equality.HasValue)
                return Evaluation.ChangedLiteral(equality.Value);
            else
                return Evaluation.Unchanged(node);
        }

        private bool? NodesEqual(Node node1, Node node2)
        {
            if (node1 != null && node1.TypedValue<Operator>(null) is OpenBracketOperator)
                return NodesEqual(node1.LeftNode, node2);
            else if (node2 != null && node2.TypedValue<Operator>(null) is OpenBracketOperator)
                return NodesEqual(node1, node2.LeftNode);
            return NodesEqualX(node1, node2) ?? NodesEqualX(node2, node1);
        }

        private bool? NodesEqualX(Node node1, Node node2)
        {
            if (node1.IsNullOrEmpty())
            {
                if (node2.IsNullOrEmpty())
                    return true;
                else if (node2.IsAtomic)
                    return false;
                else
                    return null;
            }

            if (node1.IsAtomic)
            {
                if (node2.IsNullOrEmpty())
                    return false;
                else if (node2.IsAtomic)
                    return node1.ValueEquals(node2, true);
                else
                    return null;
            }

            if (ListHelper.IsValidList(node1))
            {
                if (ListHelper.IsValidList(node2))
                {
                    var list1 = ListHelper.GetNodeList(node1);
                    var list2 = ListHelper.GetNodeList(node2);

                    for (int i = 0; i < Math.Max(list1.Length, list2.Length); i++)
                    {
                        Node lNode1 = null, lNode2 = null;
                        if (i < list1.Length)
                            lNode1 = list1[i];
                        if (i < list2.Length)
                            lNode2 = list2[i];
                        var result = NodesEqual(lNode1, lNode2);
                        if (!result.HasValue)
                            return null;
                        else if (!result.Value)
                            return false;
                    }

                    return true;
                }
                else
                    return null;
            }

            return null;
        }
    }

    class LessThanOperator : BinaryOperator
    {

        public LessThanOperator()
            : base("<", Associativity.Left, Precedence.Comparison)
        {
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            Node leftTree = node.LeftNode;
            Node rightTree = node.RightNode;

            double? num1 = MathHelper.GetNumber(leftTree);
            double? num2 = MathHelper.GetNumber(rightTree);

            if (num1.HasValue && num2.HasValue)
            {
                var result = num1.Value < num2.Value;
                return Evaluation.Changed(new LiteralNode<bool>(result));
            }
            else
                return Evaluation.Unchanged(node);
        }
    }

    class GreaterThanOperator : BinaryOperator
    {

        public GreaterThanOperator()
            : base(">", Associativity.Left, Precedence.Comparison)
        {
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            Node leftTree = node.LeftNode;
            Node rightTree = node.RightNode;

            double? num1 = MathHelper.GetNumber(leftTree);
            double? num2 = MathHelper.GetNumber(rightTree);

            if (num1.HasValue && num2.HasValue)
            {
                var result = num1.Value > num2.Value;
                return Evaluation.Changed(new LiteralNode<bool>(result));
            }
            else
                return Evaluation.Unchanged(node);
        }
    }

}
