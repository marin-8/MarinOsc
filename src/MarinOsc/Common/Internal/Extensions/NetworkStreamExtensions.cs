
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MarinOsc.Common.Internal.Extensions;

internal static class NetworkStreamExtensions
{
	public static async Task ReadExactlyAsync (
		this NetworkStream stream,
		byte[] buffer,
		CancellationToken cancellationToken = default)
	{
		var offset = 0;
		var remaining = buffer.Length;

		while (remaining > 0)
		{
			var numberBytesRead =
				await stream.ReadAsync(buffer, offset, remaining, cancellationToken).CAF();

			if (numberBytesRead == 0)
				throw new EndOfStreamException(
					"Stream ended before reading enough bytes");

			offset += numberBytesRead;
			remaining -= numberBytesRead;
		}
	}
}
