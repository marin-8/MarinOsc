
using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MarinOsc1.Common;
using MarinOsc1.Common.Internal;

namespace MarinOsc1.Server.Internal;

internal sealed class OscServerTcp10 : IOscServer
{
	#region fields

	private readonly TcpListener _TcpListener;
	private readonly CancellationTokenSource _CancellationTokenSource = new();
	private readonly Func<TcpClient, OscMessage, ValueTask> _OscMessageHandlerMethod;

	#endregion fields
	#region public

	public OscServerTcp10 (
		int port,
		Func<TcpClient, OscMessage, ValueTask> oscMessageHandlerMethod)
	{
		_TcpListener = new TcpListener(IPAddress.Any, port);
		_OscMessageHandlerMethod = oscMessageHandlerMethod;
	}

	public async Task StartAsync ()
	{
		_TcpListener.Start();

		while (!_CancellationTokenSource.IsCancellationRequested)
		{
			TcpClient tcpClient;

			try
			{
				tcpClient = await _TcpListener.AcceptTcpClientAsync();
			}
			catch (ObjectDisposedException)
			{
				break;
			}

			_ = Task.Run(() => HandleTcpClient(tcpClient));
		}
	}

	public void Stop () => _CancellationTokenSource.Cancel();

	public void Dispose ()
	{
		try { _TcpListener.Stop(); } catch { }
	}

	#endregion public
	#region private

	private async Task HandleTcpClient (TcpClient tcpClient)
	{
		var cancellationToken = _CancellationTokenSource.Token;
		var networkStream = tcpClient.GetStream();

		var lengthBuffer = new byte[4];

		while (!cancellationToken.IsCancellationRequested)
		{
			await networkStream.ReadExactlyAsync(lengthBuffer, cancellationToken);

			var length = BinaryPrimitives.ReadInt32BigEndian(lengthBuffer);

			if (length <= 0 || length > 64 * 1024)
				throw new InvalidDataException(
					$"Invalid framed OSC length: {length}");

			var recievedPacket = new byte[length];

			await networkStream.ReadExactlyAsync(recievedPacket, cancellationToken);

			await ProcessRecievedPacket(recievedPacket, tcpClient);
		}
	}

	private async Task ProcessRecievedPacket (
		ReadOnlyMemory<byte> recievedPacket,
		TcpClient tcpClient)
	{
		var recievedPacketSpan = recievedPacket.Span;

		if (recievedPacket.Length == 0 || recievedPacketSpan[0] == (byte)'#')
			return; // skip bundles for now

		var recievedMessage = OscMessage.Parse(recievedPacketSpan);

		await _OscMessageHandlerMethod(tcpClient, recievedMessage);
	}

	#endregion private
}
