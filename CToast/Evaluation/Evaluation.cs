using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    public enum Traversal
    {
        None = 0,
        RightOnly = 10,
        Preorder = 20,
        LeftRoots = 30 // yields the left node of the root and the LeftRoots traversal of the Right Node
    }

    public enum EvaluationResult
    {
        Changed,
        Unchanged,
        IllegalOperation,
        ChangeContext //todo, this may be unneccessary
    }

    public enum ValidationResult
    {
        Valid,
        RedoNode,
        RedoLeft,
        RedoRight,
        RedoBoth
    }

    public class ReplaceResult
    {
        public bool ReplaceChildren { get; private set; }
        public Node Result { get; private set; }

        public static ReplaceResult Continue(Node result)
        {
            return new ReplaceResult { ReplaceChildren = true, Result = result.Copy() };
        }

        public static ReplaceResult Stop(Node result)
        {
            return new ReplaceResult { ReplaceChildren = false, Result = result.Copy() };
        }
    }

    public class Evaluation
    {
        public Context NewContext { get; private set; }
        public Node NewNode { get; private set; }
        public EvaluationResult Result { get; private set; }

        //todo
        public bool SkipRightNode { get; set; }
        public bool SkipLeftNode { get; set; }

        public override string ToString()
        {
            return Result.ToString();
        }

        public Evaluation() { }

        public Evaluation(EvaluationResult result, Node newNode)
        {
            this.Result = result;
            this.NewNode = newNode;
        }

        //public static Evaluation Completed(Node n)
        //{
        //    Evaluation e = new Evaluation();
        //    e.NewNode = n;
        //    e.Result = EvaluationResult.Completed;
        //    return e;
        //}

        public static Evaluation Changed(Node node)
        {
            Evaluation e = new Evaluation();
            e.NewNode = node;
            e.Result = EvaluationResult.Changed;

            return e;
        }

        public static Evaluation ChangedContext(Context context)
        {
            Evaluation e = new Evaluation();
            e.NewContext = context;
            e.Result = EvaluationResult.ChangeContext;
            return e;
        }

        public static Evaluation ChangedLiteral<T>(T value)
        {
            Evaluation e = new Evaluation();
            e.NewNode = new LiteralNode<T>(value);
            e.Result = EvaluationResult.Changed;
            return e;
        }

        public static Evaluation Unchanged(Node node)
        {
            Evaluation e = new Evaluation();
            e.NewNode = node;
            e.Result = EvaluationResult.Unchanged;
            return e;
        }

        public static Evaluation IllegalOperation(Node node)
        {
            Evaluation e = new Evaluation();
            e.NewNode = node;
            e.Result = EvaluationResult.IllegalOperation;
            return e;
        }
    }

  
}
