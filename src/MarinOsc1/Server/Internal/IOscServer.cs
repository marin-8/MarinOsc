
using System;
using System.Threading.Tasks;

namespace MarinOsc1.Server.Internal;

internal interface IOscServer : IDisposable
{
	Task StartAsync ();
	void Stop ();
}
