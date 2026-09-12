        //Test("+5");
        //Test("01");
        Test(".5");
        Test("5.");
        Test("--5");
        Test("5e+");

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