
using System;

namespace MarinOsc.Common.Internal.Exceptions;

internal sealed class InvalidOscTypeTagStringException : Exception
{
	public InvalidOscTypeTagStringException (string message)
		: base(message)
	{ }
}
