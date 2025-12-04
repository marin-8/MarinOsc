
using System;
using System.Threading.Tasks;

namespace MarinOsc.Server.Internal;

internal interface IOscServer : IDisposable
{
	Task StartAsync ();
	void Stop ();
}
