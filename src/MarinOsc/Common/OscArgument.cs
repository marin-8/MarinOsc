
using System;
using System.Diagnostics.CodeAnalysis;
using MarinOsc.Common.Internal.Exceptions;

namespace MarinOsc.Common;

public readonly struct OscArgument
{
	#region public

	public OscTypeTag TypeTag { get; private init; }

	public required object? Value
	{
		get => _value;

		init
		{
			switch (value)
			{
				case null:
					_value = null;
					TypeTag = OscTypeTag.Nil;
					break;

				case bool boolArgument:
					_value = boolArgument;
					TypeTag = boolArgument ? OscTypeTag.True : OscTypeTag.False;
					break;

				case int intArgument:
					_value = intArgument;
					TypeTag = OscTypeTag.Int32;
					break;

				case float floatArgument:
					_value = floatArgument;
					TypeTag = OscTypeTag.Float32;
					break;

				case string stringArgument:
					if (!IsValidOscString(stringArgument))
						throw new InvalidOscStringException(stringArgument);
					_value = stringArgument;
					TypeTag = OscTypeTag.OscString;
					break;

				default: throw new UnsupportedOscArgumentTypeException(value);
			}
		}
	}

	public static OscArgument Null { get; } = new(null, OscTypeTag.Nil);
	public static OscArgument True { get; } = new(true, OscTypeTag.True);
	public static OscArgument False { get; } = new(false, OscTypeTag.False);

	[SetsRequiredMembers]
	public OscArgument () => Value = null;

	[SetsRequiredMembers]
	public OscArgument (bool? boolValue) => Value = boolValue;

	[SetsRequiredMembers]
	public OscArgument (int? intValue) => Value = intValue;

	[SetsRequiredMembers]
	public OscArgument (float? floatValue) => Value = floatValue;

	[SetsRequiredMembers]
	public OscArgument (string? stringValue) => Value = stringValue;

	public static implicit operator OscArgument (bool? boolValue) => new(boolValue);
	public static implicit operator OscArgument (int? intValue) => new(intValue);
	public static implicit operator OscArgument (float? floatValue) => new(floatValue);
	public static implicit operator OscArgument (string? stringValue) => new(stringValue);

	#endregion public
	#region internal

	[SetsRequiredMembers]
	internal OscArgument (object? value, OscTypeTag typeTag)
	{
		_value = value;
		TypeTag = typeTag;
	}

	#endregion internal
	#region private

	private readonly object? _value = null!;

	private bool IsValidOscString (string stringArgument)
	{
		for (var i = 0; i < stringArgument.Length; i++)
		{
			var character = stringArgument[i];

			if (character == '\0' || character > 127)
				return false;
		}

		return true;
	}

	#endregion private
}
