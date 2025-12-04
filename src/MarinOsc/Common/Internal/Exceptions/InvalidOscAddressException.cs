
using System;

namespace MarinOsc.Common.Internal.Exceptions;

internal sealed class InvalidOscAddressException : Exception
{
	public InvalidOscAddressException (string oscAddress)
		: base($"Invalid OSC address ({oscAddress}).")
	{ }
}
