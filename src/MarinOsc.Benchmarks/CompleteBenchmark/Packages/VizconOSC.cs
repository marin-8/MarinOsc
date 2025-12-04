
using System;
using System.Threading;
using Vizcon.OSC;

namespace MarinOsc.Benchmarks.CompleteBenchmark.Packages;

internal sealed class VizconOSC
{
	private readonly int _MessageCount;
	private readonly int _SumTarget;
	private int _i = 0;

	private readonly UDPSender _Client = null!;
	private readonly UDPListener _Server = null!;
	private readonly OscMessage _Message1;
	private const string _Address1 = "/address/1";
	private const string _Address2 = "/address/2";
	private const string _Address3 = "/address/3";
	private const string _Address4 = "/address/4";

	public VizconOSC (int port, int messageCount)
	{
		if (messageCount % 4 != 0)
			throw new ArgumentException(
				"Must be a multiple of 4.", nameof(messageCount));

		_MessageCount = messageCount;
		_SumTarget = 10 * messageCount / 4;

		_Client = new UDPSender("127.0.0.1", port);
		_Server = new UDPListener(port, OnBytePacketCallback);
		_Message1 = new OscMessage(_Address1);
	}

	public void IterationSetup ()
	{
		Interlocked.Exchange(ref _i, 0);
	}

	public void Benchmark ()
	{
		for (var i = 0; i < _MessageCount; i++)
		{
			switch (i / (_MessageCount / 4))
			{
				case 0:
					_Client.Send(_Message1);
					break;
				case 1:
					_Client.Send(new OscMessage(_Address2, [123]));
					break;
				case 2:
					_Client.Send(new OscMessage(_Address3, ["test"]));
					break;
				case 3:
					_Client.Send(new OscMessage(_Address4, [42, 3.14f, "hello"]));
					break;
			}
		}

		while (Interlocked.CompareExchange(ref _i, 0, 0) != _SumTarget) { }
	}

	private void OnBytePacketCallback (byte[] bytes)
	{
		var oscMessage = (OscPacket.GetPacket(bytes) as OscMessage)!;

		switch (oscMessage.Address)
		{
			case _Address1:
				Interlocked.Add(ref _i, 1);
				break;
			case _Address2:
				Interlocked.Add(ref _i, 2);
				break;
			case _Address3:
				Interlocked.Add(ref _i, 3);
				break;
			case _Address4:
				Interlocked.Add(ref _i, 4);
				break;
		}
	}

	public void Cleanup ()
	{
		try { _Client.Close(); } catch { }
		try { _Server.Close(); } catch { }
	}
}
