# Activity 2: Applying Copilot Generated Algorithms in Real-World Contexts

## Work performed

Retained [the list baseline](../src/DataStructures/Baseline/ApiRequestQueue.cs):
every enqueue sorts the entire list and every dequeue shifts its remaining
elements. Added only a count property for validation.

Implemented [a binary min-heap](../src/DataStructures/Optimized/ApiRequestQueue.cs):

- Sift-up/down replaces repeated full sorting and front removal.
- Lower numeric priorities still run first.
- An insertion sequence guarantees FIFO for equal priorities. This is an
  intentional clarification: the baseline's unstable `List.Sort` did not promise
  FIFO ties.
- One lock protects heap state, count, and sequence allocation. Requests are
  returned before execution, so handlers do not run inside the queue lock.
- Batch sources are snapshotted and validated before mutation; bottom-up heapify
  rebuilds the combined heap in O(n + k) for n queued and k incoming requests.
- Optimized requests are immutable and reject blank endpoints. This intentional
  API change prevents priority mutation from silently breaking heap ordering.
- Null requests/sources and sequence overflow produce explicit exceptions.
  Empty dequeue still returns null; an empty batch is valid.

Annotated comments explain the Copilot-assisted atomic batch validation and
linear-time heap construction. Batch insertion is atomic with respect to other
queue operations, although it holds the lock while heapifying. FIFO is defined by
lock acquisition order for overlapping producers, not their wall-clock start
times. The queue is unbounded and is not a complete API execution service.

## Correctness evidence

[QueueTests.cs](../tests/DataStructures.Tests/QueueTests.cs): 5 passing tests cover
priority/extreme values, FIFO ties across batches, batch/individual equivalence,
empty behavior, invalid or throwing batch sources with no partial mutation, and
four concurrent producers plus four consumers receiving exactly 2000 distinct
requests. A baseline test confirms original priority order and empty results.
The demo processes `/auth`, `/auth-next`, `/healthcheck`, then `/data`.

Concurrency testing checks no loss/duplication; it does not assert an impossible
global priority ordering against requests that have not arrived yet.

## Complexity and actual measurements

| Operation | List baseline | Heap |
|---|---|---|
| Single enqueue | O(n log n) worst-case sort | O(log n) amortized |
| Single dequeue | O(n) shifting | O(log n) |
| n individual enqueues | O(n² log n) upper bound | O(n log n) amortized |
| Drain n requests | O(n²) | O(n log n) |
| Batch of k into n existing | Repeated sorting | O(n + k) |
| Retained storage | O(n) | O(n), with a sequence per entry |

`List` capacity growth occasionally copies O(n) elements; therefore single heap
enqueue's O(log n) statement is amortized, not a strict worst-case guarantee.
Batch snapshot requires O(k) temporary references. Lock waiting is not bounded by
the algorithmic complexity.

Median milliseconds (2 warmups, 7 samples, seed 42):

| Workload | n | List | Heap | Heap batch |
|---|---:|---:|---:|---:|
| Enqueue all | 256 | 1.323 | 0.033 | 0.030 |
| Drain all | 256 | 0.013 | 0.138 | - |
| Enqueue all | 2048 | 32.451 | 0.105 | 0.046 |
| Drain all | 2048 | 0.311 | 0.247 | - |

At n=2048 individual insertion improved about 309x; batch insertion improved
about 705x relative to repeated sorting. Small-queue heap drain was slower:
comparison, sequence, and lock overhead exceed fast contiguous list shifting at
that size. The larger drain improved only about 1.3x. These measurements do not
claim unlimited scalability or a measured concurrent throughput improvement.

[Raw samples](activity-2-samples.csv) include allocation counts;
[environment](activity-2-environment.txt) documents the runtime. Request creation
and drain-result allocation are excluded. Enqueue validation drains outside the
timed region; drain measures removal plus equivalent result recording. Heap batch
snapshot allocation is included. Process-wide allocation includes runtime noise.

## Reproduce

```powershell
dotnet test DataStructures.sln -c Release --filter FullyQualifiedName~QueueTests
dotnet run --project src\Demo -c Release -- 2
dotnet run --project benchmarks\DataStructures.Benchmarks -c Release -- 2
```

Run from the repository root. Regenerated timing values will vary.

## Reflection answers

**How did the LLM assist in refining the algorithm?** The Copilot SDK assistant
generated the explicit heap, synchronization, sequence tie-breaker and
prevalidated batch operation, then implemented the correctness/concurrency tests
and performance comparison.

**Were any LLM-generated suggestions inaccurate or unnecessary?** Tests found no
inaccurate generated behavior in their covered scenarios. It would be inaccurate
to call the lock-free or universally faster; neither claim is made. A framework
priority queue was a valid alternative but an explicit heap better illustrates
the assignment. Complex execution concurrency was unnecessary and deliberately
excluded; thread safety covers queue operations only.

**What were the most impactful improvements?** Eliminating per-insertion full
sorting, bulk heap construction, and protecting state during concurrent access.
Immutable priorities and deterministic FIFO ties make behavior dependable.
