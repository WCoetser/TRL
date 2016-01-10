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
using Interpreter.Execution;
using Parser.AbstractSyntaxTree.Keywords;
using Interpreter.Entities.Terms;

namespace Interpreter.Entities.Keywords
{
  /// <summary>
  /// Placeholder class for NATIVE keyword in reduction rules.
  /// </summary>
  public class TrsNativeKeyword : TrsTermBase
  {
    public AstNativeKeyword AstNativeKeyword { get; private set; }

    public TrsNativeKeyword(AstNativeKeyword astNativeKeyword)
    {
      AstNativeKeyword = astNativeKeyword;
    }
    protected override TrsTermBase ApplySubstitution(Substitution substitution)
    {
      return this;
    }

    public override bool ContainsVariable(TrsVariable testVariable)
    {
      return false;
    }

    public override TrsTermBase CreateCopyAndReplaceSubTerm(TrsTermBase termToReplace, TrsTermBase replacementTerm)
    {
      return this;
    }

    public override List<TrsVariable> GetVariables()
    {
      return new List<TrsVariable>();
    }

    public override bool Equals(object other)
    {
      return other != null && other is TrsNativeKeyword;
    }

    public override int GetHashCode()
    {
      return Parser.Tokenization.Keywords.Native.GetHashCode();
    }

    public override string ToSourceCode()
    {
      return Parser.Tokenization.Keywords.Native;
    }
  }
}
