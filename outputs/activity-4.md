# Activity 4: Optimizing Data Structures With the LLM

## Work performed and reproduced defects

Preserved [the original executor](../src/DataStructures/Baseline/TaskExecutor.cs).
Only nullable annotations, an injectable `TextWriter`, and completed/pending
counters were added for observation. It still accepts null tasks and throws the
original general exceptions. Once a task fails it is already dequeued, processing
aborts, and later tasks remain stranded.

Implemented [the improved executor](../src/DataStructures/Optimized/TaskExecutor.cs):

- Null/blank tasks are rejected at submission with a structured validation log
  and an explicit `ArgumentException`. Callers may handle that rejection; it is
  not silently ignored or returned as a successful submission.
- Structured log records distinguish validation, processing, retry, success,
  and final failure; an injectable sink enables testing or integration with
  persistent application logging. The default console sink renders those events.
- Expected permanent failures are logged and returned as failed outcomes while
  processing continues. The original case-sensitive `Fail` example remains a
  permanent failure.
- Only a declared `TransientTaskException` is retried, with two retries maximum
  (three attempts total). Exhausted retries produce a failed outcome.
- Every accepted task processed under the expected failure contract returns an
  explicit success/failure result and attempt count.
- Unexpected handler exceptions and logger failures propagate; programming bugs
  are not disguised as routine task failures.

Annotated comments explain validation and narrowly scoped retries.
The executor is sequential and not thread-safe. Retries are immediate and bounded,
not a production backoff policy. Retried handlers must be idempotent or otherwise
safe to repeat; partial external side effects are not rolled back. A propagated
unexpected error aborts the call and leaves later tasks queued; this educational
executor does not persist already-produced results across such an abort.

## Correctness evidence

[ExecutorTests.cs](../tests/DataStructures.Tests/ExecutorTests.cs): 12 passing cases
reproduce both baseline crash paths and cover invalid submission, success,
permanent failure with continuation, recovery after zero/one/two transient
failures, retry exhaustion, unexpected handler errors, and logging failure.

The demo logs an invalid rejection, permanent failure, two transient retries and
successful recovery, then completes the following task. Its result is three
completed, one failed, and zero pending accepted tasks.

## Reliability comparison

The original four submissions are `Task 1`, null, `Fail Task`, `Task 2`.
The benchmark asserts these counts before writing
[reliability evidence](activity-4-reliability.csv):

| Implementation | Submitted | Rejected | Completed | Recorded failed | Aborted current task | Pending |
|---|---:|---:|---:|---:|---:|---:|
| Baseline | 4 | 0 | 1 | 0 | 1 | 2 |
| Optimized | 4 | 1 | 2 | 1 | 0 | 0 |

The baseline abort is the null task, not a recorded outcome. The optimized
permanent failure is `Fail Task`; null never enters its queue. No speedup is
computed from a crashing/incomplete workload.

## Complexity and measured throughput

| Operation | Baseline | Optimized |
|---|---|---|
| Queue insertion/removal | O(1) amortized insertion, O(1) removal | Same |
| Process m valid tasks | O(m) plus handler/output work | O(m) plus handler/log/result work |
| Expected retries | None; first error aborts | At most 3m attempts, still O(m) for bounded-cost handlers |
| Storage | O(m) queued references | O(m) queued references + O(m) returned results |

String validation/log formatting also depends on string length. Handler cost
dominates in real applications and is not assumed constant universally.

Median valid-task processing milliseconds, 2 warmups and 7 samples:

| Tasks | Baseline | Optimized |
|---:|---:|---:|
| 256 | 0.014 | 0.112 |
| 8192 | 0.405 | 2.305 |

The optimized valid path is slower (about 5.7x at 8192) because it creates
structured events and durable-to-the-caller outcome records. Both paths format
two messages per successful task and discard output through `TextWriter.Null`;
the optimized message contains additional fields. This compares comparable
discarded logging configurations, not identical record-allocation work or actual
I/O performance. Task submission is excluded, result/event creation is included.
The key gain is correctness and completing later work after expected failures,
not a fabricated throughput improvement.

[Raw timing/allocation samples](activity-4-samples.csv) and
[environment](activity-4-environment.txt) provide measurement context.
Process-wide allocation includes runtime noise.

## Reproduce

```powershell
dotnet test DataStructures.sln -c Release --filter FullyQualifiedName~ExecutorTests
dotnet run --project src\Demo -c Release -- 4
dotnet run --project benchmarks\DataStructures.Benchmarks -c Release -- 4
```

Run from the repository root. The last command regenerates timing and reliability
evidence. It catches only the exact intentional baseline null exception;
unexpected benchmark errors propagate.

## Reflection answers

**How did the LLM assist in debugging and optimizing the code?** The Copilot SDK
assistant identified invalid-input acceptance and exception-driven queue
abandonment, generated the validation/log/result design and bounded retry loop,
and built tests reproducing the original failures plus the corrected behavior.

**Were any LLM-generated suggestions inaccurate or unnecessary?** No inaccurate
result was found in the covered tests. Blanket exception catches, retrying every
failure, unlimited retries, and complex concurrent execution were evaluated as
inappropriate to the requested simple executor and not implemented. The measured
overhead demonstrates why describing error-handling additions as an automatic
valid-path speed improvement would be inaccurate.

**What were the most impactful improvements?** Rejecting malformed tasks before
queuing, continuing after declared permanent failures, explicit failure outcomes,
structured error logs, and limiting retries to recoverable errors.
