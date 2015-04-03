using System;
using System.Windows.Forms;

namespace ADGV
{
    public enum TripleTreeNodeType : byte
    {
        Default,
        AllsNode,
        EmptysNode,
        MSecDateTimeNode,
        SecDateTimeNode,
        MinDateTimeNode,
        HourDateTimeNode,
        DayDateTimeNode,
        MonthDateTimeNode,
        YearDateTimeNode
    }

    public class TripleTreeNode : TreeNode
    {
        private CheckState checkState = CheckState.Unchecked;
        private TripleTreeNode parent;

        public TripleTreeNodeType NodeType { get; private set; }

        public object Value { get; private set; }

        new public TripleTreeNode Parent
        {
            get
            {
                if (this.parent is TripleTreeNode)
                    return this.parent;
                else
                    return null;
            }
            set
            {
                this.parent = value;
            }
        }

        new public Boolean Checked
        {
            get
            {
                return this.checkState == CheckState.Checked;
            }

            set
            {
                this.CheckState = (value == true ? CheckState.Checked : CheckState.Unchecked);
            }
        }

        public CheckState CheckState
        {
            get
            {
                return this.checkState;
            }
            set
            {
                this.checkState = value;
                SetCheckImage();
            }
        }

        private TripleTreeNode(String Text, object Value, CheckState State, TripleTreeNodeType NodeType)
            : base(Text)
        {
            this.CheckState = State;
            this.NodeType = NodeType;
            this.Value = Value;
        }

        public static TripleTreeNode CreateNode(String Text, object Value, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, Value, State, TripleTreeNodeType.Default);
        }

        public static TripleTreeNode CreateYearNode(String Text, object Value, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, Value, State, TripleTreeNodeType.YearDateTimeNode);
        }

        public static TripleTreeNode CreateMonthNode(String Text, object Value, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, Value, State, TripleTreeNodeType.MonthDateTimeNode);
        }

        public static TripleTreeNode CreateDayNode(String Text, object Value, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, Value, State, TripleTreeNodeType.DayDateTimeNode);
        }

        public static TripleTreeNode CreateHourNode(String Text, object Value, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, Value, State, TripleTreeNodeType.HourDateTimeNode);
        }

        public static TripleTreeNode CreateMinNode(String Text, object Value, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, Value, State, TripleTreeNodeType.MinDateTimeNode);
        }

        public static TripleTreeNode CreateSecNode(String Text, object Value, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, Value, State, TripleTreeNodeType.SecDateTimeNode);
        }

        public static TripleTreeNode CreateMSecNode(String Text, object Value, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, Value, State, TripleTreeNodeType.MSecDateTimeNode);
        }

        public static TripleTreeNode CreateEmptysNode(String Text, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, null, State, TripleTreeNodeType.EmptysNode);
        }

        public static TripleTreeNode CreateAllsNode(String Text, CheckState State = CheckState.Checked)
        {
            return new TripleTreeNode(Text, null, State, TripleTreeNodeType.AllsNode);
        }

        public TripleTreeNode CreateChildNode(String Text, object Value, CheckState State = CheckState.Checked)
        {
            TripleTreeNode n = null;
            switch (this.NodeType)
            {
                case TripleTreeNodeType.YearDateTimeNode:
                    n = TripleTreeNode.CreateMonthNode(Text, Value, State);
                    break;

                case TripleTreeNodeType.MonthDateTimeNode:
                    n = TripleTreeNode.CreateDayNode(Text, Value, State);
                    break;

                case TripleTreeNodeType.DayDateTimeNode:
                    n = TripleTreeNode.CreateHourNode(Text, Value, State);
                    break;

                case TripleTreeNodeType.HourDateTimeNode:
                    n = TripleTreeNode.CreateMinNode(Text, Value, State);
                    break;

                case TripleTreeNodeType.MinDateTimeNode:
                    n = TripleTreeNode.CreateSecNode(Text, Value, State);
                    break;

                case TripleTreeNodeType.SecDateTimeNode:
                    n = TripleTreeNode.CreateMSecNode(Text, Value, State);
                    break;

                default:
                    n = null;
                    break;
            }

            if (n != null)
                this.AddChild(n);

            return n;
        }

        public TripleTreeNode CreateChildNode(String Text, object Value)
        {
            return this.CreateChildNode(Text, Value, this.checkState);
        }

        public new TripleTreeNode Clone()
        {
            TripleTreeNode n = null;
            switch (this.NodeType)
            {
                case TripleTreeNodeType.YearDateTimeNode:
                    n = TripleTreeNode.CreateYearNode(this.Text, this.Value, this.checkState);
                    break;

                case TripleTreeNodeType.MonthDateTimeNode:
                    n = TripleTreeNode.CreateMonthNode(this.Text, this.Value, this.checkState);
                    break;

                case TripleTreeNodeType.DayDateTimeNode:
                    n = TripleTreeNode.CreateDayNode(this.Text, this.Value, this.checkState);
                    break;

                case TripleTreeNodeType.HourDateTimeNode:
                    n = TripleTreeNode.CreateHourNode(this.Text, this.Value, this.checkState);
                    break;

                case TripleTreeNodeType.MinDateTimeNode:
                    n = TripleTreeNode.CreateMinNode(this.Text, this.Value, this.checkState);
                    break;

                case TripleTreeNodeType.SecDateTimeNode:
                    n = TripleTreeNode.CreateSecNode(this.Text, this.Value, this.checkState);
                    break;

                case TripleTreeNodeType.MSecDateTimeNode:
                    n = TripleTreeNode.CreateMSecNode(this.Text, this.Value, this.checkState);
                    break;

                case TripleTreeNodeType.EmptysNode:
                    n = TripleTreeNode.CreateEmptysNode(this.Text, this.checkState);
                    break;

                case TripleTreeNodeType.AllsNode:
                    n = TripleTreeNode.CreateAllsNode(this.Text, this.checkState);
                    break;

                default:
                    n = TripleTreeNode.CreateNode(this.Text, this.Value, this.checkState);
                    break;
            }

            n.NodeFont = this.NodeFont;

            if (this.GetNodeCount(false) > 0)
            {
                foreach (TripleTreeNode child in this.Nodes)
                    n.AddChild(child.Clone());
            }

            return n;
        }

        private void SetCheckImage()
        {
            switch (this.checkState)
            {
                case CheckState.Checked:
                    this.StateImageIndex = 1;
                    break;

                case CheckState.Indeterminate:
                    this.StateImageIndex = 2;
                    break;

                default:
                    this.StateImageIndex = 0;
                    break;
            }
        }

        private void AddChild(TripleTreeNode child)
        {
            child.Parent = this;
            this.Nodes.Add(child);
        }
    }
}