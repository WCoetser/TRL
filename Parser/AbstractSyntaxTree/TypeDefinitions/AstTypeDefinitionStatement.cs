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
  /// <summary>
  /// Defines a transition rule in a bottom up tree automaton for validating terms.
  /// </summary>
  public class AstTypeDefinitionStatement : AstStatement
  {
    public AstTypeDefinitionName TypeName { get; private set; }

    /// <summary>
    /// This is the list of terms that can be matched by the bottom-up finite state tree automaton 
    /// to validadate a term.
    /// </summary>
    public List<AstTypeDefinitionTermBase> CandidateTerms { get; private set; }

    public AstTypeDefinitionStatement(AstTypeDefinitionName typeName, List<AstTypeDefinitionTermBase> candidateTerms)
    {
      if (candidateTerms == null || candidateTerms.Count < 1) throw new ArgumentException("Invalid term list", "candidateTerms");
      if (typeName == null) throw new ArgumentException("Invalid type name", "typeName");

      TypeName = typeName;
      CandidateTerms = candidateTerms;
    }

    public override string ToSourceCode()
    {
      StringBuilder builder = new StringBuilder();
      builder.Append("type ");
      builder.Append(TypeName.ToSourceCode());
      builder.Append(" = ");
      builder.Append(CandidateTerms.First().ToSourceCode());
      foreach (var term in CandidateTerms.Skip(1))
      {
        builder.Append(" | ");
        builder.Append(term.ToSourceCode());
      }
      return builder.ToString();
    }
  }
}
