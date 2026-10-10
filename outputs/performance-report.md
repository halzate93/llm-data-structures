# Consolidated performance report

## Submission map

| Required item | Deliverable |
|---|---|
| Optimized balanced binary tree | [AVL implementation](../src/DataStructures/Optimized/AvlTree.cs), [activity 1](activity-1.md) |
| Refined task scheduling | [Thread-safe min-heap](../src/DataStructures/Optimized/ApiRequestQueue.cs), [activity 2](activity-2.md) |
| O(n²) to O(n log n) sorting | [Heapsort and merge sort](../src/DataStructures/Optimized/Sorting.cs), [activity 3](activity-3.md) |
| Debugged task execution | [Validated executor](../src/DataStructures/Optimized/TaskExecutor.cs), [activity 4](activity-4.md) |
| Performance evidence | This report, [combined CSV](benchmark-results.csv), individual activity CSVs |
| LLM contribution | [Consolidated reflection](llm-contribution-reflection.md), all four activity reflection sections |

## Before/after time and space complexity

Here n denotes current/input element count, k a batch size, m processed tasks,
and u distinct tree values. Queue insertion complexities account for amortized
dynamic-array growth. Handler cost and lock waiting are additional to queue work.

| Task | Before | After | Space/trade-off |
|---|---|---|---|
| Tree insert/search | O(n) worst case; ordered build O(n²) | AVL O(log n); ordered build O(n log n) | O(u) nodes, worst-case O(n); cached heights add node fields; duplicates compressed |
| In-order traversal | O(n) time, O(n) worst-case recursive stack | O(n) time, O(log n) explicit stack | All duplicate values retained |
| Request enqueue | O(n log n) worst-case per sort | O(log n) amortized | O(n) entries with sequence field and one lock |
| Request dequeue | O(n) shift | O(log n) | Heap order, lower priority number first |
| Bulk request enqueue | Repeated sorts | O(n + k) heapify | O(k) snapshot plus combined heap |
| Sorting | Bubble O(n²) | Heap O(n log n) worst case | O(1) auxiliary storage, unstable |
| Optional merge sorting | No parallel baseline | O(n log n) total work | O(n) shared buffer, bounded parallel tasks |
| Task execution | O(m) valid path; aborts on first error | O(m) for bounded-cost tasks and at most 3m attempts | O(m) results/logging work; explicit failed outcomes |

Complexity guarantees do not mean every input or small workload becomes faster.
Space complexities describe algorithmic storage, whereas the allocation figures
below count bytes allocated during a timed workload, not peak/live memory.

## Checked-in elapsed-time measurements

All values are medians in milliseconds, from seven samples after two warmups.

| Activity/workload | Size | Before | After | Interpretation |
|---|---:|---:|---:|---|
| 1: Ordered tree build | 2048 | 5.326 | 0.472 | About 11.3x faster with AVL |
| 1: Ordered search of every value | 2048 | 2.573 | 0.170 | About 15.2x faster with AVL |
| 1: Random tree build | 2048 | 0.107 | 0.206 | AVL is slower here; height guarantee has cost |
| 2: Individual enqueue of all requests | 2048 | 32.384 | 0.346 | About 93.7x faster with heap |
| 2: Batch enqueue vs repeated sorting | 2048 | 32.384 | 0.043 | About 756x faster using unrounded raw medians |
| 2: Drain all requests | 2048 | 0.297 | 0.239 | About 1.2x faster; small heaps can be slower |
| 3: Random sorting, bubble vs heap | 32768 | 757.747 | 1.975 | About 384x faster |
| 3: Random sorting, sequential vs parallel merge | 262144 | 15.776 | 4.275 | About 3.7x faster; extra memory/scheduling |
| 4: Valid-task processing | 8192 | 0.405 | 2.305 | About 5.7x slower; logging/results improve observability |

Individual reports include additional sizes/distributions and all non-improvements.
The quadratic bubble workload is not run at 262144 elements; no result is invented.
Ratios use raw medians where available, not the rounded display cells.

## Allocation observations

Median process-wide allocated bytes, setup excluded:

| Workload | Baseline/reference | Optimized |
|---|---:|---:|
| Tree build, 2048 distinct ordered values | 81,920 | AVL 98,304 |
| Enqueue 2048 requests | List 32,976 | Heap individual 65,712; heap batch 49,200 |
| Random sorting, 262144 integers | Heapsort 0 | Sequential merge 1,048,600; parallel merge 1,060,496 |
| Process 8192 valid tasks | 1,491,328 | Logged executor 3,006,944 |

Extra height/sequence fields and execution records have real memory costs.
Duplicate tree compression is a design benefit, not a measured distinct-input
memory reduction. An evidence-driven merge refinement reduced sequential
allocation from 1,834,984 to 1,048,600 bytes by avoiding closures on sequential
recursion; the earlier run is retained in Git history. A regression test checks
the single-buffer allocation budget. Timing changes between runs are not
attributed exclusively to that refinement.

## Reliability is not a speedup

For the original `Task 1`, null, `Fail Task`, `Task 2` submissions:

- Baseline completes one task, aborts on null, and leaves two pending.
- Improved execution rejects/logs null at submission, completes two tasks,
  records the permanent failure, and leaves none pending.
- A separate demo/test scenario recovers on the third transient attempt.

These are asserted outcomes in
[reliability.csv](activity-4-reliability.csv), not a timing comparison between
equivalent completed workloads. General programming errors still propagate.

## Methodology and limitations

- Runtime: .NET 8.0.23, Windows 10.0.26300, X64, 16 logical processors, Release.
  Each activity includes its own environment text file.
- Deterministic seed 42, equivalent inputs, two warmups, seven measurements.
- `Stopwatch` measures execution only; fixture creation/cloning and subsequent
  reference/invariant validation are outside the timed region. Search includes
  identical successful-result checks; queue drain includes recording results.
- Allocation uses `GC.GetTotalAllocatedBytes(true)` so parallel worker allocation
  is included. Runtime/background allocations can also contribute.
- Executor logging is formatted then discarded in both versions; the structured
  version has more fields/records. No file/network I/O or real API latency is
  represented.
- The CSV `operations` field counts input elements/tasks/lookups, not primitive
  algorithm comparisons. Seven rows per workload retain variability rather than
  hiding it behind a single duration.
- Measurements are small educational runs on a shared environment. Tiered JIT,
  caches, OS scheduling, clock resolution, and CPU availability affect them.
  No universal speedup, parallel scaling guarantee, or statistically rigorous
  production capacity conclusion is claimed.
- Baseline tree sizes are bounded for stack safety; bubble sort is bounded to
  keep the executable workload practical.

## Reproduce and validation

From the repository root:

```powershell
dotnet build DataStructures.sln -c Release
dotnet test DataStructures.sln -c Release
dotnet run --project src\Demo -c Release -- all
dotnet run --project benchmarks\DataStructures.Benchmarks -c Release -- all
```

An individual activity number selects a smaller demo/benchmark run. Benchmark
commands regenerate activity CSVs/environment metadata and consolidate all
existing sample files. Update report tables when replacing the recorded run.

Validation covers 43 passing test cases: 10 tree, 5 scheduling, 16 sorting,
and 12 execution cases. It checks exact output shapes, duplicate preservation,
AVL ordering/height invariants, concurrent no-loss/no-duplication scheduling,
sorting cutoff boundaries, a buffer-allocation budget, retry limits, and both
original execution failures. Timing thresholds are intentionally not test gates.

The local history retains setup, baseline/optimization/validation steps for each
activity, one allocation refinement, and this consolidated submission. No remote
publication or invented prior learning history is included.
