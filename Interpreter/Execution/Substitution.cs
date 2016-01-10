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
using Interpreter.Entities;
using Interpreter.Entities.Terms;

namespace Interpreter.Execution
{
  public class Substitution
  {
    /// <summary>
    /// Variable to be substituted.
    /// </summary>
    public TrsVariable Variable { get; set; }

    /// <summary>
    /// Term to substitute the variable with. This term must be a clone of an existing term, 
    /// because the we do not want to overwrite the working set of terms.
    /// </summary>
    public TrsTermBase SubstitutionTerm { get; set; }

    public Substitution CreateCopy() 
    {
      return new Substitution 
      {
        SubstitutionTerm = (TrsTermBase)this.SubstitutionTerm.CreateCopy(), 
        Variable = (TrsVariable)this.Variable.CreateCopy() 
      };
    }

    public string ToSourceCode()
    {
      return Variable.ToSourceCode() + " => " + SubstitutionTerm.ToSourceCode();
    }

    public override bool Equals(object obj)
    {
      Substitution other = obj as Substitution;
      return other != null
        && Variable.Equals(other.Variable)
        && SubstitutionTerm.Equals(other.SubstitutionTerm);
    }

    public override int GetHashCode()
    {
      return Variable.GetHashCode() ^ ~SubstitutionTerm.GetHashCode();
    }

    public override string ToString()
    {
      return GetType() + " " + ToSourceCode();
    }
  }
}
