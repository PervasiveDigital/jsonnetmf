//
// See LICENSE file in the project root for full license information.
//

using System;
using System.Text;

namespace PervasiveDigital.Json
{
    internal static class StringExtensions
    {
		public static bool Contains(this string source, string search)
		{
			return source.IndexOf(search) >= 0;
		}

		public static bool EndsWith(this string source, string search)
		{
			return source.IndexOf(search) == source.Length - search.Length;
		}

	}
}
