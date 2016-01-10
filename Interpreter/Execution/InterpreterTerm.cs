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
using System.ComponentModel;
using Interpreter.Entities.Terms;

namespace Interpreter.Execution
{
  /// <summary>
  /// Used internally by the interpreter to keep tracfk of which terms have been rewritten
  /// so that they can be removed from the execution cache.
  /// </summary>
  internal class InterpreterTerm
  {
    public InterpreterTerm(TrsTermBase root)
    {
      if (root == null) throw new ArgumentException("Root may not be null","root");
      RootTerm = root;
    }

    /// <summary>
    /// The term represented.
    /// </summary>
    public TrsTermBase RootTerm { get; private set; }
    
    /// <summary>
    /// When a term has been rewritten, it should be deleted to prevent infinite loops.
    /// </summary>
    public bool MustDeletFromCache { get; set; }
    
    [DefaultValue(false)]
    public bool IsNewTerm { get; set; }
  }
}
