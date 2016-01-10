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

namespace Interpreter.Execution
{
  public class InterpreterResult
  {
    public InterpreterResult(TrsProgramBlock programIn)
    {
      Messages = new List<InterpreterResultMessage>();
      ProgramIn = programIn;
      ProgramOut = new TrsProgramBlock();
    }

    public bool Succeed { get { return Messages.Where(msg => msg.MessageType == InterpreterMessageType.Error).Count() > 0; } }

    public List<InterpreterResultMessage> Messages { get; private set; }

    public TrsProgramBlock ProgramIn { get; private set; }

    public TrsProgramBlock ProgramOut { get; private set; }

    /// <summary>
    /// This is true when rewriting took place. "false" means no further rewriting can take place.
    /// </summary>
    [DefaultValue(false)]
    public bool IsRewritten { get; set; }
  }
}
