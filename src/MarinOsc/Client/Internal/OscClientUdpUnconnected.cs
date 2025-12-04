
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MarinOsc.Common;
using MarinOsc.Common.Internal;
using MarinOsc.Common.Internal.Extensions;

namespace MarinOsc.Client.Internal;

internal sealed class OscClientUdpUnconnected : IOscClientUnconnected
{
	#region fields

	private readonly UdpClient _UdpClient = new();

	#endregion fields
	#region public

	public OscClientUdpUnconnected () { }

	public Task SendAsync (
		string ipAddress, int port, string oscAddress)
		=> SendAsync(new IPEndPoint(IPAddress.Parse(ipAddress), port), new OscMessage(oscAddress));

	public Task SendAsync (
		string ipAddress, int port, string oscAddress, params IReadOnlyList<OscArgument> arguments)
		=> SendAsync(new IPEndPoint(IPAddress.Parse(ipAddress), port), new OscMessage(oscAddress, arguments));

	public Task SendAsync (
		string ipAddress, int port, OscMessage oscMessage)
		=> SendAsync(new IPEndPoint(IPAddress.Parse(ipAddress), port), oscMessage);

	public Task SendAsync (
		IPAddress ipAddress, int port, string oscAddress)
		=> SendAsync(new IPEndPoint(ipAddress, port), new OscMessage(oscAddress));

	public Task SendAsync (
		IPAddress ipAddress, int port, string oscAddress, params IReadOnlyList<OscArgument> arguments)
		=> SendAsync(new IPEndPoint(ipAddress, port), new OscMessage(oscAddress, arguments));

	public Task SendAsync (
		IPAddress ipAddress, int port, OscMessage oscMessage)
		=> SendAsync(new IPEndPoint(ipAddress, port), oscMessage);

	public Task SendAsync (
		IPEndPoint ipEndPoint, string oscAddress)
		=> SendAsync(ipEndPoint, new OscMessage(oscAddress));

	public Task SendAsync (
		IPEndPoint ipEndPoint, string oscAddress, params IReadOnlyList<OscArgument> arguments)
		=> SendAsync(ipEndPoint, new OscMessage(oscAddress, arguments));

	public async Task SendAsync (IPEndPoint ipEndPoint, OscMessage oscMessage)
	{
		var oscMessageBytes = OscEncoding.EncodeWithNoFraming(oscMessage);

		await _UdpClient.SendAsync(oscMessageBytes, oscMessageBytes.Length, ipEndPoint).CAF();
	}

	public void Dispose ()
	{
		try { _UdpClient.Dispose(); } catch { }
	}

	#endregion public
}
