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
  public abstract class TrsTypeDefinitionAtom : TrsTypeDefinitionTermBase
  {
    public string AtomValue { get; private set; }

    public TrsTypeDefinitionAtom(string atomValue, AstTypeDefinitionAtom source = null)
    {
      AstSource = source;
      AtomValue = atomValue;
    }

    public override bool Equals(object other)
    {
      TrsTypeDefinitionAtom typedOther = other as TrsTypeDefinitionAtom;
      return typedOther != null
        && typedOther.GetType().Equals(this.GetType())
        && typedOther.AtomValue == this.AtomValue;
    }

    public override int GetHashCode()
    {
      return this.GetType().Name.GetHashCode() ^ AtomValue.GetHashCode();
    }

    public override TrsTypeDefinitionTermBase CreateCopy()
    {
      // For intepreter purposes, it all copies should always be equal
      return this;
    }

    public override IEnumerable<TrsTypeDefinitionTypeName> GetReferencedTypeNames()
    {
      return new TrsTypeDefinitionTypeName[0];
    }

    public override TrsTypeDefinitionTermBase CreateCopyAndReplaceSubTermRefEquals(TrsTypeDefinitionTermBase termToReplace,
          TrsTypeDefinitionTermBase replacementTerm)
    {
      if (object.ReferenceEquals(this, termToReplace)) return replacementTerm;
      else return this.CreateCopy();
    }

    public override List<TrsTypeDefinitionAtom> GetAllAtoms()
    {
      return new[] { this }.ToList();
    }

    public override List<TrsTypeDefinitionVariable> GetAllVariables()
    {
      return new List<TrsTypeDefinitionVariable>();
    }
  }

  public class TrsTypeDefinitionConstant : TrsTypeDefinitionAtom
  {
    public TrsTypeDefinitionConstant(string constName, AstTypeDefinitionAtom source = null)
      : base(constName, source) 
    {}

    public override string ToSourceCode()
    {
      return AtomValue;
    }
  }

  public class TrsTypeDefinitionString : TrsTypeDefinitionAtom
  {
    public TrsTypeDefinitionString(string strValue, AstTypeDefinitionAtom source = null)
      : base(strValue, source)
    { }

    public override string ToSourceCode()
    {
      return "\"" + AtomValue + "\"";
    }
  }

  public class TrsTypeDefinitionNumber : TrsTypeDefinitionAtom
  {
    public TrsTypeDefinitionNumber(string numString, AstTypeDefinitionAtom source = null)
      : base(numString, source)
    { }

    public override string ToSourceCode()
    {
      return AtomValue;
    }
  }
}
