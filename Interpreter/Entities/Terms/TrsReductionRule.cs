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
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Terms;

namespace Interpreter.Entities.Terms
{
  public class TrsReductionRule : TrsStatement
  {
    public TrsReductionRule(TrsTermBase head, TrsTermBase tail, AstReductionRule astSource)
    {
      Head = head;
      Tail = tail;
      AstSource = astSource;
    }

    public TrsReductionRule(TrsTermBase head, TrsTermBase tail) : this(head, tail, null)
    {
    }

    /// <summary>
    /// The maching part
    /// </summary>
    public TrsTermBase Head { get; private set; }

    /// <summary>
    /// The substituted part.
    /// </summary>
    public TrsTermBase Tail { get; private set; }

    public override bool Equals(object other)
    {
      TrsReductionRule otherRule = other as TrsReductionRule;
      return otherRule != null
        && otherRule.Head.Equals(this.Head)
        && otherRule.Tail.Equals(this.Tail);
    }

    public override int GetHashCode()
    {
      return Head.GetHashCode() ^ ~Tail.GetHashCode();
    }

    public override string ToSourceCode()
    {
      return Head.ToSourceCode() + " => " + Tail.ToSourceCode();
    }

    public override TrsStatement CreateCopy()
    {
      return new TrsReductionRule((TrsTermBase)(Head.CreateCopy()), (TrsTermBase)(Tail.CreateCopy()));
    }
  }
}
