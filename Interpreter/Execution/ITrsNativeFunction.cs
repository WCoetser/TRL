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
  /// When a reduction rule in this form is matched:
  /// 
  /// head_term => native;
  /// 
  /// where "native" is a keyword, this function is run on the rule head with the variables already substituted.
  /// Therefore if add(:x,:y) => native; is applied to add(:x,1) with the MGU :x => 2, this interface will be called 
  /// with add(2,1) and must then produce a new TrsNumber (inheriting from TrsAtom) with the value 3.
  /// </summary>
  public interface ITrsNativeFunction
  {
    TrsTermBase Evaluate(TrsTermBase termIn);
  }
}
