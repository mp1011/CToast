using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    public class Context
    {
        private bool mImportingLibraries;
        public bool IsImportingLibraries
        {
            get { return mImportingLibraries; }
            set
            {
                TreeTest.TestEnabled = !value;
                mImportingLibraries = value;
            }
        }

        public void NullOp() { }

        private bool mSkipFunctionSelectors = false;
        public bool SkipFunctionSelectors
        {
            get
            {
                return mSkipFunctionSelectors;
            }
            set
            {
                mSkipFunctionSelectors = value;
            }
        }

        public bool PostEvalMathNodes { get; set; }

        class ContextFrame
        {
            private Dictionary<string, List<Node>> mFunctionPatterns = new Dictionary<string, List<Node>>();
            private List<OperatorOccurrence> mIllegalOperations = new List<OperatorOccurrence>();
            private FunctionPatternNode mResolvedFunctionPattern;

            public ContextFrame()
            {
            }

            public ContextFrame(FunctionPatternNode f)
            {
                mResolvedFunctionPattern = f;
            }

            public void AddIllegalOperation(OperatorOccurrence occurrence)
            {
                mIllegalOperations.Add(occurrence);
            }

            public void ClearIllegalOperations()
            {
                mIllegalOperations.Clear();
            }

            public bool IsIllegal(OperatorOccurrence occurrence)
            {
                //todo, this is not right
                return mIllegalOperations.Any(p => p.Equals(occurrence));
            }

            public void AddFunctionPattern(string name, Node pattern)
            {
                List<Node> nodes = mFunctionPatterns.TryGet(name, null);
                if (nodes == null)
                {
                    nodes = new List<Node>();
                    mFunctionPatterns.Add(name, nodes);
                }

                //ensure pattern is not already defined
                if (!nodes.Any(n => pattern.TreeEqualsExact(n)))
                    nodes.Add(pattern);
            }

            public void RemoveFunctionPatterns(string name)
            {
                List<Node> nodes = mFunctionPatterns.TryGet(name, null);

                if (nodes == null)
                    return;
                else
                    nodes.Clear();
            }

            public IEnumerable<Node> GetFunctionPatterns(string name)
            {
                List<Node> nodes = mFunctionPatterns.TryGet(name, null);

                if (nodes == null)
                    return new List<Node>();
                else
                    return nodes.Select(p => p.Copy());
            }

            public Node FillPlaceholder(string name)
            {
                if (mResolvedFunctionPattern == null)
                    return null;

                return mResolvedFunctionPattern.ResolvePlaceholder(name);
            }
        }

        private Stack<ContextFrame> mFrames = new Stack<ContextFrame>();

        public Context(string library)
        {
            if (String.IsNullOrEmpty(library))
                library = "@import(main.toast)";

            BuiltinFunction_Import.Create(this);
            BuiltinFunction_DefineOperator.Create(this);
            this.SkipFunctionSelectors = true;


            if (String.IsNullOrEmpty(library))
                return;

            Node[] nodes = Parser.ExtractStatements(library.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)).Select
               (p => p.Trim()).Where(p => p.Length > 0).Select(p => Parser.Parse(p, this)).ToArray();
            foreach (var node in nodes)
                node.EvaluateFull(this);

        }

        private ContextFrame CurrentFrame
        {
            get
            {
                if (mFrames.Count == 0)
                    mFrames.Push(new ContextFrame());

                return mFrames.Peek();
            }
        }

        public void AddFunctionPattern(string name, Node pattern)
        {
            //todo - don't like this
            if (name.StartsWith("ANON"))
            {
                foreach (var frame in mFrames)
                    frame.RemoveFunctionPatterns(name);
            }

            this.CurrentFrame.AddFunctionPattern(name, pattern);
        }

        public IEnumerable<Node> GetFunctionPatterns(string name)
        {
            //todo, order of frames
            foreach (var frame in mFrames)
            {
                foreach (var f in frame.GetFunctionPatterns(name))
                    yield return f;
            }
        }

        public Context PushVariableMapping(FunctionPatternNode node)
        {
            Context newContext = new Context("");

            foreach (var frame in mFrames)
                newContext.mFrames.Push(frame);

            newContext.mFrames.Push(new ContextFrame(node));
            return newContext;
        }

        public Node FillPlaceholder(string name)
        {
            return CurrentFrame.FillPlaceholder(name);
        }

        //todo, this needs more work
        public void AddIllegalOperation(OperatorOccurrence occurrence)
        {
            this.CurrentFrame.AddIllegalOperation(occurrence);
        }

        public void ClearIllegalOperations()
        {
            this.CurrentFrame.ClearIllegalOperations();
        }


        public bool IsIllegal(OperatorOccurrence occurrence)
        {
            return this.CurrentFrame.IsIllegal(occurrence);
        }
    }


}
