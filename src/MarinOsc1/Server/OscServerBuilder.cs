
using System;
using System.Collections.Generic;
using MarinOsc1.Common;

namespace MarinOsc1.Server;

public interface IOscServerBuilderTransportType
{
	IOscServerBuilderPort WithTransportType (TransportType transportType);
}

public interface IOscServerBuilderPort
{
	IOscServerBuilderOptionalConfiguration WithPort (int port);
}

public interface IOscServerBuilderOptionalConfiguration
{
	IOscServerBuilderOptionalConfiguration WithHandler (string path, Delegate handler);

	IOscServerBuilderOptionalConfiguration WithLoggin (
		Action<string> logMethod, LogLevel minimunLogLevel);

	OscServer Build ();
}

public sealed class OscServerBuilder :
	IOscServerBuilderTransportType,
	IOscServerBuilderPort,
	IOscServerBuilderOptionalConfiguration
{
	#region fields

	private TransportType _TransportType;
	private int _Port;

	private readonly Dictionary<string, Delegate> _HandlersByPath = [];

	private (Action<string> LogMethod, LogLevel MinimunLogLevel)? _Logger;

	#endregion fields
	#region public

	public IOscServerBuilderPort WithTransportType (TransportType transportType)
	{
		_TransportType = transportType;
		return this;
	}

	public IOscServerBuilderOptionalConfiguration WithPort (int port)
	{
		_Port = port;
		return this;
	}

	public IOscServerBuilderOptionalConfiguration WithHandler (
		string path, Delegate handler)
	{
		_HandlersByPath.Add(path, handler);
		return this;
	}

	public IOscServerBuilderOptionalConfiguration WithLoggin (
		Action<string> logMethod, LogLevel minimunLogLevel)
	{
		_Logger = (logMethod, minimunLogLevel);
		return this;
	}

	public OscServer Build ()
	{
		return new OscServer(_TransportType, _Port, _HandlersByPath, _Logger);
	}

	#endregion public
}
