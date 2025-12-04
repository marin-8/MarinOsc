
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MarinOsc1.Client;
using MarinOsc1.Common;

namespace MarinOsc1.Client.Internal;

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
		await tcpClient.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port);
		return new OscClientTcp10(tcpClient);
	}

	public Task SendAsync (string oscAddress)
		=> SendAsync(new OscMessage(oscAddress));

	public Task SendAsync (string oscAddress, params IReadOnlyList<object?> arguments)
		=> SendAsync(new OscMessage(oscAddress, arguments));

	public async Task SendAsync (OscMessage oscMessage)
	{
		var oscMessageBytes = oscMessage.ToBytes();

		var oscMessageBytesLengthInNetworkOrder =
			IPAddress.HostToNetworkOrder(oscMessageBytes.Length);

		var oscMessageBytesLengthInNetworkOrderBytes =
			System.BitConverter.GetBytes(oscMessageBytesLengthInNetworkOrder);

		await _NetworkStream.WriteAsync(
			oscMessageBytesLengthInNetworkOrderBytes,
			0,
			oscMessageBytesLengthInNetworkOrderBytes.Length);

		await _NetworkStream.WriteAsync(oscMessageBytes, 0, oscMessageBytes.Length);
	}

	public void Dispose ()
	{
		try { _TcpClient.Dispose(); } catch { }
	}

	#endregion public
}
