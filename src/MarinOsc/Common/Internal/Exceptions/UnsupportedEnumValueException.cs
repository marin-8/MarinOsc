
using System;

namespace MarinOsc.Common.Internal.Exceptions;

internal sealed class UnsupportedEnumValueException<TEnum> : Exception
	where TEnum : Enum
{
	public UnsupportedEnumValueException (TEnum value)
		: base($"Unsupported enum value for {nameof(TEnum)} ({value}).")
	{ }
}
