string input = Console.ReadLine();

try
{
    var tokenizer = new JsonTokenizer(input);
    var tokens = tokenizer.Tokenize();

    foreach (var token in tokens)
    {
        if (token.Type == TokenType.EOF)
            Console.WriteLine("EOF");
        else
            Console.WriteLine($"{token.Type} {token.Value}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"ERR {ex.Message}");
}