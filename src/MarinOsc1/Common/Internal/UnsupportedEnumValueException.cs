
using System;

namespace MarinOsc1.Common.Internal;

internal class UnsupportedEnumValueException<T> : Exception
	where T : Enum
{
	public UnsupportedEnumValueException (T value)
		: base($"Unsupported value for enum {nameof(T)} ({value})")
	{ }
}
