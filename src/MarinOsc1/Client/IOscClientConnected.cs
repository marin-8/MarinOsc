
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarinOsc1.Common;

namespace MarinOsc1.Client;

public interface IOscClientConnected : IDisposable
{
	Task SendAsync (string oscAddress);

	Task SendAsync (string oscAddress, params IReadOnlyList<object?> arguments);

	Task SendAsync (OscMessage oscMessage);
}
