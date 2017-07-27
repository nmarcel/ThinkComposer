// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : SimpleList.cs
// Object : Instrumind.Common.SimpleList (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.23 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides a collection of type single linked list that just allows items addition and sequential access.
    /// This is more fast that .NET based collections of recreating arrays.
    /// Plus, it implements an stack for fast access to relevant nodes pushed during previous accessing.
    /// </summary>
    /// <typeparam name="TData">Type of the contained data.</typeparam>
    [Serializable]
    public class SimpleList<TData> : IEnumerable<TData>
    {
        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        public SimpleList()
        { }

        public SimpleList(TData[] InitialArray)
        {
            this.Add(InitialArray);
        }

        public SimpleList(SimpleList<TData> InitialList)
        {
            this.Add(InitialList);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds new node at the end.
        /// </summary>
        public void Add(TData Data)
        {
            SimpleNode<TData> Node = new SimpleNode<TData>();
            Node.Data = Data;
            Node.NextNode = null;

            if (this.LastNode == null)
            {
                this.FirstNode = Node;
                this.Count = 1;
            }
            else
            {
                this.LastNode.NextNode = Node;
                this.Count++;
            }

            this.LastNode = Node;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Remove the first node.
        /// </summary>
        public void RemoveFirst()
        {
            if (this.FirstNode == null)
                throw new IndexOutOfRangeException();

            if (this.FirstNode == this.LastNode)
            {
                this.FirstNode = null;
                this.LastNode = null;
            }
            else
                this.FirstNode = this.FirstNode.NextNode;

            this.Count--;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds a set of new nodes, passed in an array, to the end.
        /// </summary>
        public void Add(TData[] DataArray)
        {
            foreach (TData Data in DataArray)
                this.Add(Data);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds a set of new nodes, passed in another list, to the end.
        /// </summary>
        public void Add(SimpleList<TData> DataList)
        {
            SimpleNode<TData> PointedNode = DataList.FirstNode;

            while (PointedNode != null)
            {
                this.Add(PointedNode.Data);
                PointedNode = PointedNode.NextNode;
            }
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns data of the first node.
        /// </summary>
        public TData GetInitialData()
        {
            if (this.FirstNode == null)
                return default(TData);

            return this.FirstNode.Data;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns data of the last node.
        /// </summary>
        public TData GetFinalData()
        {
            if (this.LastNode == null)
                return default(TData);

            return this.LastNode.Data;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets data of the first node. Returns whether the assignment was successful.
        /// </summary>
        public bool SetInitialData(TData Data)
        {
            if (this.FirstNode == null)
                return false;

            this.FirstNode.Data = Data;
            return true;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets data of the current node. Returns whether the assignment was successful.
        /// </summary>
        public bool SetCurrentData(TData Data)
        {
            if (this.CurrentNode == null)
                return false;

            this.CurrentNode.Data = Data;
            return true;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets data of the last node. Returns whether the assignment was successful.
        /// </summary>
        public bool SetFinalData(TData Data)
        {
            if (this.LastNode == null)
                return false;

            this.LastNode.Data = Data;
            return true;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the currently pointed node into the traveling stack.
        /// </summary>
        public void PushNode()
        {
            if (TravelingStack == null)
                TravelingStack = new Stack<SimpleNode<TData>>();  // CAUTION: Could not work inside an iterator.

            TravelingStack.Push(this.CurrentNode);
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Extracts the last stored node from the traveling stack making it the current. Returns whether the operation was successful.
        /// </summary>
        public bool PopNode()
        {
            if (TravelingStack == null || TravelingStack.Count < 1)
                return false;

            this.CurrentNode = TravelingStack.Pop();
            return true;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Resets the traveling stack.
        /// </summary>
        public void ResetStack()
        {
            if (TravelingStack != null)
                TravelingStack.Clear();
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes all nodes.
        /// </summary>
        public void Clear()
        {
            this.Count = 0;
            this.FirstNode = null;
            this.CurrentNode = null;
            this.LastNode = null;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Exposes generic enumerator.
        /// This implementation does not support concurrency.
        /// </summary>
        public IEnumerator<TData> GetEnumerator()
        {
            return (new InternalEnumerator(this));
        }

        /// <summary>
        /// Exposes standard enumerator (for compatiblity)
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (new InternalEnumerator(this));
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns an array containing the elements of the list.
        /// </summary>
        public TData[] GetArray()
        {
            TData[] Elements = new TData[this.Count];
            int Ind = 0;

            foreach (TData Data in this)
            {
                Elements[Ind] = Data;
                Ind++;
            }

            return Elements;
        }

        // -------------------------------------------------------------------------------------------
        /*
        /// <summary>
        /// Deletes node at the pointed position.
        /// CANCELLED: If a node is deleted it would impossible to update the traveling stack without workarounds.
        /// </summary>
        public void Delete(int Index)
        {
            SimpleNode<Tipo> PointedNode = this.FirstNode,
                             PriorNode = null;
            int Ind = 0;

            while (PointedNode != null)
            {
                if (Ind == Index)
                {
                    if (FirstNode == PointedNode)
                        FirstNode = PointedNode.NextNode;
                    else
                        PriorNode.NextNode = PointedNode.NextNode;

                    // Here the traveling stack should be updated, but cannot be done.
                    return;
                }
                Ind++;

                PriorNode = PointedNode;
                PointedNode = PointedNode.NextNode;
            }

            throw new IndexOutOfRangeException();
        } */

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns index of the container node of the supplied object, or -1 if not found.
        /// </summary>
        public int GetIndex(object Data)
        {
            SimpleNode<TData> PointedNode = this.FirstNode;
            int Ind = 0;

            while (PointedNode != null)
            {
                if (Object.Equals(PointedNode.Data, Data))
                    return Ind;
                Ind++;

                PointedNode = PointedNode.NextNode;
            }

            return -1;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets data of the node at the position of the supplied index.
        /// </summary>
        public TData this[int Index]
        {
            get
            {
                SimpleNode<TData> PointedNode = this.FirstNode;
                int Ind = 0;

                while (PointedNode != null)
                {
                    if (Ind == Index)
                        return PointedNode.Data;
                    Ind++;

                    PointedNode = PointedNode.NextNode;
                }

                throw new IndexOutOfRangeException();
            }
            set
            {
                SimpleNode<TData> PointedNode = this.FirstNode;
                int Ind = 0;

                while (PointedNode != null)
                {
                    if (Ind == Index)
                    {
                        PointedNode.Data = value;
                        return;
                    }
                    Ind++;

                    PointedNode = PointedNode.NextNode;
                }

                throw new IndexOutOfRangeException();
            }
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Enumerator to allow node traveling through iteration.
        /// </summary>
        internal class InternalEnumerator : IEnumerator<TData>, IEnumerator
        {
            /// <summary>
            /// Constructor
            /// </summary>
            internal InternalEnumerator(SimpleList<TData> List)
            {
                this.TraveledList = List;
            }

            /// <summary>
            /// Returns data of the current generic node.
            /// </summary>
            TData IEnumerator<TData>.Current
            {
                get
                {
                    if (this.TraveledNode == null)
                        throw new InvalidOperationException();

                    return this.TraveledNode.Data;
                }
            }

            /// <summary>
            /// Returns data of the current object node (for compatibility)
            /// </summary>
            object IEnumerator.Current
            {
                get
                {
                    return this.TraveledNode.Data;
                }
            }

            /// <summary>
            /// Advances to the next node, returning true if possible, else false.
            /// </summary>
            public bool MoveNext()
            {
                if (this.TraveledNode == null)
                    this.TraveledNode = this.TraveledList.FirstNode;
                else
                    this.TraveledNode = this.TraveledNode.NextNode;

                return (this.TraveledNode != null);
            }

            /// <summary>
            /// Reset
            /// </summary>
            public void Reset()
            {
                TraveledNode = null;
            }

            /// <summary>
            /// Resources freeing. Invocated after use as enumerator.
            /// </summary>
            public void Dispose()
            {
            }

            // Traveling control variables
            SimpleList<TData> TraveledList = null;
            SimpleNode<TData> TraveledNode = null;
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Number of currently existing nodes.
        /// </summary>
        public int Count { get; protected set; }

        /// <summary>
        /// Reference to the first node of the list.
        /// </summary>
        private SimpleNode<TData> FirstNode = null;

        /// <summary>
        /// Reference to the current node of the list.
        /// </summary>
        private SimpleNode<TData> CurrentNode = null;

        /// <summary>
        /// Reference to the last node of the list.
        /// </summary>
        private SimpleNode<TData> LastNode = null;

        /// <summary>
        /// Traveling stack for indicating nodes to be pointed again later.
        /// </summary>
        private Stack<SimpleNode<TData>> TravelingStack = null;
    }
}