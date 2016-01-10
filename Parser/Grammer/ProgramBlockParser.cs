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
  /// Parse a program block, with terms and reduction rules seperated by semi-colons.
  /// </summary>
  public class ProgramBlockParser : BaseParser
  {
    private static StatementParser statementParser = new StatementParser();
    private static ListParser<StatementParser, AstStatement> listParser 
      = new ListParser<StatementParser, AstStatement>(statementParser, TokenType.Semicolon, true);

    /// <summary>
    /// Program blocks can contain reduction rules or terms
    /// </summary>
    public override ParseResult Parse(List<Token> tokens, int startLocation)
    {
      // Semi-colon seperated list of terms and reduction rules
      var statementListResult = listParser.Parse(tokens, startLocation);
      if (statementListResult.Succeed
        && statementListResult.NextParsePosition == tokens.Count)
      {
        return new ParseResult(tokens, startLocation)
        {
          AstResult = new AstProgramBlock(((AstListResult<AstStatement>)statementListResult.AstResult).ListResult),
          NextParsePosition = statementListResult.NextParsePosition
        };
      }
      else return ParseResult.MakeFail(tokens, statementListResult.StartPosition, "Expected statement, reduction rule, type definition or limit.");
    }
  }
}
