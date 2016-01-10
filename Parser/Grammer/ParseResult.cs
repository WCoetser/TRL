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
  public class ParseResult
  {
    public ParseResult(List<Token> tokens, int startPosition) 
    {     
      if (tokens == null || tokens.Count == 0)
      {
        ErrorMessage = "No input given.";
      }
      else if (tokens.Count <= startPosition)
      {
        ErrorMessage = "Unexpected end of input.";
      }
      StartPosition = startPosition;
      NextParsePosition = startPosition;
      SourceTokens = tokens;
    }

    /// <summary>
    /// Utility function.
    /// </summary>
    public static ParseResult MakeFail(List<Token> tokens, int startPosition, string errorMessage)
    {
      var retFail = new ParseResult(tokens, startPosition);
      retFail.ErrorMessage = errorMessage;
      return retFail;
    }

    public bool Succeed { get { return string.IsNullOrWhiteSpace(ErrorMessage); } }
    public string ErrorMessage { get; set; }
    public int NextParsePosition { get; set; }
    public int StartPosition { get; private set; }
    public List<Token> SourceTokens { get; private set; }
    public AstBase AstResult { get; set; }
  }
}
