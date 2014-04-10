using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace CToast
{
    /// <summary>
    /// Adapted from http://billmill.org/pymag-trees/
    /// </summary>
    class BuchheimVisualNodeData
    {
        public int Mod { get; set; }
        public VisualTreeNode<BuchheimVisualNodeData> Ancestor { get; set; }
        public VisualTreeNode<BuchheimVisualNodeData> Thread { get; set; }

        public int Change { get; set; }
        public int Shift { get; set; }

        public BuchheimVisualNodeData()
        {
        }
    }

    static class BuchheimUtil
    {
        public static VisualTreeNode<BuchheimVisualNodeData> BcLeft(this VisualTreeNode<BuchheimVisualNodeData> node)
        {
            return node.Data.Thread ?? node.Children.FirstOrDefault();
        }

        public static VisualTreeNode<BuchheimVisualNodeData> BcRight(this VisualTreeNode<BuchheimVisualNodeData> node)
        {
            return node.Data.Thread ?? node.Children.LastOrDefault();
        }
    }

    /// <summary>
    /// Adapted from http://billmill.org/pymag-trees/
    /// </summary>
    class BuchheimLayout : TreeLayout<BuchheimVisualNodeData>
    {
        public override string Name
        {
            get { return "Buchheim Layout"; }
        }

        public override LayoutOrientation Orientation
        {
            get { return LayoutOrientation.TopDown; }
        }

        protected override VisualTreeNode<BuchheimVisualNodeData> CreateNewNode(Node n, int depth)
        {
            var vn = new VisualTreeNode<BuchheimVisualNodeData>(n) { Data = new BuchheimVisualNodeData() };
            vn.Data.Ancestor = vn;
            return vn;
        }

        protected override IEnumerable<ITreeLayoutStep<BuchheimVisualNodeData>> GetLayoutSteps()
        {
            yield return new BuchheimInit();
            yield return new BuchheimFirstWalk();
            yield return new BuchheimSecondWalk();
            yield return new SplitCollidingChildren<BuchheimVisualNodeData>();
        }

        class BuchheimInit : ITreeLayoutStep<BuchheimVisualNodeData>
        {
            public void DoLayout(VisualTreeNode<BuchheimVisualNodeData> tree, Size layoutArea)
            {
                if (tree == null)
                    return;

                tree.Position = new Point(-1, tree.Depth * tree.NodeBoundsWithPadding.Height);
                DoLayout(tree.LeftChild, layoutArea);
                DoLayout(tree.RightChild, layoutArea);
            }
        }

        class BuchheimFirstWalk: ITreeLayoutStep<BuchheimVisualNodeData>
        {
            public void DoLayout(VisualTreeNode<BuchheimVisualNodeData> tree, Size layoutArea)
            {
                FirstWalk(tree, tree.NodeBoundsWithPadding.Width);
            }

            private void FirstWalk(VisualTreeNode<BuchheimVisualNodeData> node, int distance)
            {
                if (node.Children.Count() == 0)
                {
                    if (node.LeftMostSibling != null)
                        node.X = node.LeftBrother.X + distance;
                    else
                        node.X = 0;
                }
                else
                {
                    var defaultAncestor = node.Children.First();
                    foreach (var child in node.Children)
                    {
                        FirstWalk(child, distance);
                        defaultAncestor = Apportion(child, defaultAncestor, distance);
                    }

                    ExecuteShifts(node);

                    var midPoint = (node.Children.First().X + node.Children.Last().X) / 2;
                    var w = node.LeftBrother;

                    if (w != null)
                    {
                        node.X = w.X + distance;
                        node.Data.Mod = node.X - midPoint;
                    }
                    else
                        node.X = midPoint;
                }
            }


            private VisualTreeNode<BuchheimVisualNodeData> Apportion(VisualTreeNode<BuchheimVisualNodeData> v, VisualTreeNode<BuchheimVisualNodeData> defaultAncestor, int distance)
            {
                var w = v.LeftBrother;
                if (w != null)
                {

                    //in buchheim notation:
                    //i == inner; o == outer; r == right; l == left;
                    var vir = v;
                    var vor = v;
                    var vil = w;
                    var vol = v.LeftMostSibling;
                    var sir = v.Data.Mod;
                    var sor = v.Data.Mod;
                    var sil = vil.Data.Mod;
                    var sol = vol.Data.Mod;

                    while (vil.BcRight() != null && vir.BcLeft() != null)
                    {
                        vil = vil.BcRight();
                        vir = vir.BcLeft();

                        if (vol != null)
                            vol = vol.BcLeft();

                        if (vor != null)
                            vor = vor.BcRight();

                        if (vor != null)
                            vor.Data.Ancestor = v;

                        //should detect and fix overlap
                        var shift = (vil.NodeBoundsWithPadding.Right + sil) - (vir.NodeBoundsWithPadding.Left + sir) + distance;
                        if (shift > 0)
                        {
                            var a = Ancestor(vil, v, defaultAncestor);
                            MoveSubtree(a, v, shift);
                            sir = sir + shift;
                            sor = sor + shift;
                        }

                        sil += vil.Data.Mod;
                        sir += vir.Data.Mod;

                        //todo
                        if (vol != null)
                            sol += vol.Data.Mod;
                        if (vor != null)
                            sor += vor.Data.Mod;
                    }

                    if (vil != null && vor != null && vil.BcRight() != null && vor.BcRight() == null)
                    {
                        vor.Data.Thread = vil.BcRight();
                        vor.Data.Mod += sil - sor;
                    }
                    else if (vir != null && vol != null)
                    {
                        if (vir.BcLeft() != null && vol.BcLeft() == null)
                        {
                            vol.Data.Thread = vir.BcLeft();
                            vol.Data.Mod += sir - sol;
                        }
                        defaultAncestor = v;
                    }
                }

                return defaultAncestor;
            }

            private void MoveSubtree(VisualTreeNode<BuchheimVisualNodeData> wl, VisualTreeNode<BuchheimVisualNodeData> wr, int shift)
            {
                var subtrees = wr.Index - wl.Index;

                wr.Data.Change -= shift / subtrees;
                wr.Data.Shift += shift;
                wl.Data.Change += shift / subtrees;
                wr.X += shift;
                wr.Data.Mod += shift;
            }

            private void ExecuteShifts(VisualTreeNode<BuchheimVisualNodeData> v)
            {
                var shift = 0;
                var change = 0;

                foreach (var w in v.Children.Reverse())
                {
                    w.X += shift;
                    w.Data.Mod += shift;
                    change += w.Data.Change;
                    shift += w.Data.Shift + change;
                }
            }

            private VisualTreeNode<BuchheimVisualNodeData> Ancestor(VisualTreeNode<BuchheimVisualNodeData> vil, VisualTreeNode<BuchheimVisualNodeData> v, VisualTreeNode<BuchheimVisualNodeData> defaultAncestor)
            {
                if (v.Parent.Children.Contains(vil.Data.Ancestor))
                    return vil.Data.Ancestor;
                else
                    return defaultAncestor;
            }

        
        }

        class BuchheimSecondWalk : ITreeLayoutStep<BuchheimVisualNodeData>
        {
            public void DoLayout(VisualTreeNode<BuchheimVisualNodeData> tree, Size layoutArea)
            {
                SecondWalk(tree, 0, 0);
            }

            private void SecondWalk(VisualTreeNode<BuchheimVisualNodeData> node, int m, int depth)
            {
                node.X += m;
                node.Y = depth; 

                foreach (var child in node.Children)
                    SecondWalk(child, m + node.Data.Mod, depth + node.NodeBoundsWithPadding.Height);
            }

        }



    }
}
