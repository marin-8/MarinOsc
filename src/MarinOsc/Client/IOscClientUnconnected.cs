
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MarinOsc.Common;

namespace MarinOsc.Client;

public interface IOscClientUnconnected : IDisposable
{
	Task SendAsync (
		string ipAddress, int port, string oscAddress);

	Task SendAsync (
		string ipAddress, int port, string oscAddress, params IReadOnlyList<OscArgument> arguments);

	Task SendAsync (
		string ipAddress, int port, OscMessage oscMessage);

	Task SendAsync (
		IPAddress ipAddress, int port, string oscAddress);

	Task SendAsync (
		IPAddress ipAddress, int port, string oscAddress, params IReadOnlyList<OscArgument> arguments);

	Task SendAsync (
		IPAddress ipAddress, int port, OscMessage oscMessage);

	Task SendAsync (
		IPEndPoint ipEndPoint, string oscAddress);

	Task SendAsync (
		IPEndPoint ipEndPoint, string oscAddress, params IReadOnlyList<OscArgument> arguments);

	Task SendAsync (
		IPEndPoint ipEndPoint, OscMessage oscMessage);
}
