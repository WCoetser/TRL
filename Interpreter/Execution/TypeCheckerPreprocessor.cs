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
using Interpreter.Entities.TypeDefinitions;

namespace Interpreter.Execution
{
  public class TypeCheckerPreprocessor
  {
    /// <summary>
    /// Rewrites the input type definitions to flatten the AC terms so that
    /// sub-terms with the same AC symbol can also be matched using the non-
    /// deterministic tree automaton.
    /// 
    /// Duplicate term definitions is also removed.
    /// </summary>
    public void RewriteTerms(TrsProgramBlock programIn)
    {
      var typeStructureDictionary = ConvertTypeStructureToDictionary(programIn);
      bool rewriteTookPlace = true;
      // Expand sub-AC subtypes to be consistent with ONF convewrsion for AC terms
      Stack<TypeRewritePair> rewriteStack = new Stack<TypeRewritePair>();
      while (rewriteTookPlace)
      {
        rewriteStack.Clear();
        rewriteTookPlace = false;
        // Compile list of expandable AC types, taking IEnumerable semantics into account
        CalculateChangeList(typeStructureDictionary, rewriteStack);
        // Expand the types
        rewriteTookPlace = RewriteTypeDefinitions(typeStructureDictionary, rewriteTookPlace, rewriteStack);
      }
      // Rebuild 
      SubstituteDictionaryToTypeStructure(programIn, typeStructureDictionary);
    }

    /// <summary>
    /// Substitutes the type structure in the given program block with one equivilant to the given dictionary.
    /// </summary>
    private void SubstituteDictionaryToTypeStructure(TrsProgramBlock programIn,
      Dictionary<TrsTypeDefinitionTypeName, HashSet<TrsTypeDefinitionTermBase>> typeStructureDictionary)
    {
      programIn.Statements.RemoveAll(stm => stm is TrsTypeDefinition);
      foreach (var typeDefPair in typeStructureDictionary)
        programIn.Statements.Add(new TrsTypeDefinition(typeDefPair.Key, typeDefPair.Value.ToList()));
    }

    /// <summary>
    /// Rewrites the type definitions using the change list from CalculateChangeList
    /// </summary>    
    private bool RewriteTypeDefinitions(Dictionary<TrsTypeDefinitionTypeName, HashSet<TrsTypeDefinitionTermBase>> typeStructureDictionary,
      bool rewriteTookPlace, Stack<TypeRewritePair> rewriteStack)
    {
      rewriteTookPlace = rewriteStack.Count() > 0;
      while (rewriteStack.Count > 0)
      {
        var current = rewriteStack.Pop();
        typeStructureDictionary[current.ParentTypeName].Remove(current.TermToRewrite);
        var removeType = current.TermToRewrite.OnfArgumentTypes.Where(argType => argType.Term.Equals(current.TypeToRemove)).First();
        current.TermToRewrite.OnfArgumentTypes.Remove(removeType);
        foreach (var replacement in current.SubstitutionTerms)
        {
          var copy = current.TermToRewrite.CreateCopy() as TrsTypeDefinitionAcTerm;
          // Keep cardinalities of type arguments consistent
          foreach (var argument in replacement.OnfArgumentTypes)
          {
            var exisitingArg = copy.OnfArgumentTypes.Where(arg => arg.Term.Equals(argument.Term)).FirstOrDefault();
            if (exisitingArg != null) exisitingArg.Cardinality += argument.Cardinality * removeType.Cardinality;
            else copy.OnfArgumentTypes.Add(new TrsTypeDefinitionOnfAcTermArgument
            {
              Term = argument.Term,
              Cardinality = argument.Cardinality * removeType.Cardinality
            });
          }
          typeStructureDictionary[current.ParentTypeName].Add(copy);
        }
      }
      return rewriteTookPlace;
    }

