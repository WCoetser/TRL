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
using Interpreter.Entities.TypeDefinitions;

namespace Interpreter.Execution
{
  /// <summary>
  /// Optimized data structure for NFTA language membership test and state storage.  
  /// </summary>
  public class InterpreterType
  {
    private TrsTypeDefinitionTermBase CurrentNode = null; 

    /// <summary>
    /// Top level node or generated options ... these options will be type names
    /// </summary>
    private HashSet<TrsTypeDefinitionTypeName> CurrentNodeMatchedTypes = new HashSet<TrsTypeDefinitionTypeName>();

    /// <summary>
    /// Arguments have corresponding positions with their matched type names
    /// </summary>
    private List<InterpreterType> ArgumentMatchedTypes = new List<InterpreterType>();

    /// <summary>
    /// For AC matching, count the number of occurances of substypes
    /// </summary>
    private Dictionary<InterpreterType, int> AcArgumentMatchedTypes = new Dictionary<InterpreterType, int>();

    private InterpreterType ParentNode = null;

    public InterpreterType(TrsTypeDefinitionTermBase sourceTypeDefinition, InterpreterType parentNode = null) 
    {
      this.ParentNode = parentNode;
      this.CurrentNode = sourceTypeDefinition;
      if (sourceTypeDefinition is TrsTypeDefinitionTerm)
        ArgumentMatchedTypes.AddRange(((TrsTypeDefinitionTerm)sourceTypeDefinition).ArgumentTypes.Select(arg => new InterpreterType(arg,this)));
      else if (sourceTypeDefinition is TrsTypeDefinitionAcTerm)
        AcArgumentMatchedTypes = ((TrsTypeDefinitionAcTerm)sourceTypeDefinition).OnfArgumentTypes.ToDictionary(arg => new InterpreterType(arg.Term), arg => arg.Cardinality);
    }

    /// <summary>
    /// Checks type definition against NFTA. If this is the root note, all end states must be matched before true is returned.
    /// </summary>
    public bool IsTermValid(Dictionary<TrsTypeDefinitionTermBase, List<TrsTypeDefinitionTypeName>>  transitionFunction,
      HashSet<TrsTypeDefinitionTypeName> endStates)
    {
      CurrentNodeMatchedTypes.Clear();

      // Match leaf nodes for non-AC terms
      foreach (var arg in ArgumentMatchedTypes) arg.IsTermValid(transitionFunction, endStates);

      // Match leaf nodes for AC terms
      foreach (var arg in AcArgumentMatchedTypes) arg.Key.IsTermValid(transitionFunction, endStates);

      // If this is a type name, simply add: it is a result in itself
      if (CurrentNode is TrsTypeDefinitionTypeName) 
        CurrentNodeMatchedTypes.Add((TrsTypeDefinitionTypeName)CurrentNode);

      ProcessLeafNodes(transitionFunction);      
      ProcessEmptyTransitions(transitionFunction);

      // Only the root node has a meaningful test case for end states
      if (ParentNode == null)
      {
        // Compile list of matched end states.      
        HashSet<TrsTypeDefinitionTypeName> matchedEndStates = new HashSet<TrsTypeDefinitionTypeName>(CurrentNodeMatchedTypes.Intersect(endStates));
        return matchedEndStates.Count == endStates.Count;
      }
      else return true;
    }

    /// <summary>
    /// This function applies the transition function to the current node, making a record of the matched types in CurrentNodeMatchedTypes.
    /// </summary>
    private void ProcessLeafNodes(Dictionary<TrsTypeDefinitionTermBase, List<TrsTypeDefinitionTypeName>> transitionFunction)
    {
      // Match this node
      TrsTypeDefinitionTerm currentNodeTerm = CurrentNode as TrsTypeDefinitionTerm;
      TrsTypeDefinitionAcTerm currentNodeAcTerm = CurrentNode as TrsTypeDefinitionAcTerm;
      foreach (var termTypePair in transitionFunction)
      {
        // Match current node as leaf node
        List<TrsTypeDefinitionTypeName> nextStates = null;
        if (transitionFunction.TryGetValue(CurrentNode, out nextStates))
          foreach (var typeName in nextStates) CurrentNodeMatchedTypes.Add(typeName);

        // Match current node as a term
        TrsTypeDefinitionTerm matchTerm = termTypePair.Key as TrsTypeDefinitionTerm;
        TrsTypeDefinitionAcTerm matchAcTerm = termTypePair.Key as TrsTypeDefinitionAcTerm;

        if (matchAcTerm != null)
        {
          if (matchAcTerm != null
            && currentNodeAcTerm != null
            && matchAcTerm.TermName == currentNodeAcTerm.TermName)
          {
            Dictionary<TrsTypeDefinitionTypeName, int> groupedMatchedTypes = new Dictionary<TrsTypeDefinitionTypeName, int>();
            foreach (var argument in this.AcArgumentMatchedTypes)
              foreach (var type in argument.Key.CurrentNodeMatchedTypes)
                if (!groupedMatchedTypes.ContainsKey(type))
                  groupedMatchedTypes.Add(type, argument.Value);
                else groupedMatchedTypes[type] = groupedMatchedTypes[type] + argument.Value;

            var foundCount = (from argMatch in groupedMatchedTypes
                              join patternArgMatch in matchAcTerm.OnfArgumentTypes
                                on argMatch.Key equals patternArgMatch.Term
                              where argMatch.Value >= patternArgMatch.Cardinality
                              select 1).Count();
            if (foundCount != 0 && foundCount == matchAcTerm.OnfArgumentTypes.Count)
              foreach (var typeName in termTypePair.Value) CurrentNodeMatchedTypes.Add(typeName);
          }
        }
        else if (matchTerm != null)
        {
          if (matchTerm != null
            && currentNodeTerm != null
            && matchTerm.TermName == currentNodeTerm.TermName)
          {
            bool allFound = true;
            for (var argIndex = 0; argIndex < ArgumentMatchedTypes.Count; argIndex++)
              allFound = allFound
                && ArgumentMatchedTypes[argIndex].CurrentNodeMatchedTypes.Contains(matchTerm.ArgumentTypes[argIndex]);
            if (allFound)
              foreach (var typeName in termTypePair.Value) CurrentNodeMatchedTypes.Add(typeName);
          }
        }
      }
    }

    /// <summary>
    /// Add next states for empty transitions to CurrentNodeMatchedTypes
    /// </summary>
    private void ProcessEmptyTransitions(Dictionary<TrsTypeDefinitionTermBase, List<TrsTypeDefinitionTypeName>> transitionFunction)
    {
      bool found = true;
      HashSet<TrsTypeDefinitionTypeName> processed = new HashSet<TrsTypeDefinitionTypeName>();
      while (found)
      {
        found = false;
        HashSet<TrsTypeDefinitionTypeName> addTypes = new HashSet<TrsTypeDefinitionTypeName>();
        foreach (var typeName in CurrentNodeMatchedTypes)
        {
          if (!processed.Contains(typeName))
          {
            processed.Add(typeName);
            List<TrsTypeDefinitionTypeName> nextStates = null;
            if (transitionFunction.TryGetValue(typeName, out nextStates))
            {
              found = true;
              foreach (var nextState in nextStates) addTypes.Add(nextState);
            }
          }
        }
        CurrentNodeMatchedTypes.UnionWith(addTypes);
      }
    }
  }
}
