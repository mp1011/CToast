using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VNode = CToast.VisualTreeNode<CToast.RadialTreeNodeData>;
using System.Drawing;

namespace CToast
{
    class RadialTreeNodeData
    {
        public int RangeStart { get; set; }
        public int RangeSweep { get; set; }
    }

    class RadialLayout : TreeLayout<RadialTreeNodeData>
    {
        public override string  Name
        {
	        get { return "Radial";}
        }

        public override LayoutOrientation Orientation
        {
            get { return LayoutOrientation.Center; }
        }

        protected override VNode  CreateNewNode(Node n, int depth)
        {
            return new VNode(n) { Data = new RadialTreeNodeData() };
        }

        protected override IEnumerable<ITreeLayoutStep<RadialTreeNodeData>> GetLayoutSteps()
        {
            yield return new RadialLayoutStep();
        }

        class RadialLayoutStep : ITreeLayoutStep<RadialTreeNodeData>
        {

            public void DoLayout(VisualTreeNode<RadialTreeNodeData> root, Size layoutArea)
            {
                List<int> levelRadiuses = new List<int>();
                levelRadiuses.Add(30);
                levelRadiuses.Add(20);
                levelRadiuses.Add(10);
                levelRadiuses.Add(8);
                while (levelRadiuses.Count < root.TreeDepth)
                    levelRadiuses.Add(8);

                Point center = Point.Empty;
                LayoutNode(center, center, levelRadiuses, 0, root,0,360);
                LayoutChildNodes(center, center, levelRadiuses, 0, 360, root);

            }

            private void LayoutChildNodes(Point center, Point thisNodeLocation, List<int> levelRadiuses, int parentRangeStart, int parentRangeSweep, VNode node)
            {

                if (node.LeftTree != null && node.RightTree != null)
                {
                    var count1 = node.LeftTree.ChildNodeCount+1;
                    var count2 = node.RightTree.ChildNodeCount + 1;

                    var pct1 = (float)count1 / (float)(count1 + count2);

                    var limit = .1f;
                    if (pct1 < limit)
                        pct1 = limit;
                    else if (pct1 > 1 - limit)
                        pct1 = 1 - limit;

                    int leftStart = parentRangeStart;
                    int leftSweep = (int)(parentRangeSweep * pct1);

                    int rightStart = leftStart + leftSweep;
                    int rightSweep = (int)(parentRangeSweep * (1 - pct1));

                    var leftLoc = LayoutNode(center, thisNodeLocation, levelRadiuses, leftStart + (leftSweep / 2), node.LeftChild,leftStart,leftSweep);
                    LayoutChildNodes(center, leftLoc, levelRadiuses, leftStart, leftSweep, node.LeftChild);

                    var rightLoc = LayoutNode(center, thisNodeLocation, levelRadiuses, rightStart + (rightSweep / 2), node.RightChild,rightStart,rightSweep);
                    LayoutChildNodes(center, rightLoc, levelRadiuses, rightStart, rightSweep, node.RightChild);
                }
                else if (node.LeftTree != null)
                {
                    int leftStart = parentRangeStart;
                    int leftSweep = parentRangeSweep;

                    var leftLoc = LayoutNode(center, thisNodeLocation, levelRadiuses, leftStart + (leftSweep / 2), node.LeftChild, leftStart, leftSweep);
                    LayoutChildNodes(center, leftLoc, levelRadiuses, leftStart, leftSweep, node.LeftChild);
                }
                else if (node.RightTree != null)
                {
                    int rightStart = parentRangeStart;
                    int rightSweep = parentRangeSweep;

                    var rightLoc = LayoutNode(center, thisNodeLocation, levelRadiuses, rightStart + (rightSweep / 2), node.RightChild, rightStart, rightSweep);
                    LayoutChildNodes(center, rightLoc, levelRadiuses, rightStart, rightSweep, node.RightChild);
                }
            }

            private Point LayoutNode(Point center, Point parentLocation, List<int> levelRadiuses, int degree, VNode node, int start, int sweep)
            {
                node.Data.RangeStart = Util.FixAngle(start);
                node.Data.RangeSweep = sweep;

                degree = Util.FixAngle(degree);
                
                int distanceFromCenter = 0;

                int level = node.Depth;
                if (level > 0)
                    distanceFromCenter = levelRadiuses[0] + (levelRadiuses.Skip(1).Take(level - 1).Sum() * 2) + levelRadiuses[level];

                var radius = levelRadiuses[level];

                Point location = center;
                Point offset = Util.AngleToXY(degree, distanceFromCenter);

                node.Position = new Point(center.X + offset.X, center.Y + offset.Y);
                return node.Position;
            }

        }


    }

}
