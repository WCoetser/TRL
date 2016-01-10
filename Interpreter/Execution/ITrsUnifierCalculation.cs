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
  /// <summary>
  /// This interface is used by the interpreter to find term unifiers. The MGU Calculation class is 
  /// an implementation of this.
  /// </summary>
  public interface ITrsUnifierCalculation
  {
    /// <summary>
    /// Gets a list of unifiers for the given input terms. The matching terms variables are preserved.
    /// 
    /// Two terms (ex. a higher order polinomial and a constant) can have more than one unifier (ex. factors)
    /// </summary>
    List<UnificationResult> GetUnifier(TrsTermBase ruleHead, TrsTermBase matchTerm);
  }
}
