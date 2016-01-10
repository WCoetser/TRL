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

namespace Parser.Grammer
{
  /// <summary>
  /// Class used for parsing comma seperated grammer elements. This is a utility class
  /// used to address the repeating pattern of comma seperated lists.
  /// </summary>
  /// <typeparam name="TParser"></typeparam>
  public class ListParser<TParser, TAstResult> : BaseParser
    where TParser : BaseParser
    where TAstResult : AstBase
  {
    private readonly TParser elementParser;
    private readonly TokenType delimeter;
    private readonly bool allowEndDelimiter;

    /// <summary>
    /// Setting <param name="allowEndDelimiter"/> to true
    /// means that the list must end with the given delimiter.
    /// </summary>
    public ListParser(TParser listElementParser, TokenType delimiter, bool allowEndDelimiter)
    {
      elementParser = listElementParser;
      this.delimeter = delimiter;
      this.allowEndDelimiter = allowEndDelimiter;
    }

    /// <summary>
    /// Parse a list of elements, containing at least one element.
    /// </summary>
    public override ParseResult Parse(List<Token> tokens, int startPosition)
    {
      // Validaytion
      if (startPosition >= tokens.Count)
        return ParseResult.MakeFail(tokens, startPosition, "Expected at least one parsable element.");
      ParseResult firstElement = elementParser.Parse(tokens, startPosition);

      List<TAstResult> astResult = new List<TAstResult>();

      // First element
      if (!firstElement.Succeed
        || firstElement.NextParsePosition >= tokens.Count)
        return ParseResult.MakeFail(tokens, startPosition, "Failed to parse first element in list");
      else astResult.Add((TAstResult)firstElement.AstResult);

      // Parse the rest
      var currentResult = firstElement;
      int lastDelimiterPosition = -1;
      int lastParsePosition = currentResult.NextParsePosition; // keep track of last parse position for improved error handling
      while (currentResult.NextParsePosition < tokens.Count
        && tokens[currentResult.NextParsePosition].TokenType == delimeter
        && currentResult.Succeed)
      {
        lastDelimiterPosition = currentResult.NextParsePosition;
        lastParsePosition = lastDelimiterPosition + 1;
        // Skip delimiter
        currentResult = elementParser.Parse(tokens, lastParsePosition);
        if (currentResult.Succeed) astResult.Add((TAstResult)currentResult.AstResult);
      }

      if (allowEndDelimiter)
      {
        // Must end in delimiter ... therefore the current element must fail to parse
        if (!currentResult.Succeed && firstElement.Succeed)
        {
          return new ParseResult(tokens, startPosition)
          {
            AstResult = new AstListResult<TAstResult>(astResult),
            // Skip delimiter
            NextParsePosition = lastDelimiterPosition == -1 ?
              firstElement.NextParsePosition + 1 : lastDelimiterPosition + 1
          };
        }
        else return ParseResult.MakeFail(tokens, lastParsePosition, "Expected list ending in delimiter.");
      }
      else
      {
        if (currentResult.Succeed)
          return new ParseResult(tokens, startPosition)
          {
            AstResult = new AstListResult<TAstResult>(astResult),
            NextParsePosition = currentResult.NextParsePosition
          };
        else return ParseResult.MakeFail(tokens, lastParsePosition, "Expected delimiter seperated list.");
      }
    }
  }
}
