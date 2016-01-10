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
using Parser.AbstractSyntaxTree.TypeDefinitions;
using Parser.Tokenization;

namespace Interpreter.Entities.TypeDefinitions
{
  /// <summary>
  /// NB: Hashing and equality does not take type merging into account here.
  /// </summary>
  public class TrsTypeDefinition : TrsStatement
  {
    public TrsTypeDefinitionTypeName Name { get; private set; }

    public List<TrsTypeDefinitionTermBase> AcceptedTerms { get; private set; }

    public TrsTypeDefinition(TrsTypeDefinitionTypeName name, List<TrsTypeDefinitionTermBase> acceptedTerms, 
      AstTypeDefinitionStatement astIn = null)
    {
      AstSource = astIn;
      AcceptedTerms = acceptedTerms;
      Name = name;
    }

    /// <summary>
    /// Syntactical equality: type merging is checked in the interpreter
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
      TrsTypeDefinition typedOther = other as TrsTypeDefinition;
      if (typedOther == null 
        || !typedOther.Name.Equals(this.Name)
        || typedOther.AcceptedTerms.Count != this.AcceptedTerms.Count) return false;
      for (int i = 0; i < typedOther.AcceptedTerms.Count; i++)
        if (!typedOther.AcceptedTerms[i].Equals(this.AcceptedTerms[i])) return false;
      return true;
    }

    /// <summary>
    /// NB: Hashiong does not take type merging into account
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = Name.GetHashCode();
      foreach (var type in AcceptedTerms)
      {
        hashCode = hashCode ^ ~type.GetHashCode();
      }
      return hashCode;
    }

    public override TrsStatement CreateCopy()
    {
      return new TrsTypeDefinition(Name, AcceptedTerms.Select(term => term.CreateCopy()).ToList(), (AstTypeDefinitionStatement)AstSource);
    }

    public override string ToSourceCode()
    {
      StringBuilder builder = new StringBuilder();
      builder.Append(Parser.Tokenization.Keywords.Type);
      builder.Append(" ");
      builder.Append(Name.ToSourceCode());
      builder.Append(" = ");
      builder.Append(AcceptedTerms.First().ToSourceCode());
      foreach (var type in AcceptedTerms.Skip(1)) 
      {
        builder.Append(" | ");
        builder.Append(type.ToSourceCode());
      }
      return builder.ToString();
    }

    public IEnumerable<TrsTypeDefinitionTypeName> GetReferencedTypeNames()
    {
      IEnumerable<TrsTypeDefinitionTypeName> retVal = new TrsTypeDefinitionTypeName[0];
      foreach (var refTypes in AcceptedTerms.Select(st => st.GetReferencedTypeNames()))
        retVal = retVal.Concat(refTypes);
      return retVal;
    }
  }
}
