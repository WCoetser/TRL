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
using Parser.AbstractSyntaxTree.Terms;

namespace Parser.AbstractSyntaxTree.TypeDefinitions
{
  public class AstTypeDefinitionAcTerm : AstTypeDefinitionTermBase
  {
    public Token TermName { get; private set; }

    public List<AstTypeDefinitionName> SubTypeArgumentNames { get; private set; }

    public AstTypeDefinitionAcTerm(Token termName, List<AstTypeDefinitionName> subTypeArgumentNames)
    {
      if (subTypeArgumentNames == null || subTypeArgumentNames.Count < 1) throw new ArgumentException("Invalid subtype arguments", "subTypeArgumentNames");
      if (termName == null) throw new ArgumentException("Invalid term name.", "termName");

      TermName = termName;
      SubTypeArgumentNames = subTypeArgumentNames;
    }   

    /// <summary>
    /// ONF confersion is only neccesary for the actual entities
    /// </summary>
    public AstArgumentList TermArguments { get; set; }

    public override string ToSourceCode()
    {
      return TermName.TokenString + "[" + TermArguments.ToSourceCode() + "]";
    }
  }
}
