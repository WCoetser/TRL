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

namespace Interpreter.Execution
{
  public class UnificationResult
  {
    public bool Succeed { get; set; }
    public List<Substitution> Unifier { get; set; }

    public override int GetHashCode()
    {
      var retVal = Succeed ? 0 : -1;
      foreach (var subStitution in Unifier) retVal = retVal ^ subStitution.GetHashCode();
      return retVal;
    }

    public override bool Equals(object obj)
    {
      UnificationResult otherUnifier = obj as UnificationResult;
      if (otherUnifier == null) return false;
      var substitutionSetOther = new HashSet<Substitution>(otherUnifier.Unifier);
      var subtitutionSetThis = new HashSet<Substitution>(Unifier);
      substitutionSetOther.IntersectWith(subtitutionSetThis);
      return Succeed == otherUnifier.Succeed
        && substitutionSetOther.Count == subtitutionSetThis.Count;
    }
  }
}
