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
    public object ParseValue(){
        if (Peek().Type == TokenType.STRING) return Consume().Value;
        else if (Peek().Type == TokenType.NUMBER)
        {
            if(IsFloat()) return float.Parse(Consume().Value);
            else return int.Parse(Consume().Value);
        }
        //to-do: implement array and object parsing
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
