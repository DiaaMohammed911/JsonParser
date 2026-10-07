using System;
using System.Collections.Generic;
using System.Text;

public enum TokenType
{
    PUNCT,
    INVALID,
    STRING,
    NUMBER,
    TRUE,
    FALSE,
    NULL,
    EOF
}

public class Token
{
    public TokenType Type { get; set; }
    public string Value { get; set; }

    public int Line { get; }
    public int Column { get; }

    public Token(TokenType type, string value, int line, int column)
    {
        Type = type;
        Value = value;
        Line = line;
        Column = column;
    }
}
public class JsonTokenizer
{
    private readonly PositionTracker _pos = new PositionTracker();
    private readonly string _src;
    private int _i = 0;

    public JsonTokenizer(string src)
    {
        _src = src;
    }
    private char Advance()
    {
        char c = _src[_i++];
        _pos.Consume(c);
        return c;
    }
    private static bool IsAsciiDigit(char c) => c >= '0' && c <= '9';
    private void Advance(int count)
    {
        for (int k = 0; k < count; k++)
            Advance();
    }
    private bool TryReadKeyword(string word, TokenType type, int line, int col, List<Token> tokens)
    {
        if (string.CompareOrdinal(_src, _i, word, 0, word.Length) != 0)
            return false;

        Advance(word.Length);
        tokens.Add(new Token(type, word, line, col));
        return true;
    }
    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_i < _src.Length)
        {
            char c = _src[_i];

            // position BEFORE consuming the token
            int line = _pos.Line;
            int col = _pos.Column;

            if (c is ' ' or '\t' or '\n' or '\r')
            {
                Advance();
                continue;
            }

            if ("{}[],:".Contains(c))
            {
                Advance();
                tokens.Add(new Token(TokenType.PUNCT, c.ToString(), line, col));
                continue;
            }

            if (c == '"')
            {
                tokens.Add(ReadString(line, col));
                continue;
            }

            if (c == '-' || IsAsciiDigit(c))
            {
                tokens.Add(ReadNumber(line, col));
                continue;
            }

            if (TryReadKeyword("true", TokenType.TRUE, line, col, tokens)) continue;
            if (TryReadKeyword("false", TokenType.FALSE, line, col, tokens)) continue;
            if (TryReadKeyword("null", TokenType.NULL, line, col, tokens)) continue;

            // Unknown character: don't throw here. "123abc" must be reported by the
            // parser as "trailing data" at 'a', not as an "unexpected character".
            Advance();
            tokens.Add(new Token(TokenType.INVALID, c.ToString(), line, col));
        }

        tokens.Add(new Token(TokenType.EOF, "", _pos.Line, _pos.Column));
        return tokens;
    }

    private Token ReadNumber(int line, int col)
    {
        int start = _i;

        if (_src[_i] == '-')
            Advance();

        while (_i < _src.Length && JsonNumberValidator.IsDigit(_src[_i]))
            Advance();

        if (_i < _src.Length && _src[_i] == '.')
        {
            Advance();

            while (_i < _src.Length && JsonNumberValidator.IsDigit(_src[_i]))
                Advance();
        }

        if (_i < _src.Length && (_src[_i] == 'e' || _src[_i] == 'E'))
        {
            Advance();

            if (_i < _src.Length && (_src[_i] == '+' || _src[_i] == '-'))
                Advance();

            while (_i < _src.Length && JsonNumberValidator.IsDigit(_src[_i]))
                Advance();
        }

        string value = _src[start.._i];
        if (JsonNumberValidator.IsValid(value))
            return new Token(TokenType.NUMBER, value, line, col);

        throw new JsonParseException("invalid number", line, col);
    }

    private Token ReadString(int startLine, int startCol)
    {
        var sb = new StringBuilder();
        Advance(); // opening quote

        while (_i < _src.Length)
        {
            char c = _src[_i];

            if (c == '"')
            {
                Advance();
                return new Token(TokenType.STRING, sb.ToString(), startLine, startCol);
            }

            if (c == '\\')
            {
                ReadEscape(sb);
                continue;
            }

            if (c < 0x20)
                throw new JsonParseException("control character in string", _pos.Line, _pos.Column);

            sb.Append(Advance());
        }

        throw new JsonParseException("unterminated string", startLine, startCol);
    }
    private void ReadEscape(StringBuilder sb)
    {
        int line = _pos.Line;
        int col = _pos.Column;

        Advance(); // the backslash

        if (_i >= _src.Length)
            throw new JsonParseException("unterminated string", line, col);

        char escaped = Advance();

        switch (escaped)
        {
            case '"': sb.Append('"'); break;
            case '\\': sb.Append('\\'); break;
            case '/': sb.Append('/'); break;
            case 'n': sb.Append('\n'); break;
            case 'r': sb.Append('\r'); break;
            case 't': sb.Append('\t'); break;
            case 'b': sb.Append('\b'); break;
            case 'f': sb.Append('\f'); break;
            case 'u': ReadUnicodeEscape(sb, line, col); break;
            default:
                throw new JsonParseException($"invalid escape '\\{escaped}'", line, col);
        }
    }
    // Called right after "\u" has been consumed.
    private void ReadUnicodeEscape(StringBuilder sb, int line, int col)
    {
        int first = ReadHex4(line, col);

        if (first >= 0xD800 && first <= 0xDBFF)
        {
            // A high surrogate must be followed by \uDC00-\uDFFF.
            if (_i + 1 < _src.Length && _src[_i] == '\\' && _src[_i + 1] == 'u')
            {
                int pairLine = _pos.Line;
                int pairCol = _pos.Column;

                Advance(2); // "\u"
                int second = ReadHex4(pairLine, pairCol);

                if (second < 0xDC00 || second > 0xDFFF)
                    throw new JsonParseException("invalid low surrogate", pairLine, pairCol);

                sb.Append((char)first);
                sb.Append((char)second);
                return;
            }

            throw new JsonParseException("unpaired high surrogate", line, col);
        }

        if (first >= 0xDC00 && first <= 0xDFFF)
            throw new JsonParseException("unpaired low surrogate", line, col);

        sb.Append((char)first);
    }
    private int ReadHex4(int line, int col)
    {
        if (_i + 4 > _src.Length)
            throw new JsonParseException("unterminated string", line, col);

        int value = 0;
        for (int k = 0; k < 4; k++)
        {
            char h = _src[_i];
            if (!Uri.IsHexDigit(h))
                throw new JsonParseException("invalid unicode escape", line, col);

            value = (value << 4) | Uri.FromHex(h);
            Advance();
        }
        return value;
    }
}