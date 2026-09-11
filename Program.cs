        Test("42");
        Test("3.14");
        Test("1e3");
        Test("\"hello\"");
        Test("true");
        Test("false");
        Test("null");
    static void Test(string json)
    {
        Console.WriteLine($"JSON: {json}");

        var tokenizer = new JsonTokenizer(json);
        var tokens = tokenizer.Tokenize();

        var parser = new JsonParser(tokens);
        var result = parser.ParseValue();

        Console.WriteLine($"Result: {result}");
        Console.WriteLine($"Type: {result?.GetType().Name ?? "null"}");
        Console.WriteLine("--------------------");
    }