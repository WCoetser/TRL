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

namespace Parser.Grammer
{
  /// <summary>
  /// Common base class for parser classes.
  /// </summary>
  public abstract class BaseParser
  {
    public abstract ParseResult Parse(List<Token> tokens, int startPosition);

    public ParseResult Parse(string sourceString)
    {
      Tokenizer tokenizer = new Tokenizer();
      var tokensResult = tokenizer.Tokenize(sourceString);
      ParseResult retVal = new ParseResult(tokensResult.Tokens, 0);
      if (!tokensResult.Succeed) retVal.ErrorMessage = tokensResult.ErrorMessage;
      else
      {
        retVal = Parse(tokensResult.Tokens, 0);
        if (retVal.Succeed && retVal.NextParsePosition != tokensResult.Tokens.Count)
        {
          retVal.ErrorMessage = "Expected end of input at position " + tokensResult.Tokens[retVal.NextParsePosition].From;
          retVal.AstResult = null;
        }
      }
      return retVal;
    }
  }
}
