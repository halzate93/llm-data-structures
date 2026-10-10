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
- A measured follow-up isolated delegate closures to actual parallel splits:
  sequential recursion now allocates only the shared merge buffer.
- All optimized operations validate null and mutate the caller's array into
  ascending order, preserving the assignment's output shape.

Comments explain the Copilot-assisted heap and bounded parallel decomposition.
Heapsort is not stable; the original bubble sort is stable. Stability is not
observable for this integer-only API, but heapsort should not be substituted
unchanged for a stable object sort.

## Correctness evidence

[SortingTests.cs](../tests/DataStructures.Tests/SortingTests.cs): 16 passing cases
cover empty/single arrays, duplicate/negative/extreme values, sorted/reverse data,
insertion cutoff boundaries, parallel cutoff boundaries, and 65537 random
values. All three optimized paths are compared element-for-element with
`Array.Sort`; small baseline inputs are also checked. The demo verifies both the
five-item assignment output and parallel merge sort on 32768 random integers.
An allocation regression test bounds the warmed sequential path to one integer
buffer plus at most 1024 bytes of overhead.

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
| Random | 256 | 0.043 | 0.030 | 0.024 | 0.026 |
| Reverse | 256 | 0.033 | 0.035 | 0.028 | 0.027 |
| Random | 4096 | 6.918 | 0.211 | 0.483 | 0.468 |
| Reverse | 4096 | 5.077 | 0.160 | 0.441 | 0.441 |
| Random | 32768 | 757.747 | 1.974 | 1.859 | 1.103 |
| Reverse | 32768 | 369.236 | 1.549 | 0.959 | 0.360 |
| Random | 262144 | Not run | 23.136 | 15.776 | 4.275 |
| Reverse | 262144 | Not run | 11.006 | 8.980 | 1.893 |

Random sorting at n=32768 improved about 384x with heapsort. At n=262144 parallel
merge sort was about 3.7x faster than sequential merge sort for random input.
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

Allocation inspection revealed 1,834,984 bytes for sequential merge sort on
262144 random integers before the closure refinement. After it, the rerun
measured 1,048,600 bytes, matching one 1,048,576-byte integer payload plus its
24-byte array header. Parallel merge sort allocated a median 1,060,496 bytes
including bounded task scheduling. The prior raw run is preserved in Git history;
the current CSV reflects the refined version. Timing differences between those
runs are not attributed solely to this change.

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
sort output was observed in the covered tests. The initial generated parallel
implementation unnecessarily allocated closures even along sequential recursion;
allocation evidence exposed that overhead and it was fixed with a regression
test. Quicksort was evaluated but not
chosen because its straightforward form lacks a worst-case O(n log n) guarantee.
Unbounded recursive task creation and a claim that merge sort is in-place would
be inappropriate; neither was implemented. The cutoff/depth choices are simple
heuristics, not claimed globally optimal or backed by invented tuning runs.

**What were the most impactful improvements?** Replacing O(n²) comparison work
with guaranteed O(n log n) heapsort while retaining constant auxiliary memory,
and optionally parallelizing large merge-sort partitions with explicit bounds.
