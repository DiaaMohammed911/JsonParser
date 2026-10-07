public class PositionTracker
{
    private int _line = 1;
    private int _column = 1;

    public int Line => _line;
    public int Column => _column;

    public void Consume(char c)
    {
        if (c == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
    }
    public (int line, int column) CurrentPosition()
    {
        return (_line, _column);
    }
}