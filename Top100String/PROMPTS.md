# Top 100 Frequent Words Stream — Prompt Log

This document records the prompts used during the development of this project, in the order they were issued.

---

## Prompt 1 — Initial Spec Generation

> I am want to create and application and I have a problem statement I would like help generating the SPECS for copilot or other agents to help produce a final result. Here is the problem statement:
>
> Please provide the solution in a C# and avoid the use of libraries or system functions.
>
> Top 100 Frequent Words Stream Problem: Write a function that takes a stream of text as input and maintains a data structure where you can quickly grab the top 100 most frequent words seen so far in the stream.

**Outcome:** Generated `SPECS.md` with full specification — problem statement, definitions, functional/non-functional requirements, architecture, data structures, heap-update strategy, file layout, sample output, testing guidance, and acceptance criteria.

---

## Prompt 2 — Spec Review & Refinement

> I made some modifications. See if there is anything else I should reconsider or update here:
>
> *(pasted full modified SPECS.md)*

**Outcome:** Identified and fixed 7 issues in the spec:
1. Typo "Deliminated" ? "Delimited", clarified Word definition to explicitly say ASCII letters only and list digits as delimiters.
2. Added missing `ContainsKey`, `UpdateKey`, `RemoveKey` methods to the MinHeap API table (§5.3) to match the indexed-heap strategy described in §6.
3. Added an **Index map** paragraph explaining the internal `Dictionary<string, int>`.
4. Fixed inverted/double-negative parsing pseudocode in §5.5 to positive logic ("if c is an ASCII letter ? append").
5. Fixed "puncutation" typo.
6. Clarified §6 that only sift-down is needed (frequency only increases).
7. Noted that `ReplaceMin` must remove the evicted word from the index map.

---

## Prompt 3 — Build the Implementation

> Now build given our spec please build

**Outcome:** Created all source files per §7 of the spec:
- `HeapEntry.cs` — `record struct HeapEntry(string Word, int Frequency)`
- `MinHeap.cs` — Binary min-heap with index map, `Insert`, `Peek`, `ExtractMin`, `ReplaceMin`, `ContainsKey`, `UpdateKey`, `RemoveKey`, `UnorderedItems`
- `WordParser.cs` — Struct-based character-by-character enumerator, ASCII-letter-only, yields lowercase words
- `WordFrequencyTracker.cs` — Orchestrator with `ProcessWord`, `ProcessLine`, `ProcessStream`, `GetTop100` (hand-written insertion sort)
- `Program.cs` — Entry point reading from stdin or file arg, prints ranked output

Build succeeded. Smoke test confirmed correct output.

---

## Prompt 4 — Create Test Project

> Let's test this out with different stream options including file with varying lengths and console echos that test all cases. Create a set a unit test or unit test project as well.

**Outcome:** Created `Top100String.Tests` xUnit project with 43 tests across 4 files:

| File | # Tests | Coverage |
|---|---|---|
| `WordParserTests.cs` | 10 | Empty, whitespace, single word, multi-word, punctuation, digits, mixed, uppercase, leading/trailing delimiters, tabs |
| `MinHeapTests.cs` | 12 | Empty throws, insert/peek, extract ordering, replace-min eviction, contains, update (valid + invalid), remove (middle + last), unordered items, index-map consistency stress test |
| `WordFrequencyTrackerTests.cs` | 11 | All §9 spec cases — empty stream, single word, case insensitivity, punctuation, descending order, alphabetical tiebreak, <100 / =100 / >100 unique words, multi-line, empty ignored, incremental processing, digit stripping |
| `IntegrationTests.cs` | 10 | 100K-word stream, 1M-word performance (<5s), multi-line equivalence, word rising into top-100, all-same-word, only-punctuation, only-digits |

**Bug caught & fixed during testing:** Test words like `word1`, `top000` contained digits (which are delimiters), collapsing all generated words to the same base. Added `ToLetterWord` helper using base-26 letter encoding. All 43 tests pass.

---

## Prompt 5 — Save Prompt Log

> Create and save the prompts we used in another MD file

**Outcome:** This file (`PROMPTS.md`).
