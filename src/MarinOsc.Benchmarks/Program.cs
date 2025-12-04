
using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace MarinOsc.Benchmarks;

public class Program
{
	static void Main ()
	//static async Task Main ()
	{
		//var package = new Packages.CoreOSC(9001, 100);

		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();
		//await package.Benchmark();

		//package.Cleanup();

		BenchmarkRunner.Run<CompleteBenchmark.Benchmark>(
		//BenchmarkRunner.Run<EncodeBenchmark.Benchmark>(
		//BenchmarkRunner.Run<DecodeBenchmark.Benchmark>(
			ManualConfig.Create(DefaultConfig.Instance)
			.WithOptions(ConfigOptions.DisableOptimizationsValidator));

		Console.Write("\n\n TERMINADO ");
		Console.ReadKey();
	}
}
