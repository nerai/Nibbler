namespace Utils.BasicDataStructures
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Security.Permissions;

	[DebuggerDisplay ("Count = {Count}")]
	public class LinkedList2<T> :
		ICollection<T>,
		System.Collections.ICollection
		where T : LinkedListNode2<T>
	{
		// This LinkedList is a doubly-Linked circular list.
		internal T head;

		internal int count;
		internal int version;
		private Object _syncRoot;

		public LinkedList2 ()
		{
		}

		public LinkedList2 (IEnumerable<T> collection)
		{
			if (collection == null) {
				throw new ArgumentNullException ("collection");
			}

			foreach (var item in collection) {
				AddLast (item);
			}
		}

		public int Count
		{
			get { return count; }
		}

		public T First
		{
			get { return head; }
		}

		public T Last
		{
			get { return head == null ? null : head.prev; }
		}

		bool ICollection<T>.IsReadOnly
		{
			get { return false; }
		}

		void ICollection<T>.Add (T node)
		{
			AddLast (node);
		}

		public void AddAfter (T node, T newNode)
		{
			ValidateNode (node);
			ValidateNewNode (newNode);
			InternalInsertNodeBefore (node.next, newNode);
			newNode.list = this;
		}

		public void AddBefore (T node, T newNode)
		{
			ValidateNode (node);
			ValidateNewNode (newNode);
			InternalInsertNodeBefore (node, newNode);
			newNode.list = this;
			if (node == head) {
				head = newNode;
			}
		}

		public void AddFirst (T node)
		{
			ValidateNewNode (node);

			if (head == null) {
				InternalInsertNodeToEmptyList (node);
			}
			else {
				InternalInsertNodeBefore (head, node);
				head = node;
			}
			node.list = this;
		}

		public void AddLast (T node)
		{
			ValidateNewNode (node);

			if (head == null) {
				InternalInsertNodeToEmptyList (node);
			}
			else {
				InternalInsertNodeBefore (head, node);
			}
			node.list = this;
		}

		public void Clear ()
		{
			var current = head;
			while (current != null) {
				var temp = current;
				current = current.Next;   // use Next the instead of "next", otherwise it will loop forever
				temp.Invalidate ();
			}

			head = null;
			count = 0;
			version++;
		}

		public bool Contains (T node)
		{
			return node != null && node.list == this;
		}

		public void CopyTo (T[] array, int index)
		{
			if (array == null) {
				throw new ArgumentNullException ("array");
			}

			if (index < 0 || index > array.Length) {
				throw new ArgumentOutOfRangeException ("index", "IndexOutOfRange: " + index);
			}

			if (array.Length - index < Count) {
				throw new ArgumentException ("InsufficientSpace");
			}

			var node = head;
			if (node != null) {
				do {
					array[index++] = node;
					node = node.next;
				} while (node != head);
			}
		}

		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public void Remove (T node)
		{
			ValidateNode (node);
			InternalRemoveNode (node);
		}

		public void RemoveFirst ()
		{
			if (head == null) { throw new InvalidOperationException ("LinkedListEmpty"); }
			InternalRemoveNode (head);
		}

		public void RemoveLast ()
		{
			if (head == null) { throw new InvalidOperationException ("LinkedListEmpty"); }
			InternalRemoveNode (head.prev);
		}

		private void InternalInsertNodeBefore (T node, T newNode)
		{
			newNode.next = node;
			newNode.prev = node.prev;
			node.prev.next = newNode;
			node.prev = newNode;
			version++;
			count++;
		}

		private void InternalInsertNodeToEmptyList (T newNode)
		{
			Debug.Assert (head == null && count == 0, "LinkedList must be empty when this method is called!");
			newNode.next = newNode;
			newNode.prev = newNode;
			head = newNode;
			version++;
			count++;
		}

		internal void InternalRemoveNode (T node)
		{
			Debug.Assert (node.list == this, "Deleting the node from another list!");
			Debug.Assert (head != null, "This method shouldn't be called on empty list!");
			if (node.next == node) {
				Debug.Assert (count == 1 && head == node, "this should only be true for a list with only one node");
				head = null;
			}
			else {
				node.next.prev = node.prev;
				node.prev.next = node.next;
				if (head == node) {
					head = node.next;
				}
			}
			node.Invalidate ();
			count--;
			version++;
		}

		internal void ValidateNewNode (T node)
		{
			if (node == null) {
				throw new ArgumentNullException ("node");
			}

			if (node.list != null) {
				throw new InvalidOperationException ("LinkedListNodeIsAttached");
			}
		}

		internal void ValidateNode (T node)
		{
			if (node == null) {
				throw new ArgumentNullException ("node");
			}

			if (node.list != this) {
				throw new InvalidOperationException ("ExternalLinkedListNode");
			}
		}

		bool System.Collections.ICollection.IsSynchronized
		{
			get { return false; }
		}

		object System.Collections.ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) {
					System.Threading.Interlocked.CompareExchange<Object> (ref _syncRoot, new Object (), null);
				}
				return _syncRoot;
			}
		}

		void System.Collections.ICollection.CopyTo (Array array, int index)
		{
			if (array == null) {
				throw new ArgumentNullException ("array");
			}

			if (array.Rank != 1) {
				throw new ArgumentException ("Arg_MultiRank");
			}

			if (array.GetLowerBound (0) != 0) {
				throw new ArgumentException ("Arg_NonZeroLowerBound");
			}

			if (index < 0) {
				throw new ArgumentOutOfRangeException ("index", index, "IndexOutOfRange");
			}

			if (array.Length - index < Count) {
				throw new ArgumentException ("Arg_InsufficientSpace");
			}

			T[] tArray = array as T[];
			if (tArray == null) {
				throw new NotSupportedException ();
			}
			CopyTo (tArray, index);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		[SuppressMessage ("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario")]
		public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
		{
			private LinkedList2<T> list;
			private T node;
			private int version;
			private T current;
			private int index;

			const string LinkedListName = "LinkedList";
			const string CurrentValueName = "Current";
			const string VersionName = "Version";
			const string IndexName = "Index";

			internal Enumerator (LinkedList2<T> list)
			{
				this.list = list;
				version = list.version;
				node = list.head;
				current = default (T);
				index = 0;
			}

			public T Current
			{
				get { return current; }
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					if (index == 0 || (index == list.Count + 1)) {
						throw new InvalidOperationException ("InvalidOperation_EnumOpCantHappen");
					}

					return current;
				}
			}

			public bool MoveNext ()
			{
				if (version != list.version) {
					throw new InvalidOperationException ("InvalidOperation_EnumFailedVersion");
				}

				if (node == null) {
					index = list.Count + 1;
					return false;
				}

				++index;
				current = node;
				node = node.next;
				if (node == list.head) {
					node = null;
				}
				return true;
			}

			void System.Collections.IEnumerator.Reset ()
			{
				if (version != list.version) {
					throw new InvalidOperationException ("InvalidOperation_EnumFailedVersion");
				}

				current = default (T);
				node = list.head;
				index = 0;
			}

			public void Dispose ()
			{
			}
		}

		bool ICollection<T>.Remove (T item)
		{
			throw new NotImplementedException ();
		}
	}

	public class LinkedListNode2<T>
		where T : LinkedListNode2<T>
	{
		internal LinkedList2<T> list;
		internal T next;
		internal T prev;

		public LinkedListNode2 ()
		{
		}

		internal LinkedListNode2 (LinkedList2<T> list)
		{
			this.list = list;
		}

		public LinkedList2<T> List
		{
			get { return list; }
		}

		public T Next
		{
			get { return next == null || next == list.head ? null : next; }
		}

		public T Prev
		{
			get { return prev == null || this == list.head ? null : prev; }
		}

		internal void Invalidate ()
		{
			list = null;
			next = null;
			prev = null;
		}

		public IEnumerable<T> Right ()
		{
			var node = this.Next;
			while (node != null) {
				yield return node;
				node = node.Next;
			}
		}

		public IEnumerable<T> Left ()
		{
			var node = list.First;
			while (node != this) {
				yield return node;
				node = node.Next;
			}
		}

		public IEnumerable<T> LeftReversed ()
		{
			throw new NotImplementedException ();
		}

		public IEnumerable<T> RightReversed ()
		{
			throw new NotImplementedException ();
		}
	}
}
