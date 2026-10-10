# Data structures and algorithms with Copilot

Educational C# implementations retaining the assignment baselines alongside
optimized versions. Requires .NET SDK 8.0.417 (see `global.json`).

## Commands

Run from the repository root:

```powershell
dotnet build DataStructures.sln -c Release
dotnet test DataStructures.sln -c Release
dotnet run --project src\Demo -c Release -- all
dotnet run --project benchmarks\DataStructures.Benchmarks -c Release -- all
```

The demo and benchmark commands also accept an activity number (`1` through `4`).

## Assignments and deliverables

Original instructions are in [activities](activities) and
[specification.md](specification.md). The approved implementation plan is
[plan.md](plan.md).

| Activity | Implementation | Required write-up |
|---|---|---|
| 1: Binary tree | [AVL tree](src/DataStructures/Optimized/AvlTree.cs) | [Activity 1 report](outputs/activity-1.md) |
| 2: Scheduling | [Thread-safe heap](src/DataStructures/Optimized/ApiRequestQueue.cs) | [Activity 2 report](outputs/activity-2.md) |
| 3: Sorting | [Heapsort and merge sort](src/DataStructures/Optimized/Sorting.cs) | [Activity 3 report](outputs/activity-3.md) |
| 4: Execution | [Logged bounded-retry executor](src/DataStructures/Optimized/TaskExecutor.cs) | [Activity 4 report](outputs/activity-4.md) |

Consolidated submission:

- [Performance report](outputs/performance-report.md)
- [LLM contribution reflection](outputs/llm-contribution-reflection.md)
- [Combined raw benchmark samples](outputs/benchmark-results.csv)
- [Correctness tests](tests/DataStructures.Tests)
- [Preserved baselines](src/DataStructures/Baseline)

Each report answers all three reflection questions and includes measured
results, limitations, correctness evidence, and reproduction commands. Tests
check behavior and invariants, not environment-sensitive timing thresholds.

## Benchmark methodology

Run benchmarks from the repository root in Release mode. Inputs use seed 42,
two warmups, and seven measured samples per workload. Timed sections exclude
setup and subsequent correctness validation. Results include elapsed milliseconds
and process-wide allocated bytes (including worker-thread/runtime allocations).
The `operations` column counts submitted elements/tasks/lookups, not primitive
comparisons. Activity CSVs and environment metadata are regenerated; the combined
CSV includes all existing activity sample files. Run `all` to refresh everything.

Report tables describe the checked-in run; rerunning benchmarks changes raw
data, so update report tables before submitting a new run. There are no claimed
universal speedups. The executor's reliability improvement adds valid-path
overhead; parallel sort trades memory for possible throughput gains.

## Commit progression

```powershell
git --no-pager log --reverse --oneline
```

The local history has a setup commit, three commits per activity (baseline,
optimization, tests/measurements/report), one evidence-driven merge-allocation
refinement commit, and a consolidated submission commit (15 commits total).
Each commit includes a Copilot co-author trailer. No remote is required.

Baseline implementations intentionally retain defects for learning, including
general exceptions and abort-on-failure execution. They are not production
recommendations. Optimized APIs clarify immutable request priorities, FIFO ties,
and validation at the queue boundary; activity reports document those changes.
