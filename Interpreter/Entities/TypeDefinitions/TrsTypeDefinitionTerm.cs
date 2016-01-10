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

namespace Interpreter.Entities.TypeDefinitions
{
  /// <summary>
  /// Represents a type definition term. These terms are used to validate 
  /// substitutions in the unification algorithm.
  /// </summary>
  public class TrsTypeDefinitionTerm : TrsTypeDefinitionTermBase
  {
    public string TermName { get; private set; }

    public List<TrsTypeDefinitionTermBase> ArgumentTypes { get; private set; }

    public TrsTypeDefinitionTerm(string termName, List<TrsTypeDefinitionTermBase> argumentTypes, 
      AstTypeDefinitionTerm astSource = null)
    {
      TermName = termName;
      AstSource = astSource;
      ArgumentTypes = argumentTypes;
    }

    public override string ToSourceCode()
    {
      StringBuilder builder = new StringBuilder();
      builder.Append(TermName);
      builder.Append("(");
      builder.Append(ArgumentTypes.First().ToSourceCode());
      foreach (var subTypeName in ArgumentTypes.Skip(1))
      {
        builder.Append(",");
        builder.Append(subTypeName.ToSourceCode());
      }
      builder.Append(")");
      return builder.ToString();
    }

    public override bool Equals(object other)
    {
      var typedOther = other as TrsTypeDefinitionTerm;
      if (typedOther == null
        || typedOther.TermName != this.TermName
        || typedOther.ArgumentTypes.Count != this.ArgumentTypes.Count) return false;
      for (int i = 0; i < typedOther.ArgumentTypes.Count; i++)
      {
        if (!typedOther.ArgumentTypes[i].Equals(this.ArgumentTypes[i])) return false;
      }
      return true;
    }

    public override int GetHashCode()
    {
      int code = TermName.GetHashCode();
      foreach (var type in ArgumentTypes)
      {
        code = code ^ ~type.GetHashCode();
      }
      return code;
    }

    public override TrsTypeDefinitionTermBase CreateCopy()
    {
      return new TrsTypeDefinitionTerm(TermName, ArgumentTypes.Select(argType => argType.CreateCopy()).ToList(), (AstTypeDefinitionTerm)AstSource);
    }

    public override IEnumerable<TrsTypeDefinitionTypeName> GetReferencedTypeNames()
    {
      return this.ArgumentTypes.Cast<TrsTypeDefinitionTypeName>();
    }

    public override TrsTypeDefinitionTermBase CreateCopyAndReplaceSubTermRefEquals(TrsTypeDefinitionTermBase termToReplace, TrsTypeDefinitionTermBase replacementTerm)
    {
      if (object.ReferenceEquals(this, termToReplace)) return replacementTerm;
      else return new TrsTypeDefinitionTerm(TermName, ArgumentTypes.Select(arg => arg.CreateCopyAndReplaceSubTermRefEquals(termToReplace, replacementTerm)).ToList());
    }

    public override List<TrsTypeDefinitionAtom> GetAllAtoms()
    {
      List<TrsTypeDefinitionAtom> atomList = new List<TrsTypeDefinitionAtom>();
      foreach (var arg in ArgumentTypes)
        atomList.AddRange(arg.GetAllAtoms());
      return atomList;
    }

    public override List<TrsTypeDefinitionVariable> GetAllVariables()
    {
      List<TrsTypeDefinitionVariable> varList = new List<TrsTypeDefinitionVariable>();
      foreach (var arg in ArgumentTypes)
        varList.AddRange(arg.GetAllVariables());
      return varList;
    }
  }
}
