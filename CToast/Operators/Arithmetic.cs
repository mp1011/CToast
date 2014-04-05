using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    /// <summary>
    /// stuff to do, make int/double better
    /// </summary>
    static class MathHelper
    {
        public static Int64[] GetInts(Node leftNode, Node rightNode)
        {
            Int64 left, right;

            if (leftNode != null && leftNode.Value is Int64)
                left = Convert.ToInt64(leftNode.Value);
            else
                return null;

            if (rightNode != null && rightNode.Value is Int64)
                right = Convert.ToInt64(rightNode.Value);
            else
                return null;

            return new Int64[] { left, right };
        }

        public static double[] GetDoubles(Node leftNode, Node rightNode)
        {
            double left, right;

            if (leftNode != null && (leftNode.Value is Int64 || leftNode.Value is double))
                left = Convert.ToDouble(leftNode.Value);
            else
                return null;

            if (rightNode != null && (rightNode.Value is Int64 || rightNode.Value is double))
                right = Convert.ToDouble(rightNode.Value);
            else 
                return null;
    
            return new double[] { left, right };
        }

        public static double? GetNumber(Node node)
        {
            if (node != null && (node.Value is Int64 || node.Value is double))
                return Convert.ToDouble(node.Value);
            else
                return null;
        }
    }

    public class PlusOperator : BinaryOperator 
    {
        public PlusOperator() : base("+", Associativity.Left, Precedence.Add_Sub)
        {             
        }

        public override Evaluation PostEvaluate(Node node, Context context)
        {
            if(context.PostEvalMathNodes)
                return StepEvaluate(node, context);
            else
                return null;
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            Node leftTree = node.LeftNode;
            Node rightTree = node.RightNode;

            Int64[] ints = MathHelper.GetInts(leftTree, rightTree);
            if (ints != null)
                return Evaluation.Changed(new LiteralNode<Int64>(ints[0] + ints[1]));

            double[] dbls = MathHelper.GetDoubles(leftTree, rightTree);
            if(dbls != null)
                return Evaluation.Changed(new LiteralNode<double>(dbls[0] + dbls[1]));

            return null;
        }
    }

    public class MinusOperator : BinaryOperator
    {
        public MinusOperator()
            : base("-", Associativity.Left, Precedence.Add_Sub)
        {
        }

        public override Evaluation PostEvaluate(Node node, Context context)
        {
            if (context.PostEvalMathNodes)
                return StepEvaluate(node, context);
            else
                return null;
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            Node leftTree = node.LeftNode;
            Node rightTree = node.RightNode;

            Int64[] ints = MathHelper.GetInts(leftTree, rightTree);
            if (ints != null)
                return Evaluation.Changed(new LiteralNode<Int64>(ints[0] - ints[1]));

            double[] dbls = MathHelper.GetDoubles(leftTree, rightTree);
            if (dbls != null)
                return Evaluation.Changed(new LiteralNode<double>(dbls[0] - dbls[1]));

            return null;
        }
    }

    public class TimesOperator : BinaryOperator
    {
        public TimesOperator()
            : base("*", Associativity.Left, Precedence.Multiply_Divide)
        {
        }

        public override Evaluation PostEvaluate(Node node, Context context)
        {
            if (context.PostEvalMathNodes)
                return StepEvaluate(node, context);
            else
                return null;
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            Node leftTree = node.LeftNode;
            Node rightTree = node.RightNode;

            if (MathHelper.GetNumber(leftTree).Equals(0) || MathHelper.GetNumber(rightTree)==0)
                return Evaluation.Changed(new LiteralNode<Int64>(0));

            Int64[] ints = MathHelper.GetInts(leftTree, rightTree);
            if (ints != null)
                return Evaluation.Changed(new LiteralNode<Int64>(ints[0] * ints[1]));

            double[] dbls = MathHelper.GetDoubles(leftTree, rightTree);
            if (dbls != null)
                return Evaluation.Changed(new LiteralNode<double>(dbls[0] * dbls[1]));

            return null;
        }
    }

    public class DivisionOperator : BinaryOperator
    {
        public DivisionOperator()
            : base("/", Associativity.Left, Precedence.Multiply_Divide)
        {
        }

        public override Evaluation PostEvaluate(Node node, Context context)
        {
            if (context.PostEvalMathNodes)
                return StepEvaluate(node, context);
            else
                return null;
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            Node leftTree = node.LeftNode;
            Node rightTree = node.RightNode;

            if (MathHelper.GetNumber(rightTree)==0)
                return Evaluation.Changed(new SystemMessageNode(new DivideByZeroException()));

            Int64[] ints = MathHelper.GetInts(leftTree, rightTree);
            if (ints != null)
                return Evaluation.Changed(new LiteralNode<Int64>(ints[0] / ints[1]));

            double[] dbls = MathHelper.GetDoubles(leftTree, rightTree);
            if (dbls != null)
                return Evaluation.Changed(new LiteralNode<double>(dbls[0] / dbls[1]));

            return null;
        }
    }

    public class ModOperator : BinaryOperator
    {

        public ModOperator()
            : base("%", Associativity.Left, Precedence.Mod)
        {
        }

        public override Evaluation PostEvaluate(Node node, Context context)
        {
            if (context.PostEvalMathNodes)
                return StepEvaluate(node, context);
            else
                return null;
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            Node leftTree = node.LeftNode;
            Node rightTree = node.RightNode;


            Int64[] ints = MathHelper.GetInts(leftTree, rightTree);
            if (ints != null)
            {
                if(ints[1] == 0)
                    return Evaluation.Changed(new LiteralNode<Int64>(0));
                return Evaluation.Changed(new LiteralNode<Int64>(ints[0] % ints[1]));
            }

            double[] dbls = MathHelper.GetDoubles(leftTree, rightTree);
            if (dbls != null)
            {
                if (dbls[1] == 0)
                    return Evaluation.Changed(new LiteralNode<double>(0));
                return Evaluation.Changed(new LiteralNode<double>(dbls[0] % dbls[1]));
            }

            return null;
        }
    }
}
