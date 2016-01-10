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
using Parser.AbstractSyntaxTree;
using Parser.AbstractSyntaxTree.Terms;

namespace Interpreter.Entities.Terms
{
  public class TrsTerm : TrsTermBase
  {
    public string Name { get; private set; }

    public List<TrsTermBase> Arguments { get; private set; }

    public TrsTerm(string name, IEnumerable<TrsTermBase> arguments, AstTerm sourceTerm) 
    {
      if (arguments == null || arguments.Count() == 0) 
        throw new ArgumentException("Expected at least one argument for term definition, otherwise use atom.");
      Name = name;
      Arguments = arguments.ToList();
      AstSource = sourceTerm;
    }

    public TrsTerm(string name, IEnumerable<TrsTermBase> arguments) 
      : this (name, arguments, null)
    {

    }

    public override bool Equals(object other)
    {
      TrsTerm otherTerm = other as TrsTerm;
      if (otherTerm == null) return false;
      return Name.Equals(otherTerm.Name)
        && Arguments.Count == otherTerm.Arguments.Count
        && Enumerable.Range(0, Arguments.Count).Where(i => Arguments[i].Equals(otherTerm.Arguments[i])).Count() == otherTerm.Arguments.Count;
    }

    public override int GetHashCode()
    {
      int currCode = Name.GetHashCode();
      foreach (var arg in Arguments)
      {
        currCode = currCode ^ ~arg.GetHashCode();
      }
      return currCode;
    }

    public override bool ContainsVariable(TrsVariable testVariable)
    {
      return Arguments.Any(arg => arg.ContainsVariable(testVariable));
    }

    /// <summary
    /// A term will always have at least one argument, otherwise it will not parse. "t()" is not a valid term, and should be written "t"
    /// </summary>
    public override string ToSourceCode()
    {
      StringBuilder result = new StringBuilder();
      result.Append(Name);
      result.Append("(");
      result.Append(Arguments.First().ToSourceCode());
      foreach (var arg in Arguments.Skip(1))
      {
        result.Append(",");
        result.Append(arg.ToSourceCode());
      }
      result.Append(")");
      return result.ToString();
    }

    protected override TrsTermBase ApplySubstitution(Execution.Substitution substitution)
    {
      return new TrsTerm(Name, Arguments.Select(arg => arg.ApplySubstitutions(new [] { substitution })));
    }

    public override TrsTermBase CreateCopyAndReplaceSubTerm(TrsTermBase termToReplace, TrsTermBase replacementTerm)
    {
      if (this.Equals(termToReplace))
      {
        return replacementTerm;
      }
      else
      {
        return new TrsTerm(Name, Arguments.Select(arg => arg.CreateCopyAndReplaceSubTerm(termToReplace, replacementTerm)));
      }
    }

    public override List<TrsVariable> GetVariables()
    {
      IEnumerable<TrsVariable> retVal = new TrsVariable[0];
      foreach (var subVars in Arguments.Select(arg => arg.GetVariables()))
      {
        retVal = retVal.Concat(subVars);
      }
      return retVal.ToList();
    }
  }
}
