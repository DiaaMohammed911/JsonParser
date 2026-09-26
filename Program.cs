
Test("{\"name\":\"Diaa\",\"age\":22}");
Test("[1, 2, 3]");
Test("{\"a\": [1, 2], \"b\": {\"c\": true}}");
Test("true");
Test("null");
Test("[1, [2, [3, [4, [5, 6]]]]]");
Test("[[[[[1]]]]]");
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