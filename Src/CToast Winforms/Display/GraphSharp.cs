using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphSharp;
using System.ComponentModel;

namespace CToast
{
    class GraphSharpEdge : GraphSharp.TypedEdge<Node>
    {
        public GraphSharpEdge(Node parent, Node child) : base(parent, child, GraphSharp.EdgeTypes.Hierarchical) { }
    }

    class GraphSharpGraph : GraphSharp.HierarchicalGraph<Node, GraphSharpEdge>
    {
        public GraphSharpGraph()
            : base()
        {
        }

        public GraphSharpGraph(bool a)
            : base(a)
        {
        }

        public GraphSharpGraph(bool a, int i) : base(a, i) { }
    }

    class GraphSharpRenderer : TreeRenderer<GraphSharpViewModel>
    {
        protected override GraphSharpViewModel RenderNode(Node root)
        {
            return new GraphSharpViewModel(RenderGraph(root));
        }

        private GraphSharpGraph RenderGraph(Node root)
        {
            var graph = new GraphSharpGraph();

            graph.AddVertexRange(root.Traverse(Traversal.Preorder, true));
            graph.AddEdgeRange(root.Traverse(Traversal.Preorder, true).SelectMany(p => this.CreateEdges(p)));
            return graph;
        }

        private IEnumerable<GraphSharpEdge> CreateEdges(Node root)
        {
            if (root.LeftNode != null)
                yield return new GraphSharpEdge(root, root.LeftNode);

            if (root.RightNode != null)
                yield return new GraphSharpEdge(root, root.RightNode);

        }
    }

    class NodeGraphLayout : GraphSharp.Controls.GraphLayout<Node, GraphSharpEdge, GraphSharpGraph> { }

    class GraphSharpViewModel : INotifyPropertyChanged
    {
        #region Data

        private string layoutAlgorithmType;
        private GraphSharpGraph graph;
        private List<String> layoutAlgorithmTypes = new List<string>();
        private int count;
        #endregion

        #region Ctor
        public GraphSharpViewModel(GraphSharpGraph g)
        {
            graph = g;

            //layoutAlgorithmTypes.Add("BoundedFR");
            //layoutAlgorithmTypes.Add("Circular");
            //layoutAlgorithmTypes.Add("CompoundFDP");
            //layoutAlgorithmTypes.Add("EfficientSugiyama");
            //layoutAlgorithmTypes.Add("FR");
            //layoutAlgorithmTypes.Add("ISOM");
            //layoutAlgorithmTypes.Add("KK");
            //layoutAlgorithmTypes.Add("LinLog");
            //layoutAlgorithmTypes.Add("Tree");

            LayoutAlgorithmType = "Tree";
        }
        #endregion


        #region Public Properties

        public string LayoutAlgorithmType
        {
            get { return layoutAlgorithmType; }
            set
            {
                layoutAlgorithmType = value;
                NotifyPropertyChanged("LayoutAlgorithmType");
            }
        }

        public GraphSharpGraph Graph
        {
            get { return graph; }
            set
            {
                graph = value;
                NotifyPropertyChanged("Graph");
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}
