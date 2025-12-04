
using System;
using System.Buffers.Binary;
using System.IO;
using MarinOsc.Common.Internal.Exceptions;

namespace MarinOsc.Common;

public static partial class OscEncoding
{
	#region public

	public static OscMessage DecodeNoFraming (this byte[] bytes)
	{
		var index = 0;

		return DecodeNoFramingInternal(bytes, ref index);
	}

	#endregion public
	#region private

	private static OscMessage DecodeNoFramingInternal (in byte[] bytes, ref int index)
	{
		var address = ReadString(bytes, ref index);

		if (bytes[index] != (byte)',')
			throw new InvalidOscTypeTagStringException(
				"Type tag string doesn't start with ','");

		var typeTags = ReadString(bytes, ref index)[1..];

		if (typeTags.Length == 0)
			return new OscMessage(address);

		var arguments = new OscArgument[typeTags.Length];

		for (var i = 0; i < typeTags.Length; i++)
			arguments[i] = ReadArgument(bytes, ref index, (byte)typeTags[i]);

		return new OscMessage(address, arguments);
	}

	private static string ReadString (byte[] bytes, ref int index)
	{
		var startIndex = index;

		while (index < bytes.Length && bytes[index] != 0)
			index++;

		var @string = _AsciiEncoding.GetString(bytes, startIndex, index - startIndex);

		index++;

		while (index % 4 != 0)
			index++;

		return @string;
	}

	private static OscArgument ReadArgument (byte[] bytes, ref int index, byte typeTag)
	{
		switch (typeTag)
		{
			case (byte)OscTypeTag.Int32:
				var @int = ReadInt(bytes, ref index);
				return new(@int, OscTypeTag.Int32);

			case (byte)OscTypeTag.Float32:
				var @float = ReadFloat(bytes, ref index);
				return new(@float, OscTypeTag.Float32);

			case (byte)OscTypeTag.OscString:
				var @string = ReadString(bytes, ref index);
				return new(@string, OscTypeTag.OscString);

			case (byte)OscTypeTag.True: return OscArgument.True;
			case (byte)OscTypeTag.False: return OscArgument.False;
			case (byte)OscTypeTag.Nil: return OscArgument.Null;

			default: throw new UnsupportedOscTypeTagException(typeTag);
		}
	}

	private static int ReadInt (ReadOnlySpan<byte> bytes, ref int index)
	{
		var @int = BinaryPrimitives.ReadInt32BigEndian(bytes[index..]);
		index += 4;
		return @int;
	}

	private static float ReadFloat (ReadOnlySpan<byte> bytes, ref int index)
	{
		var floatAsInt = ReadInt(bytes, ref index);
		var @float = Internal.BitConverter.Int32BitsToSingle(floatAsInt);
		return @float;
	}

	#endregion private
}
