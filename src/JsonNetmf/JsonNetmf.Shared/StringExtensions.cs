using System;
using System.Text;

#if (MF_FRAMEWORK_VERSION_V4_3 || MF_FRAMEWORK_VERSION_V4_4)
namespace PervasiveDigital.Json
#else
namespace GHIElectronics.TinyCLR.Data.Json
#endif
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
