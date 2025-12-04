
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CoreOSC;
using CoreOSC.IO;

namespace MarinOsc.Benchmarks.CompleteBenchmark.Packages;

internal sealed class CoreOSC
{
	private readonly int _MessageCount;
	private int _i = 0;

	private readonly UdpClient _Client = null!;
	private readonly UdpClient _Server = null!;
	private readonly OscMessage _Message1;
	private const string _Address1String = "/address/1";
	private const string _Address2String = "/address/2";
	private const string _Address3String = "/address/3";
	private const string _Address4String = "/address/4";
	private readonly Address _Address1;
	private readonly Address _Address2;
	private readonly Address _Address3;
	private readonly Address _Address4;

	public CoreOSC (int port, int messageCount)
	{
		if (messageCount % 4 != 0)
			throw new ArgumentException(
				"Must be a multiple of 4.", nameof(messageCount));

		_MessageCount = messageCount;

		_Client = new UdpClient();
		_Client.Connect("127.0.0.1", port);
		_Server = new UdpClient(port);
		_Address1 = new Address(_Address1String);
		_Address2 = new Address(_Address2String);
		_Address3 = new Address(_Address3String);
		_Address4 = new Address(_Address4String);
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

					switch (oscMessage.Address.Value)
					{
						case _Address1String:
							Interlocked.Add(ref _i, 1);
							break;
						case _Address2String:
							Interlocked.Add(ref _i, 2);
							break;
						case _Address3String:
							Interlocked.Add(ref _i, 3);
							break;
						case _Address4String:
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
					await _Client.SendMessageAsync(_Message1);
					break;
				case 1:
					await _Client.SendMessageAsync(new OscMessage(_Address2, [123]));
					break;
				case 2:
					await _Client.SendMessageAsync(new OscMessage(_Address3, ["test"]));
					break;
				case 3:
					await _Client.SendMessageAsync(new OscMessage(_Address4, [42, 3.14f, "hello"]));
					break;
			}
		}

		await Task.WhenAll(tasks);
	}

	public void Cleanup ()
	{
		try { _Client.Close(); } catch { }
		try { _Server.Close(); } catch { }
	}
}
