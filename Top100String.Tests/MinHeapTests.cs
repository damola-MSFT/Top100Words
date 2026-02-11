namespace Top100String.Tests;

public class MinHeapTests
{
    [Fact]
    public void Peek_EmptyHeap_Throws()
    {
        var heap = new MinHeap();
        Assert.Throws<InvalidOperationException>(() => heap.Peek());
    }

    [Fact]
    public void ExtractMin_EmptyHeap_Throws()
    {
        var heap = new MinHeap();
        Assert.Throws<InvalidOperationException>(() => heap.ExtractMin());
    }

    [Fact]
    public void Insert_SingleItem_PeekReturnsIt()
    {
        var heap = new MinHeap();
        heap.Insert(new HeapEntry("hello", 5));

        Assert.Equal(1, heap.Count);
        Assert.Equal("hello", heap.Peek().Word);
        Assert.Equal(5, heap.Peek().Frequency);
    }

    [Fact]
    public void Insert_MultipleItems_PeekReturnsMinimum()
    {
        var heap = new MinHeap();
        heap.Insert(new HeapEntry("c", 30));
        heap.Insert(new HeapEntry("a", 10));
        heap.Insert(new HeapEntry("b", 20));

        Assert.Equal(3, heap.Count);
        Assert.Equal(10, heap.Peek().Frequency);
        Assert.Equal("a", heap.Peek().Word);
    }

    [Fact]
    public void ExtractMin_ReturnsItemsInAscendingFrequency()
    {
        var heap = new MinHeap();
        heap.Insert(new HeapEntry("c", 30));
        heap.Insert(new HeapEntry("a", 10));
        heap.Insert(new HeapEntry("d", 40));
        heap.Insert(new HeapEntry("b", 20));

        Assert.Equal(new HeapEntry("a", 10), heap.ExtractMin());
        Assert.Equal(new HeapEntry("b", 20), heap.ExtractMin());
        Assert.Equal(new HeapEntry("c", 30), heap.ExtractMin());
        Assert.Equal(new HeapEntry("d", 40), heap.ExtractMin());
        Assert.Equal(0, heap.Count);
    }

    [Fact]
    public void ReplaceMin_EvictsMinAndInserts()
    {
        var heap = new MinHeap();
        heap.Insert(new HeapEntry("a", 1));
        heap.Insert(new HeapEntry("b", 5));
        heap.Insert(new HeapEntry("c", 10));

        heap.ReplaceMin(new HeapEntry("d", 3));

        // "a" evicted; new min should be "d"(3)
        Assert.False(heap.ContainsKey("a"));
        Assert.True(heap.ContainsKey("d"));
        Assert.Equal(3, heap.Peek().Frequency);
    }

    [Fact]
    public void ContainsKey_ReturnsTrueForInserted()
    {
        var heap = new MinHeap();
        heap.Insert(new HeapEntry("abc", 1));

        Assert.True(heap.ContainsKey("abc"));
        Assert.False(heap.ContainsKey("xyz"));
    }

    [Fact]
    public void UpdateKey_IncreasesFrequency_MaintainsHeapOrder()
    {
        var heap = new MinHeap();
        heap.Insert(new HeapEntry("a", 1));
        heap.Insert(new HeapEntry("b", 2));
        heap.Insert(new HeapEntry("c", 3));

        // Increase "a" from 1 to 10; it should no longer be the min
        heap.UpdateKey("a", new HeapEntry("a", 10));

        Assert.Equal("b", heap.Peek().Word);
        Assert.Equal(2, heap.Peek().Frequency);
    }

    [Fact]
    public void UpdateKey_NonExistentKey_Throws()
    {
        var heap = new MinHeap();
        Assert.Throws<InvalidOperationException>(
            () => heap.UpdateKey("nope", new HeapEntry("nope", 1)));
    }

    [Fact]
    public void RemoveKey_RemovesAndMaintainsOrder()
    {
        var heap = new MinHeap();
        heap.Insert(new HeapEntry("a", 1));
        heap.Insert(new HeapEntry("b", 2));
        heap.Insert(new HeapEntry("c", 3));
        heap.Insert(new HeapEntry("d", 4));

        heap.RemoveKey("b");

        Assert.Equal(3, heap.Count);
        Assert.False(heap.ContainsKey("b"));

        // Extracting all should give ascending order of remaining items
        Assert.Equal(new HeapEntry("a", 1), heap.ExtractMin());
        Assert.Equal(new HeapEntry("c", 3), heap.ExtractMin());
        Assert.Equal(new HeapEntry("d", 4), heap.ExtractMin());
    }

    [Fact]
    public void RemoveKey_LastElement()
    {
        var heap = new MinHeap();
        heap.Insert(new HeapEntry("only", 1));

        heap.RemoveKey("only");

        Assert.Equal(0, heap.Count);
        Assert.False(heap.ContainsKey("only"));
    }

    [Fact]
    public void UnorderedItems_ReturnsAllEntries()
    {
        var heap = new MinHeap();
        heap.Insert(new HeapEntry("a", 1));
        heap.Insert(new HeapEntry("b", 2));
        heap.Insert(new HeapEntry("c", 3));

        HeapEntry[] items = heap.UnorderedItems();
        Assert.Equal(3, items.Length);

        var words = new HashSet<string>();
        for (int i = 0; i < items.Length; i++)
            words.Add(items[i].Word);

        Assert.Contains("a", words);
        Assert.Contains("b", words);
        Assert.Contains("c", words);
    }

    [Fact]
    public void IndexMap_StaysConsistent_AfterManyOperations()
    {
        var heap = new MinHeap();

        // Insert 20 items
        for (int i = 20; i >= 1; i--)
            heap.Insert(new HeapEntry($"w{i}", i));

        // Update some keys
        heap.UpdateKey("w1", new HeapEntry("w1", 100));
        heap.UpdateKey("w5", new HeapEntry("w5", 99));

        // Remove some keys
        heap.RemoveKey("w10");
        heap.RemoveKey("w15");

        Assert.Equal(18, heap.Count);
        Assert.False(heap.ContainsKey("w10"));
        Assert.False(heap.ContainsKey("w15"));

        // Extract all and verify ascending order
        int prev = -1;
        while (heap.Count > 0)
        {
            HeapEntry entry = heap.ExtractMin();
            Assert.True(entry.Frequency >= prev,
                $"Heap order violated: {entry.Frequency} < {prev}");
            prev = entry.Frequency;
        }
    }
}
