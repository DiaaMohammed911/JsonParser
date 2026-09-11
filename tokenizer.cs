using System;
using System.Collections.Generic;
using System.Text;

public enum TokenType
{
    PUNCT,
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

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }
}
public class JsonTokenizer
{
    private readonly string _src;
    private int _i = 0;

    public JsonTokenizer(string src)
    {
        _src = src;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_i < _src.Length)
        {
            char c = _src[_i];
            if (c is ' ' or '\t' or '\n' or '\r')
            {
                _i++;
                continue;
            }

            if ("{}[],:".Contains(c))
            {
                tokens.Add(new Token(TokenType.PUNCT, c.ToString()));
                _i++;
                continue;
            }

            if (c == '"')
            {
                tokens.Add(ReadString());
                continue;
            }

            if (c == '-' || char.IsDigit(c))
            {
                tokens.Add(ReadNumber());
                continue;
            }

            if (_src.AsSpan(_i).StartsWith("true"))
            {
                tokens.Add(new Token(TokenType.TRUE, "true"));
                _i += 4;
                continue;
            }

            if (_src.AsSpan(_i).StartsWith("false"))
            {
                tokens.Add(new Token(TokenType.FALSE, "false"));
                _i += 5;
                continue;
            }

            if (_src.AsSpan(_i).StartsWith("null"))
            {
                tokens.Add(new Token(TokenType.NULL, "null"));
                _i += 4;
                continue;
            }

            throw new Exception(
                $"unexpected character '{c}' at position {_i}"
            );
        }

        tokens.Add(new Token(TokenType.EOF, ""));
        return tokens;
    }

    private Token ReadNumber()
    {
        int start = _i;

        if (_src[_i] == '-')
            _i++;

        while (_i < _src.Length && char.IsDigit(_src[_i]))
            _i++;

        if (_i < _src.Length && _src[_i] == '.')
        {
            _i++;

            while (_i < _src.Length && char.IsDigit(_src[_i]))
                _i++;
        }

        if (_i < _src.Length &&
            (_src[_i] == 'e' || _src[_i] == 'E'))
        {
            _i++;

            if (_i < _src.Length &&
                (_src[_i] == '+' || _src[_i] == '-'))
            {
                _i++;
            }

            while (_i < _src.Length && char.IsDigit(_src[_i]))
                _i++;
        }

        string value = _src[start.._i];

        return new Token(TokenType.NUMBER, value);
    }

    private Token ReadString()
    {

        var sb = new StringBuilder();
        _i++;

        while (_i < _src.Length)
        {
            char c = _src[_i];

            if (c == '"')
            {
                _i++;
                return new Token(TokenType.STRING, sb.ToString()); 
            }

            if (c == '\\')
            {
                _i++;

                if (_i >= _src.Length)
                    throw new Exception("unterminated string");

                char escaped = _src[_i];

                switch (escaped)
                {
                    case '"':
                        sb.Append('"');
                        break;

                    case '\\':
                        sb.Append('\\');
                        break;

                    case '/':
                        sb.Append('/');
                        break;

                    case 'n':
                        sb.Append('\n');
                        break;

                    case 'r':
                        sb.Append('\r');
                        break;

                    case 't':
                        sb.Append('\t');
                        break;

                    case 'b':
                        sb.Append('\b');
                        break;

                    case 'f':
                        sb.Append('\f');
                        break;
                    case 'u':
                        if (_i + 10 < _src.Length &&
                            _src[_i + 5] == '\\' &&
                            _src[_i + 6] == 'u')
                        {
                            string pair1 = _src[(_i + 1)..(_i + 5)];
                            string pair2 = _src[(_i + 7)..(_i + 11)];

                            if (!int.TryParse(
                                    pair1,
                                    System.Globalization.NumberStyles.HexNumber,
                                    null,
                                    out int hi))
                                throw new Exception("invalid unicode escape");

                            if (!int.TryParse(
                                    pair2,
                                    System.Globalization.NumberStyles.HexNumber,
                                    null,
                                    out int lo))
                                throw new Exception("invalid unicode escape");
                            if (hi < 0xD800 || hi > 0xDBFF)
                                throw new Exception("invalid high surrogate");
                            if (lo < 0xDC00 || lo > 0xDFFF)
                                throw new Exception("invalid low surrogate");

                            int codePoint =
                                0x10000 +
                                ((hi - 0xD800) << 10) +
                                (lo - 0xDC00);

                            sb.Append(char.ConvertFromUtf32(codePoint));

                            _i += 10;
                        }
                        else
                        {
                            if (_i + 4 >= _src.Length)
                                throw new Exception("unterminated string");

                            string hex = _src[(_i + 1)..(_i + 5)];

                            if (!int.TryParse(hex,System.Globalization.NumberStyles.HexNumber,null,out int codePoint))
                                throw new Exception("invalid unicode escape");
                            if (codePoint >= 0xD800 && codePoint <= 0xDBFF)
                                throw new Exception("unpaired high surrogate");
                            if (codePoint >= 0xDC00 && codePoint <= 0xDFFF)
                                throw new Exception("unpaired low surrogate");

                            sb.Append(char.ConvertFromUtf32(codePoint));

                            _i += 4;
                        }
                        break;
                    default:
                        throw new Exception(
                            $"invalid escape '\\{escaped}'"
                        );
                }

                _i++;
                continue;
            }

            sb.Append(c);
            _i++;
        }

        throw new Exception("unterminated string");
    }
}