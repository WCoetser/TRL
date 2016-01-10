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
using Parser.AbstractSyntaxTree.Terms;
using Parser.AbstractSyntaxTree.TypeDefinitions;

namespace Interpreter.Entities.TypeDefinitions
{
  public class TrsTypeDefinitionVariable : TrsTypeDefinitionTermBase
  {
    public string VariableName { get; private set; }

    public TrsTypeDefinitionVariable(string variableName, AstTypeDefinitionVariable astSource = null)
    {
      AstSource = astSource;
      VariableName = variableName;
    }

    public override string ToSourceCode()
    {
      return ":" + VariableName;
    }

    public override bool Equals(object other)
    {
      var typedOther = other as TrsTypeDefinitionVariable;
      return typedOther != null
        && typedOther.VariableName == this.VariableName;
    }

    public override int GetHashCode()
    {
      return ":".GetHashCode() ^ VariableName.GetHashCode();
    }

    public override TrsTypeDefinitionTermBase CreateCopy()
    {
      // All copies of a variable should always be equal
      return this;
    }

    public override IEnumerable<TrsTypeDefinitionTypeName> GetReferencedTypeNames()
    {
      return new TrsTypeDefinitionTypeName[0];
    }

    public override TrsTypeDefinitionTermBase CreateCopyAndReplaceSubTermRefEquals(TrsTypeDefinitionTermBase termToReplace, TrsTypeDefinitionTermBase replacementTerm)
    {
      if (object.ReferenceEquals(this, termToReplace)) return replacementTerm;
      else return new TrsTypeDefinitionVariable(VariableName);
    }

    public override List<TrsTypeDefinitionAtom> GetAllAtoms()
    {
      return new List<TrsTypeDefinitionAtom>();
    }

    public override List<TrsTypeDefinitionVariable> GetAllVariables()
    {
      return new[] { this }.ToList();
    }
  }
}
