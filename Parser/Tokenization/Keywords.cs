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

namespace Parser.Tokenization
{
  public static class Keywords
  {
    /// <summary>
    /// This keyword should be used in reduction rule tails to indicate that a 
    /// function registerred with the interpreter will rewrite the unified head
    /// to creatre a tail for plcaement in the term being rewritten.
    /// </summary>
    public const string Native = "native";
    
    /// <summary>
    /// Declares a bottom up finite tree automaton based type
    /// </summary>
    public const string Type = "type";

    /// <summary>
    /// Used to specify all variables as a type
    /// </summary>
    public const string TrsVariable = "TrsVariable";

    /// <summary>
    /// Used to specify all constannts as a type
    /// </summary>
    public const string TrsConstant = "TrsConstant";

    /// <summary>
    /// Used to specify all strings as a type
    /// </summary>
    public const string TrsString = "TrsString";

    /// <summary>
    /// Used to specify all numbers as a type.
    /// </summary>
    public const string TrsNumber = "TrsNumber";

    /// <summary>
    /// Part of the type system, used to map variables to types. As in "limit ... to ...;"
    /// </summary>
    public const string Limit = "limit";

    /// <summary>
    /// Part of the type system, used to map variables to types. As in "limit ... to ...;"
    /// </summary>
    public const string To = "to";

    /// <summary>
    /// List of native types, ie. numbers, strings, constants and variables.
    /// </summary>
    public readonly static string[] NativeTypes = new string [] { Keywords.TrsConstant, Keywords.TrsNumber, Keywords.TrsString, Keywords.TrsVariable };

  }
}
