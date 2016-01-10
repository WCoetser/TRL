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
using Parser.AbstractSyntaxTree.Terms;

namespace Parser.Grammer
{
  /// <summary>
  /// A term product is represented as a list of terms in square brackets. There must be at least two terms in the list.
  /// 
  /// Ex. [t1(...),t2(...),etc.]
  /// 
  /// This must appear in a reduction rule head.
  /// </summary>
  public class TermProductParser : BaseParser
  {
    private static TermParser parser = new TermParser();
    private static ListParser<TermParser, AstTermBase> listParser 
      = new ListParser<TermParser, AstTermBase>(parser, TokenType.Comma, false);

    public override ParseResult Parse(List<Tokenization.Token> tokens, int startPosition)
    {
      ParseResult result = new ParseResult(tokens, startPosition);
      if (startPosition >= tokens.Count)
      {
        result.ErrorMessage = "Unexpected end of input, tried to parse term product.";
        return result;
      }
      if (tokens[startPosition].TokenType != TokenType.OpenTermProduct)
      {        
        result.ErrorMessage = "Expected square bracket for term product.";
        return result;
      }

      var productList = listParser.Parse(tokens, startPosition + 1);

      if (!productList.Succeed) 
      {
        result.ErrorMessage = "Failed to parse term product.";
        return result;
      }

      var termList = ((AstListResult<AstTermBase>)productList.AstResult).ListResult;

      if (productList.NextParsePosition >= tokens.Count
        && tokens[productList.NextParsePosition].TokenType != TokenType.CloseTermProduct)
      {
        result.ErrorMessage = @"Expected term product close bracket ""]"".";
      }
      else if (termList.Count <= 1)
      {
        result.ErrorMessage = @"A term product must contain at least two terms.";
      }
      else
      {
        result.NextParsePosition = productList.NextParsePosition + 1;
        result.AstResult = new AstTermProduct(termList);
      }
      return result;
    }
  }
}
