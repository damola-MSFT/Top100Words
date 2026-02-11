# Top 100 Frequent Words Stream

A C# console application that reads a **stream of text** and continuously maintains a data structure capable of returning the **top 100 most frequent words** seen so far — at any point in the stream.

All sorting, ranking, and parsing logic is hand-implemented using only core .NET 8 BCL types (no LINQ, no `PriorityQueue`, no third-party libraries).

## Problem Statement

Given an arbitrarily large stream of text, efficiently track word frequencies and support querying the 100 most frequent words at any time with:

- **O(1)** average hash-map access per word
- **O(log K)** amortised heap update per word (K = 100)
- **O(K log K)** for `GetTop100()`
- **Memory proportional to unique words**, not total input size

## Architecture

```
TextReader (stdin / file / StringReader)
  ?
  ?
????????????????
?  WordParser   ? ?? Character-by-character extraction, ASCII letters only, lowercased
????????????????
  ?
  ?
??????????????????????
? WordFrequencyTracker? ?? Orchestrator: frequency map + indexed min-heap
?                    ?
?  Dictionary<K,V>   ?    O(1) upsert
?  MinHeap (size?100)?    O(log K) insert / update / evict
??????????????????????
  ?
  ?
GetTop100() ? (string Word, int Count)[] sorted descending
```

### Key Components

| File | Description |
|---|---|
| `Program.cs` | Entry point — reads from stdin or a file argument, prints ranked output |
| `WordParser.cs` | Static helper yielding lowercase words from a line (ASCII letters only; digits, punctuation, and whitespace are delimiters) |
| `WordFrequencyTracker.cs` | Orchestrator maintaining a `Dictionary<string, int>` frequency map and an indexed min-heap of size ? 100 |
| `MinHeap.cs` | Generic binary min-heap with an internal `Dictionary<string, int>` index map for O(1) lookup + O(log K) fix-up |
| `HeapEntry.cs` | `record struct HeapEntry(string Word, int Frequency)` |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build

```bash
dotnet build
```

### Run

**From stdin** (type or pipe text, then send EOF with `Ctrl+Z` on Windows / `Ctrl+D` on Linux/macOS):

```bash
dotnet run --project Top100String
```

**From a file:**

```bash
dotnet run --project Top100String -- path/to/input.txt
```

### Sample Output

```
 1. the — 4523
 2. of — 3210
 3. and — 2987
 ...
100. still — 42
```

## Testing

The solution includes a comprehensive xUnit test project with **43 tests** across four files:

| File | Tests | Coverage |
|---|---|---|
| `WordParserTests.cs` | 10 | Empty, whitespace, single word, multi-word, punctuation, digits, mixed case, leading/trailing delimiters, tabs |
| `MinHeapTests.cs` | 12 | Empty throws, insert/peek, extract ordering, replace-min, contains, update, remove, unordered items, index-map consistency |
| `WordFrequencyTrackerTests.cs` | 11 | Empty stream, single word, case insensitivity, punctuation, descending order, alphabetical tiebreak, <100 / =100 / >100 unique words, multi-line, incremental processing, digit stripping |
| `IntegrationTests.cs` | 10 | 100K-word stream, 1M-word performance (<5 s), multi-line equivalence, word rising into top-100, all-same-word, only-punctuation, only-digits |

```bash
dotnet test
```

## Design Decisions

- **Indexed min-heap** — Words already in the top 100 are updated in-place via an index map, avoiding a full rescan of the frequency map on every word.
- **Sift-down only** — Because word frequency can only increase, an updated entry can only violate heap order downward.
- **Hand-written insertion sort** for `GetTop100()` — At most 100 elements, so simplicity is preferred over algorithmic overhead.
- **Streaming** — Input is processed line-by-line via `TextReader.ReadLine()`; the full input is never loaded into memory.
- **Case-insensitive** — All words are lowercased during parsing; `"The"` and `"the"` are counted as the same word.

## Project Structure

```
Top100String/
??? Program.cs
??? WordParser.cs
??? MinHeap.cs
??? HeapEntry.cs
??? WordFrequencyTracker.cs
??? Top100String.csproj
??? SPECS.md                  ? Full specification
??? PROMPTS.md                ? Prompt log used during development

Top100String.Tests/
??? WordParserTests.cs
??? MinHeapTests.cs
??? WordFrequencyTrackerTests.cs
??? IntegrationTests.cs
??? Top100String.Tests.csproj
```

## License

This project is provided as-is for interview / educational purposes.
