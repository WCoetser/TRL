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

namespace Parser.Tokenization
{
  public class Tokenizer
  {
    public TokenizationResult Tokenize(string sourceString)
    {
      TokenizationResult result = new TokenizationResult();
      result.SourceString = sourceString;
      if (string.IsNullOrWhiteSpace(sourceString))
      {
        result.ErrorMessage = "Input string is empty";
        result.Tokens = null;
        result.Succeed = false;
        return result;
      }
      int nextParsePosition = 0;
      TokenType matchedType = TokenType.None;
      result.Tokens = new List<Token>();
      do
      {
        Match match = null;
        matchedType = TokenType.None;
        // Sequence is important to the matches to avoid confusing a term with an atom
        foreach (var pair in ParseRegexes.GetRegexToTokenTypeMappings())
        {
          if ((match = pair.Item1.Match(sourceString, nextParsePosition)).Success
            && match.Index == nextParsePosition)
          {
            matchedType = pair.Item2;
            break;
          }
        }
        if (matchedType != TokenType.None)
        {
          // Comments are removed here
          if (matchedType != TokenType.SingleLineComment)
          {
            result.Tokens.Add(new Token
            {
              From = match.Groups[1].Index,
              Length = match.Groups[1].Length,
              SourceString = sourceString,
              TokenType = matchedType
            });
          }
          nextParsePosition += match.Length;
        }
      }
      while (nextParsePosition < sourceString.Length && matchedType != TokenType.None);
      result.Succeed = nextParsePosition == sourceString.Length;
      if (!result.Succeed) result.ErrorMessage = "Unexpected characters at position " + nextParsePosition;
      return result;
    }
  }
}
