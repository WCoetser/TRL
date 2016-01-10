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
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interpreter.Execution;
using Interpreter.Entities;
using Interpreter.Entities.Terms;

namespace TrsEditor
{
  /// <summary>
  /// These "native" functions does addition, subtraction, multiplication and division.
  /// </summary>
  public class ArithmaticFunctions : ITrsNativeFunction
  {
    private const string Addition = "add";
    private const string Subtraction = "sub";
    private const string Divide = "div";
    private const string Multiply = "mul";

    public TrsTermBase Evaluate(TrsTermBase termIn)
    {
      var tIn = termIn as TrsTerm;
      var tInAc = termIn as TrsAcTerm;

      if (tIn != null)
      {
        if (tIn.Arguments.Count != 2) return termIn;
        if (!(tIn.Arguments[0] is TrsNumber) || !(tIn.Arguments[1] is TrsNumber)) return termIn;
        var numLhs = Convert.ToDouble(((TrsNumber)tIn.Arguments[0]).Value);
        var numRhs = Convert.ToDouble(((TrsNumber)tIn.Arguments[1]).Value);
        double retVal;
        switch (tIn.Name.ToLower())
        {
          case Subtraction:
            retVal = numLhs - numRhs;
            break;
          case Divide:
            if (numRhs == 0) return tIn;
            else retVal = numLhs / numRhs;
            break;
          default:
            return tIn;
        }
        return new TrsNumber(retVal.ToString());
      }
      else if (tInAc != null)
      {
        if (tInAc.TotalCardinality < 2) throw new InvalidProgramException("AC term with less than 2 arguments");
        if (tInAc.OnfArguments.Where(arg => arg.Term is TrsNumber).Select(arg => arg.Cardinality).Count() != tInAc.TotalCardinality)
          return tInAc;
        if (tInAc.Name == Addition)
        {
          return new TrsNumber(tInAc.ExpandedArguments.Select(arg => Convert.ToDouble(((TrsNumber)arg).Value)).Sum().ToString());
        }
        else if (tInAc.Name == Multiply)
        {
          double retVal = 1.0;
          foreach (var number in tInAc.ExpandedArguments.Select(arg => Convert.ToDouble(((TrsNumber)arg).Value)))
            retVal *= number;
          return new TrsNumber(retVal.ToString());
        }
        else return tInAc;
      }
      else return termIn;
    }
  }
}
