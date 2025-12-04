
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarinOsc.Client;
using MarinOsc.Common;
using MarinOsc.Server;

namespace MarinOsc.Benchmarks.CompleteBenchmark.Packages;

internal sealed class MarinOsc
{
	private readonly int _MessageCount;
	private readonly int _SumTarget;
	private int _i = 0;

	private readonly OscClientConnected _Client = null!;
	private readonly OscServer _Server = null!;
	private readonly OscMessage _Message1;
	private const string _Address1 = "/address/1";
	private const string _Address2 = "/address/2";
	private const string _Address3 = "/address/3";
	private const string _Address4 = "/address/4";
	private readonly Task _ServerStartTask = null!;

	public MarinOsc (int port, int messageCount)
	{
		if (messageCount % 4 != 0)
			throw new ArgumentException(
				"Must be a multiple of 4.", nameof(messageCount));

		_MessageCount = messageCount;
		_SumTarget = 10 * messageCount / 4;

		_Client =
			OscClientConnected.CreateAndConnectAsync(
				TransportTypeConnected.Udp, "127.0.0.1", port)
			.Result;

		_Server = new OscServer(TransportType.Udp, port, new Dictionary<string, Delegate>
		{
			[_Address1] = () => { Interlocked.Add(ref _i, 1); },
			[_Address2] = (int i) => { Interlocked.Add(ref _i, 2); },
			[_Address3] = (string s) => { Interlocked.Add(ref _i, 3); },
			[_Address4] = (int i, float f, string s) => { Interlocked.Add(ref _i, 4); },
		});

		_Message1 = new OscMessage(_Address1);

		_ServerStartTask = _Server.StartAsync();
	}

	public void IterationSetup ()
	{
		Interlocked.Exchange(ref _i, 0);
	}

	public async Task Benchmark ()
	{
		for (var i = 0; i < _MessageCount; i++)
		{
			switch (i / (_MessageCount / 4))
			{
				case 0:
					await _Client.SendAsync(_Message1);
					break;
				case 1:
					await _Client.SendAsync(new OscMessage(_Address2, [123]));
					break;
				case 2:
					await _Client.SendAsync(new OscMessage(_Address3, ["test"]));
					break;
				case 3:
					await _Client.SendAsync(new OscMessage(_Address4, [42, 3.14f, "hello"]));
					break;
			}
		}

		while (Interlocked.CompareExchange(ref _i, 0, 0) != _SumTarget) { }
	}

	public async Task Cleanup ()
	{
		_Client.Dispose();
		_Server.Stop();
		await _ServerStartTask;
		_Server.Dispose();
	}
}
