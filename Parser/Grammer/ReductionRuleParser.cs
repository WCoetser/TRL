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
  public class ReductionRuleParser : BaseParser
  {
    private static TermParser termParser = new TermParser();

    private static TermProductParser termProductParser = new TermProductParser();

    public override ParseResult Parse(List<Token> tokens, int startLocation)
    {
      // Head part
      var head = termParser.Parse(tokens, startLocation);
      if (!head.Succeed) head = termProductParser.Parse(tokens, startLocation);
      if (!head.Succeed)
      {
        head.ErrorMessage = "Expected reduction rule head term or term product: " + head.ErrorMessage;
        return head;
      }
      // Arrow
      if (head.NextParsePosition >= tokens.Count 
        || tokens[head.NextParsePosition].TokenType != TokenType.Arrow)
      {
        var arrowError = new ParseResult(tokens, head.NextParsePosition);
        arrowError.ErrorMessage = "Expected reduction rule arrow symbol";
        return arrowError;
      }
      // Tail part
      var tail = termParser.Parse(tokens, head.NextParsePosition + 1);
      if (!tail.Succeed)
      {
        tail.ErrorMessage = "Expected reduction rule tail term: " + tail.ErrorMessage;
        return tail;
      }
      var reductionRule = new ParseResult(tokens, startLocation);
      reductionRule.AstResult = new AstReductionRule((AstTermBase)head.AstResult, (AstTermBase)tail.AstResult);
      reductionRule.NextParsePosition = tail.NextParsePosition;
      return reductionRule;
    }
  }
}
