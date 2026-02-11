namespace Top100String.Tests;

public class WordFrequencyTrackerTests
{
    // ---- §9 test cases ----

    [Fact]
    public void EmptyStream_ReturnsEmptyArray()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader(""));

        var top = tracker.GetTop100();
        Assert.Empty(top);
    }

    [Fact]
    public void SingleWord_ReturnsSingleEntry()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader("hello"));

        var top = tracker.GetTop100();
        Assert.Single(top);
        Assert.Equal("hello", top[0].Word);
        Assert.Equal(1, top[0].Count);
    }

    [Fact]
    public void CaseInsensitivity()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader("Hello HELLO hello"));

        var top = tracker.GetTop100();
        Assert.Single(top);
        Assert.Equal("hello", top[0].Word);
        Assert.Equal(3, top[0].Count);
    }

    [Fact]
    public void PunctuationStripping()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader("it's a well-known fact"));

        var top = tracker.GetTop100();
        var words = new HashSet<string>();
        for (int i = 0; i < top.Length; i++)
            words.Add(top[i].Word);

        Assert.Equal(6, top.Length);
        Assert.Contains("it", words);
        Assert.Contains("s", words);
        Assert.Contains("a", words);
        Assert.Contains("well", words);
        Assert.Contains("known", words);
        Assert.Contains("fact", words);
    }

    [Fact]
    public void DescendingFrequencyOrder()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader(
            "the the the fox fox dog"));

        var top = tracker.GetTop100();
        Assert.Equal(3, top.Length);
        Assert.Equal("the", top[0].Word);
        Assert.Equal(3, top[0].Count);
        Assert.Equal("fox", top[1].Word);
        Assert.Equal(2, top[1].Count);
        Assert.Equal("dog", top[2].Word);
        Assert.Equal(1, top[2].Count);
    }

    [Fact]
    public void TieBreaking_AlphabeticalAscending()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader("banana apple cherry"));

        var top = tracker.GetTop100();
        Assert.Equal(3, top.Length);
        // All frequency 1 — should be alphabetical
        Assert.Equal("apple", top[0].Word);
        Assert.Equal("banana", top[1].Word);
        Assert.Equal("cherry", top[2].Word);
    }

    [Fact]
    public void FewerThan100UniqueWords()
    {
        var tracker = new WordFrequencyTracker();

        // Generate 50 unique words with varying frequencies
        var sb = new System.Text.StringBuilder();
        for (int i = 1; i <= 50; i++)
        {
            string word = ToLetterWord("word", i);
            for (int j = 0; j < i; j++)
                sb.Append(word).Append(' ');
        }

        tracker.ProcessStream(new StringReader(sb.ToString()));

        var top = tracker.GetTop100();
        Assert.Equal(50, top.Length);
        // Highest frequency first
        Assert.Equal(50, top[0].Count);
        Assert.Equal(1, top[49].Count);
    }

    [Fact]
    public void Exactly100UniqueWords()
    {
        var tracker = new WordFrequencyTracker();

        var sb = new System.Text.StringBuilder();
        for (int i = 1; i <= 100; i++)
        {
            string word = ToLetterWord("w", i);
            for (int j = 0; j < i; j++)
                sb.Append(word).Append(' ');
        }

        tracker.ProcessStream(new StringReader(sb.ToString()));

        var top = tracker.GetTop100();
        Assert.Equal(100, top.Length);
        Assert.Equal(100, top[0].Count);
        Assert.Equal(1, top[99].Count);
    }

    [Fact]
    public void MoreThan100UniqueWords_OnlyTop100Returned()
    {
        var tracker = new WordFrequencyTracker();

        // 200 unique words. Words 101-200 get frequency = index (101..200).
        // Words 1-100 get frequency = 1 each.
        // So the top 100 should be words 101-200.
        var sb = new System.Text.StringBuilder();

        // Low-frequency words (1 occurrence each)
        for (int i = 1; i <= 100; i++)
            sb.Append(ToLetterWord("low", i)).Append(' ');

        // High-frequency words (101..200 occurrences)
        for (int i = 101; i <= 200; i++)
        {
            string word = ToLetterWord("high", i);
            for (int j = 0; j < i; j++)
                sb.Append(word).Append(' ');
        }

        tracker.ProcessStream(new StringReader(sb.ToString()));

        var top = tracker.GetTop100();
        Assert.Equal(100, top.Length);

        // The top entry should have 200 occurrences
        Assert.Equal(200, top[0].Count);

        // No low-frequency word should appear — all top words start with "high"
        for (int i = 0; i < top.Length; i++)
            Assert.StartsWith("high", top[i].Word);
    }

    [Fact]
    public void MultipleLines_ProcessedCorrectly()
    {
        var input = "line one\nline two\nline three\nline one";
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader(input));

        var top = tracker.GetTop100();

        int lineCount = 0;
        int oneCount = 0;
        for (int i = 0; i < top.Length; i++)
        {
            if (top[i].Word == "line") lineCount = top[i].Count;
            if (top[i].Word == "one") oneCount = top[i].Count;
        }

        Assert.Equal(4, lineCount); // "line" appears on every line
        Assert.Equal(2, oneCount);  // "one" appears twice
    }

    [Fact]
    public void ProcessWord_EmptyString_Ignored()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessWord("");

        var top = tracker.GetTop100();
        Assert.Empty(top);
    }

    [Fact]
    public void ProcessLine_CanBeCalledIncrementally()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessLine("hello world");
        tracker.ProcessLine("hello again");

        var top = tracker.GetTop100();
        Assert.Equal("hello", top[0].Word);
        Assert.Equal(2, top[0].Count);
    }

    [Fact]
    public void DigitsStrippedFromInput()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader("abc123def 456 ghi"));

        var top = tracker.GetTop100();
        var words = new HashSet<string>();
        for (int i = 0; i < top.Length; i++)
            words.Add(top[i].Word);

        Assert.Equal(3, top.Length);
        Assert.Contains("abc", words);
        Assert.Contains("def", words);
        Assert.Contains("ghi", words);
    }

    /// <summary>
    /// Generates a unique all-letter word: prefix + base-26 encoding of n.
    /// e.g. ToLetterWord("pfx", 0) ? "pfxa", ToLetterWord("pfx", 27) ? "pfxbb".
    /// </summary>
    private static string ToLetterWord(string prefix, int n)
    {
        if (n < 0) n = -n;
        var chars = new List<char>();
        do
        {
            chars.Add((char)('a' + (n % 26)));
            n /= 26;
        } while (n > 0);
        chars.Reverse();
        return prefix + new string(chars.ToArray());
    }
}
