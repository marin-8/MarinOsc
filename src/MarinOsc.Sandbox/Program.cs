
using System;
using System.Threading.Tasks;
using MarinOsc.Client;
using MarinOsc.Common;
using MarinOsc.Server;

namespace MarinOsc.Sandbox;

internal class Program
{
	private static async Task Main ()
	{
		var oscServer =
			OscServer.CreateBuilder()
			.WithTransportType(TransportType.Tcp11)
			.WithPort(9090)
			.WithLoggin(Console.WriteLine, LogLevel.Information)
			.WithHandler(
				"/pruebas/server/1",
				async (string @string, int @int, float @float, IOscClientConnected oscClientConnected) =>
				{
					await oscClientConnected.SendAsync("/pruebas/client/1/string", "server-" + Random.Shared.Next(100, 1000));
					await oscClientConnected.SendAsync("/pruebas/client/1/int", Random.Shared.Next(1, 11));
					await oscClientConnected.SendAsync("/pruebas/client/1/float", Random.Shared.NextSingle());
				})
			.WithHandler(
				"/pruebas/server/2",
				() => { })
			.Build();

		await oscServer.StartAsync();
	}
}
