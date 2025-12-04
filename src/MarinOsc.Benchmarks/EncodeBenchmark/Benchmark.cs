
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Order;

namespace MarinOsc.Benchmarks.EncodeBenchmark;

[MemoryDiagnoser]
[Orderer(
	SummaryOrderPolicy.FastestToSlowest,
	MethodOrderPolicy.Declared)]
[HideColumns(Column.Error, Column.Median, Column.StdDev, Column.RatioSD)]
public class Benchmark
{
	private const string _AddressString = "/avatar/parameters/VRCOSC/Media/Position";
	private const float _ArgumentFloat = 0.5f;

	private FastOSC.OSCMessage _FastOSC_OSCMessage = null!;
	private Common.OscMessage _MarinOsc_OSCMessage = default;
	private MarinOsc1.Common.OscMessage _MarinOsc1_OSCMessage = null!;
	private Vizcon.OSC.OscMessage _VizconOSC_OSCMessage = null!;

	[GlobalSetup]
	public void Setup ()
	{
		_FastOSC_OSCMessage = new(_AddressString, _ArgumentFloat);
		_MarinOsc_OSCMessage = new(_AddressString, _ArgumentFloat);
		_MarinOsc1_OSCMessage = new(_AddressString, _ArgumentFloat);
		_VizconOSC_OSCMessage = new(_AddressString, _ArgumentFloat);
	}

	[Benchmark(Baseline = true)]
	public byte[] FastOSC_Encode ()
	{
		return FastOSC.OSCEncoder.Encode(_FastOSC_OSCMessage);
	}

	[Benchmark]
	public byte[] MarinOsc_Encode ()
	{
		return Common.OscEncoding.EncodeWithNoFraming(_MarinOsc_OSCMessage);
	}


	[Benchmark]
	public byte[] MarinOsc1_Encode ()
	{
		return _MarinOsc1_OSCMessage.ToBytes();
	}

	[Benchmark]
	public byte[] VizconOSC1_Encode ()
	{
		return _VizconOSC_OSCMessage.GetBytes();
	}
}
