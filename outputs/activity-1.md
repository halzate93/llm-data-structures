# Activity 1: Hands-On Microsoft Copilot Code Generation

## Work performed

Reviewed the supplied unbalanced binary search tree and retained it in
[BinaryTree.cs](../src/DataStructures/Baseline/BinaryTree.cs). Its ascending
insertions form a linked-list-shaped tree. The original snippet had no search;
an iterative `Contains` was added to the baseline solely for fair retrieval
comparisons. Recursive insertion and duplicate-right behavior remain intact.

Implemented [AvlTree.cs](../src/DataStructures/Optimized/AvlTree.cs):

- AVL single and double rotations prevent degeneration.
- Cached heights avoid repeated subtree-height calculations.
- Iterative search avoids recursive call overhead; iterative in-order traversal
  uses an explicit O(log n) stack.
- Duplicate multiplicities preserve every original value without duplicate chains.
- Private nodes prevent callers from corrupting the balanced structure.
- Insertion retains O(log n)-depth recursion because ancestor rebalancing is
  clearer than an additional parent/path abstraction.

Concise annotated comments identify Copilot-assisted height caching and duplicate
handling. The tree is not thread-safe; synchronization and deletion are outside
this assignment's scope.

## Correctness evidence

[TreeTests.cs](../tests/DataStructures.Tests/TreeTests.cs) contains 10 passing cases:
all four rotation shapes, ordered/reverse/random/duplicate input, empty/extreme
values, and demonstrated baseline degeneration. Tests verify BST bounds, cached
heights, balance factors, counts, sorted multiset traversal, lookup, and a
logarithmic height bound. The demo retains duplicate 30 and finds priority 40.

## Complexity and measured performance

| Operation | Baseline worst case | AVL worst case |
|---|---|---|
| Single insertion/search | O(n) | O(log n) |
| Build from n ordered values | O(n²) | O(n log n) |
| Traverse all values | O(n) time, O(n) recursive stack | O(n) time, O(log n) explicit stack |
| Stored memory | O(n) nodes | O(u) nodes for u distinct values, O(n) worst case |

Median elapsed milliseconds from seven samples after two warmups:

| Workload | n | Baseline | AVL |
|---|---:|---:|---:|
| Ordered build | 256 | 0.085 | 0.045 |
| Ordered search of all values | 256 | 0.122 | 0.017 |
| Random build | 256 | 0.010 | 0.041 |
| Random search of all values | 256 | 0.015 | 0.024 |
| Ordered build | 2048 | 5.326 | 0.472 |
| Ordered search of all values | 2048 | 2.573 | 0.170 |
| Random build | 2048 | 0.107 | 0.206 |
| Random search of all values | 2048 | 0.085 | 0.080 |

At n=2048, ordered build improved about 11.3x and ordered retrieval about 15.2x.
Random insertion was slower with AVL: maintaining a guaranteed height costs work
when the baseline happens already to be reasonably balanced. Memory improvement
is specifically duplicate compression, not a claim that AVL nodes are smaller.

Raw timings and process-wide allocated bytes:
[samples](activity-1-samples.csv), [environment](activity-1-environment.txt).
Seed 42, Release .NET 8, equivalent input arrays, setup excluded, validation
after each sample. Search includes the same successful-lookup checks for both
implementations. Baseline sizes are bounded to avoid recursive stack exhaustion.
Very short timings and JIT/runtime effects limit precision; these are lightweight
educational measurements, not production capacity guarantees.

## Reproduce

Run from the repository root in PowerShell:

```powershell
dotnet test DataStructures.sln -c Release --filter FullyQualifiedName~TreeTests
dotnet run --project src\Demo -c Release -- 1
dotnet run --project benchmarks\DataStructures.Benchmarks -c Release -- 1
```

The last command regenerates raw samples; timings vary between runs.

## Reflection answers

**How did the LLM assist in refining the code?** The Copilot SDK assistant
analyzed the missing lookup and worst-case insertion shape, generated the AVL
implementation, proposed multiplicity storage, and wrote tests, demo, and
measurement code. This repository records assistance actually performed during
its creation, not a claim about earlier course work.

**Were any LLM-generated suggestions inaccurate or unnecessary?** No inaccurate
generated implementation was observed in the tested cases. We deliberately
avoided claiming balancing always speeds up insertion or reduces node size:
the random-input measurements contradict the former, and cached heights add
per-node state. Eliminating all recursion or adding concurrent tree machinery
would have been unnecessary for this exercise; those were evaluated design
alternatives, not invented historical Copilot prompts.

**What were the most impactful improvements?** Guaranteed logarithmic height
and efficient search remove ordered-input degeneration. Duplicate compression
and cached heights maintain that guarantee without redundant subtree scans.
