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
using Interpreter.Execution;
using Parser.AbstractSyntaxTree;

namespace Interpreter.Entities.Terms
{
  public abstract class TrsTermBase : TrsStatement
  {   
    /// <summary>
    /// Applies the given substituion to this term, creating a new term. This method is used by the public
    /// version to apply a collection of substitutions.
    /// </summary>
    protected abstract TrsTermBase ApplySubstitution(Substitution substitution);

    /// <summary>
    /// Applies a set of substitutions to this term, swapping variables. If no substitution applies to a term,
    /// the term is returned.
    /// </summary>
    /// <param name="substitutions"></param>
    /// <returns></returns>
    public TrsTermBase ApplySubstitutions(IEnumerable<Substitution> substitutions)
    {
      TrsTermBase retVal = this;
      foreach (var substitution in substitutions) retVal = retVal.ApplySubstitution(substitution);
      return retVal;
    }

    /// <summary>
    /// Checks if this term contains the given variable. This is part of the "occurs check" which 
    /// forms part of the MGU calculation, and the definition of reduction rules.
    /// </summary>
    public abstract bool ContainsVariable(TrsVariable testVariable);

    /// <summary>
    /// Create a copy of the current term.
    /// </summary>
    public override TrsStatement CreateCopy()
    {
      return CreateCopyAndReplaceSubTerm(null, null);
    }

    /// <summary>
    /// Creates a copy of this term, replacing the termToReplace with the replacementTerm.
    /// Term equality is defined using the overloaded equals method.
    /// </summary>
    /// <returns></returns>
    public abstract TrsTermBase CreateCopyAndReplaceSubTerm(TrsTermBase termToReplace, TrsTermBase replacementTerm);

    /// <summary>
    /// Gets a list of variables in this term and all subterms.
    /// </summary>
    /// <returns></returns>
    public abstract List<TrsVariable> GetVariables();
        
    /// <summary>
    /// Tests if this is a ground term (ie. that it contains no variables)
    /// </summary>
    public bool IsGroundTerm
    {
      get
      {
        return GetVariables().Count == 0;
      }
    }
  }
}
