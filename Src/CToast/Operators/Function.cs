using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    public abstract class FunctionOperator : Operator
    {
        protected FunctionOperator(string label, string displayLabel, Precedence precedence)
            : base(label, displayLabel, Associativity.Left, precedence)
        {
        }

        public override ValidationResult Validate(Node node)
        {
            if (node.RightNode == null)
                return ValidationResult.RedoNode;
            else
                return ValidationResult.Valid;
        }

        protected override OperatorOccurrence CreateOccurrence(string expression, int expressionPosition, int index)
        {
            expression += " "; //adding a space to make parsing easier

            //look forward for anything besides a letter or digit
            int endIndex = index;
            while (endIndex < expression.Length - 1 && Char.IsLetterOrDigit(expression[++endIndex])) ;

            string leftExpression = expression.Substring(index + 1, endIndex - index - 1);
            string rightExpression;
            if (expression[endIndex] == '(')
            {
                var op = new GroupingOperator("(", ")", true);
                var paramGroup = op.FindAllOccurrences(expression.Substring(endIndex), 9).FirstOrDefault();

                rightExpression = paramGroup.LeftExpression;
                if (String.IsNullOrEmpty(rightExpression))
                    rightExpression = "[]";
            }
            else
                rightExpression = "";

            return new OperatorOccurrence(expression, leftExpression, expressionPosition + index + 1, rightExpression, expressionPosition + endIndex, index, expressionPosition, this);

        }

    }

    // does not expand
    public class FunctionDeclarationOperator : FunctionOperator
    {
        public FunctionDeclarationOperator()
            : base("@", "#@", Precedence.FunctionDeclaration)
        {
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            return Evaluation.Unchanged(node);
        }
    }

    // will expand into the evaluation of a function
    public class FunctionCallOperator : FunctionOperator
    {
        public FunctionCallOperator()
            : base("@", "@", Precedence.FunctionCall)
        {
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            if (node.RightNode == null)
                return Evaluation.Unchanged(node); // todo, no params MUST have parentheses

            var listCheck = ListHelper.EnsureList(node.RightNode, true);
            if (listCheck.Result != EvaluationResult.Unchanged)
                return Evaluation.Changed(node.Copy(node.LeftNode, listCheck.NewNode));

            //when the first argument is a list, it is safer to wrap it in a bracket operator
            if (node.RightNode != null && node.RightNode.TypedValue<Operator>(null) is CommaOperator &&
                node.RightNode.LeftNode != null && node.RightNode.LeftNode.TypedValue<Operator>(null) is CommaOperator && node.RightNode.RightNode == null)
            {
                var bracket = new OperatorNode(new OpenBracketOperator(), node.RightNode.LeftNode, null);
                var newRight = node.RightNode.Copy(bracket, node.RightNode.RightNode);
                return Evaluation.Changed(node.Copy(node.LeftNode, newRight));
            }

            string functionName = node.LeftNode.Value as string;
            if (String.IsNullOrEmpty(functionName))
                return Evaluation.Unchanged(node);

            var patterns = context.GetFunctionPatterns(functionName).ToArray();

            if (patterns.FirstOrDefault() == null)
                return Evaluation.Unchanged(new SystemMessageNode("Unable to resolve function: " + functionName));

            Node newNode = new FunctionSelectorNode(functionName, patterns, ListHelper.EnsureList2(node.RightNode, true), context);

            if (!context.SkipFunctionSelectors)
                return Evaluation.Changed(newNode);

            if (!context.IsImportingLibraries)
                context.NullOp();

            //note: evaluation of function patterns must not evaluate the arguments, or else this sequence may not terminate.
            while ((newNode is FunctionSelectorNode || newNode is FunctionPatternNode))
            {
                var eval = newNode.StepEvaluateNodeAndChildren(context);
                if (eval.Result == EvaluationResult.Changed)
                    newNode = eval.NewNode;
                else
                    break;
            }

            if (newNode is FunctionSelectorNode || newNode is FunctionPatternNode)
                return Evaluation.Unchanged(node);
            else
                return Evaluation.Changed(newNode);

        }
    }



    class PatternOperator : BinaryOperator
    {

        public PatternOperator() : base("|", Associativity.Left, Precedence.Pattern) { }


        public override Evaluation StepEvaluate(Node node, Context context)
        {
            if (node.RightNode != null && node.RightNode.IsAtomic)
            {
                if (node.RightNode.TypedValue<bool>(false))
                    return Evaluation.Changed(node.LeftNode);
                else
                    return Evaluation.Changed(new NullNode());
            }

            return Evaluation.Unchanged(node);
        }
    }


}
