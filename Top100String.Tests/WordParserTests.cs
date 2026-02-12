namespace Top100String.Tests;

public class WordParserTests
{
    [Fact]
    public void ParseWords_EmptyString_ReturnsNothing()
    {
        var words = Collect(WordParser.ParseWords(""));
        Assert.Empty(words);
    }

    [Fact]
    public void ParseWords_WhitespaceOnly_ReturnsNothing()
    {
        var words = Collect(WordParser.ParseWords("   \t  "));
        Assert.Empty(words);
    }

    [Fact]
    public void ParseWords_SingleWord_ReturnsLowercase()
    {
        var words = Collect(WordParser.ParseWords("Hello"));
        Assert.Single(words);
        Assert.Equal("hello", words[0]);
    }

    [Fact]
    public void ParseWords_MultipleWords_SplitBySpaces()
    {
        var words = Collect(WordParser.ParseWords("The quick brown fox"));
        Assert.Equal(new[] { "the", "quick", "brown", "fox" }, words);
    }

    [Fact]
    public void ParseWords_PunctuationStripping()
    {
        // �9: "it's a well-known fact" ? it, s, a, well, known, fact
        var words = Collect(WordParser.ParseWords("it's a well-known fact"));
        Assert.Equal(new[] { "it", "s", "a", "well", "known", "fact" }, words);
    }

    [Fact]
    public void ParseWords_DigitsArePartOfWords()
    {
        var words = Collect(WordParser.ParseWords("abc123def"));
        Assert.Equal(new[] { "abc123def" }, words);
    }

    [Fact]
    public void ParseWords_MixedPunctuationAndDigits()
    {
        var words = Collect(WordParser.ParseWords("hello...world!! 42 test"));
        Assert.Equal(new[] { "hello", "world", "42", "test" }, words);
    }

    [Fact]
    public void ParseWords_AllUpperCase_ReturnsLowercase()
    {
        var words = Collect(WordParser.ParseWords("ABC DEF"));
        Assert.Equal(new[] { "abc", "def" }, words);
    }

    [Fact]
    public void ParseWords_LeadingAndTrailingDelimiters()
    {
        var words = Collect(WordParser.ParseWords("...hello..."));
        Assert.Single(words);
        Assert.Equal("hello", words[0]);
    }

    [Fact]
    public void ParseWords_TabsNewlineChars()
    {
        var words = Collect(WordParser.ParseWords("word1\tword2"));
        Assert.Equal(new[] { "word1", "word2" }, words);
    }

    private static string[] Collect(WordParser.WordEnumerable enumerable)
    {
        var list = new List<string>();
        foreach (string w in enumerable)
            list.Add(w);
        return list.ToArray();
    }
}
