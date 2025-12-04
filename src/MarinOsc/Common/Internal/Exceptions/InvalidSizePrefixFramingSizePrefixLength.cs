
using System;

namespace MarinOsc.Common.Internal.Exceptions;

internal sealed class InvalidSizePrefixFramingSizePrefixLength : Exception
{
	public InvalidSizePrefixFramingSizePrefixLength (int length)
		: base($"Invalid size prefix framing size prefix length ({length}).")
	{ }
}
