using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nibbler.Utils
{
	public static class EnumerableExtensions
	{
		public static IEnumerable<TSource[]> Combine<TSource> (
			this IEnumerable<IEnumerable<TSource>> sources)
		{
			var enums = sources
				.Select (s => s.GetEnumerator ())
				.ToArray ();

			while (enums.All (e => e.MoveNext ())) {
				yield return enums.Select (e => e.Current).ToArray ();
			}
		}

		public static IEnumerable<TSource[]> Combine<TSource> (
			params IEnumerable<TSource>[] sources)
		{
			return sources.Combine ();
		}

		public static T[] SubArray<T> (this T[] data, int index, int length)
		{
			T[] result = new T[length];
			Array.Copy (data, index, result, 0, length);
			return result;
		}

		public static T[] SubArray<T> (this T[] data, int index)
		{
			return data.SubArray (index, data.Length - index);
		}
	}
}
