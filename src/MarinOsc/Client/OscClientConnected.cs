
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MarinOsc.Client.Internal;
using MarinOsc.Common;
using MarinOsc.Common.Internal;
using MarinOsc.Common.Internal.Exceptions;
using MarinOsc.Common.Internal.Extensions;

namespace MarinOsc.Client;

public sealed class OscClientConnected : IOscClientConnected
{
	#region fields

	private readonly IOscClientConnected _OscClientConnected;
	private readonly IPEndPoint _Recipient;
	private readonly Logger? _Logger;

	#endregion fields
	#region public

	public static Task<OscClientConnected> CreateAndConnectAsync (
		TransportTypeConnected transportTypeConnected, string ipAddress, int port)
		=> CreateAndConnectAsync(transportTypeConnected, new IPEndPoint(IPAddress.Parse(ipAddress), port), null);

	public static Task<OscClientConnected> CreateAndConnectWithLoggingAsync (
		TransportTypeConnected transportTypeConnected, string ipAddress, int port, Action<string> logMethod, LogLevel minimunLogLevel)
		=> CreateAndConnectAsync(transportTypeConnected, new IPEndPoint(IPAddress.Parse(ipAddress), port), (logMethod, minimunLogLevel));

	public static Task<OscClientConnected> CreateAndConnectAsync (
		TransportTypeConnected transportTypeConnected, IPAddress ipAddress, int port)
		=> CreateAndConnectAsync(transportTypeConnected, new IPEndPoint(ipAddress, port), null);

	public static Task<OscClientConnected> CreateAndConnectWithLoggingAsync (
		TransportTypeConnected transportTypeConnected, IPAddress ipAddress, int port, Action<string> logMethod, LogLevel minimunLogLevel)
		=> CreateAndConnectAsync(transportTypeConnected, new IPEndPoint(ipAddress, port), (logMethod, minimunLogLevel));

	public static Task<OscClientConnected> CreateAndConnectAsync (
		TransportTypeConnected transportTypeConnected, IPEndPoint ipEndPoint)
		=> CreateAndConnectAsync(transportTypeConnected, ipEndPoint, null);

	public static Task<OscClientConnected> CreateAndConnectWithLoggingAsync (
		TransportTypeConnected transportTypeConnected, IPEndPoint ipEndPoint, Action<string> logMethod, LogLevel minimunLogLevel)
		=> CreateAndConnectAsync(transportTypeConnected, ipEndPoint, (logMethod, minimunLogLevel));

	public Task SendAsync (string oscAddress)
	{
		var oscMessage = new OscMessage(oscAddress);
		return SendAsync(oscMessage);
	}

	public Task SendAsync (string oscAddress, params IReadOnlyList<OscArgument> arguments)
	{
		var oscMessage = new OscMessage(oscAddress, arguments);
		return SendAsync(oscMessage);
	}

	public async Task SendAsync (OscMessage oscMessage)
	{
		await _OscClientConnected.SendAsync(oscMessage).CAF();
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

	private static async Task<OscClientConnected> CreateAndConnectAsync (
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
					await OscClientTcp10.CreateAndConnectAsync(recipient).CAF(),

				TransportTypeConnected.Tcp11 => (IOscClientConnected)
					await OscClientTcp11.CreateAndConnectAsync(recipient).CAF(),

				_ => throw new UnsupportedEnumValueException<TransportTypeConnected>(
					transportTypeConnected)
			};

		var loggerObject = logger is not null ? new Logger(logger.Value) : null;

		return new(oscClientConnected, recipient, loggerObject);
	}

	#endregion private
}
