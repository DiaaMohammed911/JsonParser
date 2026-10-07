public class JsonParseException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public JsonParseException(string message, int line, int column) : base(message)
    {
        Line = line;
        Column = column;
    }
}