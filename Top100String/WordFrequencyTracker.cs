namespace Top100String;

/// <summary>
/// Orchestrator: maintains a frequency map and an indexed min-heap of size ? 100
/// that always contains the current top-100 most frequent words.
/// </summary>
public sealed class WordFrequencyTracker
{
    private const int K = 100;

    private readonly Dictionary<string, int> _frequencyMap = new();
    private readonly MinHeap _heap = new();

    public void ProcessWord(string word)
    {
        if (word.Length == 0)
            return;

        // Update frequency map
        if (_frequencyMap.TryGetValue(word, out int count))
            _frequencyMap[word] = count + 1;
        else
            _frequencyMap[word] = 1;

        int freq = _frequencyMap[word];

        // Update heap
        if (_heap.ContainsKey(word))
        {
            // Word already tracked in top-K — update its frequency in-place.
            _heap.UpdateKey(word, new HeapEntry(word, freq));
        }
        else if (_heap.Count < K)
        {
            // Heap not full — just insert.
            _heap.Insert(new HeapEntry(word, freq));
        }
        else if (freq > _heap.Peek().Frequency)
        {
            // New frequency beats the current minimum — evict min and insert.
            _heap.ReplaceMin(new HeapEntry(word, freq));
        }
        // else: word is not in the top K right now — do nothing.
    }

    public void ProcessLine(string line)
    {
        foreach (string word in WordParser.ParseWords(line))
            ProcessWord(word);
    }

    public void ProcessStream(TextReader reader)
    {
        string? line;
        while ((line = reader.ReadLine()) != null)
            ProcessLine(line);
    }

    /// <summary>
    /// Returns the current top-100 words sorted by frequency descending.
    /// Uses a hand-written insertion sort (?100 items).
    /// </summary>
    public (string Word, int Count)[] GetTop100()
    {
        HeapEntry[] items = _heap.UnorderedItems();
        int n = items.Length;

        // Insertion sort descending by frequency, then alphabetically ascending on tie.
        for (int i = 1; i < n; i++)
        {
            HeapEntry key = items[i];
            int j = i - 1;
            while (j >= 0 && CompareDescending(items[j], key) > 0)
            {
                items[j + 1] = items[j];
                j--;
            }
            items[j + 1] = key;
        }

        var result = new (string Word, int Count)[n];
        for (int i = 0; i < n; i++)
            result[i] = (items[i].Word, items[i].Frequency);
        return result;
    }

    /// <summary>
    /// Sort descending by frequency; ties broken alphabetically ascending.
    /// Returns &gt; 0 when a should come after b in the result.
    /// </summary>
    private static int CompareDescending(HeapEntry a, HeapEntry b)
    {
        int cmp = b.Frequency.CompareTo(a.Frequency); // descending
        if (cmp != 0) return cmp;
        return string.CompareOrdinal(a.Word, b.Word);  // ascending alpha
    }
}
