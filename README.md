# Parser
A Lexer (Tokenizer) converts JSON characters into Tokens, while the Parser uses those tokens to build the JSON structure.
## Token Types
PUNCT → { } [ ] , :
STRING → "hello diaa"
NUMBER → 123, 3.14
TRUE → true
FALSE → false
NULL → null
EOF → End of input
-----------------------------------
Lexer = identifies and splits.
Parser = understands and builds.
