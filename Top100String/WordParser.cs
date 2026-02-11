using System.Collections;

namespace Top100String;

/// <summary>
/// Character-by-character word extraction. Only ASCII letters form words;
/// everything else is a delimiter. Words are yielded in lowercase.
/// </summary>
public static class WordParser
{
    public static WordEnumerable ParseWords(string line) => new(line);

    /// <summary>
    /// Struct-based enumerable to avoid allocating an IEnumerator object per line.
    /// </summary>
    public readonly struct WordEnumerable : IEnumerable<string>
    {
        private readonly string _line;
        public WordEnumerable(string line) => _line = line;

        public WordEnumerator GetEnumerator() => new(_line);
        IEnumerator<string> IEnumerable<string>.GetEnumerator() => new WordEnumerator(_line);
        IEnumerator IEnumerable.GetEnumerator() => new WordEnumerator(_line);
    }

    public struct WordEnumerator : IEnumerator<string>
    {
        private readonly string _line;
        private int _pos;
        private string? _current;

        public WordEnumerator(string line)
        {
            _line = line;
            _pos = 0;
            _current = null;
        }

        public string Current => _current!;
        object IEnumerator.Current => _current!;

        public bool MoveNext()
        {
            int len = _line.Length;

            // Skip non-letter characters
            while (_pos < len && !IsAsciiLetter(_line[_pos]))
                _pos++;

            if (_pos >= len)
                return false;

            int start = _pos;

            // Consume letter characters
            while (_pos < len && IsAsciiLetter(_line[_pos]))
                _pos++;

            _current = ToLower(_line, start, _pos - start);
            return true;
        }

        public void Reset() { _pos = 0; _current = null; }
        public void Dispose() { }

        private static bool IsAsciiLetter(char c)
            => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');

        private static string ToLower(string source, int start, int length)
        {
            char[] buf = new char[length];
            for (int i = 0; i < length; i++)
            {
                char c = source[start + i];
                buf[i] = (c >= 'A' && c <= 'Z') ? (char)(c + 32) : c;
            }
            return new string(buf);
        }
    }
}
