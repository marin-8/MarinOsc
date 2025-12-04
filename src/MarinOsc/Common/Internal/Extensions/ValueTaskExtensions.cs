
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MarinOsc.Common.Internal.Extensions;

internal static class ValueTaskExtensions
{
	public static ConfiguredValueTaskAwaitable CAF (this ValueTask task)
		=> task.ConfigureAwait(false);

	public static ConfiguredValueTaskAwaitable<T> CAF<T> (this ValueTask<T> task)
		=> task.ConfigureAwait(false);
}
