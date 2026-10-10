using DataStructures.Benchmarks;

string selection = args.Length == 0 ? "all" : args[0];
if (args.Length > 1 || selection is not ("all" or "1" or "2" or "3" or "4"))
    throw new ArgumentException("Usage: benchmarks [all|1|2|3|4]; run from repository root.");
if (!Directory.Exists("activities"))
    throw new InvalidOperationException("Run benchmarks from the repository root.");
if (selection is "all" or "1") TreeBenchmarks.Run();
if (selection is "all" or "2") QueueBenchmarks.Run();
if (selection is "all" or "3") SortBenchmarks.Run();
if (selection is "all" or "4") ExecutorBenchmarks.Run();
