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
// File   : ConstrainedStack.cs
// Object : Instrumind.Common.ConstrainedStack (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.21 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Stack data structure made of linked nodes, for which size can be limited and the support for overloads (discarding the oldest node and shifting the rest).
    /// </summary>
    /// <typeparam name="TValue">Type of the values to be stored.</typeparam>
    public class ConstrainedStack<TValue> : IEnumerable<TValue>
    {
        /// <summary>
        /// Maximum number of nodes to be contained by the stack, or zero when no maximum is defined.
        /// </summary>
        public int MaxSize { get; protected set; }

        /// <summary>
        /// Indicates whether to support attempts to push new nodes, when the maximum size is reached, by discarding the oldest node and shifting the rest.
        /// </summary>
        public bool SupportOverload { get; protected set; }

        /// <summary>
        /// Internal store of the stack.
        /// </summary>
        private LinkedList<TValue> StackList = new LinkedList<TValue>();

        /// <summary>
        /// Current number of nodes of the stack.
        /// </summary>
        public int Count { get { return this.StackList.Count; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="MaxSize">Maximum number of nodes to be contained by the stack, or zero when no maximum is defined.</param>
        /// <param name="SupportOverload">Indicates whether to support attempts to push new nodes, when the maximum size is reached, by discarding the oldest node and shifting the rest.</param>
        public ConstrainedStack(int MaxSize = 0, bool SupportOverload = false)
        {
            this.MaxSize = MaxSize;
            this.SupportOverload = SupportOverload;
        }

        /// <summary>
        /// Adds the specified value to the top of the stack.
        /// If maximum size plus one is reached, then an exception is thrown if no overload is suppported else the oldest node is removed and the remainings are shifted.
        /// </summary>
        public void Push(TValue Value)
        {
            if (this.MaxSize > 0 && this.StackList.Count == this.MaxSize)
                if (this.SupportOverload)
                {
                    LinkedListNode<TValue> TempNode =this.StackList.First;

                    while (TempNode.Next != null)
                    {
                        TempNode.Value = TempNode.Next.Value;
                        TempNode = TempNode.Next;
                    }

                    TempNode.Value = Value;
                    return;
                }
                else
                    throw new UsageAnomaly("Cannot exceed maximum size of the stack.", this.MaxSize);

            this.StackList.AddLast(Value);
        }

        /// <summary>
        /// Takes and removes the top value of the stack, and returns it.
        /// </summary>
        public TValue Pop()
        {
            var LastValue = this.StackList.Last.Value;
            this.StackList.RemoveLast();

            return LastValue;
        }

        /// <summary>
        /// Takes the top value of the stack and returns it, without removing.
        /// </summary>
        public TValue Peek()
        {
            return this.StackList.Last.Value;
        }

        /// <summary>
        /// Discards all the nodes of the stack.
        /// </summary>
        public void Clear()
        {
            this.StackList.Clear();
        }


        #region IEnumerable<TNode> Members

        public IEnumerator<TValue> GetEnumerator()
        {
            return this.StackList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.StackList.GetEnumerator();
        }

        #endregion
    }
}