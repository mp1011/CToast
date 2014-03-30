using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CToast;

namespace CToast_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);
         
            var context = Reset();
            var renderer = new ConsoleRenderer();
            var stepSize = 50;
            var eval = new EvaluationChain();

            Console.WriteLine("Type help for commands");

            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();

                if (String.IsNullOrEmpty(line))
                {
                   Evaluate(eval, stepSize, renderer);
                }
                else if (line == "help")
                {
                    Console.WriteLine();
                    Console.WriteLine("exit - Close the program");
                    Console.WriteLine("clear - Clear the console window");
                    Console.WriteLine("reset - Reload the library file");
                    Console.WriteLine("step=# - Sets the number of evaluation steps ber patch (currently " + stepSize + ")");
                    Console.WriteLine("step=0 - Displays evaluation results immediately and continues until evaluation completes or any key is pressed");                    
                }
                else if (line == "exit")
                    break;
                else if (line == "clear")
                    Console.Clear();
                else if (line == "reset")
                {
                    var skipSel = context.SkipFunctionSelectors;
                    context = Reset();
                    context.SkipFunctionSelectors = skipSel;
                }
                else if (line == "toggleskip")
                {
                    context.SkipFunctionSelectors = !context.SkipFunctionSelectors;
                    Console.WriteLine("Skip Selectors=" + context.SkipFunctionSelectors.ToString());
                }
                else if (line.StartsWith("step="))
                {
                    stepSize = Int32.Parse(line.Replace("step=", ""));
                }
                else
                {
                    eval.Reset(context, Parser.Parse(line, context), line);
                    if (stepSize == 0)
                    {
                        Evaluate(eval, 1, renderer);
                        int size = 0;
                        int newSize = eval.TotalSteps;
                        while (newSize > size)
                        {
                            size = newSize;
                            Evaluate(eval, 1, renderer);
                            newSize = eval.TotalSteps;

                            if (Console.KeyAvailable)
                                break;
                        }
                    }
                    else
                        Evaluate(eval, stepSize, renderer);
                }
            }
        }

        private static void Evaluate(EvaluationChain eval, int numSteps, ConsoleRenderer renderer)
        {
            foreach (var node in eval.AddSteps(numSteps))
            {
                renderer.Render(node);
                Console.WriteLine();
            }
        }

        private static Context Reset()
        {
            var ctx = new Context(Environment.GetCommandLineArgs().Skip(1).LastOrDefault());          
            Console.WriteLine("Context Reset");
            return ctx;
        }
    }

    class ConsoleRenderer : TreeRenderer<object>
    {
        const ConsoleColor LiteralColor = ConsoleColor.White;
        const ConsoleColor OperatorColor = ConsoleColor.Red;
        const ConsoleColor FunctionColor = ConsoleColor.Green;
        const ConsoleColor SelectorColor = ConsoleColor.Blue;


        public ConsoleRenderer() : base() { }


        protected override object RenderNode(Node root)
        {
            if (root == null)
                return new object();

            if (ReflectionHelper.DynamicDispatch<object>(this, root) == null)
                Write(root.ToString(), LiteralColor);

            return new object();
        }

        private void Write(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
        }

        public object RenderNode(FunctionSelectorNode sel)
        {
            foreach (var node in sel.Traverse(Traversal.LeftRoots, false))
                RenderNode(node);

            return new object();
        }

        public object RenderNode(FunctionPatternNode pat)
        {
            Console.WriteLine();
            Write("Pattern Choice:", SelectorColor);
            RenderNode(pat.RightNode);

            return new object();
        }

        public object RenderNode(OperatorNode op)
        {
            if (op.Op is FunctionCallOperator || op.Op is FunctionDeclarationOperator)
            {
                var name = op.LeftNode.ToString();
                if (name.StartsWith("ANON"))
                    name = "@";
                else
                    name = "@" + name;

                Write(name + "(", FunctionColor);
                Render(op.RightNode);
                Write(")", FunctionColor);
                return new object();
            }

            if (op.Op is AnonAssignmentOperator)
            {
                Render(op.LeftNode);
                Write(op.Op.DisplayLabel, OperatorColor);
                Render(op.RightNode);
                return new object();
            }

            if (op.Op is CommaOperator)
            {
                bool first = true;
                foreach (Node node in ListHelper.GetNodeList(op))
                {
                    if(!first)
                        Write(",", LiteralColor);

                    first = false;
                    Render(node);
                }
                return new object();
            }

            if (op.Op is HeadOperator)
            {
                Write("h:", OperatorColor);
                Render(op.RightNode);
                return new object();
            }

            if (op.Op is TailOperator)
            {
                Write("t:", OperatorColor);
                Render(op.RightNode);
                return new object();
            }

            Render(op.LeftNode);            
            Write(" " + op.Op.DisplayLabel + " ", OperatorColor);
            Render(op.RightNode);

            return new object();
        }



    }
}
