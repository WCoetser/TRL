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
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Terms;

namespace Interpreter.Entities.Terms
{
  public class TrsVariable : TrsTermBase
  {
    public string Name { get; private set; }

    public TrsVariable(string name, AstVariable astSource)
    {
      Name = name;
      AstSource = astSource;
    }

    public TrsVariable(string name) : this(name, null) { }

    public override bool Equals(object other)
    {
      var otherVar = other as TrsVariable;
      return otherVar != null && otherVar.Name.Equals(this.Name);
    }

    public override int GetHashCode()
    {      
      // Use negation to distinguish bestween variables and atoms in terms of hash codes. 
      // Better hash functions are possible but not explored here.
      return ~Name.GetHashCode();
    }

    public override bool ContainsVariable(TrsVariable testVariable)
    {
      return testVariable.Equals(this);
    }

    public override string ToSourceCode()
    {
      return ":" + Name;
    }

    protected override TrsTermBase ApplySubstitution(Execution.Substitution substitution)
    {
      if (substitution.Variable.Equals(this)) return substitution.SubstitutionTerm;
      else return this;
    }

    public override TrsTermBase CreateCopyAndReplaceSubTerm(TrsTermBase termToReplace, TrsTermBase replacementTerm)
    {
      if (this.Equals(termToReplace))
      {
        return replacementTerm;
      }
      else
      {
        return new TrsVariable(Name);
      }
    }

    public override List<TrsVariable> GetVariables()
    {
      return new List<TrsVariable>(new[] { this });      
    }
  }
}
