
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using MarinOsc.Common.Internal.Exceptions;
using MarinOsc.Common.Internal.Extensions;

namespace MarinOsc.Common.Internal;

internal static class SlipEncoding
{
	#region public

	public static byte[] Encode (byte[] bytes)
	{
		var encodedByteArrayLength = CalculateByteArrayLength(bytes);

		var encodedByteArray = new byte[encodedByteArrayLength];

		var index = 0;

		encodedByteArray[index++] = SlipConstants.END;

		for (var i = 0; i < bytes.Length; i++)
		{
			var @byte = bytes[i];

			if (@byte == SlipConstants.END)
			{
				encodedByteArray[index++] = SlipConstants.ESC;
				encodedByteArray[index++] = SlipConstants.ESC_END;
			}
			else if (@byte == SlipConstants.ESC)
			{
				encodedByteArray[index++] = SlipConstants.ESC;
				encodedByteArray[index++] = SlipConstants.ESC_ESC;
			}
			else
			{
				encodedByteArray[index++] = @byte;
			}
		}

		encodedByteArray[index++] = SlipConstants.END;

		return encodedByteArray;
	}

	public static async IAsyncEnumerable<byte[]> DecodeAsync (
		NetworkStream networkStream, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		var readBuffer = new byte[1024];
		var returnBuffer = new byte[1024];

		var position = 0;

		var expectingEscapedByte = false;

		while (!cancellationToken.IsCancellationRequested)
		{
			var numberBytesRead =
				await networkStream.ReadAsync(
					readBuffer, 0, readBuffer.Length, cancellationToken)
				.CAF();

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
						throw new InvalidSlipEscapeSequenceException(readByte);
					}

					continue;
				}

				if (readByte == SlipConstants.END)
				{
					if (position > 0)
					{
						yield return returnBuffer.AsSpan(0, position).ToArray();
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
		public const byte END = 0xC0;     // 192 - Frame End
		public const byte ESC = 0xDB;     // 219 - Frame Escape
		public const byte ESC_END = 0xDC; // 220 - Transposed Frame End
		public const byte ESC_ESC = 0xDD; // 221 - Transposed Frame Escape
	}

	private static int CalculateByteArrayLength (byte[] bytes)
	{
		var endCharacterCount = 0;
		var escCharacterCount = 0;

		for (var i = 0; i < bytes.Length; i++)
		{
			var @byte = bytes[i];

			if (@byte == SlipConstants.END)
				endCharacterCount++;
			else if (@byte == SlipConstants.ESC)
				escCharacterCount++;
		}

		var encodedByteArrayLength =
			1 + bytes.Length + endCharacterCount + escCharacterCount + 1;

		return encodedByteArrayLength;
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
