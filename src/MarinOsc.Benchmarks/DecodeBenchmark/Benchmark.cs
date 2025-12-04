
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Order;

namespace MarinOsc.Benchmarks.DecodeBenchmark;

[MemoryDiagnoser]
[Orderer(
	SummaryOrderPolicy.FastestToSlowest,
	MethodOrderPolicy.Declared)]
[HideColumns(Column.Error, Column.Median, Column.StdDev, Column.RatioSD)]
public class Benchmark
{
	private const string _AddressString = "/avatar/parameters/VRCOSC/Media/Position";
	private const float _ArgumentFloat = 0.5f;

	private byte[] _FastOSC_OSCMessage_ByteArray = null!;
	private byte[] _MarinOsc_OSCMessage_ByteArray = null!;
	private byte[] _MarinOsc1_OSCMessage_ByteArray = null!;
	private byte[] _VizconOSC_OSCMessage_ByteArray = null!;

	[GlobalSetup]
	public void Setup ()
	{
		_FastOSC_OSCMessage_ByteArray = FastOSC.OSCEncoder.Encode(new FastOSC.OSCMessage(_AddressString, _ArgumentFloat));
		_MarinOsc_OSCMessage_ByteArray = Common.OscEncoding.EncodeWithNoFraming(new(_AddressString, _ArgumentFloat));
		_MarinOsc1_OSCMessage_ByteArray = new MarinOsc1.Common.OscMessage(_AddressString, _ArgumentFloat).ToBytes();
		_VizconOSC_OSCMessage_ByteArray = new Vizcon.OSC.OscMessage(_AddressString, _ArgumentFloat).GetBytes();
	}

	[Benchmark(Baseline = true)]
	public FastOSC.OSCMessage FastOSC_Decode ()
	{
		return FastOSC.OSCDecoder.Decode(_FastOSC_OSCMessage_ByteArray).AsMessage();
	}

	[Benchmark]
	public Common.OscMessage MarinOsc_Decode ()
	{
		return Common.OscEncoding.DecodeNoFraming(_MarinOsc_OSCMessage_ByteArray);
	}


	[Benchmark]
	public MarinOsc1.Common.OscMessage MarinOsc1_Decode ()
	{
		return MarinOsc1.Common.OscMessage.Parse(_MarinOsc1_OSCMessage_ByteArray);
	}

	[Benchmark]
	public Vizcon.OSC.OscMessage VizconOSC1_Decode ()
	{
		return (Vizcon.OSC.OscPacket.GetPacket(_VizconOSC_OSCMessage_ByteArray) as Vizcon.OSC.OscMessage)!;
	}
}
