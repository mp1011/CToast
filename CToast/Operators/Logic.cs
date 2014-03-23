using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    class AndOperator : BinaryOperator
    {
        public AndOperator() : base(" and ", Associativity.Left, Precedence.And) { }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            var left = node.LeftNode.NotNull().TypedValue<bool?>(null);
            var right = node.RightNode.NotNull().TypedValue<bool?>(null);

            if (!left.HasValue || !right.HasValue)
                return Evaluation.Unchanged(node);

            bool result = true;
            if (!left.Value)
                result = false;
            if (!right.Value)
                result = false;

            return Evaluation.Changed(new LiteralNode<bool>(result));
        }
    }

    class OrOperator : BinaryOperator
    {
        public OrOperator() : base(" or ", Associativity.Left, Precedence.Or) { }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            var left = node.LeftNode.NotNull().TypedValue<bool?>(null);
            var right = node.RightNode.NotNull().TypedValue<bool?>(null);

            if (!left.HasValue || !right.HasValue)
                return Evaluation.Unchanged(node);

            bool result = false;
            if (left.Value)
                result = true;
            if (right.Value)
                result = true;


            return Evaluation.Changed(new LiteralNode<bool>(result));
        }
    }

    class NotOperator : Operator
    {

        public NotOperator() : base("not ", "not ", Associativity.Left, Precedence.Not) { }


        protected override OperatorOccurrence CreateOccurrence(string expression, int expressionPosition, int index)
        {
            return new OperatorOccurrence(expression, index, expressionPosition, this);
        }

        public sealed override Evaluation StepEvaluate(Node node, Context context)
        {
            var right = node.RightNode.NotNull().TypedValue<bool?>(null);

            if (!right.HasValue)
                return Evaluation.Unchanged(node);

            bool result = true;
            if (right.Value)
                result = false;

            return Evaluation.Changed(new LiteralNode<bool>(result));
        }

    }


}
