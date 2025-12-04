
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MarinOsc.Common;
using MarinOsc.Common.Internal;
using MarinOsc.Common.Internal.Extensions;

namespace MarinOsc.Client.Internal;

internal sealed class OscClientTcp10 : IOscClientConnected
{
	#region fields

	private readonly TcpClient _TcpClient;
	private readonly NetworkStream _NetworkStream;

	#endregion fields
	#region public

	public OscClientTcp10 (TcpClient connectedTcpClient)
	{
		_TcpClient = connectedTcpClient;
		_NetworkStream = connectedTcpClient.GetStream();
	}

	public static async Task<OscClientTcp10> CreateAndConnectAsync (
		IPEndPoint ipEndPoint)
	{
		var tcpClient = new TcpClient();
		await tcpClient.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port).CAF();
		return new OscClientTcp10(tcpClient);
	}

	public Task SendAsync (string oscAddress)
		=> SendAsync(new OscMessage(oscAddress));

	public Task SendAsync (string oscAddress, params IReadOnlyList<OscArgument> arguments)
		=> SendAsync(new OscMessage(oscAddress, arguments));

	public async Task SendAsync (OscMessage oscMessage)
	{
		var oscMessageBytesWithSizePrefix =
			OscEncoding.EncodeWithSizePrefixFraming(oscMessage);

		await _NetworkStream.WriteAsync(
			oscMessageBytesWithSizePrefix, 0, oscMessageBytesWithSizePrefix.Length).CAF();
	}

	public void Dispose ()
	{
		try { _TcpClient.Dispose(); } catch { }
	}

	#endregion public
}
