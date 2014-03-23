using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    public class GroupingOperator : Operator
    {
        public bool IsOpening { get { return this.OpeningLabel == this.Label; } }

        public string OpeningLabel { get; private set; }
        public string ClosingLabel { get; private set; }

        public GroupingOperator(string openLabel, string closeLabel, bool isOpen)
            : base(isOpen ? openLabel : closeLabel, openLabel + closeLabel, Associativity.Left, isOpen ? Precedence.Parentheses : Precedence.Max)
        {
            OpeningLabel = openLabel;
            ClosingLabel = closeLabel;
        }

        protected override OperatorOccurrence CreateOccurrence(string expression, int expressionPosition, int index)
        {
            if (!this.IsOpening)
                return new OperatorOccurrence(expression, "", expressionPosition, "", expressionPosition, index, expressionPosition, this);

            int startIndex = index;

            int stackIndex = 1;

            while (++index < expression.Length)
            {
                if (expression.Substring(index, this.OpeningLabel.Length) == this.OpeningLabel)
                    stackIndex++;

                if (expression.Substring(index, this.ClosingLabel.Length) == this.ClosingLabel)
                {
                    if (--stackIndex == 0)
                    {
                        int start = startIndex + this.OpeningLabel.Length;
                        string innerExpression = expression.Substring(start, index - start);
                        return new OperatorOccurrence(expression, innerExpression, expressionPosition + startIndex, "", expressionPosition, startIndex, expressionPosition, this);
                    }
                }

            }

            throw new ParsingException("Mismatched group");
        }

        public override Evaluation PostEvaluate(Node node, Context context)
        {
            return StepEvaluate(node, context);
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            return Evaluation.Changed(node.LeftNode);
        }
    }
    class OpenParenOperator : GroupingOperator { public OpenParenOperator() : base("(", ")", true) { } }
    class CloseParenOperator : GroupingOperator { public CloseParenOperator() : base("(", ")", false) { } }

    public class OpenBracketOperator : GroupingOperator
    {
        public OpenBracketOperator() : base("[", "]", true) { }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            return Evaluation.Unchanged(node);
        }

        public static Evaluation TryUnpack(Node node, Context context)
        {
            var op = node.TypedValue<OpenBracketOperator>(null);
            if (op == null)
                return Evaluation.Unchanged(node);

            return op.Unpack(node, context);

        }

        public Evaluation Unpack(Node node, Context context)
        {
            var listEval = ListHelper.EnsureList(node.LeftNode, false);
            if (listEval.Result == EvaluationResult.Unchanged)
                return Evaluation.Changed(listEval.NewNode);
            else
                return new Evaluation(listEval.Result, node.Copy(listEval.NewNode, node.RightNode));
        }

    }
    class CloseBracketOperator : GroupingOperator { public CloseBracketOperator() : base("[", "]", false) { } }

}
