# LLM contribution reflection

This repository was created with an AI assistant using Copilot SDK in VS Code.
The assistant read the assignment documents, proposed a plan, generated C#
implementations and tests, ran validation commands, recorded actual benchmarks,
and wrote the activity reports. The learner selected C#/.NET 8, local-only Git,
stepwise commits, heapsort with optional parallel merge sort, bounded retries,
and English activity reports.

| Activity | Actual assistance | Main contribution |
|---|---|---|
| [1](activity-1.md) | Analyzed skewed insertion, generated AVL rotations/search/multiplicity storage and invariant tests | Guaranteed logarithmic height |
| [2](activity-2.md) | Generated explicit min-heap, FIFO sequence, batch validation/heapify, lock protection and concurrent tests | Avoid repeated sorting and prevent state races |
| [3](activity-3.md) | Evaluated sorting trade-offs, generated in-place heapsort and bounded parallel merge sort, reference tests and benchmarks | O(n²) to O(n log n), optional parallelism |
| [4](activity-4.md) | Reproduced invalid-task aborts, generated structured logging/outcomes and selective retries, tested continuation | Stable expected-failure handling |

The generated code was not accepted on trust alone: tests check rotations,
ordering, duplicates, concurrency, sorting against `Array.Sort`, validation,
retry limits, failure continuation, and unexpected exception propagation.
Measurements distinguish worst-case complexity guarantees from observed timing.
For example, random AVL insertion and valid-task execution can be slower after
the changes, and small heap drains can lose to the baseline.

Allocation measurements also exposed unnecessary closure allocation in the first
generated merge-sort implementation. The assistant isolated it to parallel splits
and added a single-buffer allocation regression test before refreshing measurements.

No historical prompts, earlier student work, or tuning experiments are invented.
Each activity report identifies evaluated alternatives
and answers the requested reflection questions. Tested correctness is evidence
for those scenarios, not proof of every possible behavior. Human review remains
necessary before production use, especially for retry-safe external side effects,
logging persistence, resource limits, and concurrent application integration.
