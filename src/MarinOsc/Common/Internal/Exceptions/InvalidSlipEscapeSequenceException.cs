
using System;

namespace MarinOsc.Common.Internal.Exceptions;

internal sealed class InvalidSlipEscapeSequenceException : Exception
{
	public InvalidSlipEscapeSequenceException (byte invalidByte)
		: base($"Invalid SLIP escape sequence (0x{invalidByte:X2}).")
	{ }
}
