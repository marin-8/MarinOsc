
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MarinOsc1.Common.Internal;

internal static class SlipEncoding
{
	#region public

	public static byte[] Encode (ReadOnlyMemory<byte> memory)
	{
		using var memoryStream = new MemoryStream();

		memoryStream.WriteByte(SlipConstants.END);

		foreach (var @byte in memory.Span)
		{
			if (@byte == SlipConstants.END)
			{
				memoryStream.WriteByte(SlipConstants.ESC);
				memoryStream.WriteByte(SlipConstants.ESC_END);
			}
			else if (@byte == SlipConstants.ESC)
			{
				memoryStream.WriteByte(SlipConstants.ESC);
				memoryStream.WriteByte(SlipConstants.ESC_ESC);
			}
			else
			{
				memoryStream.WriteByte(@byte);
			}
		}

		memoryStream.WriteByte(SlipConstants.END);

		return memoryStream.ToArray();
	}

	public static async IAsyncEnumerable<ReadOnlyMemory<byte>> DecodeAsync (
		NetworkStream networkStream, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var readBuffer = new byte[256];
		var returnBuffer = new byte[256];

		var position = 0;

		var expectingEscapedByte = false;

		while (!cancellationToken.IsCancellationRequested)
		{
			var numberBytesRead =
				await networkStream.ReadAsync(
					readBuffer, 0, readBuffer.Length, cancellationToken);

			if (numberBytesRead == 0)
				yield break; // Connection closed

			for (var i = 0; i < numberBytesRead; i++)
			{
				var readByte = readBuffer[i];

				if (expectingEscapedByte)
				{
					expectingEscapedByte = false;

					if (readByte == SlipConstants.ESC_END)
					{
						EnsureCapacity(ref returnBuffer, position + 1);
						returnBuffer[position++] = SlipConstants.END;
					}
					else if (readByte == SlipConstants.ESC_ESC)
					{
						EnsureCapacity(ref returnBuffer, position + 1);
						returnBuffer[position++] = SlipConstants.ESC;
					}
					else
					{
						throw new InvalidDataException(
							$"Invalid SLIP escape sequence: 0x{readByte:X2}");
					}

					continue;
				}

				if (readByte == SlipConstants.END)
				{
					if (position > 0)
					{
						yield return new ReadOnlyMemory<byte>(returnBuffer, 0, position);
						position = 0;
					}
				}
				else if (readByte == SlipConstants.ESC)
				{
					expectingEscapedByte = true;
				}
				else
				{
					EnsureCapacity(ref returnBuffer, position + 1);
					returnBuffer[position++] = readByte;
				}
			}
		}
	}

	#endregion public
	#region private

	private static class SlipConstants
	{
		public const byte END =     0xC0; // 192 - Frame End
		public const byte ESC =     0xDB; // 219 - Frame Escape
		public const byte ESC_END = 0xDC; // 220 - Transposed Frame End
		public const byte ESC_ESC = 0xDD; // 221 - Transposed Frame Escape
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void EnsureCapacity (ref byte[] buffer, int requiredSize)
	{
		if (requiredSize > buffer.Length)
		{
			var newSize = Math.Max(requiredSize, buffer.Length * 2);
			Array.Resize(ref buffer, newSize);
		}
	}

	#endregion private
}
