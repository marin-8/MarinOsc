
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MarinOsc1.Common;

namespace MarinOsc1.Server.Internal;

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

				var completedTask = await Task.WhenAny(udpReceiveTask, cancelTask);

				if (completedTask == cancelTask)
					throw new OperationCanceledException();

				var udpReceiveResult = await udpReceiveTask;

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
		ReadOnlyMemory<byte> recievedPacket, IPEndPoint senderIpEndPoint)
	{
		var recievedPacketSpan = recievedPacket.Span;

		if (recievedPacket.Length == 0 || recievedPacketSpan[0] == (byte)'#')
			return; // skip bundles for now

		var recievedOscMessage = OscMessage.Parse(recievedPacketSpan);

		await _OscMessageHandlerMethod(senderIpEndPoint, recievedOscMessage);
	}

	#endregion private
}
