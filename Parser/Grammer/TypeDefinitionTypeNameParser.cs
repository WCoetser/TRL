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
  public class TypeDefinitionTypeNameParser : BaseParser
  {
    public override ParseResult Parse(List<Token> tokens, int startPosition)
    {
      if (startPosition >= tokens.Count)
        return ParseResult.MakeFail(tokens, startPosition, "Unexpected end of input.");

      if (tokens[startPosition].TokenType == TokenType.TypeName)
      {
        return new ParseResult(tokens, startPosition)
        {
          AstResult = new AstTypeDefinitionName(tokens[startPosition]),
          NextParsePosition = startPosition + 1
        };
      }
      else return ParseResult.MakeFail(tokens, startPosition, "Expected type name.");
    }
  }
}
