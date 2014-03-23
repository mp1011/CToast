using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    /// <summary>
    /// 
    /// Left side will be a Function Pattern, if it hasn't been resolved
    /// 
    /// right side will be null or another FunctionSelector
    /// If pattern matches, left side will be replaced with a node
    /// contains variable mappings
    /// </summary>
    public class FunctionSelectorNode : Node
    {

        public FunctionSelectorNode(string name, IEnumerable<Node> patterns, Node arguments, Context context)
            : base("SEL@" + name,null,null)
        {
            this.LeftNode = new FunctionPatternNode(patterns.FirstOrDefault(), arguments,context);
           
            var rest = patterns.Skip(1);
            if (rest.Count() == 0)
                this.RightNode = new NullNode();
            else
                this.RightNode = new FunctionSelectorNode(name, rest, arguments, context);                
        }

        private FunctionSelectorNode(object val):base(val,null,null)
        {

        }

        protected override Node CopyInner()
        {
            return new FunctionSelectorNode(this.Value);
        }
        protected override Evaluation StepEvaluateNode(Context context)
        {
            var patternNode = this.LeftNode as FunctionPatternNode;

            if (patternNode == null || (patternNode.Matches.HasValue && patternNode.Matches.Value))
                return Evaluation.Changed(this.LeftNode);
            else if (patternNode.Matches.HasValue && !patternNode.Matches.Value)
                return Evaluation.Changed(this.RightNode);
            else
            {
                var e = Evaluation.Unchanged(this);
                if(!patternNode.Matches.HasValue)
                    e.SkipRightNode = true;
                return e;

            }
        }
    }

    /// <summary>
    /// left side will be a list of arguments
    /// right side is the evaluation
    /// </summary>
    public class FunctionPatternNode : Node
    {
        public bool? Matches
        {
            get
            {
                return this.TypedValue<bool?>(null);
            }
            private set { this.Value = value; }
        }

        public FunctionPatternNode(Node pattern, Node args, Context context)
        {
            var providedArgsArray = new Queue<Node>(args.Traverse(Traversal.LeftRoots, false));
            List<Node> parameters = new List<Node>();

            foreach (var patternArg in pattern.LeftNode.RightNode.NotNull().Traverse(Traversal.LeftRoots, false))
            {
                if (providedArgsArray.FirstOrDefault() == null)
                {
                    this.Matches = false;
                    return;
                }

                var providedArg = providedArgsArray.Dequeue();
                parameters.Add(new ArgNode(patternArg, providedArg));
              }
         
            if (providedArgsArray.FirstOrDefault() != null)
            {
                this.Matches = false;
                return;
            }
           
            this.LeftNode = ListHelper.CreateList(parameters);
            this.LeftNode = this.LeftNode.ReplaceNodes2(ResolvePlaceholder);
            this.RightNode = pattern.RightNode.ReplaceNodes2(ResolvePlaceholder);
            
        }

        public override string ToString()
        {
            if (!Matches.HasValue)
                return "PAT@";
            else if (Matches.Value)
                return "PAT@-M";
            else
                return "PAT@-X";
        }

        protected override Evaluation StepEvaluateNode(Context context)
        {
            var result = StepEvaluateNodeX(context);
            result.SkipRightNode = true;
            return result;
        }

        private Evaluation StepEvaluateNodeX(Context context)
        {
            if (this.Matches.HasValue)
                return Evaluation.Unchanged(this);

      
            bool unknown = false;

            foreach (var node in LeftNode.Traverse(Traversal.LeftRoots, false))
            {
                if(node.TypedValue<PatternResult>(PatternResult.Unknown) == PatternResult.NoMatch)
                {
                    this.Matches = false;
                    return Evaluation.Changed(this);
                }
                else if (node.Value == null)
                    unknown = true;              
            }
         
            if (unknown)
            {
                this.Matches = null;
                var eval = Evaluation.Unchanged(this);
                return eval;
            }
            else
            {
                this.Matches = true;

                if (this.RightNode.IsAtomic)
                    return Evaluation.Changed(this.RightNode);
                else
                {
                    Node resolvedNode = this.RightNode.ReplaceNodes2(this.ResolvePlaceholder);
                    return Evaluation.Changed(resolvedNode);
                }
            }
        }

        //private bool? ArgMatches(Node arg)
        //{
        //    NullNode n = arg as NullNode;
        //    if(n != null)
        //        return false;

        //    MergedNode merged = arg as MergedNode;
        //    if (merged != null)
        //    {
        //        var pattern = merged.First;
        //        var provided = merged.Second;

        //        if (!pattern.IsAtomic)
        //            return false;

        //        // string literals will match anything
        //        if (pattern.Value is string)
        //            return true;

        //        // numbers must match exact
        //        double? providedNum = CToast.MathHelper.GetNumber(provided);
        //        double? patternNum = CToast.MathHelper.GetNumber(pattern);

        //        if (providedNum.HasValue && patternNum.HasValue && providedNum.Value == patternNum.Value)
        //            return true;

        //        if (providedNum.HasValue && patternNum.HasValue && providedNum.Value != patternNum.Value)
        //            return false;

        //        return null; // don't know
        //    }

        //    return null;
        //}

        public ReplaceResult ResolvePlaceholder(Node source)
        {
            return ResolvePlaceholder(source, new PlaceholderNode[]{ });
        }

        private ReplaceResult ResolvePlaceholder(Node source, PlaceholderNode[] exclusions)
        {
            if (source.TypedValue<Operator>(null) is AssignmentOperator || source.TypedValue<Operator>(null) is AnonAssignmentOperator)
                return ReplaceResult.Stop(ResolveAssignmentPlaceholders(source));

            ArgNode sa = source as ArgNode;
            if (sa != null)
            {
                if (sa.LeftNode is PlaceholderNode)
                    return ReplaceResult.Continue(sa.Copy(new LiteralNode(sa.LeftNode.Value), sa.RightNode));
                else if(sa.LeftNode != null && sa.LeftNode.TypedValue<Operator>(null) is PatternOperator)
                {
                    return ReplaceResult.Continue(sa.Copy(sa.LeftNode.Copy(new LiteralNode(sa.LeftNode.LeftNode.Value), sa.LeftNode.RightNode), sa.RightNode));
                }
            }

            var functionCallOp = source.TypedValue<FunctionCallOperator>(null);
            if(functionCallOp != null)
            {
                foreach (var node in this.LeftNode.Traverse(Traversal.LeftRoots, true))
                {
                      ArgNode m = node as ArgNode;

                      if (m != null && m.PatternValue != null)
                      {
                          if (m.PatternValue.ToString() == "@" + source.LeftNode.Value)
                          {
                              var right = source.RightNode;//.ReplaceNodes2(n => ResolvePlaceholder(n, true));

                              if(m.ProvidedValue.TypedValue<Operator>(null) is AnonAssignmentOperator)
                              {
                                  return ReplaceResult.Continue(source.Copy(new LiteralNode<string>(m.ProvidedValue.LeftNode.LeftNode.Value.ToString()), right));
                              }
                              else if(m.ProvidedValue.TypedValue<Operator>(null) is FunctionDeclarationOperator) // todo: not so sure about this
                              {
                                  return ReplaceResult.Continue(source.Copy(new LiteralNode<string>(m.ProvidedValue.LeftNode.Value.ToString()), right));
                              }
                              else
                                  return ReplaceResult.Continue(source.Copy(new LiteralNode<string>(m.ProvidedValue.Value.ToString().Substring(1)), right));
                          }
                      }

                }
            }

            string name;
            if (source != null && source is PlaceholderNode)
            {
                name = source.Value.ToString();
                if (exclusions.Any(p => p.Value.ToString() == name))
                    return ReplaceResult.Continue(source);

                foreach (var node in this.LeftNode.Traverse(Traversal.LeftRoots, true))
                {
                    ArgNode m = node as ArgNode;

                    if (m != null && m.PatternValue != null)
                    {
                        if (m.PatternValue.Value.ToString() == name)
                        {
                           // if (m.ProvidedValue.TypedValue<Operator>(null) is FunctionCallOperator)
                             //   return m.ProvidedValue.LeftNode;
                            //else
                                return ReplaceResult.Continue(m.ProvidedValue);
                        }

                        if (m.PatternValue.TypedValue<Operator>(null) is FunctionDeclarationOperator || m.PatternValue.TypedValue<Operator>(null) is FunctionCallOperator)
                        {
                            if (m.PatternValue.LeftNode.Value.ToString() == name && m.ProvidedValue.LeftNode != null)
                            {
                                if (m.ProvidedValue.LeftNode.IsAtomic)
                                    return ReplaceResult.Continue(m.ProvidedValue.LeftNode); //todo, why?
                                return ReplaceResult.Continue(m.ProvidedValue.LeftNode.LeftNode); // <--
                            }
                        }

                        if (m.PatternValue.TypedValue<Operator>(null) is PatternOperator)
                        {
                            if (m.PatternValue.LeftNode.Value.ToString() == name)
                                return ReplaceResult.Continue(m.ProvidedValue);
                        }

                    }

                }                    
            }

            return ReplaceResult.Continue(source);
        }

        //given an Assignment or AnonAssignment node, resolves any external variables while maintaining the assignment's own private variables
        private Node ResolveAssignmentPlaceholders(Node source)
        {
            var placeholders = source.LeftNode.Traverse(Traversal.Preorder, false).OfType<PlaceholderNode>().ToArray();
            return source.Copy(source.LeftNode, source.RightNode.ReplaceNodes2(p => ResolvePlaceholder(p, placeholders)));
        }

        public Node ResolvePlaceholder(string name)
        {
            var parameter = this.LeftNode.Traverse(Traversal.LeftRoots, true).Cast<ArgNode>().FirstOrDefault(p => p.PatternValue.Value.ToString() == name);
            if(parameter == null)
                return null;

            return parameter.ProvidedValue;
        }

        private FunctionPatternNode() { }

        protected override Node CopyInner()
        {
            return new FunctionPatternNode();
        }

    }

    enum PatternResult
    {
        Unknown,
        IsMatch,
        NoMatch
    }
    
    /// <summary>
    /// left side is pattern value
    /// right side is provided value
    /// 
    /// resolves to error if pattern does not match
    /// </summary>
    public class ArgNode : Node
    {
        public override string ToString()
        {
            switch (this.TypedValue<PatternResult>(PatternResult.Unknown))
            {
                case PatternResult.Unknown:
                    return "@Arg";
                case PatternResult.IsMatch:
                    return "@Arg-M";
                default:
                    return "@Arg-X";
            }
        }

        public Node PatternValue { get { return this.LeftNode; } }
        public Node ProvidedValue { get { return this.RightNode; } }

        public ArgNode(Node patternValue, Node providedValue)
        {
            if (patternValue.IsAtomic)
                this.LeftNode = new LiteralNode(patternValue.Value);
            else
            {
                var op = patternValue.TypedValue<Operator>(null);

                if (op is FunctionDeclarationOperator)
                    this.LeftNode = patternValue.Copy(new LiteralNode(patternValue.LeftNode.Value), null);
                else if (op is FunctionCallOperator)
                {
                    //hack, this should be a function declaration!
                    this.LeftNode = patternValue.Copy(new LiteralNode(patternValue.LeftNode.Value), null);
                }
                else if (op is PatternOperator)
                    this.LeftNode = patternValue;
            }

            this.RightNode = providedValue;
        }

        private ArgNode(PatternResult value, Node patternValue, Node providedValue) : base(value,patternValue,providedValue)
        {           
        }

        protected override Node CopyInner()
        {
            return new ArgNode(this.PatternValue, this.ProvidedValue);
        }

        protected override Evaluation StepEvaluateNode(Context context)
        {
            var result = StepEvaluateNodeX(context);
            result.SkipRightNode = true;
            return result;
        }

        private Evaluation StepEvaluateNodeX(Context context)
        {

            if (this.Value != null)
                return Evaluation.ChangedLiteral<PatternResult>(this.TypedValue<PatternResult>(PatternResult.Unknown));

            var isMatch = this.PatternMatches();

            if (!isMatch.HasValue)
                return Evaluation.Unchanged(this);
            else if (isMatch.Value)
                return Evaluation.Changed(new ArgNode(PatternResult.IsMatch, this.PatternValue, this.ProvidedValue));
            else
                return Evaluation.Changed(new ArgNode(PatternResult.NoMatch, this.PatternValue, this.ProvidedValue));               

        }

        private bool? PatternMatches()
        {            
            if (PatternValue == null)
                return null;

            //this needs to be improved
            if (PatternValue.IsAtomic && PatternValue.Value.ToString().StartsWith("#",StringComparison.InvariantCulture))
            {
                var pattern = PatternValue.Value.ToString().Substring(1);
                return ProvidedValue.IsAtomic && ProvidedValue.Value.ToString() == pattern;
            }

            if (PatternValue.TypedValue<Operator>(null) is PatternOperator)
            {
                if (PatternValue.RightNode.IsAtomic)
                    return PatternValue.RightNode.TypedValue<bool>(false);
                else
                    return null;

            }

            // string literals will match anything, except functions which require a @ prefix
            if (PatternValue.Value is string)
            {
                if (PatternValue.Value.ToString().StartsWith("@", StringComparison.InvariantCulture))
                {
                    if (ProvidedValue.TypedValue<Operator>(null) is FunctionDeclarationOperator)
                        return true;
                    else if (ProvidedValue.Value.ToString().StartsWith("@", StringComparison.InvariantCulture))
                        return true;
                    else
                        return null;
                }
                else
                {
                    var opNode = ProvidedValue.TypedValue<Operator>(null);
                    if (opNode != null)
                    {
                        if (opNode is AnonAssignmentOperator)
                            return false;
                    }
                }

                return true;
            }

            //function declaration match function call
            if (PatternValue.TypedValue<Operator>(null) is FunctionDeclarationOperator && ProvidedValue.TypedValue<Operator>(null) is FunctionCallOperator)
                return true;

            if (PatternValue.TypedValue<Operator>(null) is FunctionDeclarationOperator && ProvidedValue.TypedValue<Operator>(null) is FunctionDeclarationOperator)
                return true;

            if (PatternValue.TypedValue<Operator>(null) is FunctionCallOperator && ProvidedValue.TypedValue<Operator>(null) is FunctionCallOperator)
                return true;


            // numbers must match exact
            double? providedNum = CToast.MathHelper.GetNumber(this.ProvidedValue);
            double? patternNum = CToast.MathHelper.GetNumber(this.PatternValue);

            if (providedNum.HasValue && patternNum.HasValue && providedNum.Value == patternNum.Value)
                return true;

            if (providedNum.HasValue && patternNum.HasValue && providedNum.Value != patternNum.Value)
                return false;

            // [] matches an empty list
            if (PatternValue.IsEmptyList)
            {
                if (this.ProvidedValue == null)
                    return true;

                var op = ProvidedValue as OperatorNode;
                if (op != null && op.Value is ListOperator)
                    return op.LeftNode == null;
                if (op != null && op.Value is OpenBracketOperator)
                    return op.LeftNode == null || (op.LeftNode.TypedValue<Operator>(null) is CommaOperator && op.LeftNode.LeftNode == null);

                if (ProvidedValue.IsEmptyList)
                    return true;

                if (ProvidedValue.IsAtomic)
                    return false;
            }

            return null; // don't know

        }
       

    }

    class PlaceholderNode : Node
    {
        public string Name { get { return this.Value.ToString(); } }

        //public override string ToString()
        //{
        //    return "{{" + base.ToString() + "}}";
        //}

        public PlaceholderNode(string value) : base(value.Trim(), null, null) 
        {
        }

        protected override Evaluation StepEvaluateNode(Context context)
        {
            var replacement = context.FillPlaceholder(Name);
            if (replacement == null)
                return Evaluation.Unchanged(this);
            else
                return Evaluation.Changed(replacement);
        }

        protected override Node CopyInner()
        {
            return new PlaceholderNode(this.Name);
        }
    }
}
