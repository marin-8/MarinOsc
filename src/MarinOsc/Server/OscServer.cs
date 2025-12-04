
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MarinOsc.Client;
using MarinOsc.Client.Internal;
using MarinOsc.Common;
using MarinOsc.Common.Internal;
using MarinOsc.Common.Internal.Exceptions;
using MarinOsc.Common.Internal.Extensions;
using MarinOsc.Server.Internal;

namespace MarinOsc.Server;

public sealed class OscServer : IOscServer
{
	#region fields

	private readonly IOscServer _OscServer;
	private readonly Dictionary<string, Delegate> _UserOscMessageHandlersByPath;
	private readonly Logger? _Logger;

	#endregion fields
	#region public

	public OscServer (
		TransportType transportType,
		int port,
		Dictionary<string, Delegate> userOscMessageHandlersByPath,
		(Action<string> logMethod, LogLevel minimunLogLevel)? logger = null)
	{
		_OscServer =
			transportType switch
			{
				TransportType.Udp => new OscServerUdp(port, OscMessageHandlerMethodUdp),
				TransportType.Tcp10 => new OscServerTcp10(port, OscMessageHandlerMethodTcp10),
				TransportType.Tcp11 => new OscServerTcp11(port, OscMessageHandlerMethodTcp11),

				_ => throw new UnsupportedEnumValueException<TransportType>(transportType)
			};

		_UserOscMessageHandlersByPath = userOscMessageHandlersByPath;

		_Logger = logger.HasValue ? new(logger.Value) : null;
	}

	public static IOscServerBuilderTransportType CreateBuilder () => new OscServerBuilder();

	public Task StartAsync ()
	{
		Console.CancelKeyPress += (sender, e) => { e.Cancel = true; Stop(); };

		var startTask = _OscServer.StartAsync();

		_Logger?.LogInfo("Server started. Listening...");

		return startTask;
	}

	public void Stop () => _OscServer.Stop();

	public void Dispose ()
	{
		_OscServer.Dispose();
	}

	#endregion public
	#region private

	private ValueTask OscMessageHandlerMethodUdp (
		IPEndPoint sender, OscMessage oscMessage)
	{
		OscClientConnected oscClientConnectedFactory ()
			=> new(new OscClientUdpConnected(sender), sender, _Logger);

		return OscMessageHandlerMethodCommon(
			sender, oscMessage, oscClientConnectedFactory);
	}

	private ValueTask OscMessageHandlerMethodTcp10 (
		TcpClient sender, OscMessage oscMessage)
	{
		var ipEndPoint = (sender.Client.RemoteEndPoint as IPEndPoint)!;

		OscClientConnected oscClientConnectedFactory ()
			=> new(new OscClientTcp10(sender), ipEndPoint, _Logger);

		return OscMessageHandlerMethodCommon(
			ipEndPoint, oscMessage, oscClientConnectedFactory);
	}

	private ValueTask OscMessageHandlerMethodTcp11 (
		TcpClient sender, OscMessage oscMessage)
	{
		var ipEndPoint = (sender.Client.RemoteEndPoint as IPEndPoint)!;

		OscClientConnected oscClientConnectedFactory ()
			=> new(new OscClientTcp11(sender), ipEndPoint, _Logger);

		return OscMessageHandlerMethodCommon(
			ipEndPoint, oscMessage, oscClientConnectedFactory);
	}

	private async ValueTask OscMessageHandlerMethodCommon (
		IPEndPoint sender,
		OscMessage oscMessage,
		Func<IOscClientConnected> oscClientConnectedWithLoggingFactory)
	{
		Logger.GenerateAmbientCorrelationId();

		_Logger?.LogOscMessageReceived(oscMessage, sender);

		if (!_UserOscMessageHandlersByPath.TryGetValue(
			oscMessage.Address, out var userOscMessageHandler))
		{
			_Logger?.LogWarning(
				$"Handler not found for Address \"{oscMessage.Address}\"");

			return;
		}

		object?[]? addressHandlerParameters;

		try
		{
			addressHandlerParameters =
				CreateUserOscMessageHandlerMethodParametersArray(
					userOscMessageHandler,
					oscMessage.Arguments,
					oscClientConnectedWithLoggingFactory);
		}
		catch
		{
			_Logger?.LogError("Failed to prepare parameters for handler.");

			return;
		}

		try
		{
			var dynamicInvokeResult =
				userOscMessageHandler.DynamicInvoke(addressHandlerParameters);

			if (dynamicInvokeResult is Task task) await task.CAF();
			else if (dynamicInvokeResult is ValueTask valueTask) await valueTask.CAF();
		}
		catch (Exception exception)
		{
			_Logger?.LogException(exception);
		}
	}

	private static object?[] CreateUserOscMessageHandlerMethodParametersArray (
		Delegate userOscMessageHandler,
		IReadOnlyList<OscArgument>? oscMessageArguments,
		Func<IOscClientConnected> oscClientConnectedWithLoggingFactory)
	{
		var oscClientConnectedParameterIndex =
			GetOscClientConnectedParameterIndex(userOscMessageHandler);

		if (oscMessageArguments is not null)
		{
			if (oscClientConnectedParameterIndex is not null)
			{
				var oscClientConnectedWithLogging =
					oscClientConnectedWithLoggingFactory();

				return ConvertOscMessageArgumentsToObjectArrayInsertingElementAt(
					oscMessageArguments,
					oscClientConnectedWithLogging,
					oscClientConnectedParameterIndex.Value);
			}
			else
			{
				return oscMessageArguments.Select(a => a.Value).ToArray();
			}
		}
		else
		{
			if (oscClientConnectedParameterIndex is not null)
			{
				var oscClientConnectedWithLogging =
					oscClientConnectedWithLoggingFactory();

				return [oscClientConnectedWithLogging];
			}
			else
			{
				return [];
			}
		}
	}

	private static int? GetOscClientConnectedParameterIndex (Delegate @delegate)
	{
		var parameters = @delegate.GetType().GetMethod("Invoke")!.GetParameters();

		if (parameters.Length == 0)
			return null;

		for (var index = 0; index < parameters.Length; index++)
		{
			if (typeof(IOscClientConnected).IsAssignableFrom(parameters[index].ParameterType))
				return index;
		}

		return null;
	}

	private static object?[] ConvertOscMessageArgumentsToObjectArrayInsertingElementAt (
		IReadOnlyList<OscArgument> oscMessageArguments, object element, int index)
	{
		var resultArray = new object?[oscMessageArguments.Count + 1];

		for (var i = 0; i < index; i++)
			resultArray[i] = oscMessageArguments[i].Value;

		resultArray[index] = element;

		for (var i = index; i < oscMessageArguments.Count; i++)
			resultArray[i + 1] = oscMessageArguments[i].Value;

		return resultArray;
	}

	#endregion private
}
