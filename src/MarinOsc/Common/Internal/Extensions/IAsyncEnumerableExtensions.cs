
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MarinOsc.Common.Internal.Extensions;

internal static class IAsyncEnumerableExtensions
{
	public static ConfiguredCancelableAsyncEnumerable<T> CAF<T> (
		this IAsyncEnumerable<T> asyncEnumerable)
		=> asyncEnumerable.ConfigureAwait(false);
}
