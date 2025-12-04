
using System;

namespace MarinOsc.Common.Internal.Exceptions;

internal sealed class UnsupportedOscArgumentTypeException : Exception
{
	public UnsupportedOscArgumentTypeException (object oscArgument)
		: base($"Unsupported OSC argument type ({oscArgument.GetType()}).")
	{ }
}
