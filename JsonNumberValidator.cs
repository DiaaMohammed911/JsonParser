using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public class JsonNumberValidator{
    
    private const string NumberPattern = @"^-?(0|[1-9][0-9]*)(\.[0-9]+)?([eE][+-]?[0-9]+)?$";
    public static bool IsValid(string str)
    {
        return Regex.IsMatch(str,NumberPattern);
    }
    public static bool IsDigit(char c)
    {
        return c >= '0' && c <= '9';
    }
}

