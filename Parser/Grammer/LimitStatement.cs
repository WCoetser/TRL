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
using Parser.AbstractSyntaxTree.Keywords;
using Parser.Tokenization;
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.TypeDefinitions;
using Parser.AbstractSyntaxTree.Terms;

namespace Parser.Grammer
{
  /// <summary>
  /// Parses a "limit ... to ...;" statement
  /// </summary>
  public class LimitStatement : BaseParser
  {
    private static KeywordParser<AstLimitKeyword> limitKeyword = new KeywordParser<AstLimitKeyword>(TokenType.Limit);
    private static KeywordParser<AstToKeyword> toKeyword = new KeywordParser<AstToKeyword>(TokenType.To);
    private static ListParser<TypeDefinitionVariableParser, AstVariable> variableListParser = new ListParser<TypeDefinitionVariableParser, AstVariable>(new TypeDefinitionVariableParser(), TokenType.Comma, false);
    private static TypeDefinitionTypeNameParser typeNameParser = new TypeDefinitionTypeNameParser();

    public override ParseResult Parse(List<Tokenization.Token> tokens, int startPosition)
    {
      ParseResult resultLimit = null;
      ParseResult resultVariableList = null;
      ParseResult resultTo = null;
      ParseResult resultTypeName = null;

      resultLimit = limitKeyword.Parse(tokens, startPosition);
      if (!resultLimit.Succeed) return resultLimit;
      
      resultVariableList = variableListParser.Parse(tokens, resultLimit.NextParsePosition);
      if (!resultVariableList.Succeed) return resultVariableList;

      resultTo = toKeyword.Parse(tokens, resultVariableList.NextParsePosition);
      if (!resultTo.Succeed) return resultTo;

      resultTypeName = typeNameParser.Parse(tokens, resultTo.NextParsePosition);
      if (!resultTypeName.Succeed) return resultTypeName;

      return new ParseResult(tokens, startPosition)
      {
        AstResult = new AstLimitStatement(((AstListResult<AstVariable>)resultVariableList.AstResult).ListResult, (AstTypeDefinitionName)resultTypeName.AstResult),
        NextParsePosition = resultTypeName.NextParsePosition
      };
    }
  }
}
