
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LucHeart.CoreOSC;

namespace MarinOsc.Benchmarks.CompleteBenchmark.Packages;

internal sealed class LucHeartCoreOSC
{
	private readonly int _MessageCount;
	private int _i = 0;

	private readonly OscSender _Client = null!;
	private readonly OscListener _Server = null!;
	private readonly OscMessage _Message1;
	private const string _Address1 = "/address/1";
	private const string _Address2 = "/address/2";
	private const string _Address3 = "/address/3";
	private const string _Address4 = "/address/4";

	public LucHeartCoreOSC (int port, int messageCount)
	{
		if (messageCount % 4 != 0)
			throw new ArgumentException(
				"Must be a multiple of 4.", nameof(messageCount));

		_MessageCount = messageCount;

		_Client = new OscSender(new(IPAddress.Loopback, port));
		_Server = new OscListener(new(IPAddress.Loopback, port));
		_Message1 = new OscMessage(_Address1);
	}

	public void IterationSetup ()
	{
		Interlocked.Exchange(ref _i, 0);
	}

	public async Task Benchmark ()
	{
		var tasks = new Task[_MessageCount];

		for (var i = 0; i < _MessageCount; i++)
		{
			tasks[i] = Task.Run(async () =>
			{
				try
				{
					var oscMessage = await _Server.ReceiveMessageAsync();

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
				catch { }
			});
		}

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

		await Task.WhenAll(tasks);
	}

	public void Cleanup ()
	{
		try { _Client.Dispose(); } catch { }
		try { _Server.Dispose(); } catch { }
	}
}
