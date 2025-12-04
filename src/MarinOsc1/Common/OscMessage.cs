
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace MarinOsc1.Common;

public class OscMessage
{
	#region public

	public required string Address { get; init; }
	public IReadOnlyList<object?>? Arguments { get; init; }

	[SetsRequiredMembers]
	public OscMessage (string address)
	{
		Address = address;
		Arguments = null;
	}

	[SetsRequiredMembers]
	public OscMessage (
		string address, params IReadOnlyList<object?> arguments)
	{
		Address = address;
		Arguments = arguments;
	}

	#endregion public
	#region internal

	public static OscMessage Parse (ReadOnlySpan<byte> bytes)
	{
		var index = 0;

		var address = ReadString(bytes, ref index);

		if (bytes[index] != (byte)',')
			throw new InvalidDataException("Type tag string must start with ','");

		var typeTags = ReadString(bytes, ref index)[1..];
		var arguments = new List<object?>();

		foreach (var typeTag in typeTags)
		{
			var argument = ReadArgument(bytes, ref index, typeTag);
			arguments.Add(argument);
		}

		return new OscMessage(address, arguments);
	}

	public byte[] ToBytes ()
	{
		using var memoryStream = new MemoryStream();

		var typeTags = GetTypeTags(Arguments);

		WriteString(Address, memoryStream);
		WriteString("," + typeTags, memoryStream);

		for (var i = 0; i < typeTags.Length; i++)
		{
			var typeTag = typeTags[i];
			var argument = Arguments![i];

			switch (typeTag)
			{
				case 'i':
					WriteInt((int)argument!, memoryStream);
					break;
				case 'f':
					WriteFloat((float)argument!, memoryStream);
					break;
				case 's':
					WriteString((string)argument!, memoryStream);
					break;
				case 'T':
				case 'F':
				case 'N':
					break;
				default:
					throw new NotSupportedException($"Unsupported type tag: {typeTag}");
			}
		}

		return memoryStream.ToArray();
	}

	#endregion internal
	#region private

	private static void WriteString (string @string, MemoryStream memoryStream)
	{
		var bytes = Encoding.ASCII.GetBytes(@string);

		memoryStream.Write(bytes, 0, bytes.Length);
		memoryStream.WriteByte(0);

		while (memoryStream.Length % 4 != 0)
			memoryStream.WriteByte(0);
	}

	private static void WriteInt (int @int, MemoryStream memoryStream)
	{
		var buffer = (Span<byte>)stackalloc byte[4];

		BinaryPrimitives.WriteInt32BigEndian(buffer, @int);
		memoryStream.Write(buffer.ToArray(), 0, buffer.Length);
	}

	private static void WriteFloat (float @float, MemoryStream memoryStream)
	{
		var @int = Internal.BitConverter.SingleToInt32Bits(@float);

		WriteInt(@int, memoryStream);
	}

	private static string ReadString (ReadOnlySpan<byte> bytes, ref int index)
	{
		var startIndex = index;

		while (index < bytes.Length && bytes[index] != 0)
			index++;

		var @string = Encoding.ASCII.GetString(bytes[startIndex..index].ToArray());

		index++;

		while (index % 4 != 0)
			index++;

		return @string;
	}

	private static object? ReadArgument (ReadOnlySpan<byte> bytes, ref int index, char typeTag)
	{
		switch (typeTag)
		{
			case 'i':
				var i = BinaryPrimitives.ReadInt32BigEndian(bytes[index..]);
				index += 4;
				return i;
			case 'f':
				var raw = BinaryPrimitives.ReadInt32BigEndian(bytes[index..]);
				index += 4;
				return Internal.BitConverter.Int32BitsToSingle(raw);
			case 's':
				return ReadString(bytes, ref index);
			case 'T': return true;
			case 'F': return false;
			case 'N': return null;
			default:
				throw new NotSupportedException($"Unsupported type tag: {typeTag}");
		}
	}

	private static string GetTypeTags (IReadOnlyList<object?>? arguments)
	{
		if (arguments is null) return string.Empty;

		var typeTagsStringBuilder = new StringBuilder();

		foreach (var argument in arguments)
		{
			switch (argument)
			{
				case int: typeTagsStringBuilder.Append('i'); break;
				case float: typeTagsStringBuilder.Append('f'); break;
				case string: typeTagsStringBuilder.Append('s'); break;
				case true: typeTagsStringBuilder.Append('T'); break;
				case false: typeTagsStringBuilder.Append('F'); break;
				case null: typeTagsStringBuilder.Append('N'); break;

				default:
					throw new NotSupportedException(
						$"Unsupported OSC argument type: {argument?.GetType()}");
			}
		}

		return typeTagsStringBuilder.ToString();
	}

	#endregion private
}
