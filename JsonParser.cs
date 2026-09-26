using System;
using System.Collections.Generic;
using System.Text;
public class JsonParser{
    private readonly List<Token> _tokens;
    private int _index = 0;
    public JsonParser(List<Token> tokens){
        _tokens = tokens;
    }
    private Token Peek()
    {
        return _tokens[_index];
    }
    private Token Consume()
    {
        var token = _tokens[_index];
        _index++;
        return token;
    }
    private bool IsFloat()
    {
        var _value = _tokens[_index].Value;
        return _value.Contains('.')|| _value.Contains('E')||_value.Contains('e');
    }
    private bool IsPunct(string value)
    {
        return Peek().Type == TokenType.PUNCT && Peek().Value == value;
    }
    private List<object> ParseArray()
    {
        var array = new List<object>();
        
        if(IsPunct("]"))
        {            
            Consume();
            DepthTracker.leave();
            return array;
        }
        array.Add(ParseValue());
        while(IsPunct(","))
        {
            Consume();
            array.Add(ParseValue());
        }
        if (IsPunct("]"))
        {            
            Consume();
            DepthTracker.leave();
            return array;
        }
        throw new Exception("Not Expected This Value in The Array");
    }
    public Dictionary<string, object> ParseObject()
    {
        var dic = new Dictionary<string, object>();

        if (IsPunct("}"))
        {            
            Consume();
            DepthTracker.leave();
            return dic;
        }

        while (true)
        {
            var key = Peek();

            if (key.Type != TokenType.STRING)
            {
                throw new Exception("ERR object key must be a string");
            }

            Consume();

            if (!IsPunct(":"))
            {
                throw new Exception("ERR object key must be followed by a colon");
            }

            Consume();

            dic.Add(key.Value, ParseValue());

            if (IsPunct(","))
            {
                Consume();

                if (IsPunct("}"))
                {
                    throw new Exception("ERR trailing comma");
                }

                continue;
            }

            break;
        }

        if (IsPunct("}"))
        {            
            Consume();
            DepthTracker.leave();
            return dic;
        }

        throw new Exception("Not Expected This Value in The Object");
    }
    public object ParseValue(){
        if (Peek().Type == TokenType.STRING) return Consume().Value;
        else if (Peek().Type == TokenType.NUMBER)
        {
            if(IsFloat()) return float.Parse(Consume().Value);
            else return int.Parse(Consume().Value);
        }
        else if (Peek().Type == TokenType.PUNCT && Peek().Value == "{")
        {
            DepthTracker.enter();
            Consume();
            return ParseObject();
        }
        else if (Peek().Type== TokenType.PUNCT && Peek().Value == "[")
        {
            DepthTracker.enter();
            Consume();
            return ParseArray();
        }
        else if (Peek().Type == TokenType.TRUE) {
            Consume();
            return true;
        }
        else if (Peek().Type == TokenType.FALSE) {
            Consume();
            return false;
        }
        else if (Peek().Type == TokenType.NULL) {
            Consume();
            return null;
        }
        else
            throw new Exception("Unexpected token: " + Peek().Value);
    }

}
