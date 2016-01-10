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

namespace Parser.AbstractSyntaxTree.TypeDefinitions
{
  public abstract class AstTypeDefinitionAtom : AstTypeDefinitionTermBase
  {
    public Token SourceToken { get; private set; }

    public AstTypeDefinitionAtom(Token sourceToken)
    {
      if (sourceToken == null) throw new ArgumentException("Invalid source token");
      SourceToken = sourceToken;
    }
  }

  public class AstTypeDefinitionConstant : AstTypeDefinitionAtom
  {
    public AstTypeDefinitionConstant(Token constName)
      : base(constName)
    {
    }

    public override string ToSourceCode()
    {
      return SourceToken.TokenString;
    }
  }

  public class AstTypeDefinitionNumber : AstTypeDefinitionAtom
  {
    public AstTypeDefinitionNumber(Token number)
      : base(number)
    {
    }

    public override string ToSourceCode()
    {
      return SourceToken.TokenString;
    }
  }

  public class AstTypeDefinitionString : AstTypeDefinitionAtom
  {
    public AstTypeDefinitionString(Token stringValue)
      : base(stringValue)
    {      
    }

    public override string ToSourceCode()
    {
      return "\"" + SourceToken.TokenString + "\"";
    }
  }
}
