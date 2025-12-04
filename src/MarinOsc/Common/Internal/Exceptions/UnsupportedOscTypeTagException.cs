
using System;

namespace MarinOsc.Common.Internal.Exceptions;

internal sealed class UnsupportedOscTypeTagException : Exception
{
	public UnsupportedOscTypeTagException (byte oscTypeTag)
		: base($"Unsupported OSC type tag ({oscTypeTag}).")
	{ }
}
