
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FastOSC;

namespace MarinOsc.Benchmarks.CompleteBenchmark.Packages;

internal sealed class FastOSC
{
	private readonly int _MessageCount;
	private readonly int _SumTarget;
	private int _i = 0;

	private readonly OSCSender _Client = null!;
	private readonly OSCReceiver _Server = null!;
	private readonly OSCMessage _Message1;
	private const string _Address1 = "/address/1";
	private const string _Address2 = "/address/2";
	private const string _Address3 = "/address/3";
	private const string _Address4 = "/address/4";

	public FastOSC (int port, int messageCount)
	{
		if (messageCount % 4 != 0)
			throw new ArgumentException(
				"Must be a multiple of 4.", nameof(messageCount));

		_MessageCount = messageCount;
		_SumTarget = 10 * messageCount / 4;

		_Client = new OSCSender();
		_Client.ConnectAsync(new(IPAddress.Loopback, port)).GetAwaiter().GetResult();
		_Server = new OSCReceiver();
		_Server.Connect(new(IPAddress.Loopback, port));
		_Message1 = new OSCMessage(_Address1, 0);

		_Server.OnMessageReceived += OnMessageReceivedCallback;
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
					_Client.Send(new OSCMessage(_Address2, [123]));
					break;
				case 2:
					_Client.Send(new OSCMessage(_Address3, ["test"]));
					break;
				case 3:
					_Client.Send(new OSCMessage(_Address4, [42, 3.14f, "hello"]));
					break;
			}
		}

		while (Interlocked.CompareExchange(ref _i, 0, 0) != _SumTarget) { }
	}

	private void OnMessageReceivedCallback (OSCMessage oscMessage)
	{
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

	public async Task Cleanup ()
	{
		_Client.Disconnect();
		await _Server.DisconnectAsync();
	}
}
