# Top 100 Frequent Words Stream — Specification

## 1. Problem Statement

Write a C# console application that reads a **stream of text** and continuously maintains a data structure that can return the **top 100 most frequent words** seen so far at any point in the stream.

**Constraint:** Do not use LINQ, `SortedDictionary`, `SortedSet`, `PriorityQueue`, or any third-party libraries. Core collection types (`Dictionary<K,V>`, `List<T>`, arrays) are permitted only as building blocks for the custom data structures described below; all sorting/ranking logic must be implemented by hand.

---

## 2. Definitions

| Term | Definition |
|---|---|
| **Word** | A maximal contiguous sequence of **ASCII letters** (`a-z`, `A-Z`). Digits, punctuation, whitespace, and newlines are all treated as delimiters (not part of any word). All comparisons are **case-insensitive** (e.g., `"The"` and `"the"` are the same word). |
| **Stream** | Text arriving incrementally — modelled as a `TextReader` (could be `Console.In`, a `StreamReader` over a file, or a `StringReader` for tests). The solution must process text **line-by-line** without loading the entire input into memory. |
| **Frequency** | The total number of occurrences of a word observed so far. |
| **Top-K (K=100)** | The K words with the highest frequency. Ties may be broken arbitrarily (or alphabetically as a secondary sort — implementor's choice). |

---

## 3. Functional Requirements

| ID | Requirement |
|---|---|
| **FR-1** | Parse incoming text into words using the definition above (strip punctuation, digits, whitespace). |
| **FR-2** | Maintain a **word-frequency map** that is updated as each word is parsed. |
| **FR-3** | Maintain a **min-heap of size ? 100** that always contains the current top-100 most frequent words. |
| **FR-4** | Expose a method `GetTop100()` that returns the current top-100 words sorted in **descending frequency order** in O(K log K) time. |
| **FR-5** | After all input is consumed, print the top-100 list to `Console.Out` in the format: `<rank>. <word> — <count>` (one per line). |

---

## 4. Non-Functional Requirements

| ID | Requirement |
|---|---|
| **NFR-1** | **No external libraries.** Only the .NET 8 Base Class Library (BCL). |
| **NFR-2** | **Custom data structures.** The min-heap and word-parsing logic must be hand-implemented (no `PriorityQueue<T,T>`, no LINQ `OrderBy`). |
| **NFR-3** | **Streaming.** Memory usage must be proportional to the number of **unique** words, not the total input size. |
| **NFR-4** | **Performance targets.** Processing a word should be O(log K) amortised (heap operations) on top of O(1) average hash-map access. `GetTop100()` should be O(K log K). |

---

## 5. Architecture & Data Structures

### 5.1 Component Overview

```
TextReader (stream)
?
?
????????????????
? WordParser ? ?? Extracts words one-by-one from lines of text
????????????????
?
?
????????????????????
? WordFrequencyMap ? ?? Dictionary<string, int> wrapper; O(1) upsert
????????????????????
?
?
????????????????????????
? MinHeap<HeapEntry> ? ?? Custom binary min-heap (capacity 100)
? ? ordered by frequency (ascending)
????????????????????????
?
?
GetTop100() ? HeapEntry[] sorted descending
```

### 5.2 `HeapEntry`

```
record struct HeapEntry(string Word, int Frequency);
```

### 5.3 Custom Min-Heap — `MinHeap<T>`

Implement a binary min-heap backed by an array with the following operations:

| Method | Complexity | Description |
|---|---|---|
| `Insert(T item)` | O(log n) | Add item; bubble up. |
| `Peek()` | O(1) | Return the minimum element. |
| `ExtractMin()` | O(log n) | Remove and return the minimum; bubble down. |
| `ReplaceMin(T item)` | O(log n) | Replace root and bubble down (used when a new word's frequency exceeds the current min). |
| `Count` | O(1) | Number of elements currently in the heap. |
| `UnorderedItems` | O(n) | Enumerate all items (used by `GetTop100` before sorting). |
| `bool ContainsKey(string key)` | O(1) | Check whether a word is currently in the heap (backed by the internal index map). |
| `void UpdateKey(string key, T newItem)` | O(log n) | Replace the entry for `key` with `newItem` and sift-down to restore heap order (frequency can only increase, so only downward moves are needed). |
| `void RemoveKey(string key)` | O(log n) | Remove the entry for `key` by swapping with the last element and sift-down/sift-up as needed. Updates the index map. |

The heap should accept a `Comparison<T>` delegate so it can be reused.

**Index map:** Internally the heap must maintain a `Dictionary<string, int>` mapping each word to its current array index. All insert/remove/swap operations must keep this map in sync so that `ContainsKey` and `UpdateKey` are O(1) lookup + O(log K) fix-up.

### 5.4 `WordFrequencyTracker` (Orchestrator)

| Method / Property | Description |
|---|---|
| `void ProcessWord(string word)` | Lowercases the word, increments its count in the frequency map, and updates the min-heap (insert if heap has room, or replace-min if the word's new frequency exceeds the heap's minimum). |
| `void ProcessLine(string line)` | Parses words from a line and calls `ProcessWord` for each. |
| `void ProcessStream(TextReader reader)` | Reads lines from the reader until EOF, calling `ProcessLine`. |
| `(string Word, int Count)[] GetTop100()` | Extracts heap contents and sorts them descending by frequency (hand-written sort — e.g., heap-sort or insertion-sort on ?100 items). |

### 5.5 Word Parsing Logic

```
For each character c in the line:
    if c is an ASCII letter (a-z or A-Z):
        append lowercase of c to current word buffer
    else:                                         // digit, punctuation, whitespace, etc.
        if buffer is non-empty:
            yield the buffered word
            reset buffer
After the line ends, yield any remaining buffered word.
```

> **Note:** `yield return` / `foreach` (C# iterator blocks) are core language features, not LINQ, and are permitted.

---

## 6. Heap-Update Strategy

When `ProcessWord` is called with a word whose updated frequency is `f`:

1. **Word is already in the heap** (`heap.ContainsKey(word)`):
   - Call `heap.UpdateKey(word, new HeapEntry(word, f))`.
   - Because frequency can only *increase*, the entry's value grows, so it can only violate the min-heap property **downward** — only a sift-down is ever needed.

2. **Word is NOT in the heap:**
   - If `heap.Count < 100`: call `heap.Insert(new HeapEntry(word, f))`.
   - Else if `f > heap.Peek().Frequency`: call `heap.ReplaceMin(new HeapEntry(word, f))`. **Important:** the evicted min-word must also be removed from the index map (handled internally by `ReplaceMin`).
   - Else: do nothing (the word is not in the top 100 right now).

This "indexed min-heap" approach keeps per-word updates O(log K) and avoids rescanning the entire frequency map.

---

## 7. File / Class Layout

```
Top100String/
??? Program.cs                   — Entry point: wire up TextReader ? WordFrequencyTracker ? print results
??? WordParser.cs                — Static helper: IEnumerable<string> ParseWords(string line)
??? MinHeap.cs                   — Generic min-heap with index tracking
??? HeapEntry.cs                 — record struct HeapEntry(string Word, int Frequency)
??? WordFrequencyTracker.cs      — Orchestrator (frequency map + heap)
??? Top100String.csproj          — .NET 8 console app (no extra packages)
```

---

## 8. `Program.cs` Behaviour

```
1. Read from Console.In (stdin) OR accept a file path as args[0].
2. Create a WordFrequencyTracker.
3. Call tracker.ProcessStream(reader).
4. Call tracker.GetTop100().
5. Print each entry: "  1. the — 4523"
```

### Sample Output

```
  1. the — 4523
  2. of — 3012
  3. and — 2890
  ...
100. still — 42
```

---

## 9. Testing Guidance

| Test Case | Input | Expected Behaviour |
|---|---|---|
| **Empty stream** | `""` | `GetTop100()` returns empty array. |
| **Single word** | `"hello"` | Returns `[("hello", 1)]`. |
| **Fewer than 100 unique words** | 50 unique words | All 50 returned, sorted descending. |
| **Exactly 100 unique words** | 100 words, varying freq | All 100 returned correctly. |
| **More than 100 unique words** | 200 words | Only top 100 by frequency returned. |
| **Case insensitivity** | `"Hello HELLO hello"` | `[("hello", 3)]`. |
| **Punctuation stripping** | `"it's a well-known fact"` | Words: `it`, `s`, `a`, `well`, `known`, `fact`. |
| **Large stream** | 1 million+ words | Completes in reasonable time; memory ? unique words. |
| **Tie-breaking** | Multiple words same freq | All present; order among ties is stable or alphabetical. |

---

## 10. Acceptance Criteria

- [ ] All source files compile with `dotnet build` (net8.0, no warnings).
- [ ] Running `echo "some text" | dotnet run` prints the top-100 list.
- [ ] Running `dotnet run -- path/to/largefile.txt` works for file input.
- [ ] No LINQ, no `PriorityQueue`, no `SortedSet`, no external NuGet packages.
- [ ] The min-heap and sorting are hand-implemented.
- [ ] Unit tests (if added) cover the cases in §9.
