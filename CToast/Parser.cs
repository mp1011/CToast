using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    public static class Parser
    {
        public const char NullChar = '_';

        public static IEnumerable<string> ExtractStatements(string[] lines)
        {
            StringBuilder currentStatement = new StringBuilder();
            foreach (string line in lines)
            {
                currentStatement.Append(line);
                if(IsBalanced(currentStatement.ToString()))
                {
                    yield return currentStatement.ToString();
                    currentStatement = new StringBuilder();
                }
            }
        }

        public static Node Parse(string expression, Context context)
        {
            context.ClearIllegalOperations();

            if (expression.StartsWith("#"))
                return new LiteralNode<string>(expression.Substring(1));

            var node = ParseInternal(expression,0, context);
            return node.PostEvaluateNodeAndChildren(context).NewNode;
        }

        private static Node ParseInternal(string expression, int expressionPosition, Context context)
        {
            var op = GetLowestPrecedenceOperator(expression, expressionPosition, context);

            if (op != null)
                DiagnosticUtil.Trace(context, "ParseInternal", expression, expressionPosition.ToString(), op.Operator.GetType().Name, op.LeftPosition.ToString(), op.RightPosition.ToString(), op.LeftExpression, op.RightExpression);


            if (op != null)
            {
                while (true)
                {
                    Node leftSide = ParseInternal(op.LeftExpression,op.LeftPosition, context);
                    Node rightSide = ParseInternal(op.RightExpression,op.RightPosition, context);

                    OperatorOccurrence leftOp = null, rightOp = null;


                    if (leftSide is OperatorNode)
                        leftOp = (leftSide as OperatorNode).Occ;
                    
                    if(leftOp == null)
                        leftOp = GetLowestPrecedenceOperator(op.LeftExpression, op.LeftPosition, context); 

                    if (rightSide is OperatorNode)
                        rightOp = (rightSide as OperatorNode).Occ;
                    
                    if(rightOp == null)
                        rightOp = GetLowestPrecedenceOperator(op.RightExpression, op.RightPosition, context); 

                    var result = new OperatorNode(op, leftSide, rightSide);

                    var valid = result.Validate();

                    if (valid != ValidationResult.Valid)
                        DiagnosticUtil.Trace(context, "Invalid operator", expression, expressionPosition.ToString());

                    switch (valid)
                    {
                        case ValidationResult.Valid:
                            return result;
                        case ValidationResult.RedoNode:
                            context.AddIllegalOperation(op);
                            return ParseInternal(expression, expressionPosition, context);
                        case ValidationResult.RedoLeft:
                            context.AddIllegalOperation(leftOp);
                            break;
                        case ValidationResult.RedoRight:
                            context.AddIllegalOperation(rightOp);
                            break;
                        case ValidationResult.RedoBoth:
                            context.AddIllegalOperation(leftOp);
                            context.AddIllegalOperation(rightOp);
                            break;
                    }
                }
            }

            // parse literals
            DiagnosticUtil.Trace(context,"Parsed as literal", expression);
            return ParseLiteral(expression, context);
        }

        public static Node ParseLiteral(string expression, Context context)
        {
            expression = expression.Trim();
            double vald; Int64 vali;

            if (String.IsNullOrEmpty(expression))
                return null;

            if(expression.Contains(".") && double.TryParse(expression, out vald))
                return new LiteralNode<double>(vald);

            if(Int64.TryParse(expression, out vali))
                return new LiteralNode<Int64>(vali);

            if (expression.ToLower() == "true")
                return new LiteralNode<bool>(true);
            else if (expression.ToLower() == "false")
                return new LiteralNode<bool>(false);

            //strings are considered variable placeholders. todo: need to account for actual string, enclosed in quotes.
            //return new LiteralNode<string>(expression);
            return new PlaceholderNode(expression);
        }

        private static OperatorOccurrence GetLowestPrecedenceOperator(string expression, int expressionPosition, Context context)
        {
            string expressionWithoutGroups = HideGroups(expression, expressionPosition);
            return GetOperators(expression, expressionPosition).Where(p=> !context.IsIllegal(p) && expressionWithoutGroups[p.RelativePosition] != Parser.NullChar).OrderBy(p => p.Operator.Precedence).ThenByDescending(p => p.PositionPrecedence).FirstOrDefault();
        }

        /// <summary>
        /// Returns all operators in this expression, from left to right
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static IEnumerable<OperatorOccurrence> GetOperators(string expression, int expressionPosition)
        {
            return OperatorPool.AllOperators.SelectMany(p => p.FindAllOccurrences(expression, expressionPosition)).OrderBy(p => p.RelativePosition).ToArray();
        }

        /// <summary>
        /// Must return a string of the same length as the original
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static string HideGroups(string expression, int expressionPosition)
        {
            StringBuilder sb = new StringBuilder();
            GroupingOperator opening = null;
            OperatorOccurrence openingOp = null;
            int stackCount = 0;

            foreach (var op in GetOperators(expression, expressionPosition))
            {
                GroupingOperator group = op.Operator as GroupingOperator;

                if (group != null)
                {
                    if (opening == null)
                    {
                        opening = group;
                        openingOp = op;
                        sb.Append(expression.Substring(0, openingOp.RelativePosition));
                        sb.Append(group.OpeningLabel);
                        stackCount = 1;
                    }
                    else if (group.Label == opening.OpeningLabel)
                        stackCount++;
                    else if (group.Label == opening.ClosingLabel)
                    {
                        if (--stackCount == 0)
                        {
                            int interiorLength = openingOp.LeftExpression.Length;

                            sb.Append("".PadRight(interiorLength, Parser.NullChar));
                            sb.Append(group.ClosingLabel);
                            sb.Append(HideGroups(op.OriginalExpression.Substring(op.RelativePosition + group.ClosingLabel.Length), expressionPosition)); //todo ???
                            return sb.ToString();
                        }
                    }
                }
            }

            return expression;
        }

        
        /**
         * Determines if the grouping operators (,[,{ are balanced.
         */
        private static bool IsBalanced(string expression)
        {
            //todo, account for string literals

            //todo, this should be tied to the actual operators
            char[] paren = new char[] { '(', ')' };
            char[] brace = new char[] { '{', '}' };
            char[] bracket = new char[] { '[', ']' };

            List<char[]> groupChars = new List<char[]>();
            groupChars.Add(paren); groupChars.Add(brace); groupChars.Add(bracket);

            Stack<char[]> groupStack = new Stack<char[]>();

            for (int index = 0; index < expression.Length; index++)
            {
                char[] closingGroup = null;
                foreach (char[] group in groupChars)
                {
                    if (expression[index] == group[0])                    
                        groupStack.Push(group);
                    
                    if (expression[index] == group[1])
                        closingGroup = group;
                }

                if (closingGroup != null && groupStack.FirstOrDefault() != null)
                {
                    var stackTop = groupStack.Peek();              
                    if (stackTop[1] == closingGroup[1])
                        groupStack.Pop();
                    else
                        return false;
                }

            }

            return groupStack.FirstOrDefault() == null;
        }
    }

    public class ParsingException : Exception 
    {
        public ParsingException(string message) : base(message) { }
    }

    public class OperatorOccurrence
    {
        public int RelativePosition { get; private set; }
        public int AbsolutePosition { get; private set; }

        public int PositionPrecedence
        {
            get
            {
                if (this.Operator.Associativity == Associativity.Left)
                    return AbsolutePosition;
                else
                    return OriginalExpression.Length - AbsolutePosition;
            }
        }

        public string OriginalExpression { get; private set;}
        public string LeftExpression { get; private set; }
        public string RightExpression { get; private set; }

        public int LeftPosition { get; private set; }
        public int RightPosition { get; private set; }

        public Operator Operator { get; private set; }

        public override string ToString()
        {
            return Operator.Label;
        }

        public OperatorOccurrence(string original, string left, int leftPos, string right, int rightPos, int index, int expressionPosition, Operator occurrence)
        {
            this.RelativePosition = index;
            this.AbsolutePosition = expressionPosition + index;
            this.LeftExpression = left ?? "";
            this.RightExpression = right ?? "";
            this.OriginalExpression = original ?? "";
            this.Operator = occurrence;
            this.LeftPosition = leftPos;
            this.RightPosition = rightPos;
        }

        public OperatorOccurrence(string original, int index, int expressionPosition, Operator occurrence)
        {
            this.RelativePosition = index;
            this.AbsolutePosition = expressionPosition + index;
            this.LeftExpression = original.Substring(0, index);
            this.RightExpression = original.Substring(index + occurrence.Label.Length);
            this.LeftPosition = expressionPosition;
            this.RightPosition = expressionPosition + index + occurrence.Label.Length;
            this.OriginalExpression = original;
            this.Operator = occurrence;
        }

        public override bool Equals(object obj)
        {
            OperatorOccurrence other = obj as OperatorOccurrence;
            if (other == null)
                return false;
            return this.AbsolutePosition == other.AbsolutePosition && this.Operator.Equals(other.Operator);
        }
    }
}
