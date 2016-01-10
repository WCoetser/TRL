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
using Parser.AbstractSyntaxTree.TypeDefinitions;
using Parser.AbstractSyntaxTree.Terms;

namespace Interpreter.Entities.TypeDefinitions
{
  /// <summary>
  /// Utility class needed to represent TrsTypeDefinitionAcTerm's type arguments
  /// </summary>
  public class TrsTypeDefinitionOnfAcTermArgument
  {
    public int Cardinality { get; set; }

    public TrsTypeDefinitionTermBase Term { get; set; }

    public override bool Equals(object obj)
    {
      var other = obj as TrsTypeDefinitionOnfAcTermArgument;
      return other != null
        && other.Cardinality == this.Cardinality
        && other.Term.Equals(this.Term);
    }

    public override int GetHashCode()
    {
      return this.Term.GetHashCode() ^ ~this.Cardinality;
    }
  }

  public class TrsTypeDefinitionAcTerm : TrsTypeDefinitionTermBase
  {
    public string TermName { get; private set; }

    public List<TrsTypeDefinitionOnfAcTermArgument> OnfArgumentTypes { get; private set; }

    public TrsTypeDefinitionAcTerm(string termName, IEnumerable<TrsTypeDefinitionTermBase> argumentTypes,
      AstTypeDefinitionAcTerm astSource = null)
    {
      TermName = termName;
      AstSource = astSource;
      LoadOnfTypeArguments(argumentTypes);
    }

    public TrsTypeDefinitionAcTerm(string termName, IEnumerable<TrsTypeDefinitionOnfAcTermArgument> argumentTypes,
      AstTypeDefinitionAcTerm astSource = null)
    {
      TermName = termName;
      this.OnfArgumentTypes = argumentTypes.ToList();
      this.AstSource = astSource;
    }

    private void LoadOnfTypeArguments(IEnumerable<TrsTypeDefinitionTermBase> argumentTypes)
    {
      // 1. Flatten AC terms
      Dictionary<TrsTypeDefinitionTermBase, int> flattenedTerms = new Dictionary<TrsTypeDefinitionTermBase, int>();
      foreach (var arg in argumentTypes)
      {
        var acArg = arg as TrsTypeDefinitionAcTerm;
        if (acArg != null && acArg.TermName == this.TermName)
        {
          foreach (var argInnerPair in acArg.OnfArgumentTypes)
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
      this.OnfArgumentTypes = flattenedTerms.Select(pair => new TrsTypeDefinitionOnfAcTermArgument
      {
        Term = pair.Key,
        Cardinality = pair.Value
      }).OrderBy(t => t.Term, new TrsTypeDefinitionComparer()).ToList();
    }

    public override bool Equals(object other)
    {
      var typeDefOther = other as TrsTypeDefinitionAcTerm;
      if (typeDefOther == null
        || typeDefOther.TermName != this.TermName
        || typeDefOther.OnfArgumentTypes.Count != this.OnfArgumentTypes.Count) return false;
      bool equal = true;
      for (int i = 0; i < typeDefOther.OnfArgumentTypes.Count && equal; i++)
        equal = equal && typeDefOther.OnfArgumentTypes[i].Equals(this.OnfArgumentTypes[i]);
      return equal;
    }

    public override int GetHashCode()
    {
      var code = this.TermName.GetHashCode();
      foreach (var argument in this.OnfArgumentTypes)
        code = code ^ argument.GetHashCode();
      return code;
    }

    public override TrsTypeDefinitionTermBase CreateCopy()
    {
      return new TrsTypeDefinitionAcTerm(TermName, this.OnfArgumentTypes.Select(arg => new TrsTypeDefinitionOnfAcTermArgument
      {
        Cardinality = arg.Cardinality,
        Term = arg.Term.CreateCopy()
      }), (AstTypeDefinitionAcTerm)AstSource);
    }

    public override IEnumerable<TrsTypeDefinitionTypeName> GetReferencedTypeNames()
    {
      IEnumerable<TrsTypeDefinitionTypeName> typeNames = new TrsTypeDefinitionTypeName[0];
      foreach (var arg in OnfArgumentTypes)
        foreach (var i in Enumerable.Range(0, arg.Cardinality))
          typeNames = Enumerable.Concat(typeNames, arg.Term.GetReferencedTypeNames());
      return typeNames;
    }

    public override TrsTypeDefinitionTermBase CreateCopyAndReplaceSubTermRefEquals(TrsTypeDefinitionTermBase termToReplace, TrsTypeDefinitionTermBase replacementTerm)
    {
      if (object.ReferenceEquals(this, termToReplace)) return replacementTerm;
      else return new TrsTypeDefinitionAcTerm(TermName,
        OnfArgumentTypes.Select(arg => new TrsTypeDefinitionOnfAcTermArgument
        {
          Term = arg.Term.CreateCopyAndReplaceSubTermRefEquals(termToReplace, replacementTerm),
          Cardinality = arg.Cardinality
        }));
    }

    public override List<TrsTypeDefinitionAtom> GetAllAtoms()
    {
      List<TrsTypeDefinitionAtom> atoms = new List<TrsTypeDefinitionAtom>();
      foreach (var arg in OnfArgumentTypes)
        foreach (var i in Enumerable.Range(0, arg.Cardinality))
          atoms.AddRange(arg.Term.GetAllAtoms());
      return atoms;
    }

    public override List<TrsTypeDefinitionVariable> GetAllVariables()
    {
      List<TrsTypeDefinitionVariable> variables = new List<TrsTypeDefinitionVariable>();
      foreach (var arg in OnfArgumentTypes)
        if (arg.Term is TrsTypeDefinitionVariable)
          foreach (var i in Enumerable.Range(0, arg.Cardinality))
            variables.Add((TrsTypeDefinitionVariable)arg.Term);
      return variables;
    }

    public override string ToSourceCode()
    {
      StringBuilder builder = new StringBuilder();
      builder.Append(this.TermName);
      builder.Append("[");
      // Expand arguments for ToString
      var argLists = this.OnfArgumentTypes.Select(arg => Enumerable.Range(0, arg.Cardinality).Select(i => arg.Term.ToSourceCode())).ToList();
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
