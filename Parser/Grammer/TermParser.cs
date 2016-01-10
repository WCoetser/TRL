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
using System.Text.RegularExpressions;
using Parser.Tokenization;
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Keywords;
using Parser.AbstractSyntaxTree.Terms;

namespace Parser.Grammer
{
  public class TermParser : BaseParser
  {
    private static TermParser @this = new TermParser();
    private static ListParser<TermParser, AstTermBase> listParser 
      = new ListParser<TermParser, AstTermBase>(@this, TokenType.Comma, false);

    private ParseResult ParseArgumentList(List<Token> tokens, int startLocation, bool isACTerm = false)
    {
      var currentResult = listParser.Parse(tokens, startLocation);

      if (!currentResult.Succeed) 
      {
        currentResult.ErrorMessage = "Terms must have at least one argument. 0-are function symbols shoulf be written as constants.";
        currentResult.AstResult = null;
      }
      else if (currentResult.Succeed
        && !isACTerm
        && (currentResult.NextParsePosition >= tokens.Count
            || tokens[currentResult.NextParsePosition].TokenType != TokenType.EndTerm))
      {
        currentResult.ErrorMessage = "Expected term close bracket )";
        currentResult.AstResult = null;
      }
      else if (currentResult.Succeed
        && isACTerm
        && (currentResult.NextParsePosition >= tokens.Count
            || tokens[currentResult.NextParsePosition].TokenType != TokenType.CloseTermProduct))
      {
        currentResult.ErrorMessage = "Expected AC-term close bracket ]";
        currentResult.AstResult = null;
      }
      else
      {
        currentResult.ErrorMessage = null;
        currentResult.AstResult = new AstArgumentList(((AstListResult<AstTermBase>)currentResult.AstResult).ListResult);
        // Skip close bracket
        currentResult.NextParsePosition++;
      }
      return currentResult;
    }

    public override ParseResult Parse(List<Token> tokens, int startLocation)
    {
      ParseResult result = new ParseResult(tokens, startLocation);
      if (!result.Succeed) return result;
      
      switch (tokens[startLocation].TokenType)
      {
        case TokenType.NativeFunction:
        {
          result.ErrorMessage = null;
          result.NextParsePosition = startLocation + 1;
          result.AstResult = new AstNativeKeyword(tokens[startLocation]);
          break;
        }
        case TokenType.Variable:
        {
          result.ErrorMessage = null;
          result.NextParsePosition = startLocation + 1;
          result.AstResult = new AstVariable(tokens[startLocation]);
          break;
        }
        case TokenType.Atom:
        {
          result.ErrorMessage = null;
          result.NextParsePosition = startLocation + 1;
          result.AstResult = new AstConstant(tokens[startLocation]);
          break;
        }
        case TokenType.String:
        {
          result.ErrorMessage = null;
          result.NextParsePosition = startLocation + 1;
          result.AstResult = new AstString(tokens[startLocation]);
          break;
        }
        case TokenType.Number:
        {
          result.ErrorMessage = null;
          result.NextParsePosition = startLocation + 1;
          result.AstResult = new AstNumber(tokens[startLocation]);
          break;
        }
        case TokenType.StartTerm:
        {
          var argumentListResult = ParseArgumentList(tokens, startLocation + 1);
          if (!argumentListResult.Succeed)
          {
            result = argumentListResult;
          }
          else
          {
            result.ErrorMessage = null;
            result.NextParsePosition = argumentListResult.NextParsePosition;
            result.AstResult = new AstTerm(tokens[startLocation], (AstArgumentList)argumentListResult.AstResult);
          }
          break;
        }
        case TokenType.StartAcTerm:
        { 
          var argumentListResult = ParseArgumentList(tokens, startLocation + 1, true);
          if (!argumentListResult.Succeed)
          {
            result = argumentListResult;
          }
          else
          {
            result.ErrorMessage = null;
            result.NextParsePosition = argumentListResult.NextParsePosition;
            result.AstResult = new AstAcTerm(tokens[startLocation], (AstArgumentList)argumentListResult.AstResult);
          }
          break;          
        }
        default:
        {
          result.ErrorMessage = "Expected term, variable, number, string or atom.";
          result.NextParsePosition = startLocation;
          result.AstResult = null;
          break;
        }
      }
      return result;
    }
  }
}
