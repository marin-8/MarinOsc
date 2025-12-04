
using System;

namespace MarinOsc.Common.Internal.Exceptions;

internal sealed class NullOrEmptyOscAddressException : Exception
{
	public NullOrEmptyOscAddressException ()
		: base("OSC address cannot be null or empty")
	{ }
}
