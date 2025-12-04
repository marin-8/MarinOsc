
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MarinOsc.Common.Internal;
using MarinOsc.Common.Internal.Exceptions;

namespace MarinOsc.Common;

public static partial class OscEncoding
{
	#region public

	public static byte[] EncodeWithNoFraming (this in OscMessage oscMessage)
	{
		var byteArrayLength = CalculateByteArrayLength(oscMessage);

		var bytes = new byte[byteArrayLength];

		var index = 0;

		EncodeWithNoFramingInternal(oscMessage, bytes, ref index);

		return bytes;
	}

	public static byte[] EncodeWithSizePrefixFraming (this in OscMessage oscMessage)
	{
		var byteArrayLength = CalculateByteArrayLength(oscMessage);

		var bytes = new byte[byteArrayLength + 4]; // size prefix

		var index = 0;

		WriteInt(bytes, ref index, byteArrayLength);

		EncodeWithNoFramingInternal(oscMessage, bytes, ref index);

		return bytes;
	}

	#endregion public
	#region private

	private static int CalculateByteArrayLength (in OscMessage oscMessage)
	{
		var addressByteCount = CalculateAdressByteCount(oscMessage.Address);
		var argumentsTypeTagsByteCount = CalculateArgumentsTypeTagsByteCount(oscMessage.Arguments);
		var argumentsByteCount = CalculateArgumentsByteCount(oscMessage.Arguments);

		return addressByteCount + argumentsTypeTagsByteCount + argumentsByteCount;
	}

	private static int CalculateAdressByteCount (in OscAddress oscAddress)
	{
		var adressWithNullTerminatorByteCount =
			_AsciiEncoding.GetByteCount(oscAddress) + 1; // null terminator

		return RoundUpToMultipleOf4(adressWithNullTerminatorByteCount);
	}

	private static int CalculateArgumentsTypeTagsByteCount (in IReadOnlyList<OscArgument>? arguments)
	{
		if (arguments is null)
			return 4; // comma and 3 null terminators (for it to be a multiple of 4)

		return RoundUpToMultipleOf4(arguments.Count + 2); // comma and null terminator
	}

	private static int CalculateArgumentsByteCount (in IReadOnlyList<OscArgument>? arguments)
	{
		if (arguments is null) return 0;

		var byteCount = 0;

		for (var i = 0; i < arguments.Count; i++)
		{
			var argument = arguments[i];

			switch (argument.TypeTag)
			{
				case OscTypeTag.Int32:
				case OscTypeTag.Float32:
					byteCount += 4;
					break;

				case OscTypeTag.OscString:
					var argumentValueString = argument.Value as string;
					var asciiEncodedByteCount = _AsciiEncoding.GetByteCount(argumentValueString);
					byteCount += RoundUpToMultipleOf4(asciiEncodedByteCount + 1); // null terminator
					break;

				case OscTypeTag.True:
				case OscTypeTag.False:
				case OscTypeTag.Nil:
					break;

				default: throw new UnsupportedEnumValueException<OscTypeTag>(argument.TypeTag);
			}
		}

		return byteCount;
	}

	private static void EncodeWithNoFramingInternal (
		in OscMessage oscMessage, in byte[] bytes, ref int index)
	{
		WriteString(bytes, ref index, oscMessage.Address);
		WriteTypeTags(bytes, ref index, oscMessage.Arguments);

		if (oscMessage.Arguments is not null)
			WriteArguments(bytes, ref index, oscMessage.Arguments);
	}

	private static void WriteString (in byte[] bytes, ref int index, in string @string)
	{
		var numberBytesWritten =
			_AsciiEncoding.GetBytes(
				@string, 0, @string.Length, bytes, index);

		index += numberBytesWritten;

		bytes[index++] = AsciiCharacters.Null;

		ApplyNullPadding(bytes, ref index);
	}

	private static void WriteTypeTags (
		in byte[] bytes, ref int index, in IReadOnlyList<OscArgument>? arguments)
	{
		bytes[index++] = AsciiCharacters.Comma;

		if (arguments is null)
			return;

		for (var i = 0; i < arguments.Count; i++)
			bytes[index++] = (byte)arguments[i].TypeTag;

		ApplyNullPadding(bytes, ref index);
	}

	private static void WriteArguments (
		in byte[] bytes, ref int index, in IReadOnlyList<OscArgument> arguments)
	{
		for (var i = 0; i < arguments.Count; i++)
		{
			var argument = arguments[i];

			switch (argument.TypeTag)
			{
				case OscTypeTag.Int32:
					WriteInt(bytes, ref index, (int)argument.Value!);
					break;

				case OscTypeTag.Float32:
					WriteFloat(bytes, ref index, (float)argument.Value!);
					break;

				case OscTypeTag.OscString:
					WriteString(bytes, ref index, (string)argument.Value!);
					break;

				case OscTypeTag.True:
				case OscTypeTag.False:
				case OscTypeTag.Nil:
					break;

				default: throw new UnsupportedEnumValueException<OscTypeTag>(argument.TypeTag);
			}
		}
	}

	private static void WriteInt (in byte[] bytes, ref int index, in int value)
	{
		bytes[index + 0] = (byte)((value >> 24) & 0xFF);
		bytes[index + 1] = (byte)((value >> 16) & 0xFF);
		bytes[index + 2] = (byte)((value >> 8) & 0xFF);
		bytes[index + 3] = (byte)(value & 0xFF);

		index += 4;
	}

	private static void WriteFloat (in byte[] bytes, ref int index, float value)
	{
		var floatAsInt = Internal.BitConverter.SingleToInt32Bits(value);
		WriteInt(bytes, ref index, floatAsInt);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int RoundUpToMultipleOf4 (in int value) => (value + 3) & ~0x3;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ApplyNullPadding (in byte[] bytes, ref int index)
	{
		var nullPadSize = RoundUpToMultipleOf4(index) - index;

		if (nullPadSize >= 1) bytes[index++] = AsciiCharacters.Null;
		if (nullPadSize >= 2) bytes[index++] = AsciiCharacters.Null;
		if (nullPadSize == 3) bytes[index++] = AsciiCharacters.Null;
	}

	#endregion private
}
