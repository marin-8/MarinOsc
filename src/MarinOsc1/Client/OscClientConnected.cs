
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MarinOsc1.Client.Internal;
using MarinOsc1.Common;
using MarinOsc1.Common.Internal;

namespace MarinOsc1.Client;

public sealed class OscClientConnected : IOscClientConnected
{
	#region fields

	private readonly IOscClientConnected _OscClientConnected;
	private readonly IPEndPoint _Recipient;
	private readonly Logger? _Logger;

	#endregion fields
	#region public

	public static ValueTask<OscClientConnected> CreateAndConnectAsync (
		TransportTypeConnected transportTypeConnected, string ipAddress, int port)
		=> CreateAndConnectAsync(transportTypeConnected, new IPEndPoint(IPAddress.Parse(ipAddress), port), null);

	public static ValueTask<OscClientConnected> CreateAndConnectWithLoggingAsync (
		TransportTypeConnected transportTypeConnected, string ipAddress, int port, Action<string> logMethod, LogLevel minimunLogLevel)
		=> CreateAndConnectAsync(transportTypeConnected, new IPEndPoint(IPAddress.Parse(ipAddress), port), (logMethod, minimunLogLevel));

	public static ValueTask<OscClientConnected> CreateAndConnectAsync (
		TransportTypeConnected transportTypeConnected, IPAddress ipAddress, int port)
		=> CreateAndConnectAsync(transportTypeConnected, new IPEndPoint(ipAddress, port), null);

	public static ValueTask<OscClientConnected> CreateAndConnectWithLoggingAsync (
		TransportTypeConnected transportTypeConnected, IPAddress ipAddress, int port, Action<string> logMethod, LogLevel minimunLogLevel)
		=> CreateAndConnectAsync(transportTypeConnected, new IPEndPoint(ipAddress, port), (logMethod, minimunLogLevel));

	public static ValueTask<OscClientConnected> CreateAndConnectAsync (
		TransportTypeConnected transportTypeConnected, IPEndPoint ipEndPoint)
		=> CreateAndConnectAsync(transportTypeConnected, ipEndPoint, null);

	public static ValueTask<OscClientConnected> CreateAndConnectWithLoggingAsync (
		TransportTypeConnected transportTypeConnected, IPEndPoint ipEndPoint, Action<string> logMethod, LogLevel minimunLogLevel)
		=> CreateAndConnectAsync(transportTypeConnected, ipEndPoint, (logMethod, minimunLogLevel));

	public Task SendAsync (string oscAddress)
	{
		var oscMessage = new OscMessage(oscAddress);
		return SendAsync(oscMessage);
	}

	public Task SendAsync (string oscAddress, params IReadOnlyList<object?> arguments)
	{
		var oscMessage = new OscMessage(oscAddress, arguments);
		return SendAsync(oscMessage);
	}

	public async Task SendAsync (OscMessage oscMessage)
	{
		await _OscClientConnected.SendAsync(oscMessage);
		_Logger?.LogOscMessageSent(oscMessage, _Recipient);
	}

	public void Dispose ()
	{
		try { _OscClientConnected.Dispose(); } catch { }
	}

	#endregion public
	#region internal

	internal OscClientConnected (
		IOscClientConnected oscClientConnected,
		IPEndPoint recipient,
		Logger? logger)
	{
		_OscClientConnected = oscClientConnected;
		_Recipient = recipient;
		_Logger = logger;
	}

	#endregion internal
	#region private

	private static async ValueTask<OscClientConnected> CreateAndConnectAsync (
		TransportTypeConnected transportTypeConnected,
		IPEndPoint recipient,
		(Action<string> LogMethod, LogLevel MinimunLogLevel)? logger)
	{
		var oscClientConnected =
			transportTypeConnected switch
			{
				TransportTypeConnected.Udp =>
					new OscClientUdpConnected(recipient),

				TransportTypeConnected.Tcp10 =>
					await OscClientTcp10.CreateAndConnectAsync(recipient),

				TransportTypeConnected.Tcp11 => (IOscClientConnected)
					await OscClientTcp11.CreateAndConnectAsync(recipient),

				_ => throw new UnsupportedEnumValueException<TransportTypeConnected>(
					transportTypeConnected)
			};

		var loggerObject = logger is not null ? new Logger(logger.Value) : null;

		return new(oscClientConnected, recipient, loggerObject);
	}

	#endregion private
}
