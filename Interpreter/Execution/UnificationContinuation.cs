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

namespace Interpreter.Execution
{
  /// <summary>
  /// This class represents a continuation in the modified version of the Martelli-Montanari
  /// rules for syntactic unification. It was added to cater for problems with multiple MGUs
  /// (ex. polinomial factoriation) and for the eventual inclusion of AC-unification in TRL.
  /// </summary>
  public class UnificationContinuation
  {
    public List<Substitution> CurrentSubstitutions { get; set; }
    public List<Equation> CurrentEquations { get; set; }

    public UnificationContinuation CreateCopy()
    {
      return new UnificationContinuation
      {
        CurrentEquations = new List<Equation>(CurrentEquations.Select(eq => eq.CreateCopy())),
        CurrentSubstitutions = new List<Substitution>(CurrentSubstitutions.Select(sub => sub.CreateCopy()))
      };      
    }

    /// <summary>
    /// Adds the given substitution to the existing oners and apply the new composition
    /// to the existing problem.
    /// </summary>
    public void ComposeAndApplySubstitutions(Substitution newSubstitution)
    {
      // Add the new substitution to the existing ones
      List<Substitution> removeList = new List<Substitution>();
      bool found = false;
      foreach (var sub in CurrentSubstitutions)
      {
        sub.SubstitutionTerm = sub.SubstitutionTerm.ApplySubstitutions(new[] { newSubstitution });
        if (sub.Variable.Equals(sub.SubstitutionTerm)) removeList.Add(sub);
        else found = sub.Variable.Equals(newSubstitution.Variable);
      }
      CurrentSubstitutions.RemoveAll(r => removeList.Contains(r));
      if (!found) CurrentSubstitutions.Add(newSubstitution);

      // Apply the modified substitutions to the existing problem
      foreach (var eq in CurrentEquations)
      {
        eq.Lhs = eq.Lhs.ApplySubstitutions(CurrentSubstitutions);
        eq.Rhs = eq.Rhs.ApplySubstitutions(CurrentSubstitutions);
      }
    }
    
    public override bool Equals(object obj)
    {
      var other = obj as UnificationContinuation;
      if (other == null) return false;

      // Substitutions
      var thisSub = new HashSet<Substitution>(this.CurrentSubstitutions);
      var otherSub = new HashSet<Substitution>(other.CurrentSubstitutions);
      thisSub.IntersectWith(otherSub);
      if (thisSub.Count() != otherSub.Count()) return false;

      // Equations
      var thisEq = new HashSet<Equation>(this.CurrentEquations);
      var otherEq = new HashSet<Equation>(other.CurrentEquations);
      thisEq.IntersectWith(otherEq);
      if (thisEq.Count() != otherEq.Count()) return false;

      return true;
    }

    public override int GetHashCode()
    {
      int hashCode = 0;
      foreach (var sub in CurrentSubstitutions) hashCode = hashCode ^ sub.GetHashCode();
      foreach (var eq in CurrentEquations) hashCode = hashCode ^ (~eq.GetHashCode());
      return hashCode;
    }
  }
}
