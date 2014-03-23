using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{

    class AssignmentOperator : BinaryOperator
    {

        public AssignmentOperator()
            : base("->", "->", Associativity.Left, Precedence.Assignment)
        {
        }

        public override ValidationResult Validate(Node node)
        {
            //if left side is a function call, it should have been a function assignment
            var funcCallOp = OperatorNode.TryCast<FunctionCallOperator>(node.LeftNode);
            if (funcCallOp != null)
                return ValidationResult.RedoLeft;

            var funcOp = OperatorNode.TryCast<FunctionDeclarationOperator>(node.LeftNode);
            if (funcOp == null)
                return ValidationResult.RedoNode;
            else
            {
                // function must have a name, otherwise it should have been an anonymous assignment
                if (funcOp.LeftNode == null)
                    return ValidationResult.RedoNode;
            }

            return ValidationResult.Valid;
        }

        public override Evaluation StepEvaluate(Node node, Context context)
        {
            //assignment only works if the left side is a function declaration or (maybe) a string

            var funcOp = OperatorNode.TryCast<FunctionDeclarationOperator>(node.LeftNode);
            if (funcOp != null)
            {
                //ensure that function arguments are in list format
                node = node.Copy(node.LeftNode.Copy(node.LeftNode.LeftNode, ListHelper.EnsureList2(node.LeftNode.RightNode, false)), node.RightNode);
                var name = funcOp.LeftNode.Value as string;

                context.AddFunctionPattern(name, node);

                return Evaluation.Changed(new SystemMessageNode("Declared @" + name));
            }

            var funcCallOp = OperatorNode.TryCast<FunctionCallOperator>(node.LeftNode);
            if (funcCallOp != null)
            {
                // left side should have been a declaration
                var newLeft = funcCallOp.Reparse(context);
                OperatorNode o = node as OperatorNode;
                o.UpdateLeft(newLeft);
                return Evaluation.Changed(o);
            }

            var stringOp = node.LeftNode as LiteralNode<string>;
            if (stringOp != null)
            {
                //may or may not use this syntax
                throw new NotImplementedException();
            }

            return Evaluation.IllegalOperation(node as OperatorNode);
        }
    }

    /// <summary>
    /// Assignment for anonymous functions. TODO - consolidate with above class
    /// </summary>
    public class AnonAssignmentOperator : BinaryOperator
    {
        private static int nextAnonId = 0;

        private static string NextAnonFunctionName
        {
            get
            {
                string name = "ANON_" + nextAnonId;
                nextAnonId++;
                return name;
            }
        }

        public AnonAssignmentOperator()
            : base("->", "a->", Associativity.Left, Precedence.AssignmentAnon)
        {
        }

        internal override Tuple<Node, Node> InitializeNode(Node root)
        {
            return new Tuple<Node, Node>(root.LeftNode.Copy(new LiteralNode<string>("Anonymous"), root.LeftNode.RightNode), root.RightNode);
        }

        public override ValidationResult Validate(Node node)
        {
            //if left side is a function call, it should have been a function assignment
            var funcCallOp = OperatorNode.TryCast<FunctionCallOperator>(node.LeftNode);
            if (funcCallOp != null)
                return ValidationResult.RedoLeft;

            var funcOp = OperatorNode.TryCast<FunctionDeclarationOperator>(node.LeftNode);
            if (funcOp == null)
                return ValidationResult.RedoNode;
            else
            {
                return ValidationResult.Valid;
            }

        }


        public override Evaluation StepEvaluate(Node node, Context context)
        {
            //assignment only works if the left side is a function declaration or (maybe) a string

            var funcOp = OperatorNode.TryCast<FunctionDeclarationOperator>(node.LeftNode);
            if (funcOp != null)
            {
                string name = funcOp.LeftNode.Value.ToString();

                if (name == "Anonymous")
                {
                    var newLeft = node.LeftNode.Copy(new LiteralNode<string>(NextAnonFunctionName), node.LeftNode.RightNode);
                    return Evaluation.Changed(node.Copy(newLeft, node.RightNode));
                }

                //ensure that function arguments are in list format
                node = node.Copy(node.LeftNode.Copy(node.LeftNode.LeftNode, ListHelper.EnsureList2(node.LeftNode.RightNode, false)), node.RightNode);

                context.AddFunctionPattern(name, node);

                return Evaluation.Changed(node.LeftNode);
            }

            var funcCallOp = OperatorNode.TryCast<FunctionCallOperator>(node.LeftNode);
            if (funcCallOp != null)
            {
                // left side should have been a declaration
                var newLeft = funcCallOp.Reparse(context);
                OperatorNode o = node as OperatorNode;
                o.UpdateLeft(newLeft);
                return Evaluation.Changed(o);
            }

            var stringOp = node.LeftNode as LiteralNode<string>;
            if (stringOp != null)
            {
                //may or may not use this syntax
                throw new NotImplementedException();
            }

            return Evaluation.IllegalOperation(node as OperatorNode);
        }
    }


}
