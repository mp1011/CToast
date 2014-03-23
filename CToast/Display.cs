using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CToast
{
    public  interface ITreeRenderer
    {
        void Render(Node node);
        Task RenderAsync(Node node);
    }

    public abstract class TreeRenderer<T> : ITreeRenderer 
    {
        private Action<Node,T> mOnRender;

        protected TreeRenderer(Action<Node, T> onRender)
        {
            mOnRender = onRender;
        }

        protected TreeRenderer()
        {
        }

        public T Render(Node root)
        {
            var result = RenderNode(root);
            if (mOnRender != null)
                mOnRender(root,result);
            return result;
        }

        void ITreeRenderer.Render(Node root)
        {
            Render(root);
        }

        Task ITreeRenderer.RenderAsync(Node root)
        {
            return Task<T>.Factory.StartNew(() => this.RenderNode(root)).ContinueWith(t =>
                {
                    if (mOnRender != null)
                        mOnRender(root, t.Result);
                });
        }

        protected abstract T RenderNode(Node root);
    }
    public class SyntaxRenderer : TreeRenderer<string>
    {
        public SyntaxRenderer(Action<Node,string> fn) : base(fn) { }
        public SyntaxRenderer() { }

        protected override string RenderNode(Node root)
        {            
            if (root == null)
                return "";

            string result = ReflectionHelper.DynamicDispatch<string>(this, root);

            if (String.IsNullOrEmpty(result))
                result = root.ToString();

            return result;
        }

        public string RenderNode(FunctionPatternNode node)
        {
            return "PATTERN-ARGS{" + Render(node.LeftNode) + "}PATTERN-RESULT{" + Render(node.RightNode) + "}";
        }

        public string RenderNode(FunctionSelectorNode node)
        {
            return "SELECT:" + Render(node.LeftNode) + "" + Render(node.RightNode);
        }

        public string RenderNode(ArgNode node)
        {
            return Render(node.LeftNode) + "=>" + Render(node.RightNode);
        }

        public string RenderNode(OperatorNode op)
        {
            if (op.Op is FunctionCallOperator || op.Op is FunctionDeclarationOperator)
            {
                var name = op.LeftNode.ToString();
                if (name.StartsWith("ANON", StringComparison.InvariantCulture))
                    name = "@";
                else
                    name = "@" + name;

                return name + "(" + Render(op.RightNode) + ")";
            }

            if (op.Op is OpenBracketOperator)
                return "[" + Render(op.LeftNode) + "]";

            if (op.Op is AnonAssignmentOperator)
                return Render(op.LeftNode) + op.Op.DisplayLabel + Render(op.RightNode);

            if (op.Op is ListOperator)
                return String.Join(",", ListHelper.GetNodeList(op).Select(p => Render(p)).ToArray());

            if (op.Op is HeadOperator)
                return "h:" + Render(op.RightNode);

            if (op.Op is TailOperator)
                return "t:" + Render(op.RightNode);

            return Render(op.LeftNode) + " " + op.Op.DisplayLabel + " " + Render(op.RightNode);
        }

    }


    public class NullRenderer : TreeRenderer<bool>
    {
        protected override bool RenderNode(Node root)
        {
            return true;
        }
    }
}


