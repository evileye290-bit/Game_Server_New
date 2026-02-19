using ServerFrame;
using System.Collections;
using System.Collections.Generic;

namespace ZoneServerLib
{
    public class MixSkillNode
    {
        public MixSkillNode Parent { get; private set; }
        public MixSkillNode Left { get; private set; }
        public MixSkillNode Right { get; private set; }

        public int Value { get; private set; } // 对应JobType的int值
        public int BuffId { get; private set; } // 对于叶子节点，对应buffId，非叶子节点无意义

        public bool Enabled { get; private set; } // 是否已被点亮

        public MixSkillNode(MixSkillNode parent, int min_value, int max_value)
        {
            Parent = parent;
            SetValue(min_value, max_value);
        }

        public void SetValue(int min_value, int max_value)
        { 
            Value = BaseApi.Random.Next(min_value, max_value+1);
        }

        public void SetLeft(MixSkillNode node)
        {
            Left = node;
        }

        public void SetRight(MixSkillNode node)
        {
            Right = node;
        }

        public bool IsLeaf()
        {
            return Left == null && Right == null;
        }

        public void SetBuffId(int buffId)
        {
            BuffId = buffId;
        }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }
    }

    public class MixSkillTree
    {
        private int depth;
        public int Depth
        { get { return depth; } }

        private int maxDepth;

        private int minValue;
        public int MinValue
        { get { return minValue; } }

        private int maxValue;
        public int MaxValue
        { get { return maxValue; } }

        private MixSkillNode root;
        public MixSkillNode Root { get { return root; } }

        private Queue<int> buffQueue;

        // 前序遍历对应的list
        public List<MixSkillNode> toList;
        public List<MixSkillNode> ToList { get { return toList; } }

        public MixSkillTree(int min_value, int max_value, int max_depth, List<int> buffList)
        {
            minValue = min_value;
            maxValue = max_value;
            maxDepth = max_depth;
            buffQueue = new Queue<int>();
            toList = new List<MixSkillNode>(8);

            BuildTree(root);
            if(buffList != null)
            {
                foreach(var buffId in buffList)
                {
                    buffQueue.Enqueue(buffId);
                }
                BindBuff(root);
            }
            TreeToList(root);
        }


        public void BuildTree(MixSkillNode parent)
        {
            if (parent == null && root == null)
            {
                root = new MixSkillNode(null, minValue, maxValue);
                parent = root;
            }
            parent.SetLeft(new MixSkillNode(parent, minValue, maxValue));
            parent.SetRight(new MixSkillNode(parent, minValue, maxValue));
            while (parent.Left.Value == parent.Right.Value)
            {
                parent.Right.SetValue(minValue, maxValue);
            }

            if (GetNodeDepth(parent.Left) >= maxDepth)
            {
                return;
            }

            BuildTree(parent.Left);
            BuildTree(parent.Right);
        }

        public int GetNodeDepth(MixSkillNode node)
        {
            int curDepth = 0;
            while (node != null)
            {
                curDepth++;
                node = node.Parent;
            }
            return curDepth;
        }

        private void BindBuff(MixSkillNode node)
        {
            if (node == null || buffQueue.Count == 0) return;
            if(node.IsLeaf())
            {
                node.SetBuffId(buffQueue.Dequeue());
                return;
            }
            BindBuff(node.Left);
            BindBuff(node.Right);
        }

        private void TreeToList(MixSkillNode node)
        {
            if (node == null) return;
            toList.Add(node);
            TreeToList(node.Left);
            TreeToList(node.Right);
        }

        private MixSkillNode GetBrotherNode(MixSkillNode node)
        {
            if (node == null || node.Parent == null) return null;
            if (node.Parent.Left == node) return node.Parent.Right;
            if (node.Parent.Right == node) return node.Parent.Left;
            return null;
        }

        public MixSkillNode EnbaleNodeByValue(MixSkillNode node, int value)
        {
            if (node == null) return null;
            // 如果节点未被点亮 则尝试点亮该节点
            if (!node.Enabled)
            {
                // 值不等 无法点亮
                if(node.Value != value)
                {
                    return null;
                }

                // 如果兄弟节点未被点亮 则可以
                MixSkillNode brother = GetBrotherNode(node);
                if(brother != null && brother.Enabled)
                {
                    return null;
                }

                node.SetEnabled(true);
                return node; ;
            }

            // 如果已经点亮 尝试点亮子节点
            MixSkillNode left = EnbaleNodeByValue(node.Left, value);
            if(left != null)
            {
                return left;
            }
            MixSkillNode right = EnbaleNodeByValue(node.Right, value);
            if( right != null)
            {
                return right;
            }
            return null;
        }

        // 尝试返回已经点亮的叶子节点
        public MixSkillNode CheckEnbaledLeaf(MixSkillNode node)
        {
            if (node == null || !node.Enabled) return null;

            if (node.IsLeaf()) return node;
            MixSkillNode left = CheckEnbaledLeaf(node.Left);
            if(left != null)
            {
                return left;
            }
            MixSkillNode right = CheckEnbaledLeaf(node.Right);
            if(right != null)
            {
                return right;
            }

            return null;
        }

    }
}
