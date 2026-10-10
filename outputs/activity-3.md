# Activity 3: Performance Analysis and Refinement

## Work performed

Preserved [bubble sort](../src/DataStructures/Baseline/Sorting.cs), including its
lack of an early-exit optimization. Reviewed its nested loops and replaced them
with [optimized sorting implementations](../src/DataStructures/Optimized/Sorting.cs):

- Iterative in-place heapsort builds a max-heap and repeatedly extracts its root.
  It guarantees O(n log n) worst-case time and O(1) auxiliary space.
- Merge sort offers a sequential/parallel comparison with O(n) temporary storage.
  Its fixed 32-element insertion-sort cutoff reduces small-recursion overhead
  without changing asymptotic complexity.
- Optional parallel merge sort shares one buffer across disjoint ranges, waits
  for both halves before merging, and only forks at 16384 elements or above.
  Depth is bounded by floor(log2(logical processors)) and capped at four, so there
  are at most 16 leaf partitions. On one logical processor it stays sequential.
- All optimized operations validate null and mutate the caller's array into
  ascending order, preserving the assignment's output shape.

Comments explain the Copilot-assisted heap and bounded parallel decomposition.
Heapsort is not stable; the original bubble sort is stable. Stability is not
observable for this integer-only API, but heapsort should not be substituted
unchanged for a stable object sort.

## Correctness evidence

[SortingTests.cs](../tests/DataStructures.Tests/SortingTests.cs): 15 passing cases
cover empty/single arrays, duplicate/negative/extreme values, sorted/reverse data,
insertion cutoff boundaries, parallel cutoff boundaries, and 65537 random
values. All three optimized paths are compared element-for-element with
`Array.Sort`; small baseline inputs are also checked. The demo verifies both the
five-item assignment output and parallel merge sort on 32768 random integers.

## Complexity

| Algorithm | Worst-case work | Auxiliary space |
|---|---|---|
| Original bubble sort | O(n²), even for sorted input | O(1) |
| Heapsort | O(n log n) | O(1) |
| Sequential merge sort | O(n log n) | O(n) buffer + O(log n) recursion |
| Bounded parallel merge sort | O(n log n) total work | O(n) buffer + bounded tasks/recursive stacks |

Parallelism changes observed elapsed time, not total asymptotic work. Serial
merging and scheduling overhead limit speedup. Heapsort meets the requested
in-place optimization; merge sort explicitly trades memory for parallel support.

## Actual measured results

Median milliseconds, 2 warmups and 7 samples, seed 42:

| Distribution | n | Bubble | Heap | Merge | Parallel merge |
|---|---:|---:|---:|---:|---:|
| Random | 256 | 0.043 | 0.034 | 0.026 | 0.029 |
| Reverse | 256 | 0.033 | 0.035 | 0.031 | 0.029 |
| Random | 4096 | 6.036 | 0.218 | 0.569 | 0.547 |
| Reverse | 4096 | 5.551 | 0.167 | 0.502 | 0.485 |
| Random | 32768 | 572.996 | 1.877 | 1.661 | 0.704 |
| Reverse | 32768 | 328.534 | 1.225 | 0.677 | 0.257 |
| Random | 262144 | Not run | 19.526 | 17.263 | 4.407 |
| Reverse | 262144 | Not run | 10.395 | 6.277 | 1.747 |

Random sorting at n=32768 improved about 305x with heapsort. At n=262144 parallel
merge sort was about 3.9x faster than sequential merge sort for random input.
No bubble-sort result is fabricated at n=262144: that quadratic workload is
deliberately skipped. At n=256 reverse-input heapsort was slightly slower than
bubble sort; tiny timings do not establish a universal speed advantage.
Below the parallel cutoff, both merge entry points actually run sequentially.

[Raw samples](activity-3-samples.csv) and
[environment](activity-3-environment.txt) include process-wide allocation counts.
Input generation, cloning, and reference sorting are excluded; merge's buffer and
parallel scheduling allocations are included. Parallel allocation must be
process-wide rather than measured on only the caller's thread. Runtime allocation
noise and tiered JIT effects remain; comparisons are educational, not statistically
rigorous production benchmarking.

## Reproduce

```powershell
dotnet test DataStructures.sln -c Release --filter FullyQualifiedName~SortingTests
dotnet run --project src\Demo -c Release -- 3
dotnet run --project benchmarks\DataStructures.Benchmarks -c Release -- 3
```

Run at the repository root. Measurements vary with processor availability.

## Reflection answers

**How did the LLM assist in refining the algorithm?** The Copilot SDK assistant
identified bubble sort's quadratic work, evaluated sorting trade-offs, generated
heapsort and bounded parallel merge sort, and implemented reference-based tests
and reproducible measurements.

**Were any LLM-generated suggestions inaccurate or unnecessary?** No inaccurate
sort output was observed in the covered tests. Quicksort was evaluated but not
chosen because its straightforward form lacks a worst-case O(n log n) guarantee.
Unbounded recursive task creation and a claim that merge sort is in-place would
be inappropriate; neither was implemented. The cutoff/depth choices are simple
heuristics, not claimed globally optimal or backed by invented tuning runs.

**What were the most impactful improvements?** Replacing O(n²) comparison work
with guaranteed O(n log n) heapsort while retaining constant auxiliary memory,
and optionally parallelizing large merge-sort partitions with explicit bounds.
