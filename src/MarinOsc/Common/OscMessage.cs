
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarinOsc.Common;

public readonly struct OscMessage
{
	#region public

	public required OscAddress Address { get; init; }
	public IReadOnlyList<OscArgument>? Arguments { get; init; }

	[SetsRequiredMembers]
	public OscMessage (string address)
	{
		Address = address;
		Arguments = null;
	}

	[SetsRequiredMembers]
	public OscMessage (string address, params IReadOnlyList<OscArgument> arguments)
	{
		Address = address;
		Arguments = arguments;
	}

	#endregion public
}
