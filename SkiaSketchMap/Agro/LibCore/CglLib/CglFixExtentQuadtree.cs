using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agro.LibCore.CglLib
{
    /// <summary>
    ///  具有固定初始范围的死叉搜索树
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CglFixExtentQuadtree<T> 
    {
        private readonly Node _root;
        /// <summary>
        /// 四叉树节点的最小尺寸
        /// </summary>
        private readonly double _minNodeSize;
        /// <summary>
        /// 用固定初始范围和节点最小尺寸构造新实例
        /// </summary>
        /// <param name="fullExtent"></param>
        /// <param name="MinNodeSize"></param>
        public CglFixExtentQuadtree(CglSquare fullExtent, double MinNodeSize)
        {
            _root = new Node(fullExtent);
            _minNodeSize = MinNodeSize;
        }
        /// <summary>
        /// 插入空间对象（itemEnv为item的最小外包矩形）
        /// </summary>
        /// <param name="itemEnv"></param>
        /// <param name="item"></param>
        public void Insert(CglEnvelope itemEnv, T item)
        {
            if (!Extent.IsIntersect(itemEnv.MinX,itemEnv.MinY,itemEnv.MaxX,itemEnv.MaxY))
                return;
            _root.Insert(itemEnv, item, _minNodeSize);
        }
        /// <summary>
        /// 查询与线段有可能相交的空间对象
        /// </summary>
        /// <param name="ls"></param>
        /// <returns></returns>
        public List<T> Query(CglLineSegment ls)
        {
            List<T> lst = new List<T>();
            _root.Query(ls, lst);
            return lst;
        }
        /// <summary>
        /// 四叉树的搜索范围
        /// </summary>
        public CglSquare Extent { get { return _root.Envelope; } }

        internal class Node
        {
            protected readonly CglSquare _env;
            private readonly CglPoint _centre;
            /// <summary>
            /// 
            /// </summary>
            protected List<T> _items = new List<T>();
            /// <summary>
            /// subquads are numbered as follows:
            /// 2 | 3
            /// --+--
            /// 0 | 1
            /// </summary>
            protected Node[] Subnode = new Node[4];
            internal Node(CglSquare square)
            {
                _env = square;
                _centre = new CglPoint((_env.MinX + _env.MaxX) / 2, (_env.MinY + _env.MaxY) / 2);
            }
            /// <summary> 
            /// Returns the index of the subquad that wholly contains the given envelope.
            /// If none does, returns -1.
            /// </summary>
            /// <param name="env"></param>
            /// <param name="centre"></param>
            private static int GetSubnodeIndex(CglEnvelope env, CglPoint centre)
            {
                int subnodeIndex = -1;
                if (env.MinX >= centre.X)
                {
                    if (env.MinY >= centre.Y)
                        subnodeIndex = 3;
                    if (env.MaxY <= centre.Y)
                        subnodeIndex = 1;
                }
                if (env.MaxX <= centre.X)
                {
                    if (env.MinY >= centre.Y)
                        subnodeIndex = 2;
                    if (env.MaxY <= centre.Y)
                        subnodeIndex = 0;
                }
                return subnodeIndex;
            }
            /// <summary> 
            /// Insert an item into the quadtree this is the root of.
            /// </summary>
            internal void Insert(CglEnvelope itemEnv, T item, double MinNodeSize)
            {
                //Add(item);
                //return;
                if (_env.Size / 2.0 <= MinNodeSize)
                {
                    Add(item);
                    return;
                }
                int index = GetSubnodeIndex(itemEnv, _centre);
                // if index is -1, itemEnv must cross the X or Y axis.
                if (index == -1)
                {
                    Add(item);
                    return;
                }
                var node = Subnode[index];
                if (node == null)
                {
                    //Square s = GetSubNodeSquare(index);
                    //System.Diagnostics.Debug.Assert(s.LeftX >= _env.LeftX);
                    //System.Diagnostics.Debug.Assert(s.TopY >= _env.TopY);
                    //System.Diagnostics.Debug.Assert(s.MaxX <= _env.MaxX);
                    //System.Diagnostics.Debug.Assert(s.MaxY <= _env.MaxY);
                    node = new Node(GetSubNodeSquare(index));
                    Subnode[index] = node;
                }
                node.Insert(itemEnv, item, MinNodeSize);
            }
            internal void Query(CglLineSegment ls, List<T> visitor)
            {
                if (_env.IsIntersect(ls))
                {
                    visitor.AddRange(_items);
                    for (int i = 0; i < Subnode.Length; ++i)
                    {
                        if (Subnode[i] != null)
                        {
                            Subnode[i].Query(ls, visitor);
                        }
                    }
                }
            }
            internal bool IsLeafNode()
            {
                return Subnode[0] == null && Subnode[1] == null && Subnode[2] == null && Subnode[3] == null;
            }
            internal int GetItemCount()
            {
                int n = _items.Count;
                for (int i = 0; i < Subnode.Length; ++i)
                {
                    if (Subnode[i] != null)
                        n += Subnode[i].GetItemCount();
                }
                return n;
            }
            internal void VisitorItems(List<T> visitor)
            {
                //visitor.AddRange(_items);
                //if (IsLeafNode())
                visitor.AddRange(_items);
                //if (Subnode[1] != null)
                //{
                //    Subnode[1].VisitorItems(visitor);
                //}
                //return;
                foreach (Node subnode in Subnode)
                {
                    if (subnode != null)
                        subnode.VisitorItems(visitor);
                }
            }
            /// <summary>
            /// subquads are numbered as follows:
            /// 2 | 3
            /// --+--
            /// 0 | 1
            /// </summary>
            /// <param name="subnodeIndex"></param>
            /// <returns></returns>
            private CglSquare GetSubNodeSquare(int subnodeIndex)
            {
                CglSquare s = _env;
                s.Size /= 2;
                if (subnodeIndex == 1)
                {
                    s.MinX += s.Size;
                }
                else if (subnodeIndex == 2)
                {
                    s.MinY += s.Size;
                }
                else if (subnodeIndex == 3)
                {
                    s.MinX += s.Size;
                    s.MinY += s.Size;
                }
                return s;
            }
            #region debug
            public int GetNodeCount()
            {
                //if (IsLeafNode())
                //    return 1;
                int n = 1;
                for (int i = 0; i < 4; ++i)
                {
                    if (Subnode[i] != null)
                    {
                        n += Subnode[i].GetNodeCount();
                    }
                }
                return n;
            }
            public void EnumNodes(List<Node> visitor)
            {
                //if(IsLeafNode())
                visitor.Add(this);
                foreach (Node n in Subnode)
                {
                    if (n != null)
                    {
                        n.EnumNodes(visitor);
                    }
                }
            }
            private void QueryNodes(CglLineSegment ls, List<Node> visitor)
            {
                if (_env.IsIntersect(ls))
                {
                    visitor.Add(this);
                    if (IsLeafNode())
                    {
                        System.Diagnostics.Debug.Assert(true);
                    }
                    for (int i = 0; i < Subnode.Length; ++i)
                    {
                        if (Subnode[i] != null)
                        {
                            Subnode[i].QueryNodes(ls, visitor);
                        }
                    }
                }
            }

            public List<T> Value { get { return _items; } }
            public CglSquare Envelope { get { return _env; } }
            #endregion
            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            private void Add(T item)
            {
                _items.Add(item);
            }
        }
    }
}
