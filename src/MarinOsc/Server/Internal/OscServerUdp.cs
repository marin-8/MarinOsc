
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MarinOsc.Common;
using MarinOsc.Common.Internal;
using MarinOsc.Common.Internal.Extensions;

namespace MarinOsc.Server.Internal;

internal sealed class OscServerUdp : IOscServer
{
	#region fields

	private readonly UdpClient _UdpClient;
	private readonly CancellationTokenSource _CancellationTokenSource = new();
	private readonly Func<IPEndPoint, OscMessage, ValueTask> _OscMessageHandlerMethod;

	#endregion fields
	#region public

	public OscServerUdp (
		int port,
		Func<IPEndPoint, OscMessage, ValueTask> oscMessageHandlerMethod)
	{
		_UdpClient = new UdpClient(port);
		_OscMessageHandlerMethod = oscMessageHandlerMethod;
	}

	public async Task StartAsync ()
	{
		while (!_CancellationTokenSource.IsCancellationRequested)
		{
			try
			{
				var udpReceiveTask = _UdpClient.ReceiveAsync();
				var cancelTask = Task.Delay(Timeout.Infinite, _CancellationTokenSource.Token);

				var completedTask = await Task.WhenAny(udpReceiveTask, cancelTask).CAF();

				if (completedTask == cancelTask)
					throw new OperationCanceledException();

				var udpReceiveResult = await udpReceiveTask.CAF();

				_ = Task.Run(
					() => ProcessRecievedPacket(
						udpReceiveResult.Buffer,
						udpReceiveResult.RemoteEndPoint));
			}
			catch (OperationCanceledException) { }
		}
	}

	public void Stop () => _CancellationTokenSource.Cancel();

	public void Dispose ()
	{
		try { _UdpClient.Dispose(); } catch { }
	}

	#endregion public
	#region private

	private async Task ProcessRecievedPacket (
		byte[] recievedPacket, IPEndPoint senderIpEndPoint)
	{
		if (recievedPacket.Length == 0 || recievedPacket[0] == (byte)'#')
			return; // skip bundles for now

		var recievedOscMessage = OscEncoding.DecodeNoFraming(recievedPacket);

		await _OscMessageHandlerMethod(senderIpEndPoint, recievedOscMessage).CAF();
	}

	#endregion private
}
