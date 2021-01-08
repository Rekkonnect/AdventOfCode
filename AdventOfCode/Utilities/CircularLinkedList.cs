using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode.Utilities
{
    public class CircularLinkedList<T> : IList<T>
    {
        // And as always, documentation on the Garyon version
        private CircularLinkedListNode<T> head;

        private CircularLinkedListNode<T> lastIndexedNode;
        private int lastIndexedNodeIndex;

        public CircularLinkedListNode<T> Head
        {
            get => head;
            private set
            {
                head = value;
                if (Count == 1)
                    head.SetSelfLoopingHead();
                if (lastIndexedNode == null)
                    InitializeIndexedNode();
            }
        }

        public T First => Head.Next.Value;
        public T Last => Head.Previous.Value;

        public int Count { get; private set; }

        bool ICollection<T>.IsReadOnly => false;

        public CircularLinkedList() { }
        public CircularLinkedList(IEnumerable<T> values)
        {
            var first = values.First();
            Count++;
            Head = new(first);
            AddRange(values.Skip(1));
        }

        #region Insertion/Removal
        public void Add(T item)
        {
            head.Previous = new(item, head.Previous, head);
            HandleInsertion();
        }
        public void AddRange(IEnumerable<T> items)
        {
            foreach (var i in items)
                Add(i);
        }
        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index == -1)
                return false;

            var node = GetNode(index);
            node.Previous.Next = node.Next;
            HandleRemoval();

            return true;
        }

        public void Insert(int index, T item)
        {
            GetNode(index).Previous = new(item);
            HandleInsertion();
        }
        public void RemoveAt(int index)
        {
            var node = GetNode(index);
            HandleRemoval();

            if (Count > 0)
                node.Previous = node.Next;
        }

        public void Clear()
        {
            ResetState();
            Count = 0;
        }

        public void RemoveFirst()
        {
            if (HandleRemovalForLowCount())
                return;

            // This does not look finished?
            head = head.Next;
            HandleRemoval();
        }
        // Add RemoveLast too whenever it has to be implemented
        #endregion

        #region State Handling
        private bool HandleRemovalForLowCount()
        {
            if (Count > 1)
                return false;

            if (Count == 1)
                ResetState();

            return true;
        }

        private void HandleInsertion()
        {
            Count++;
        }
        private void HandleRemoval()
        {
            Count--;

            if (Count > 0)
                IndexPrevious();
            else
                ResetState();
        }
        private void ResetState()
        {
            head = null;
            lastIndexedNode = null;
            lastIndexedNodeIndex = 0;
        }
        private void InitializeIndexedNode()
        {
            lastIndexedNode = head;
            lastIndexedNodeIndex = 0;
        }
        #endregion

        #region Interface Implementations
        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            foreach (var node in this)
            {
                array[arrayIndex] = node;
                arrayIndex++;
                if (arrayIndex >= Count)
                    break;
            }
        }
        #endregion

        #region Indexing
        public CircularLinkedListNode<T> NodeOf(T item)
        {
            if (IndexOf(item) > -1)
                return lastIndexedNode;
            return null;
        }
        public int IndexOf(T item)
        {
            if (Count == 0)
                return -1;

            int firstIndex = lastIndexedNodeIndex;

            while (!lastIndexedNode.Value.Equals(item))
            {
                IndexNext();

                if (lastIndexedNodeIndex == firstIndex)
                    return -1;
            }

            return lastIndexedNodeIndex;
        }
        public bool Contains(T item) => IndexOf(item) > -1;

        public CircularLinkedListNode<T> GetNode(int index)
        {
            if (lastIndexedNode == null)
            {
                lastIndexedNode = Head;
                lastIndexedNodeIndex = 0;
            }

            if (lastIndexedNodeIndex == index)
                return lastIndexedNode;

            // This works?
            // I mean, it always works, but this aims to optimize seeking on large lists
            if ((lastIndexedNodeIndex > index) && ((lastIndexedNodeIndex - index) > (Count / 2)) ||
                (lastIndexedNodeIndex < index) && ((index - lastIndexedNodeIndex) < (Count / 2)))
            {
                IndexNextUntil(index);
            }
            else
            {
                IndexPreviousUntil(index);
            }

            return lastIndexedNode;
        }
        private void IndexNextUntil(int index)
        {
            while (lastIndexedNodeIndex != index)
                IndexNext();
        }
        private void IndexPreviousUntil(int index)
        {
            while (lastIndexedNodeIndex != index)
                IndexPrevious();
        }
        private void IndexNext()
        {
            lastIndexedNode = lastIndexedNode.Next;
            lastIndexedNodeIndex = (lastIndexedNodeIndex + 1) % Count;
        }
        private void IndexPrevious()
        {
            lastIndexedNode = lastIndexedNode.Previous;
            lastIndexedNodeIndex = (Count + lastIndexedNodeIndex - 1) % Count;
        }
        #endregion

        public T this[int index]
        {
            get => GetNode(index % Count).Value;
            set => GetNode(index % Count).Value = value;
        }

        #region Enumeration
        public IEnumerator<T> GetEnumerator() => new Enumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class Enumerator : IEnumerator<T>
        {
            private CircularLinkedListNode<T> currentNode;
            private CircularLinkedListNode<T> head;
            private bool enumeratedHead;

            public T Current => currentNode.Value;
            object IEnumerator.Current => Current;

            public Enumerator(CircularLinkedList<T> list)
            {
                currentNode = (head = list.Head).Previous;
            }

            public bool MoveNext()
            {
                if (currentNode == head)
                    enumeratedHead = true;
                currentNode = currentNode.Next;
                return !enumeratedHead || currentNode != head;
            }
            public void Reset() => currentNode = head.Previous;
            public void Dispose() { }
        }
        #endregion
    }
}
