# Data structures and algorithms coursework repository

## Problem and approach

Create a local C#/.NET 8 Git repository implementing all four assignments and the consolidated submission specification. Preserve the supplied assignment documents. Show the learning progression through independent, meaningful commits for the baseline, optimization, and validation/reporting steps of each activity.

Each activity will have an English output Markdown report answering every assignment reflection question and documenting actual implementation work, correctness evidence, reproducible performance measurements, and verified LLM contributions. No repository files will be changed until this plan is approved.

## Confirmed scope and decisions

- Local Git initialization and commits only; no GitHub publication.
- Use the installed .NET 8 SDK without installing another SDK.
- Implement all four activities, retaining baseline implementations separately from optimized implementations.
- Sorting uses in-place heapsort with guaranteed O(n log n), plus an optional parallel merge-sort implementation.
- Task execution rejects invalid tasks with logged validation errors, retries transient failures at most twice after the initial attempt, and records permanent/exhausted failures while continuing with later tasks.
- Write one complete English report per activity under `outputs`, including work performed, measurements, commands, and answers to every reflection question.
- Record only real LLM assistance and actually evaluated suggestions. Do not invent prompts, historical iterations, rejected recommendations, performance numbers, or speedups.

## Proposed repository layout

- Existing `specification.md` and `activities` remain the source assignment material.
- `README.md`: prerequisites, project map, demo/test/benchmark commands, and submission links.
- `global.json`: select the installed .NET 8 SDK feature band; project files target `net8.0`.
- `.gitignore`: generated build, IDE, and test artifacts.
- A solution containing:
  - `src/DataStructures`: clearly separated baseline and optimized implementations for each activity.
  - `src/Demo`: runnable demonstrations for all four activities, selectable by activity.
  - `tests/DataStructures.Tests`: focused xUnit tests.
  - `benchmarks/DataStructures.Benchmarks`: a Release-mode measurement console using .NET timing facilities, reproducible datasets, and machine-readable output.
- `outputs/activity-1.md`, `outputs/activity-2.md`, `outputs/activity-3.md`, `outputs/activity-4.md`: activity deliverables.
- `outputs/performance-report.md`: consolidated measured results and before/after complexity table.
- `outputs/llm-contribution-reflection.md`: brief consolidated reflection with links to the individual reports.
- `outputs/benchmark-results.csv`: actual measurement data used in the reports.

Use .NET scaffolding and package tooling for project creation. Add dependencies only where needed by project manifests, primarily the test runner. Keep implementation dependencies minimal.

## Implementation todos

### 1. Establish repository and runnable foundation

Inspect Git identity before committing. Initialize a local repository, establish the SDK selection, solution, projects, ignore rules, and README. Capture the original assignment documents in the first commit. Configure nullable reference checking and keep build diagnostics clean. Do not alter global Git settings; ask for author details if commits cannot be created with the available identity.

### 2. Implement and validate activity 1: binary tree

- Preserve the assignment's unbalanced insertion behavior as the baseline, with only required project-integration adaptations.
- Add an iterative baseline search for a fair comparison, explicitly identifying it as an addition rather than part of the original snippet.
- Implement an AVL tree with cached heights and correctly tested single/double rotations.
- Provide efficient search and in-order traversal. Preserve duplicate values by storing a multiplicity count, so balancing does not discard values from the baseline's multiset behavior.
- Reduce traversal/search recursion where useful; explain the bounded recursion retained for AVL insertion.
- Test empty trees, ordered/reverse/random inputs, duplicate values, all rotation cases, traversal ordering, and balance/height invariants.
- Measure build and retrieval workloads independently using equivalent data, including ordered input showing baseline degeneration. Keep baseline recursion depth within safe bounds.
- Write the activity report and concise comments explaining non-obvious balancing improvements and their Copilot-assisted rationale.

### 3. Implement and validate activity 2: priority scheduling

- Preserve the repeated list-sort/front-removal baseline; lower numeric values remain higher priority.
- Implement an explicit binary min-heap with O(log n) enqueue/dequeue and a sequence number for deterministic FIFO ties.
- Add batch enqueue using bottom-up heap construction over the combined queue, documenting O(n + k) work and temporary/storage costs.
- Synchronize heap state and sequence allocation with one lock; snapshot batch inputs before mutation, validate inputs explicitly, and avoid executing requests while holding the queue lock.
- Keep empty dequeue behavior explicit and nullable/try-based, rather than treating emptiness as a failure.
- Test priority order, empty behavior, duplicates/tie order, batch vs individual equivalence, invalid inputs, and concurrent producers/consumers for no loss or duplication.
- Benchmark individual and bulk workloads; report lock overhead without claiming scalability beyond the measured workloads.
- Document FIFO tie behavior as an intentional clarification because the original list sort does not guarantee it.
- Complete all activity reflection answers and the report.

### 4. Implement and validate activity 3: sorting

