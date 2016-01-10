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
using System.ComponentModel;
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Terms;

namespace Interpreter.Entities.Terms
{
  /// <summary>
  /// Common base class for variables, numbers and constants, all of which is regarded as atoms
  /// </summary>
  public abstract class TrsAtom : TrsTermBase
  {
    public string Value { get; protected set; }

    public override bool Equals(object other)
    {
      var otherAtom = other as TrsAtom;
      return otherAtom != null
        && otherAtom.Value.Equals(this.Value)
        && otherAtom.GetType().Equals(this.GetType()); // Caters for typeing between strings, numbers and constants (see child-classes)
    }

    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }

    public override bool ContainsVariable(TrsVariable testVariable)
    {
      return false;
    }

    protected override TrsTermBase ApplySubstitution(Execution.Substitution substitution)
    {
      // Atoms do not contain variables.
      return this;
    }

    public override List<TrsVariable> GetVariables()
    {
      return new List<TrsVariable>();
    }

    public override TrsTermBase CreateCopyAndReplaceSubTerm(TrsTermBase termToReplace, TrsTermBase replacementTerm)
    {
      if (this.Equals(termToReplace))
      {
        return replacementTerm;
      }
      else
      {
        // Cant directly invoke child constructor ... otherwise lots of code duplication has to take place, or ugly reflection
        if (this is TrsNumber) return new TrsNumber(Value);
        else if (this is TrsConstant) return new TrsConstant(Value);
        else if (this is TrsString) return new TrsString(Value);
        else throw new ArgumentException("this");
      }
    }
  }

  public class TrsNumber : TrsAtom
  {
    public TrsNumber(string value, AstNumber source)
    {
      AstSource = source;
      Value = value;
    }

    public TrsNumber(string value)
      : this(value, null)
    {
    }

    public override string ToSourceCode()
    {
      return Value;
    }
  }

  public class TrsString : TrsAtom
  {
    public TrsString(string value, AstString source)
    {
      Value = value;
      AstSource = source;
    }

    public TrsString(string value) : this(value, null)
    {
    }

    public override string ToSourceCode()
    {
      return "\"" + Value + "\"";
    }
  }

  public class TrsConstant : TrsAtom
  {
    public TrsConstant(string value, AstConstant source)
    {
      Value = value;
      AstSource = source;
    }

    public TrsConstant(string value) : this(value, null) { }

    public override string ToSourceCode()
    {
      return Value;
    }
  }
}
