
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using MarinOsc1.Common.Internal;

namespace MarinOsc1.Common.Internal;

internal sealed class Logger
{
	#region fields

	private readonly Action<string> _LogMethod;
	private readonly LogLevel _MinimumLogLevel;

	private static readonly AsyncLocal<Guid?> _CorrelationIdAsyncLocal = new();

	#endregion fields
	#region public

	public Logger ((Action<string> LogMethod, LogLevel MinimumLogLevel) logger)
	{
		_LogMethod = logger.LogMethod;
		_MinimumLogLevel = logger.MinimumLogLevel;
	}

	public void LogInfo (string infoMessage)
		=> LogMessage(LogLevel.Information, infoMessage);

	public void LogWarning (string warningMessage)
		=> LogMessage(LogLevel.Warning, warningMessage);

	public void LogError (string errorMessage)
		=> LogMessage(LogLevel.Error, errorMessage);

	public void LogException (Exception exception)
		=> LogMessage(LogLevel.Error, $"Unhandled exception:\n{exception}");

	public void LogOscMessageReceived (OscMessage message, IPEndPoint sender)
		=> LogOscMessageSentOrReceived(SentOrReceived.Received, message, sender);

	public void LogOscMessageSent (OscMessage message, IPEndPoint recipient)
		=> LogOscMessageSentOrReceived(SentOrReceived.Sent, message, recipient);

	public static void GenerateAmbientCorrelationId ()
		=> _CorrelationIdAsyncLocal.Value = Guid.NewGuid();

	#endregion public
	#region private

	private const string _InfoString = "INF";
	private const string _WarningString = "WRN";
	private const string _ErrorString = "ERR";

	private const string _SentString = "Sent";
	private const string _ReceivedString = "Received";

	private enum SentOrReceived { Sent, Received }

	private static string LogLevelToString (LogLevel logLevel)
		=> logLevel switch
		{
			LogLevel.Information => _InfoString,
			LogLevel.Warning => _WarningString,
			LogLevel.Error => _ErrorString,

			_ => throw new UnsupportedEnumValueException<LogLevel>(logLevel)
		};

	private static string SentOrReceivedToString (SentOrReceived sentOrReceived)
		=> sentOrReceived switch
		{
			SentOrReceived.Sent => _SentString,
			SentOrReceived.Received => _ReceivedString,

			_ => throw new UnsupportedEnumValueException<SentOrReceived>(sentOrReceived)
		};

	private void LogOscMessageSentOrReceived (
		SentOrReceived sentOrReceived,
		OscMessage oscMessage,
		IPEndPoint recipientOrSender)
	{
		if (_MinimumLogLevel > LogLevel.Information) return;

		var requestOrResponseMessageBuilder = new StringBuilder();

		requestOrResponseMessageBuilder.Append('[');
		requestOrResponseMessageBuilder.Append(SentOrReceivedToString(sentOrReceived));
		requestOrResponseMessageBuilder.Append("] [");
		requestOrResponseMessageBuilder.Append(recipientOrSender);
		requestOrResponseMessageBuilder.Append("] \"");
		requestOrResponseMessageBuilder.Append(oscMessage.Address);
		requestOrResponseMessageBuilder.Append('"');
		AppendOscMessageArguments(requestOrResponseMessageBuilder, oscMessage.Arguments);

		LogMessage(LogLevel.Information, requestOrResponseMessageBuilder);
	}

	private void LogMessage (LogLevel logLevel, string message)
	{
		if (_MinimumLogLevel > logLevel) return;

		LogMessageInternal(logLevel, sb => sb.Append(message));
	}

	private void LogMessage (
		LogLevel logLevel, StringBuilder messageStringBuilder)
	{
		if (_MinimumLogLevel > logLevel) return;

		LogMessageInternal(logLevel, sb => sb.Append(messageStringBuilder));
	}

	private void LogMessageInternal (
		LogLevel logLevel, Action<StringBuilder> appendMessage)
	{
		var logStringBuilder = new StringBuilder();

		logStringBuilder.Append('[');
		logStringBuilder.Append(GetTimeStamp());
		logStringBuilder.Append("] [");
		logStringBuilder.Append(LogLevelToString(logLevel));

		var correlationId = _CorrelationIdAsyncLocal.Value;

		if (correlationId is not null)
		{
			logStringBuilder.Append("] [");
			logStringBuilder.Append(correlationId);
		}

		logStringBuilder.Append("] ");

		appendMessage(logStringBuilder);

		var logString = logStringBuilder.ToString();

		_LogMethod(logString);
	}

	private static string GetTimeStamp ()
		=> DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff zzz");

	private static StringBuilder AppendOscMessageArguments (
		StringBuilder logStringBuilder,
		IReadOnlyList<object?>? oscMessageArguments)
	{
		if (oscMessageArguments is null) return logStringBuilder;
		if (oscMessageArguments.Count == 0) return logStringBuilder;

		foreach (var argument in oscMessageArguments)
			logStringBuilder.Append($" \"{argument}\"");

		return logStringBuilder;
	}

	#endregion private
}