    /// <summary>
    /// Get list of types to rewrite in order to keep type definitions consistent with AC terms.
    /// </summary>
    private void CalculateChangeList(Dictionary<TrsTypeDefinitionTypeName, HashSet<TrsTypeDefinitionTermBase>> typeStructureDictionary, Stack<TypeRewritePair> rewriteStack)
    {
      foreach (var pair in typeStructureDictionary)
      {
        var acArguments = pair.Value.Where(subType => subType is TrsTypeDefinitionAcTerm).Cast<TrsTypeDefinitionAcTerm>();
        foreach (var term in acArguments)
        {
          foreach (TrsTypeDefinitionTypeName typeName in term.OnfArgumentTypes.Select(arg => arg.Term))
          {
            if (!typeStructureDictionary.ContainsKey(typeName)) continue; // these cases should only correspond to native types
            var substitutionTerms = typeStructureDictionary[typeName].Where(acceptedTypesItem =>
              (acceptedTypesItem is TrsTypeDefinitionAcTerm) && ((TrsTypeDefinitionAcTerm)acceptedTypesItem).TermName == term.TermName).Cast<TrsTypeDefinitionAcTerm>();
            if (substitutionTerms.FirstOrDefault() != null) rewriteStack.Push(new TypeRewritePair
            {
              ParentTypeName = pair.Key,
              TermToRewrite = term,
              SubstitutionTerms = substitutionTerms,
              TypeToRemove = typeName
            });
          }
        }
      }
    }

    private class TypeRewritePair
    {
      public TrsTypeDefinitionTypeName ParentTypeName;
      public TrsTypeDefinitionAcTerm TermToRewrite;
      public TrsTypeDefinitionTypeName TypeToRemove;
      public IEnumerable<TrsTypeDefinitionAcTerm> SubstitutionTerms;
    }

    /// <summary>
    /// Gets the type system as a lookup table, in the process removing duplicates
    /// and merging types with the same name
    /// </summary>
    private Dictionary<TrsTypeDefinitionTypeName, HashSet<TrsTypeDefinitionTermBase>> ConvertTypeStructureToDictionary(TrsProgramBlock programIn)
    {
      var retVal = new Dictionary<TrsTypeDefinitionTypeName, HashSet<TrsTypeDefinitionTermBase>>();
      foreach (TrsTypeDefinition typeDef in programIn.Statements.Where(stm => stm is TrsTypeDefinition))
      {
        HashSet<TrsTypeDefinitionTermBase> vOut = null;
        if (!retVal.TryGetValue(typeDef.Name, out vOut))
          retVal.Add(typeDef.Name, vOut = new HashSet<TrsTypeDefinitionTermBase>());
        foreach (var subType in typeDef.AcceptedTerms) vOut.Add(subType);
      }
      return retVal;
    }

    /// <summary>
    /// Gets a graph for validating AC Term Type definitions against cycles.
    /// </summary>
    public Dictionary<TrsTypeDefinitionTypeName, List<TrsTypeDefinitionTypeName>> GetCycleTestGraph(TrsProgramBlock programBlockIn)
    {
      var typeGraphIn = ConvertTypeStructureToDictionary(programBlockIn);
      Stack<TypeRewritePair> rewriteStack = new Stack<TypeRewritePair>();
      CalculateChangeList(typeGraphIn, rewriteStack);
      Dictionary<TrsTypeDefinitionTypeName, List<TrsTypeDefinitionTypeName>> retVal = new Dictionary<TrsTypeDefinitionTypeName, List<TrsTypeDefinitionTypeName>>();
      while (rewriteStack.Count > 0)
      {
        var current = rewriteStack.Pop();
        List<TrsTypeDefinitionTypeName> nextStates = null;
        if (!retVal.TryGetValue(current.ParentTypeName, out nextStates))
          retVal.Add(current.ParentTypeName, nextStates = new List<TrsTypeDefinitionTypeName>());
        foreach (var term in current.SubstitutionTerms)
          nextStates.AddRange(term.OnfArgumentTypes.Where(arg => arg.Term is TrsTypeDefinitionTypeName
            && typeGraphIn.ContainsKey((TrsTypeDefinitionTypeName)arg.Term)).Select(arg => arg.Term as TrsTypeDefinitionTypeName));
      }
      return retVal;
    }
  }
}
