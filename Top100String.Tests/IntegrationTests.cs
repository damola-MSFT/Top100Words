using System.Diagnostics;
using System.Text;

namespace Top100String.Tests;

/// <summary>
/// Integration / end-to-end tests that exercise the full pipeline:
/// stream ? parse ? track ? GetTop100.
/// </summary>
public class IntegrationTests
{
    [Fact]
    public void LargeStream_100KWords_CompletesAndReturnsTop100()
    {
        // Build a stream with known frequencies:
        //   top word #0 appears 1000 times, #1 appears 999 times, ..., #99 appears 901 times.
        //   Then 500 filler words appear once each — none should make the top 100.
        var sb = new StringBuilder();

        string topZero = ToLetterWord("top", 0);
        for (int i = 0; i < 100; i++)
        {
            string word = ToLetterWord("top", i);
            int freq = 1000 - i;
            for (int j = 0; j < freq; j++)
                sb.Append(word).Append(' ');
        }

        for (int i = 0; i < 500; i++)
            sb.Append(ToLetterWord("filler", i)).Append(' ');

        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader(sb.ToString()));

        var top = tracker.GetTop100();
        Assert.Equal(100, top.Length);

        // Verify ordering: descending frequency
        for (int i = 1; i < top.Length; i++)
            Assert.True(top[i - 1].Count >= top[i].Count,
                $"Order violated at index {i}: {top[i - 1].Count} < {top[i].Count}");

        // The highest must be topZero with 1000 occurrences
        Assert.Equal(topZero, top[0].Word);
        Assert.Equal(1000, top[0].Count);

        // No filler word should appear
        for (int i = 0; i < top.Length; i++)
            Assert.StartsWith("top", top[i].Word);
    }

    [Fact]
    public void LargeStream_Performance_Under5Seconds()
    {
        // 1 million words from a vocabulary of 5000 unique words.
        // This tests that the O(log K) heap updates keep things fast.
        var rng = new Random(42);
        var vocab = new string[5000];
        for (int i = 0; i < vocab.Length; i++)
            vocab[i] = ToLetterWord("v", i);

        var sb = new StringBuilder(10_000_000);
        for (int i = 0; i < 1_000_000; i++)
        {
            sb.Append(vocab[rng.Next(vocab.Length)]);
            sb.Append(' ');
        }

        var tracker = new WordFrequencyTracker();
        var sw = Stopwatch.StartNew();
        tracker.ProcessStream(new StringReader(sb.ToString()));
        var top = tracker.GetTop100();
        sw.Stop();

        Assert.Equal(100, top.Length);
        Assert.True(sw.Elapsed.TotalSeconds < 5,
            $"Processing 1M words took {sw.Elapsed.TotalSeconds:F2}s (limit: 5s)");
    }

    [Fact]
    public void StreamFromMultipleLines_MatchesSingleLine()
    {
        string singleLine = "the cat sat on the mat the cat";
        string multiLine = "the cat\nsat on\nthe mat\nthe cat";

        var tracker1 = new WordFrequencyTracker();
        tracker1.ProcessStream(new StringReader(singleLine));

        var tracker2 = new WordFrequencyTracker();
        tracker2.ProcessStream(new StringReader(multiLine));

        var top1 = tracker1.GetTop100();
        var top2 = tracker2.GetTop100();

        Assert.Equal(top1.Length, top2.Length);
        for (int i = 0; i < top1.Length; i++)
        {
            Assert.Equal(top1[i].Word, top2[i].Word);
            Assert.Equal(top1[i].Count, top2[i].Count);
        }
    }

    [Fact]
    public void FrequencyUpdates_WordRisesIntoTop100()
    {
        // Start with 100 words at frequency 10 each.
        // Then a new word "newcomer" gets fed 11 times — it should displace one.
        var tracker = new WordFrequencyTracker();

        var sb = new StringBuilder();
        for (int i = 0; i < 100; i++)
        {
            string word = ToLetterWord("ex", i);
            for (int j = 0; j < 10; j++)
                sb.Append(word).Append(' ');
        }

        // newcomer starts at 0
        for (int j = 0; j < 11; j++)
            sb.Append("newcomer ");

        tracker.ProcessStream(new StringReader(sb.ToString()));

        var top = tracker.GetTop100();
        Assert.Equal(100, top.Length);

        bool found = false;
        for (int i = 0; i < top.Length; i++)
        {
            if (top[i].Word == "newcomer")
            {
                found = true;
                Assert.Equal(11, top[i].Count);
            }
        }
        Assert.True(found, "newcomer should be in the top 100");
    }

    [Fact]
    public void AllSameWord_SingleEntryReturned()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader("go go go go go"));

        var top = tracker.GetTop100();
        Assert.Single(top);
        Assert.Equal("go", top[0].Word);
        Assert.Equal(5, top[0].Count);
    }

    [Fact]
    public void OnlyPunctuation_ReturnsEmpty()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader("!@#$%^&*()_+=-[]{}|;':\",./<>?"));

        var top = tracker.GetTop100();
        Assert.Empty(top);
    }

    [Fact]
    public void OnlyDigits_ReturnsEmpty()
    {
        var tracker = new WordFrequencyTracker();
        tracker.ProcessStream(new StringReader("123 456 789 0"));

        var top = tracker.GetTop100();
        Assert.Empty(top);
    }

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