- Preserve bubble sort as the O(n²) baseline.
- Implement in-place heapsort with O(n log n) worst-case time and O(1) auxiliary space.
- Implement optional parallel merge sort with O(n) auxiliary storage, disjoint output ranges, a sequential cutoff, and bounded parallel depth.
- Keep sorting mutation/ascending ordering behavior consistent and explicitly validate null inputs.
- Test empty/single-element arrays, duplicates, negative/extreme values, sorted/reverse/random inputs, and both sides of parallel cutoffs against .NET's reference sorting result.
- Measure baseline, heapsort, and sequential/parallel merge sorting on equivalent cloned datasets, including small and larger workloads. Avoid claiming parallel sorting is always faster.
- Explain the space/parallelism trade-offs and answer every reflection question in the report.

### 5. Implement and validate activity 4: task execution

- Preserve the original crash-prone queue processor as an explicitly educational baseline.
- Keep the improved executor sequential and learner-friendly, using an injectable task handler and logging sink.
- Reject null/blank submissions explicitly and log the validation failure.
- Distinguish expected transient execution failures from permanent failures; catch only the defined execution-failure types. Do not swallow unexpected programming errors.
- Bound transient retries to three total attempts, preserve task ordering, and expose/log task outcomes and attempts.
- Log permanent or exhausted failures and continue with later tasks; keep the original `Fail` demonstration permanently failing and add a demonstrably transient example.
- Test invalid submission, normal completion, permanent failure, transient recovery, retry exhaustion, continuing after expected failure, and unexpected exception propagation.
- Measure equivalent valid-task throughput with comparable logging settings. Separately compare completed/failed/remaining task counts on failure scenarios; do not manufacture a speedup for a baseline that crashes.
- Complete the report with concrete debugging evidence and every reflection answer.

### 6. Consolidate and verify submission

- Run reproducible Release benchmarks with deterministic inputs, warmups, repeated samples, runtime/machine metadata, input sizes, operation counts, median durations, and measured allocation information where valid.
- Generate checked-in raw results and use them to populate each activity report and the consolidated performance report.
- Keep setup, cloning, and unrelated output outside measured algorithm sections where possible, and document any unavoidable inclusion.
- Validate correctness before accepting benchmark results. Use invariant/complexity tests rather than flaky wall-clock speed thresholds.
- Explain expected complexities separately from observed timings; report non-improvements honestly.
- Ensure each activity report includes: assignment mapping; baseline defects; implemented changes and concise code pointers; correctness tests; before/after complexity; measured results and limitations; exact reproduction commands; and answers to how the LLM assisted, which evaluated suggestions were inaccurate/unnecessary (or none), and which improvements mattered most.
- Write the consolidated reflection and link every required deliverable from the README.
- Run the full Release build and test suite, each demo, and the documented measurement commands. Verify Git history, tracked artifacts, and clean working state.

## Commit sequence

Create each commit only after the relevant runnable state is verified. Baseline commits intentionally preserve the defects being studied; demos/tests must expose them safely rather than accidentally crashing routine validation.

1. `chore: initialize coursework repository and .NET solution`
2. `feat(activity-1): preserve binary tree baseline`
3. `feat(activity-1): add balanced AVL tree and search`
4. `test(activity-1): verify invariants and document measured improvements`
5. `feat(activity-2): preserve list-based scheduling baseline`
6. `feat(activity-2): add thread-safe heap scheduling and batch enqueue`
7. `test(activity-2): verify scheduling and document measured improvements`
8. `feat(activity-3): preserve bubble sort baseline`
9. `feat(activity-3): add heapsort and bounded parallel merge sort`
10. `test(activity-3): verify sorting and document measured improvements`
11. `feat(activity-4): preserve task execution failure baseline`
12. `feat(activity-4): add validated execution logging and bounded retries`
13. `test(activity-4): verify failure handling and document measured results`
14. `docs: consolidate performance report and LLM contribution reflection`

Include the required Copilot co-author trailer on each commit. Do not squash the progression or commit generated build artifacts. Activity-focused benchmark support and raw data are added in the relevant validation/reporting commits; the last commit consolidates the submission without inventing new results.

## Notes and risks

- The workspace currently contains only the specification and four activity documents, and is not a Git repository.
- SDKs 8.0.417 and 9.0.303 are installed; .NET 8 was chosen by the user.
- Preserve recognizable public operations from the snippets, while documenting necessary nullable and encapsulation changes.
- Timing depends on environment and input distributions. No timing guarantee or grading guarantee is promised.
- A lock-protected heap provides correctness, not unlimited concurrent scalability. Parallel sorting has overhead and must be bounded.
- The failure baseline cannot complete malformed workloads; reliability metrics supplement throughput rather than disguise this limitation.
- Unexpected conflicting file changes, missing commit identity, or additional required behavioral choices will be surfaced before proceeding.
