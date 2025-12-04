
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MarinOsc.Common;
using MarinOsc.Common.Internal;
using MarinOsc.Common.Internal.Extensions;

namespace MarinOsc.Client.Internal;

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
		string oscAddress, params IReadOnlyList<OscArgument> arguments)
		=> SendAsync(new OscMessage(oscAddress, arguments));

	public async Task SendAsync (OscMessage oscMessage)
	{
		var oscMessageBytes = OscEncoding.EncodeWithNoFraming(oscMessage);

		await _UdpClient.SendAsync(oscMessageBytes, oscMessageBytes.Length).CAF();
	}

	public void Dispose ()
	{
		try { _UdpClient.Dispose(); } catch { }
	}

	#endregion public
}
