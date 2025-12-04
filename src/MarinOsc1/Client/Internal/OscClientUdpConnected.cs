
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MarinOsc1.Client;
using MarinOsc1.Common;

namespace MarinOsc1.Client.Internal;

internal sealed class OscClientUdpConnected : IOscClientConnected
{
	#region fields

	private readonly UdpClient _UdpClient;

	#endregion fields
	#region public

	public OscClientUdpConnected (IPEndPoint ipEndPoint)
	{
		_UdpClient = new();
		_UdpClient.Connect(ipEndPoint);
	}

	public Task SendAsync (string oscAddress)
		=> SendAsync(new OscMessage(oscAddress));

	public Task SendAsync (
		string oscAddress, params IReadOnlyList<object?> arguments)
		=> SendAsync(new OscMessage(oscAddress, arguments));

	public async Task SendAsync (OscMessage oscMessage)
	{
		var oscMessageBytes = oscMessage.ToBytes();

		await _UdpClient.SendAsync(oscMessageBytes, oscMessageBytes.Length);
	}

	public void Dispose ()
	{
		try { _UdpClient.Dispose(); } catch { }
	}

	#endregion public
}
