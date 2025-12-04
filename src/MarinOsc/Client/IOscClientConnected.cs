
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarinOsc.Common;

namespace MarinOsc.Client;

public interface IOscClientConnected : IDisposable
{
	Task SendAsync (string oscAddress);

	Task SendAsync (string oscAddress, params IReadOnlyList<OscArgument> arguments);

	Task SendAsync (OscMessage oscMessage);
}
