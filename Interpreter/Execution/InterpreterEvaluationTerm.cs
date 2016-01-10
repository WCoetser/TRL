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
  internal class InterpreterEvaluationTerm : InterpreterTerm
  {
    public InterpreterEvaluationTerm(TrsTermBase root, TrsTermBase subterm, InterpreterTerm cacheSourceTerm, 
      UnificationResult currentUnifier)
      : base(root)
    {
      if (subterm == null) throw new ArgumentException("subterm");
      if (cacheSourceTerm == null) throw new ArgumentException("cacheSourceTerm");
      if (currentUnifier == null) throw new ArgumentException("currentUnifier");
      CurrentSubTerm = subterm;
      CacheSourceTerm = cacheSourceTerm;
      Unifier = currentUnifier;
    }

    public UnificationResult Unifier { get; private set; }

    /// <summary>
    /// The cache source term ... for back referencing.
    /// </summary>
    public InterpreterTerm CacheSourceTerm { get; private set; }
   
    /// <summary>
    /// Assumed to be an ancestor or RootTerm
    /// </summary>
    public TrsTermBase CurrentSubTerm { get; private set; }
       
    /// <summary>
    /// NB: Tests membership with reference equals. Rewriting produces new terms from scratch.
    /// </summary>
    public bool IsCurrentRoot
    {
      get
      {
        return CurrentSubTerm == null || object.ReferenceEquals(RootTerm,CurrentSubTerm);
      }
    }

  }
}
