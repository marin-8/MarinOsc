
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MarinOsc1.Client.Internal;
using MarinOsc1.Common;
using MarinOsc1.Common.Internal;

namespace MarinOsc1.Client;

public sealed class OscClientUnconnected : IOscClientUnconnected
{
	#region fields

	private readonly IOscClientUnconnected _OscClientUnconnected;
	private readonly Logger? _Logger;

	#endregion fields
	#region public

	public static OscClientUnconnected Create (
		TransportTypeUnconnected transportTypeUnconnected)
		=> new(transportTypeUnconnected, null);

	public static OscClientUnconnected CreateWithLogging (
		TransportTypeUnconnected transportTypeUnconnected, Action<string> logMethod, LogLevel minimunLogLevel)
		=> new(transportTypeUnconnected, (logMethod, minimunLogLevel));

	public Task SendAsync (
		string ipAddress, int port, string oscAddress)
	{
		var recipient = new IPEndPoint(IPAddress.Parse(ipAddress), port);
		var oscMessage = new OscMessage(oscAddress);
		return SendAsync(recipient, oscMessage);
	}

	public Task SendAsync (
		string ipAddress, int port, string oscAddress, params IReadOnlyList<object?> arguments)
	{
		var recipient = new IPEndPoint(IPAddress.Parse(ipAddress), port);
		var oscMessage = new OscMessage(oscAddress, arguments);
		return SendAsync(recipient, oscMessage);
	}

	public Task SendAsync (
		string ipAddress, int port, OscMessage oscMessage)
	{
		var recipient = new IPEndPoint(IPAddress.Parse(ipAddress), port);
		return SendAsync(recipient, oscMessage);
	}

	public Task SendAsync (
		IPAddress ipAddress, int port, string oscAddress)
	{
		var recipient = new IPEndPoint(ipAddress, port);
		var oscMessage = new OscMessage(oscAddress);
		return SendAsync(recipient, oscMessage);
	}

	public Task SendAsync (
		IPAddress ipAddress, int port, string oscAddress, params IReadOnlyList<object?> arguments)
	{
		var recipient = new IPEndPoint(ipAddress, port);
		var oscMessage = new OscMessage(oscAddress, arguments);
		return SendAsync(recipient, oscMessage);
	}

	public Task SendAsync (
		IPAddress ipAddress, int port, OscMessage oscMessage)
	{
		var recipient = new IPEndPoint(ipAddress, port);
		return SendAsync(recipient, oscMessage);
	}

	public Task SendAsync (
		IPEndPoint ipEndPoint, string oscAddress)
	{
		var oscMessage = new OscMessage(oscAddress);
		return SendAsync(ipEndPoint, oscMessage);
	}

	public Task SendAsync (
		IPEndPoint ipEndPoint, string oscAddress, params IReadOnlyList<object?> arguments)
	{
		var oscMessage = new OscMessage(oscAddress, arguments);
		return SendAsync(ipEndPoint, oscMessage);
	}

	public async Task SendAsync (
		IPEndPoint ipEndPoint, OscMessage oscMessage)
	{
		await _OscClientUnconnected.SendAsync(ipEndPoint, oscMessage);
		_Logger?.LogOscMessageSent(oscMessage, ipEndPoint);
	}

	public void Dispose ()
	{
		try { _OscClientUnconnected.Dispose(); } catch { }
	}

	#endregion public

	#region private

	private OscClientUnconnected (
		TransportTypeUnconnected transportTypeUnconnected,
		(Action<string> LogMethod, LogLevel MinimunLogLevel)? logger)
	{
		_OscClientUnconnected =
			transportTypeUnconnected switch
			{
				TransportTypeUnconnected.Udp => new OscClientUdpUnconnected(),

				_ => throw new UnsupportedEnumValueException<TransportTypeUnconnected>(
					transportTypeUnconnected)
			};

		_Logger = logger is not null ? new Logger(logger.Value) : null;
	}

	#endregion private
}
