using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{

    class BuiltinFunction_Import : Node 
    {
        private BuiltinFunction_Import()
        {
            this.LeftNode = new PlaceholderNode("file");
        }

        public static Node Create(Context context)
        {
            var node = Parser.Parse("@import(file) -> 0", context);
            node = node.Copy(node.LeftNode, new BuiltinFunction_Import());
            node.EvaluateFull(context);
            return node;
        }

        protected override Evaluation StepEvaluateNode(Context context)
        {
            string file = this.LeftNode.Value.ToString();

            string[] lines = System.IO.File.ReadAllLines(PathHelper.GetLibFile(file.Replace("\"","")));

            Node[] nodes = Parser.ExtractStatements(lines).Select
              (p => p.Trim()).Where(p => p.Length > 0).Select(p => Parser.Parse(p, context)).ToArray();

            foreach (var node in nodes)
                node.EvaluateFull(context);

            return Evaluation.Changed(new SystemMessageNode("File imported"));
        }

        protected override Node CopyInner()
        {
            return new BuiltinFunction_Import();
        }
    }

    class BuiltinFunction_DefineOperator : Node
    {
        public BuiltinFunction_DefineOperator()
        {
            this.LeftNode = ListHelper.CreateList(new Node[] { new PlaceholderNode("op"), new PlaceholderNode("@fn") });
        }

        public static Node Create(Context context)
        {
            var node = Parser.Parse("@operator(op,@fn) -> 0", context);
            node = node.Copy(node.LeftNode, new BuiltinFunction_DefineOperator());
            node.EvaluateFull(context);
            return node;
        }

        protected override Evaluation StepEvaluateNode(Context context)
        {
            return Evaluation.IllegalOperation(this);
        }

        protected override Node CopyInner()
        {
            return new BuiltinFunction_DefineOperator();
        }

    }

}
