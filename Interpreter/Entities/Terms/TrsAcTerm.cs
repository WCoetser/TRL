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
using Parser.AbstractSyntaxTree.Terms;

namespace Interpreter.Entities.Terms
{
  /// <summary>
  /// Utility class for storing AC term arguments in order normal form.
  /// </summary>
  public class TrsOnfAcTermArgument 
  {
    public TrsTermBase Term { get; set; }
    public int Cardinality { get; set; }

    public override int GetHashCode()
    {
      return this.Term.GetHashCode() ^ ~Cardinality;
    }

    public override bool Equals(object obj)
    {
      var other = obj as TrsOnfAcTermArgument;
      return other != null && other.Term.Equals(this.Term) 
        && other.Cardinality == this.Cardinality;
    }
  }

  /// <summary>
  /// Represents associative-cummutative terms.
  /// </summary>
  public class TrsAcTerm : TrsTermBase
  {
    public string Name { get; private set; }
    
    /// <summary>
    /// Arguments in order normal form for AC unification algorithm
    /// </summary>
    public List<TrsOnfAcTermArgument> OnfArguments { get; private set; }

    public TrsAcTerm(string name, IEnumerable<TrsTermBase> arguments, AstAcTerm sourceTerm = null) 
    {
      if (arguments == null || arguments.Count() == 0) 
        throw new ArgumentException("Expected at least one argument for term definition, otherwise use atom/constant.");
      Name = name;
      LoadArgumentsAsOnf(arguments);
      AstSource = sourceTerm;
    }

    public TrsAcTerm(string name, IEnumerable<TrsOnfAcTermArgument> arguments)
    {
      if (arguments == null || arguments.Count() == 0)
        throw new ArgumentException("Expected at least one argument for term definition, otherwise use atom/constant.");
      Name = name;
      OnfArguments = arguments.ToList();
    }

    public int TotalCardinality
    {
      get
      {
        return this.OnfArguments.Select(arg => arg.Cardinality).Sum();
      }
    }

    public List<TrsTermBase> ExpandedArguments
    {
      get
      {
        List<TrsTermBase> retVal = new List<TrsTermBase>();
        foreach (var onfArg in this.OnfArguments)
          retVal.AddRange(Enumerable.Range(0, onfArg.Cardinality).Select(i => onfArg.Term));
        return retVal;
      }
    }

    private void LoadArgumentsAsOnf(IEnumerable<TrsTermBase> arguments)
    {
      // 1. Flatten AC terms
      Dictionary<TrsTermBase, int> flattenedTerms = new Dictionary<TrsTermBase, int>();
      foreach (var arg in arguments)
      {
        var acArg = arg as TrsAcTerm;
        if (acArg != null && acArg.Name == this.Name)
        {
          foreach (var argInnerPair in acArg.OnfArguments)
          {
            if (!flattenedTerms.ContainsKey(argInnerPair.Term)) flattenedTerms.Add(argInnerPair.Term, argInnerPair.Cardinality);
            else flattenedTerms[argInnerPair.Term] = flattenedTerms[argInnerPair.Term] + argInnerPair.Cardinality;
          }
        }
        else 
        {
          if (!flattenedTerms.ContainsKey(arg)) flattenedTerms.Add(arg, 0);
          flattenedTerms[arg] = flattenedTerms[arg] + 1;
        }
      }
      // Sort
      this.OnfArguments = flattenedTerms.Select(pair => new TrsOnfAcTermArgument 
      { 
        Term = pair.Key, 
        Cardinality = pair.Value
      }).OrderBy(t => t.Term, new TrsComparer()).ToList();
    }

    protected override TrsTermBase ApplySubstitution(Execution.Substitution substitution)
    {
      return new TrsAcTerm(Name, OnfArguments.Select(rankedPair => new TrsOnfAcTermArgument 
      { 
        Term = rankedPair.Term.ApplySubstitutions(new [] { substitution }),
        Cardinality = rankedPair.Cardinality
      }));
    }

    public override bool ContainsVariable(TrsVariable testVariable)
    {
      return this.GetVariables().Contains(testVariable);
    }
    
    public override TrsTermBase CreateCopyAndReplaceSubTerm(TrsTermBase termToReplace, TrsTermBase replacementTerm)
    {
      if (termToReplace != null && this.Equals(termToReplace)) return (TrsTermBase)replacementTerm.CreateCopy();
      else
      {
        return new TrsAcTerm(Name, OnfArguments.Select(rankedPair => new TrsOnfAcTermArgument
        {
          Term = (TrsTermBase)(rankedPair.Term.Equals(termToReplace) ? replacementTerm.CreateCopy() : rankedPair.Term.CreateCopy()),
          Cardinality = rankedPair.Cardinality
        }));
      }
    }

    public override List<TrsVariable> GetVariables()
    {
      HashSet<TrsVariable> variables = new HashSet<TrsVariable>();
      foreach (var arg in OnfArguments)
        foreach (var @var in arg.Term.GetVariables())
          variables.Add(@var);
      return variables.ToList();
    }

    public override bool Equals(object other)
    {
      var trsOther = other as TrsAcTerm;
      if (trsOther == null) return false;
      bool isEqual = trsOther.Name == this.Name;
      if (this.OnfArguments.Count != trsOther.OnfArguments.Count) isEqual = false;
      else
      {
        for (var i = 0; i < this.OnfArguments.Count; i++) 
          isEqual = isEqual && this.OnfArguments[i].Equals(trsOther.OnfArguments[i]);
      }
      return isEqual;
    }

    public override int GetHashCode()
    {
      var code = Name.GetHashCode();
      foreach (var arg in this.OnfArguments)
        code ^= arg.GetHashCode();
      return code;
    }

    public override string ToSourceCode()
    {
      StringBuilder builder = new StringBuilder();
      builder.Append(this.Name);
      builder.Append("[");
      // Expand arguments for ToString
      var argLists = this.OnfArguments.Select(arg => Enumerable.Range(0, arg.Cardinality).Select(i => arg.Term.ToSourceCode())).ToList();
      IEnumerable<string> flatList = argLists.First();
      for (int i = 1; i < argLists.Count; i++)
        flatList = Enumerable.Concat(flatList, argLists[i]);
      flatList = flatList.ToList();
      // Write it out
      builder.Append(flatList.First());
      foreach (string strArg in flatList.Skip(1))
      {
        builder.Append(",");
        builder.Append(strArg);
      }
      builder.Append("]");
      return builder.ToString();
    }
  }
}
