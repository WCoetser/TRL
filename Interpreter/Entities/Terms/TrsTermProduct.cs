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
using Parser.AbstractSyntaxTree;
using Interpreter.Execution;
using Parser.AbstractSyntaxTree.Terms;

namespace Interpreter.Entities.Terms
{
  /// <summary>
  /// Represents a term product, ex. [t1,t1,etc.]. Used as a reduction rule head.
  /// </summary>
  public class TrsTermProduct : TrsTermBase
  {
    public List<TrsTermBase> TermList { get; private set; }

    public TrsTermProduct(List<TrsTermBase> termList)
      : this(termList,null)
    {      
    }

    public TrsTermProduct(List<TrsTermBase> termList, AstTermProduct astSource)      
    {
      TermList = termList;
      AstSource = astSource;
    }

    public override string ToSourceCode()
    {
      StringBuilder builder = new StringBuilder();
      builder.Append("[");
      foreach (var term in TermList)
      {
        if (term != TermList.First()) builder.Append(",");
        builder.Append(term.ToSourceCode());
      }
      builder.Append("]");
      return builder.ToString();
    }

    protected override TrsTermBase ApplySubstitution(Substitution substitution)
    {
      TrsTermProduct product = new TrsTermProduct(new List<TrsTermBase>());
      foreach (var term in TermList)
      {
        if (term.Equals(substitution.Variable)) product.TermList.Add(substitution.SubstitutionTerm);
        else product.TermList.Add(term.ApplySubstitutions(new [] { substitution }));
      }
      return product;
    }

    public override bool ContainsVariable(TrsVariable testVariable)
    {
      return TermList.Where(term => term.ContainsVariable(testVariable)).Count() > 0;
    }

    public override TrsTermBase CreateCopyAndReplaceSubTerm(TrsTermBase termToReplace, TrsTermBase replacementTerm)
    {
      TrsTermProduct product = new TrsTermProduct(new List<TrsTermBase>());
      foreach (var term in TermList)
      {
        if (term.Equals(termToReplace)) product.TermList.Add(replacementTerm);
        else product.TermList.Add(term.CreateCopyAndReplaceSubTerm(termToReplace, replacementTerm));
      }
      return product;
    }

    public override List<TrsVariable> GetVariables()
    {
      List<TrsVariable> containedVariables = new List<TrsVariable>();
      foreach (var term in TermList) containedVariables.AddRange(term.GetVariables());
      return containedVariables;
    }

    public override bool Equals(object other)
    {
      TrsTermProduct otherTp = other as TrsTermProduct;
      if (otherTp == null || otherTp.TermList.Count != TermList.Count) return false;
      bool retVal = true;
      for (int i = 0; i < otherTp.TermList.Count; i++)
      {
        retVal = retVal && TermList[i].Equals(otherTp.TermList[i]);
      }
      return retVal;
    }

    public override int GetHashCode()
    {
      int hashVal = 0;
      foreach (var term in TermList)
        hashVal ^= ~term.GetHashCode();
      return hashVal;
    }
  }
}
