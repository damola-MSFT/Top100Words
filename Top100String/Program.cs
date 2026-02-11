using Top100String;

var tracker = new WordFrequencyTracker();

TextReader reader;
if (args.Length > 0 && File.Exists(args[0]))
    reader = new StreamReader(args[0]);
else
    reader = Console.In;

try
{
    tracker.ProcessStream(reader);
}
finally
{
    if (reader != Console.In)
        reader.Dispose();
}

var top = tracker.GetTop100();
int padWidth = top.Length.ToString().Length;

for (int i = 0; i < top.Length; i++)
{
    string rank = (i + 1).ToString().PadLeft(padWidth);
    Console.WriteLine($"{rank}. {top[i].Word} — {top[i].Count}");
}
