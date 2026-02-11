namespace Top100String;

/// <summary>
/// A binary min-heap with an internal index map for O(1) key lookup
/// and O(log n) key-based updates. The key is extracted from each item
/// via a caller-supplied delegate.
/// </summary>
public sealed class MinHeap
{
    private readonly List<HeapEntry> _items = new();
    private readonly Dictionary<string, int> _indexMap = new();

    public int Count => _items.Count;

    public HeapEntry Peek()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Heap is empty.");
        return _items[0];
    }

    public void Insert(HeapEntry item)
    {
        _items.Add(item);
        int index = _items.Count - 1;
        _indexMap[item.Word] = index;
        BubbleUp(index);
    }

    public HeapEntry ExtractMin()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Heap is empty.");

        HeapEntry min = _items[0];
        int lastIndex = _items.Count - 1;

        Swap(0, lastIndex);
        _indexMap.Remove(min.Word);
        _items.RemoveAt(lastIndex);

        if (_items.Count > 0)
            BubbleDown(0);

        return min;
    }

    public void ReplaceMin(HeapEntry newItem)
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Heap is empty.");

        string evictedWord = _items[0].Word;
        _indexMap.Remove(evictedWord);

        _items[0] = newItem;
        _indexMap[newItem.Word] = 0;
        BubbleDown(0);
    }

    public bool ContainsKey(string key)
    {
        return _indexMap.ContainsKey(key);
    }

    public void UpdateKey(string key, HeapEntry newItem)
    {
        if (!_indexMap.TryGetValue(key, out int index))
            throw new InvalidOperationException($"Key '{key}' not found in heap.");

        _items[index] = newItem;
        // Frequency can only increase, so in a min-heap the value gets
        // larger and can only move downward.
        BubbleDown(index);
    }

    public void RemoveKey(string key)
    {
        if (!_indexMap.TryGetValue(key, out int index))
            throw new InvalidOperationException($"Key '{key}' not found in heap.");

        int lastIndex = _items.Count - 1;

        if (index == lastIndex)
        {
            _indexMap.Remove(key);
            _items.RemoveAt(lastIndex);
            return;
        }

        Swap(index, lastIndex);
        _indexMap.Remove(key);
        _items.RemoveAt(lastIndex);

        if (index < _items.Count)
        {
            BubbleDown(index);
            BubbleUp(index);
        }
    }

    /// <summary>
    /// Returns a copy of all items currently in the heap (unordered).
    /// </summary>
    public HeapEntry[] UnorderedItems()
    {
        HeapEntry[] result = new HeapEntry[_items.Count];
        for (int i = 0; i < _items.Count; i++)
            result[i] = _items[i];
        return result;
    }

    // ---- private helpers ----

    private void BubbleUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;
            if (Compare(_items[index], _items[parent]) < 0)
            {
                Swap(index, parent);
                index = parent;
            }
            else
            {
                break;
            }
        }
    }

    private void BubbleDown(int index)
    {
        int count = _items.Count;
        while (true)
        {
            int smallest = index;
            int left = 2 * index + 1;
            int right = 2 * index + 2;

            if (left < count && Compare(_items[left], _items[smallest]) < 0)
                smallest = left;
            if (right < count && Compare(_items[right], _items[smallest]) < 0)
                smallest = right;

            if (smallest == index)
                break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        HeapEntry temp = _items[i];
        _items[i] = _items[j];
        _items[j] = temp;

        _indexMap[_items[i].Word] = i;
        _indexMap[_items[j].Word] = j;
    }

    /// <summary>
    /// Compare by frequency ascending. Ties broken alphabetically.
    /// </summary>
    private static int Compare(HeapEntry a, HeapEntry b)
    {
        int cmp = a.Frequency.CompareTo(b.Frequency);
        if (cmp != 0) return cmp;
        // Alphabetical descending so that "z" is treated as "smaller" in the min-heap,
        // meaning alphabetically-earlier words bubble up as the min to be evicted last.
        return string.CompareOrdinal(b.Word, a.Word);
    }
}
