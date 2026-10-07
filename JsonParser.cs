using System;
using System.Collections.Generic;
using System.Globalization;

public class JsonParser
{
    private readonly List<Token> _tokens;
    private int _index = 0;

    public JsonParser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    private Token Peek() => _tokens[_index];

    private Token Consume()
    {
        var token = _tokens[_index];
        if (token.Type != TokenType.EOF) // never walk past EOF
            _index++;
        return token;
    }

    private bool IsPunct(string value)
    {
        var t = Peek();
        return t.Type == TokenType.PUNCT && t.Value == value;
    }

    private static JsonParseException Error(Token t, string message)
        => new JsonParseException(message, t.Line, t.Column);

    // Entry point: one value, then nothing else.
    public object Parse()
    {
        var value = ParseValue();

        var next = Peek();
        if (next.Type != TokenType.EOF)
            throw Error(next, "trailing data");

        return value;
    }

    private List<object> ParseArray()
    {
        var array = new List<object>();

        if (IsPunct("]"))
        {
            Consume();
            return array;
        }

        array.Add(ParseValue());

        while (IsPunct(","))
        {
            Consume();

            if (IsPunct("]"))
                throw Error(Peek(), "trailing comma");

            array.Add(ParseValue());
        }

        if (IsPunct("]"))
        {
            Consume();
            return array;
        }

        throw Error(Peek(), "expected ',' or ']'");
    }

    public Dictionary<string, object> ParseObject()
    {
        var dic = new Dictionary<string, object>();

        if (IsPunct("}"))
        {
            Consume();
            return dic;
        }

        while (true)
        {
            var key = Peek();

            if (key.Type == TokenType.PUNCT && key.Value == ",")
                throw Error(key, "unexpected character ','");

            if (key.Type != TokenType.STRING)
                throw Error(key, "object key must be a string");

            Consume();

            if (!IsPunct(":"))
                throw Error(Peek(), "object key must be followed by a colon");

            Consume();

            dic[key.Value] = ParseValue(); // [] instead of Add: duplicate keys don't crash

            if (IsPunct(","))
            {
                Consume();

                if (IsPunct("}"))
                    throw Error(Peek(), "trailing comma");

                continue;
            }

            break;
        }

        if (IsPunct("}"))
        {
            Consume();
            return dic;
        }

        throw Error(Peek(), "expected ',' or '}'");
    }

    public object ParseValue()
    {
        var t = Peek();

        switch (t.Type)
        {
            case TokenType.STRING:
                return Consume().Value;

            case TokenType.NUMBER:
                return ParseNumber(Consume());

            case TokenType.TRUE:
                Consume();
                return true;

            case TokenType.FALSE:
                Consume();
                return false;

            case TokenType.NULL:
                Consume();
                return null;

            case TokenType.PUNCT when t.Value == "{":
                Consume();
                return ParseObject();

            case TokenType.PUNCT when t.Value == "[":
                Consume();
                return ParseArray();

            case TokenType.EOF:
                throw Error(t, "unexpected end of input");

            default: // other PUNCT (',' ':' '}' ']') or INVALID
                throw Error(t, $"unexpected character '{t.Value}'");
        }
    }

    private static object ParseNumber(Token token)
    {
        string text = token.Value;

        bool isFloat = text.Contains('.') || text.Contains('e') || text.Contains('E');

        if (!isFloat)
        {
            if (int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int i))
                return i;
            if (long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long l))
                return l;
        }

        return double.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}