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
using Interpreter.Entities.Terms;
using Interpreter.Entities.TypeDefinitions;
using Interpreter.Entities;
using System.Diagnostics;
using Parser.Tokenization;

namespace Interpreter.Execution
{
  /// <summary>
  /// Type system as non-deterministic bottom up tree automaton.
  /// </summary>
  public class InterpreterTypeChecker
  {
    private static TrsTypeDefinitionTypeName allNumbers = new TrsTypeDefinitionTypeName(Keywords.TrsNumber);
    private static TrsTypeDefinitionTypeName allStrings = new TrsTypeDefinitionTypeName(Keywords.TrsString);
    private static TrsTypeDefinitionTypeName allConstants = new TrsTypeDefinitionTypeName(Keywords.TrsConstant);
    private static TrsTypeDefinitionTypeName allVariables = new TrsTypeDefinitionTypeName(Keywords.TrsVariable);

    /// <summary>
    /// End states in bottom up tree automaton defined by the type system
    /// </summary>
    private Dictionary<TrsVariable, HashSet<TrsTypeDefinitionTypeName>> typeMappings
      = new Dictionary<TrsVariable, HashSet<TrsTypeDefinitionTypeName>>();

    /// <summary>
    /// Transition function for the automaton
    /// </summary>
    private Dictionary<TrsTypeDefinitionTermBase, List<TrsTypeDefinitionTypeName>> transitionFunction
      = new Dictionary<TrsTypeDefinitionTermBase, List<TrsTypeDefinitionTypeName>>();

    /// <summary>
    /// Note: input program's type definitions wiill be pre-processed to 
    /// cater for AC term semantics.
    /// </summary>
    public InterpreterTypeChecker(TrsProgramBlock programIn)
    {
      TypeCheckerPreprocessor preprocessor = new TypeCheckerPreprocessor();
      preprocessor.RewriteTerms(programIn);
      InitializeLookupTables(programIn);
    }

    private void InitializeLookupTables(TrsProgramBlock programIn)
    {
      // Variable mappings
      foreach (var limitStatement in programIn.Statements.
        Where(stm => stm is TrsLimitStatement).Cast<TrsLimitStatement>())
      {
        foreach (var variable in limitStatement.Variables)
        {
          HashSet<TrsTypeDefinitionTypeName> currentTypeList = null;
          if (!typeMappings.TryGetValue(variable, out currentTypeList))
            typeMappings.Add(variable, currentTypeList = new HashSet<TrsTypeDefinitionTypeName>());
          currentTypeList.Add(limitStatement.TypeDefinition);
        }
      }

      // Type definitions
      foreach (var typeStatement in programIn.Statements.
        Where(stm => stm is TrsTypeDefinition).Cast<TrsTypeDefinition>())
      {
        foreach (var acceptedTerm in typeStatement.AcceptedTerms)
        {
          List<TrsTypeDefinitionTypeName> typeNames = null;
          if (!transitionFunction.TryGetValue(acceptedTerm, out typeNames))
            transitionFunction.Add(acceptedTerm, typeNames = new List<TrsTypeDefinitionTypeName>());
          typeNames.Add(typeStatement.Name);
        }
      }
    }

    /// <summary>
    /// Uses the input type definitions as a bottom up tree automaton to 
    /// test the input mapping of a variable to a term.
    /// 
    /// Note: 
    /// * All final states must be matched for a mapping to be valid.
    /// * Types are "merged" therefore the same type definition name can have multiple definitions associated.
    /// </summary>
    public bool IsSubstitutionValid(Substitution substitution)
    {
      HashSet<TrsTypeDefinitionTypeName> endStates = null;

      // If variable not bound, it is valid by default
      if (!typeMappings.TryGetValue(substitution.Variable, out endStates)) return true;

      // Initial and final states
      var termIn = substitution.SubstitutionTerm.Convert();
      AddDynamicStates(termIn);

      InterpreterType testType = new InterpreterType(termIn);
      var retVal = testType.IsTermValid(transitionFunction, endStates);

      // Undo dynamic changes to state machine to cater for $TrsNumber, $TrsConstant, $TrsString and $TrsVariable
      RemoveDynamicStates();

      return retVal;
    }

