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

Activity reports and consolidated submission links will be added as the work is
validated. Baseline implementations are educational examples, not production
recommendations.
