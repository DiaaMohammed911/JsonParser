# JSON Parser in C#
 
A JSON tokenizer and recursive-descent parser implemented in C# from scratch.
 
The project was built to understand how JSON parsing works internally, from converting raw characters into tokens to building the final data structure.
 
## Features
 
- Hand-written tokenizer (no external libraries)
- Recursive-descent parser
- Supports all JSON value types: objects, arrays, strings, numbers, `true`, `false`, and `null`
- Clear error messages for invalid input (missing colon, non-string keys, trailing commas, unexpected tokens)

## Architecture

The parser is divided into separate responsibilities:

<img width="1808" height="152" alt="mermaid-diagram" src="https://github.com/user-attachments/assets/504e0b0a-263a-4b89-bc9d-7603a648c47a" />
