using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace CToast
{
    public enum Precedence
    {
        Dot,      
        Assignment,
        Comma,
        AssignmentAnon,
        Type,
        Pattern,

        Concat,
      

        Not,
        And,
        Or,
        Equals,
        Comparison,

        Add_Sub,
        Multiply_Divide,

        Head,
        Tail,

        Mod,
        FunctionCall,
        FunctionDeclaration,
        Parentheses,

        Max
    }
                
    public enum Associativity
    {
        Left = 0,
        Right
    }

    public static class OperatorPool
    {
        private static List<Operator> mOperators;

        public static IEnumerable<Operator> AllOperators
        {
            get
            {
                return mOperators ?? (mOperators = new List<Operator>(GetAllOperators()));
            }
        }

        private static IEnumerable<Operator> GetAllOperators()
        {
            foreach (Type opType in ReflectionHelper.GetSubtypes(typeof(Operator)))
            {
                object o = ReflectionHelper.InvokeObjectConstructor(opType);
                if (o != null)
                    yield return (Operator)o;
            }
        }
    }
     
    public abstract class Operator
    {
        public Associativity Associativity { get; private set; }
        public Precedence Precedence { get; private set; }
        public string Label { get; private set; }
        public string DisplayLabel { get; private set; }

        protected Operator(string label, string displayLabel, Associativity associativity, Precedence precedence)
        {
            this.Label = label;
            this.DisplayLabel = displayLabel;
            this.Associativity = associativity;
            this.Precedence = precedence;
        }

        public virtual ValidationResult Validate(Node node) { return ValidationResult.Valid; }

        internal IEnumerable<OperatorOccurrence> FindAllOccurrences(string expression, int expressionPosition)
        {
            int startIndex = 0;

            while (true)
            {
                int index = expression.IndexOf(this.Label,startIndex);
                if (index == -1)
                    break;

                var op = this.CreateOccurrence(expression, expressionPosition, index);
                if (op == null)
                    break;

                startIndex = index += op.Operator.Label.Length;
                yield return op;
            }
        }

        protected virtual OperatorOccurrence CreateOccurrence(string expression, int expressionPosition, int index)
        {
            throw new NotImplementedException();
        }

        public virtual Evaluation StepEvaluate(Node node, Context context)
        {
            throw new NotImplementedException();
        }

        public virtual Evaluation PostEvaluate(Node node, Context context)
        {
            return null;
        }

        internal virtual Tuple<Node, Node> InitializeNode(Node root) { return null; }

        public override bool Equals(object obj)
        {
            Operator other = obj as Operator;
            if (other == null)
                return false;

            return this.DisplayLabel == other.DisplayLabel && this.Label == other.Label && this.Precedence == other.Precedence;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return this.DisplayLabel;
        }

    }

    public abstract class BinaryOperator : Operator
    {
        protected BinaryOperator(string label, Associativity associativity, Precedence precedence)
            : base(label, label, associativity, precedence)
        {
        }

        protected BinaryOperator(string label, string displayLabel, Associativity associativity, Precedence precedence)
            : base(label, displayLabel, associativity, precedence)
        {
        }

        protected override OperatorOccurrence CreateOccurrence(string expression, int expressionPosition, int index)
        {
            return new OperatorOccurrence(expression, index,expressionPosition, this);
        }
    }



  
}
