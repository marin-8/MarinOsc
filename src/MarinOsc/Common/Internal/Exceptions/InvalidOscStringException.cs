
using System;

namespace MarinOsc.Common.Internal.Exceptions;

internal sealed class InvalidOscStringException : Exception
{
	public InvalidOscStringException (string oscString)
		: base($"Invalid OSC string ({oscString}).")
	{ }
}
