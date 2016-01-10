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
using Parser.AbstractSyntaxTree.TypeDefinitions;

namespace Parser.Grammer
{
  public class TypeDefinitionTermParser : BaseParser
  {
    private static TypeDefinitionTypeNameParser nameParser = new TypeDefinitionTypeNameParser();
    private static ListParser<TypeDefinitionTypeNameParser, AstTypeDefinitionName> listParser 
      = new ListParser<TypeDefinitionTypeNameParser, AstTypeDefinitionName>(nameParser, TokenType.Comma, false);

    public override ParseResult Parse(List<Token> tokens, int startPosition)
    {
      if (startPosition >= tokens.Count)
        return ParseResult.MakeFail(tokens, startPosition, "Found end of input, expected type definition term.");

      switch (tokens[startPosition].TokenType)
      {
        case TokenType.TypeName:
          return new ParseResult(tokens, startPosition)
          {
            AstResult = new AstTypeDefinitionName(tokens[startPosition]),
            NextParsePosition = startPosition + 1
          }; 
        case TokenType.Number:
          return new ParseResult(tokens, startPosition)
          {
            AstResult = new AstTypeDefinitionNumber(tokens[startPosition]),
            NextParsePosition = startPosition + 1
          };
        case TokenType.String:
          return new ParseResult(tokens, startPosition)
          {
            AstResult = new AstTypeDefinitionString(tokens[startPosition]),
            NextParsePosition = startPosition + 1
          };
        case TokenType.Atom:
          return new ParseResult(tokens, startPosition)
          {
            AstResult = new AstTypeDefinitionConstant(tokens[startPosition]),
            NextParsePosition = startPosition + 1
          };
        case TokenType.Variable:
          return new ParseResult(tokens, startPosition)
          {
            AstResult = new AstTypeDefinitionVariable(tokens[startPosition]),
            NextParsePosition = startPosition + 1
          };
        case TokenType.StartTerm:
        case TokenType.StartAcTerm:
          return ParseTypeDefinitionTermWithArgs(tokens, startPosition);
        default:
          return ParseResult.MakeFail(tokens, startPosition, "Expected type definition term.");
      }
    }

    private ParseResult ParseTypeDefinitionTermWithArgs(List<Token> tokens, int startPosition)
    {
      // Check length
      if (startPosition + 2 >= tokens.Count)
        return ParseResult.MakeFail(tokens, startPosition, "Unexpected end of input, expected type decleration term");

      // Check token name
      Token termName = null;
      if (tokens[startPosition].TokenType != TokenType.StartTerm
        && tokens[startPosition].TokenType != TokenType.StartAcTerm)
        return ParseResult.MakeFail(tokens, startPosition, "Expected term name.");
      else termName = tokens[startPosition];

      var listResult = listParser.Parse(tokens, startPosition + 1);

      if (!listResult.Succeed)
      {
        return ParseResult.MakeFail(tokens, startPosition + 1, "Expected term definition name list");
      }
      else if (listResult.NextParsePosition >= tokens.Count)
        return ParseResult.MakeFail(tokens, listResult.NextParsePosition, "Unexpected end of input, was expecting typpe decleration term.");
      else if (tokens[listResult.NextParsePosition].TokenType != TokenType.EndTerm
        && tokens[listResult.NextParsePosition].TokenType != TokenType.CloseTermProduct)
        return ParseResult.MakeFail(tokens, listResult.NextParsePosition, "Expected term closing bracket in type decleration.");
      else
      {
        if (tokens[startPosition].TokenType == TokenType.StartAcTerm)
        {
          return new ParseResult(tokens, startPosition)
          {
            AstResult = new AstTypeDefinitionAcTerm(termName, ((AstListResult<AstTypeDefinitionName>)listResult.AstResult).ListResult),
            NextParsePosition = listResult.NextParsePosition + 1
          };
        }
        else if (tokens[startPosition].TokenType == TokenType.StartTerm)
        {
          return new ParseResult(tokens, startPosition)
          {
            AstResult = new AstTypeDefinitionTerm(termName, ((AstListResult<AstTypeDefinitionName>)listResult.AstResult).ListResult),
            NextParsePosition = listResult.NextParsePosition + 1
          };
        }
        else throw new ArgumentException("Expected term of AC term");
      }
    }
  }
}