    /// <summary>
    /// Updates transition function to add variable and atom mappings for the base types.
    /// This is neccesary to avoid an infinite number of type definitions.
    /// </summary>
    private void AddDynamicStates(TrsTypeDefinitionTermBase termIn)
    {
      foreach (TrsTypeDefinitionTermBase targetSymbol in termIn.GetAllAtoms().Cast<TrsTypeDefinitionTermBase>().Concat(termIn.GetAllVariables()))
      {
        TrsTypeDefinitionTypeName nextStateTypeName = null;
        if (targetSymbol is TrsTypeDefinitionNumber) nextStateTypeName = allNumbers;
        else if (targetSymbol is TrsTypeDefinitionString) nextStateTypeName = allStrings;
        else if (targetSymbol is TrsTypeDefinitionConstant) nextStateTypeName = allConstants;
        else if (targetSymbol is TrsTypeDefinitionVariable) nextStateTypeName = allVariables;
        else throw new ArgumentOutOfRangeException("To collective type for " + targetSymbol.GetType().FullName);
        List<TrsTypeDefinitionTypeName> nextStates = null;
        if (!transitionFunction.TryGetValue(targetSymbol, out nextStates))
          transitionFunction.Add(targetSymbol, nextStates = new List<TrsTypeDefinitionTypeName>());
        nextStates.Add(nextStateTypeName);
      }
    }

    /// <summary>
    /// Updates transition function to remove variable and atom mappings for the base types
    /// This is neccesary to avoid an infinite number of type definitions.
    /// </summary>
    private void RemoveDynamicStates()
    {
      List<TrsTypeDefinitionTermBase> cleanupKeys = new List<TrsTypeDefinitionTermBase>();
      foreach (var pair in transitionFunction)
      {
        if (pair.Value.Contains(allConstants)) pair.Value.Remove(allConstants);
        if (pair.Value.Contains(allNumbers)) pair.Value.Remove(allNumbers);
        if (pair.Value.Contains(allStrings)) pair.Value.Remove(allStrings);
        if (pair.Value.Contains(allVariables)) pair.Value.Remove(allVariables);

        if (pair.Value.Count == 0) cleanupKeys.Add(pair.Key);
      }

      // Avoid any accidents due to empty next states ...
      foreach (var key in cleanupKeys) transitionFunction.Remove(key);
    }

    /// <summary>
    /// Get the type definitions, grouping type definition terms by type name
    /// </summary>
    public IEnumerable<TrsTypeDefinition> TypeDefinitions
    {
      get
      {
        // Reverse the type definitions back for printing purposes
        var pivoitToTypeName = new Dictionary<TrsTypeDefinitionTypeName, HashSet<TrsTypeDefinitionTermBase>>();
        foreach (var pair in transitionFunction)
        {
          foreach (var typeName in pair.Value)
          {
            HashSet<TrsTypeDefinitionTermBase> statesInverse;
            if (!pivoitToTypeName.TryGetValue(typeName, out statesInverse))
              pivoitToTypeName.Add(typeName, statesInverse = new HashSet<TrsTypeDefinitionTermBase>());
            statesInverse.Add(pair.Key);
          }
        }
        return pivoitToTypeName.Select(pair => new TrsTypeDefinition(pair.Key, pair.Value.ToList()));
      }
    }

    /// <summary>
    /// Gets the limit statements used to build this type checker.
    /// </summary>
    public IEnumerable<TrsStatement> VariableMappings
    {
      get
      {
        // Get the inverse end state mappings for the limit statements
        var inverseVariableMappings = new Dictionary<TrsTypeDefinitionTypeName, HashSet<TrsVariable>>();
        foreach (var pair in typeMappings)
        {
          foreach (var type in pair.Value)
          {
            HashSet<TrsVariable> variables;
            if (!inverseVariableMappings.TryGetValue(type, out variables))
              inverseVariableMappings.Add(type, variables = new HashSet<TrsVariable>());
            variables.Add(pair.Key);
          }
        }
        return inverseVariableMappings.Select(pair => new TrsLimitStatement(pair.Value.ToList(), pair.Key)).ToList();
      }
    }
  }
}
