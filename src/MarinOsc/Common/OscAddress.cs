
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MarinOsc.Common.Internal.Exceptions;

namespace MarinOsc.Common;

public readonly struct OscAddress
{
	#region public

	public required string Value
	{
		get => _value;

		init
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new NullOrEmptyOscAddressException();

			if (!_OscAddressRegex.IsMatch(value))
				throw new InvalidOscAddressException(value);

			_value = value;
		}
	}

	[SetsRequiredMembers]
	public OscAddress ()
	{
		Value = "/";
	}

	[SetsRequiredMembers]
	public OscAddress (string address)
	{
		Value = address;
	}

	public static implicit operator OscAddress (string address) => new(address);

	public static implicit operator string (OscAddress address) => address.Value;

	#endregion public
	#region private

	private static readonly Regex _OscAddressRegex =
		new("^\\/[A-Za-z0-9_\\-\\.\\/]*$", RegexOptions.Compiled);

	private readonly string _value = null!;

	#endregion private
}
