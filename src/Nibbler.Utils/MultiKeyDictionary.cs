using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
	public class MultiKeyDictionary<TK1, TK2, TV> // todo : IDictionary<Tuple<TK1, TK2>, TV>
	{
		private readonly Dictionary<TK1, Dictionary<TK2, TV>> D
			= new Dictionary<TK1, Dictionary<TK2, TV>> ();

		private Dictionary<TK2, TV> Inner (TK1 k1, bool create)
		{
			Dictionary<TK2, TV> d;
			if (!D.TryGetValue (k1, out d) && create) {
				d = new Dictionary<TK2, TV> ();
				D.Add (k1, d);
			}
			return d;
		}

		public void Add (TK1 key1, TK2 key2, TV value)
		{
			Inner (key1, true).Add (key2, value);
		}

		public bool ContainsKey (TK1 key1, TK2 key2)
		{
			var d = Inner (key1, false);
			return d != null && d.ContainsKey (key2);
		}

		public bool TryGetValue (TK1 key1, TK2 key2, out TV value)
		{
			var d = Inner (key1, false);
			if (d == null) {
				value = default (TV);
				return false;
			}
			return d.TryGetValue (key2, out value);
		}

		/*
		public ICollection<Tuple<TK1, TK2>> Keys
		{
			get { throw new NotImplementedException (); }
		}

		public bool Remove (Tuple<TK1, TK2> key)
		{
			throw new NotImplementedException ();
		}

		public ICollection<TV> Values
		{
			get { throw new NotImplementedException (); }
		}

		public TV this[Tuple<TK1, TK2> key]
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		public void Add (KeyValuePair<Tuple<TK1, TK2>, TV> item)
		{
			throw new NotImplementedException ();
		}

		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		public bool Contains (KeyValuePair<Tuple<TK1, TK2>, TV> item)
		{
			throw new NotImplementedException ();
		}

		public void CopyTo (KeyValuePair<Tuple<TK1, TK2>, TV>[] array, int arrayIndex)
		{
			throw new NotImplementedException ();
		}

		public int Count
		{
			get { throw new NotImplementedException (); }
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException (); }
		}

		public bool Remove (KeyValuePair<Tuple<TK1, TK2>, TV> item)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator<KeyValuePair<Tuple<TK1, TK2>, TV>> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		*/
	}
}
