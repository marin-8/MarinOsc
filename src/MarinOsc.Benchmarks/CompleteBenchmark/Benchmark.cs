
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Order;

namespace MarinOsc.Benchmarks.CompleteBenchmark;

[MemoryDiagnoser]
[Orderer(
	SummaryOrderPolicy.FastestToSlowest,
	MethodOrderPolicy.Declared)]
[HideColumns(Column.Error, Column.Median, Column.StdDev, Column.RatioSD)]
public class Benchmark
{
	private Packages.CoreOSC _CoreOSC = null!;
	private Packages.LucHeartCoreOSC _LucHeartCoreOSC = null!;
	private Packages.VizconOSC _VizconOSC = null!;
	private Packages.FastOSC _FastOSC = null!;
	private Packages.MarinOsc _MarinOsc = null!;
	private Packages.MarinOsc1 _MarinOsc1 = null!;

	private const int _MessageCount = 100;

	[GlobalSetup]
	public void Setup ()
	{
		_CoreOSC = new(9001, _MessageCount);
		_LucHeartCoreOSC = new(9002, _MessageCount);
		_VizconOSC = new(9003, _MessageCount);
		_FastOSC = new(9004, _MessageCount);
		_MarinOsc = new(9005, _MessageCount);
		_MarinOsc1 = new(9006, _MessageCount);
	}

	[IterationSetup]
	public void IterationSetup ()
	{
		_CoreOSC.IterationSetup();
		_LucHeartCoreOSC.IterationSetup();
		_VizconOSC.IterationSetup();
		_FastOSC.IterationSetup();
		_MarinOsc.IterationSetup();
		_MarinOsc1.IterationSetup();
	}

	[Benchmark]
	public Task MarinOsc ()
	{
		return _MarinOsc.Benchmark();
	}


	[Benchmark]
	public Task MarinOsc1 ()
	{
		return _MarinOsc1.Benchmark();
	}

	//[Benchmark]
	//public Task CoreOSC ()
	//{
	//	return _CoreOSC.Benchmark();
	//}

	//[Benchmark]
	//public Task LucHeartCoreOSC ()
	//{
	//	return _LucHeartCoreOSC.Benchmark();
	//}

	//[Benchmark]
	//public void VizconOSC ()
	//{
	//	_VizconOSC.Benchmark();
	//}

	//[Benchmark(Baseline = true)]
	//public void FastOSC ()
	//{
	//	_FastOSC.Benchmark();
	//}

	[GlobalCleanup]
	public async Task Cleanup ()
	{
		_CoreOSC.Cleanup();
		_LucHeartCoreOSC.Cleanup();
		_VizconOSC.Cleanup();
		await _FastOSC.Cleanup();
		await _MarinOsc.Cleanup();
	}
}
