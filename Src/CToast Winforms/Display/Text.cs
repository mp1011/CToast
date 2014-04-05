using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CToast
{
    class XMLRenderer : TreeRenderer<string>
    {
        private int mTabDepth = 0;

        private string CurrentIndent
        {
            get
            {
                return "".PadRight(mTabDepth * 5, ' ');
            }
        }

        protected override string RenderNode(Node root)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}<node value='{1}'>", this.CurrentIndent, root.ToString()); sb.AppendLine();

            mTabDepth++;

            if (root.LeftNode != null)
                sb.Append(this.Render(root.LeftNode));

            if (root.RightNode != null)
                sb.Append(this.Render(root.RightNode));

            mTabDepth--;
            sb.AppendFormat("{0}</node>", this.CurrentIndent); sb.AppendLine();

            return sb.ToString();
        }
    }

    class TextRenderer : TreeRenderer<string>
    {
        private int mTabDepth = 0;

        private string CurrentIndent
        {
            get
            {
                return "".PadRight(mTabDepth * 5, ' ');
            }
        }

        protected override string RenderNode(Node root)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}{1}", this.CurrentIndent, root.ToString()); sb.AppendLine();

            mTabDepth++;

            if (root.LeftNode != null)
                sb.Append(this.Render(root.LeftNode));

            if (root.RightNode != null)
                sb.Append(this.Render(root.RightNode));

            mTabDepth--;
            return sb.ToString();
        }
    }
}
