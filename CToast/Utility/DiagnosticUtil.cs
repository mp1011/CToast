using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    static class DiagnosticUtil
    {
        private static bool mReady = false;

        public static void Trace(Context context, params string[] args)
        {
            //if (context.IsImportingLibraries)
            //    return;

            //if (!mReady)
            //{
            //    mReady = true;
            //    System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.TextWriterTraceListener("diagnostic.csv"));
            //}

            //System.Diagnostics.Trace.WriteLine(DateTime.Now.Ticks + "," + String.Join(",", args.Select(p => @"""" + p.Replace(@"""", "'") + @"""")));
            //System.Diagnostics.Trace.Flush();
        }

    }

    public class TreeTestException : Exception
    {
        public Node Node { get; private set; }

        public TreeTestException(Node node)
            : base("Tree Test Failed")
        {
            this.Node = node;
        }
    }

    public class TreeTest
    {
        public static bool TestEnabled { get; set; }
        public static TreeTest CurrentTest { get; set; }

        public static void Run(Node node)
        {
            if (!TestEnabled)
                return; 

            if (CurrentTest == null)
            {
                CurrentTest = new TreeTest(",", ",", ",", "head", "L>>&", "@","@");
            }

            CurrentTest.BreakOnFailure(node, false);
        }


        public bool BreakOnFailure(Node root, bool throwError)
        {
            var node = TraverseToNode(root);
            if (node != null)
            {
                if (throwError)
                    throw new TreeTestException(node);
                else
                    return false;
            }
            else
                return true;

        }


        private string[] mTraversal;

        public TreeTest(params string[] traversal)
        {
            mTraversal = traversal;
        }

        public Node TraverseToNode(Node root)
        {
            return TraverseToNode(root, 0);
        }

        private Node TraverseToNode(Node node, int index)
        {
            if(node == null || index >= mTraversal.Length)
                return null;

            string name = mTraversal[index];

            bool leftOnly = name.StartsWith("L>>");
            bool rightOnly = name.StartsWith("R>>");

            if(leftOnly || rightOnly)
                name = name.Substring(3);

            if (node.ToString() != name)            
               return null;

            if (index == mTraversal.Length - 1)
                return node;

            if (leftOnly)
                return TraverseToNode(node.LeftNode, index + 1);
            else if(rightOnly)
                return TraverseToNode(node.RightNode, index + 1);
            else 
                return TraverseToNode(node.LeftNode,index+1) ?? TraverseToNode(node.RightNode,index+1);
        }

    }
}
