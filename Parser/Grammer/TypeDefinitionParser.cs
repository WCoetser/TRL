/*
This program is an implementation of the Term Rewriting Language, or TRL. 
In that sense it is also a specification for TRL by giving a reference
implementation. It contains a parser and interpreter.

Copyright (C) 2012 Wikus Coetser, 
Contact information on my blog: http://coffeesmudge.blogspot.com/

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, version 3 of the License.


This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser.Tokenization;
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Keywords;
using Parser.AbstractSyntaxTree.TypeDefinitions;

namespace Parser.Grammer
{
  public class TypeDefinitionParser : BaseParser
  {
    private static TypeDefinitionTermParser typeDefTermParser = new TypeDefinitionTermParser();
    private static ListParser<TypeDefinitionTermParser, AstTypeDefinitionTermBase> listParser
      = new ListParser<TypeDefinitionTermParser, AstTypeDefinitionTermBase>(typeDefTermParser, TokenType.Pipe, false);

    private static KeywordParser<AstTypeKeyword> typeKeyword = new KeywordParser<AstTypeKeyword>(TokenType.TypeDecleration);
    private static TypeDefinitionTypeNameParser typeNameParser = new TypeDefinitionTypeNameParser();

    public override ParseResult Parse(List<Token> tokens, int startPosition)
    {
      // Check length      
      if (startPosition + 5 > tokens.Count) 
        return ParseResult.MakeFail(tokens, startPosition, "Expected type definition, encountered end of input.");

      // Check for "type"
      int currentPosition = startPosition;
      var keywordResult = typeKeyword.Parse(tokens, currentPosition);
      if (keywordResult.Succeed) currentPosition = keywordResult.NextParsePosition;
      else return ParseResult.MakeFail(tokens, startPosition, "Expected 'type' keyword.");

      // Get the type name
      var typeNameResult = typeNameParser.Parse(tokens, currentPosition);
      if (typeNameResult.Succeed) currentPosition = typeNameResult.NextParsePosition;
      else return ParseResult.MakeFail(tokens, typeNameResult.StartPosition, "Expected type name.");

      // Check for '='
      if (tokens[currentPosition].TokenType == TokenType.Equals) currentPosition++;
      else return ParseResult.MakeFail(tokens, currentPosition, "Expected '=' as part of type decleration.");

      // Parse pipe seperated list
      ParseResult resultInner = listParser.Parse(tokens, currentPosition);

      if (!resultInner.Succeed)
        return ParseResult.MakeFail(tokens, currentPosition, "Expected type definition.");
      else
        return new ParseResult(tokens, startPosition)
        {
          AstResult = new AstTypeDefinitionStatement((AstTypeDefinitionName)typeNameResult.AstResult, ((AstListResult<AstTypeDefinitionTermBase>)resultInner.AstResult).ListResult),
          NextParsePosition = resultInner.NextParsePosition
        };
    }
  }
}
