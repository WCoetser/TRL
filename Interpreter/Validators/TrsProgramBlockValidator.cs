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
using Interpreter.Execution;
using Interpreter.Entities.Terms;
using Interpreter.Entities.TypeDefinitions;
using Parser.Tokenization;
using Interpreter.Entities.Keywords;

namespace Interpreter.Validators
{
  /// <summary>
  /// Class for validating input program blocks for the interpreter.
  /// </summary>
  public class TrsProgramBlockValidator : TrsValidatorBase<TrsProgramBlock>
  {
    private TrsReductionRuleValidator reductionRuleValidator;
    private TrsTermBaseValidator termValidator;
    private Dictionary<string, bool> isAcTerm = new Dictionary<string, bool>(); // A term may not be both AC and non-AC at the same time

    public TrsProgramBlockValidator()
    {
      termValidator = new TrsTermBaseValidator(isAcTerm);
      reductionRuleValidator = new TrsReductionRuleValidator(isAcTerm);
    }

    public override void Validate(TrsProgramBlock programBlockIn)
    {
      // Check for empty
      if (programBlockIn.Statements == null || programBlockIn.Statements.Count == 0)
      {
        ValidationMessages.Add(new InterpreterResultMessage
        {
          Message = "Empty program block.",
          MessageType = InterpreterMessageType.Warning,
          InputEntity = programBlockIn
        });
      }

      // Check types for cyclical definitions
      ValidateTypeDefinitions(programBlockIn);

      // Check limit statements
      ValidateLimitStatements(programBlockIn);

      // Check statements with sub-validators
      ValidateSubValidators(programBlockIn);

      // Check type definitions for mixed AC/non-ac terms with the same name
      CheckForMixedAcTypes(programBlockIn);
    }

    private void CheckForMixedAcTypes(TrsProgramBlock programBlockIn)
    {
      foreach (var typeDef in programBlockIn.Statements.
        Where(stm => stm is TrsTypeDefinition).Cast<TrsTypeDefinition>())
      {
        foreach (var validationInput in typeDef.AcceptedTerms)
        {
          var term = validationInput as TrsTypeDefinitionTerm;
          var acTerm = validationInput as TrsTypeDefinitionAcTerm;

          if (term != null || acTerm != null)
          {
            string strComp = term != null ? term.TermName : acTerm.TermName;
            var isAc = acTerm != null;
            if (!isAcTerm.ContainsKey(strComp))
              isAcTerm.Add(strComp, isAc);
            if (isAc != isAcTerm[strComp])
            {
              ValidationMessages.Add(new InterpreterResultMessage
              {
                Message = string.Format("The '{0}' term may not be an AC and non-AC term at the same time.", strComp),
                InputEntity = validationInput,
                MessageType = InterpreterMessageType.Error
              });
            }
          }
        }
      }
    }

    private void ValidateLimitStatements(TrsProgramBlock programBlockIn)
    {
      HashSet<TrsTypeDefinitionTypeName> typeNames = new HashSet<TrsTypeDefinitionTypeName>();
      HashSet<TrsVariable> mappedVariables = new HashSet<TrsVariable>();

      // Add native types
      foreach (var keyword in Keywords.NativeTypes.AsEnumerable())
        typeNames.Add(new TrsTypeDefinitionTypeName(keyword));

      // Add declared types
      foreach (var typeDef in programBlockIn.Statements.
        Where(stm => stm is TrsTypeDefinition).Cast<TrsTypeDefinition>()) typeNames.Add(typeDef.Name);

      // Collect mapped variables
      foreach (var limit in programBlockIn.Statements.
        Where(stm => stm is TrsLimitStatement).Cast<TrsLimitStatement>())
      {
        foreach (var variable in limit.Variables)
        {
          mappedVariables.Add(variable);
        }
      }

      // Check all mapped types exist
      foreach (var limit in programBlockIn.Statements.
        Where(stm => stm is TrsLimitStatement).Cast<TrsLimitStatement>())
      {
        if (!typeNames.Contains(limit.TypeDefinition))
        {
          termValidator.ValidationMessages.Add(new InterpreterResultMessage
          {
            InputEntity = limit,
            Message = "Unknown type referenced in limit statement: " + limit.TypeDefinition.ToSourceCode(),
            MessageType = InterpreterMessageType.Error
          });
        }
      }

      // Check all native function redexes have a mapped variables in head
      foreach (var redex in programBlockIn.Statements.
        Where(stm => stm is TrsReductionRule && ((TrsReductionRule)stm).Tail is TrsNativeKeyword).Cast<TrsReductionRule>())
      {
        foreach (var var in redex.Head.GetVariables())
        {
          if (!mappedVariables.Contains(var))
          {
            termValidator.ValidationMessages.Add(new InterpreterResultMessage
            {
              InputEntity = redex,
              Message = "Variable " + var.ToSourceCode() + " in term head for native function is not mapped to a type definition.",
              MessageType = InterpreterMessageType.Error
            });
          }
        }
      }
    }

    /// <summary>
    /// Checks sub-validators
    /// </summary>
    private void ValidateSubValidators(TrsProgramBlock programBlockIn)
    {
      foreach (var statement in programBlockIn.Statements)
      {
        var rule = statement as TrsReductionRule;
        var term = statement as TrsTermBase;
        var typeDefinition = statement as TrsTypeDefinition;

        if (rule != null)
        {
          reductionRuleValidator.Validate(rule);
        }
        else if (term != null)
        {
          if (term is TrsVariable)
          {
            termValidator.ValidationMessages.Add(new InterpreterResultMessage
            {
              InputEntity = term,
              Message = "A term cannot only be a variable, this would match all rewrite rules excluding those resulting from the occurs check, taking type definitions into account.",
              MessageType = InterpreterMessageType.Error
            });
          }
          else if (typeDefinition != null)
          {
            if (Keywords.NativeTypes.Contains(typeDefinition.Name.TypeName))
            {
              termValidator.ValidationMessages.Add(new InterpreterResultMessage
              {
                InputEntity = term,
                Message = "A type definition name may not be the same as the native type names: "
                  + Keywords.TrsConstant + " "
                  + Keywords.TrsNumber + " "
                  + Keywords.TrsString + " "
                  + Keywords.TrsVariable,
                MessageType = InterpreterMessageType.Error
              });
            }
          }
          else termValidator.Validate(term);
        }
      }
      ValidationMessages.AddRange(reductionRuleValidator.ValidationMessages);
      ValidationMessages.AddRange(termValidator.ValidationMessages);
    }

    public override void ClearMessages()
    {
      ValidationMessages.Clear();
      reductionRuleValidator.ClearMessages();
      termValidator.ClearMessages();
    }

    /// <summary>
    /// Checks bottom up tree automaton defined by type declerations e-stransitions for cycles.
    /// This would result in non-termination during runtime.
    /// </summary>
    private void ValidateTypeDefinitions(TrsProgramBlock programBlockIn)
    {
      #region Check for cycles in type graph

      // Test for non-AC terms
      var cycleTestGraph = new Dictionary<TrsTypeDefinitionTypeName, List<TrsTypeDefinitionTypeName>>();
      foreach (var typeDef in programBlockIn.Statements.
        Where(td => td is TrsTypeDefinition).Cast<TrsTypeDefinition>())
      {
        List<TrsTypeDefinitionTypeName> nextStates = null;
        if (!cycleTestGraph.TryGetValue(typeDef.Name, out nextStates))
          cycleTestGraph.Add(typeDef.Name, nextStates = new List<TrsTypeDefinitionTypeName>());
        // Do the type names
        nextStates.AddRange(typeDef.AcceptedTerms.Where(td => td is TrsTypeDefinitionTypeName).Cast<TrsTypeDefinitionTypeName>());
      }
      ValidateForCycles(cycleTestGraph);

      // Test for AC terms
      TypeCheckerPreprocessor prepocessor = new TypeCheckerPreprocessor();
      ValidateForCycles(prepocessor.GetCycleTestGraph(programBlockIn), true);

      #endregion

      #region Check type usage in type definitions

      foreach (var typeDef in programBlockIn.Statements.
        Where(td => td is TrsTypeDefinition).Cast<TrsTypeDefinition>())
      {
        foreach (var refType in typeDef.GetReferencedTypeNames())
        {
          // Check the type definition subtypes
          if (!cycleTestGraph.ContainsKey(refType) && !Keywords.NativeTypes.Contains(refType.TypeName))
          {
            ValidationMessages.Add(new InterpreterResultMessage
            {
              InputEntity = typeDef,
              Message = "Unknown type: " + refType.ToSourceCode(),
              MessageType = InterpreterMessageType.Error
            });
          }
        }
      }

      #endregion

      #region Check AC types for at least two arguments

      foreach (TrsTypeDefinition typeDef in programBlockIn.Statements.Where(stm => stm is TrsTypeDefinition))
      {
        foreach (var acceptedTerm in typeDef.AcceptedTerms)
        {
          var acTermType = acceptedTerm as TrsTypeDefinitionAcTerm;
          if (acTermType != null && acTermType.OnfArgumentTypes.Sum(arg => arg.Cardinality) < 2)
          {
            ValidationMessages.Add(new InterpreterResultMessage
            {
              InputEntity = typeDef,
              Message = acTermType.TermName + " defined in type $" + typeDef.Name.TypeName + " must have at least two arguments.",
              MessageType = InterpreterMessageType.Error
            });
          }
        }
      }

      #endregion
    }

    private void ValidateForCycles(Dictionary<TrsTypeDefinitionTypeName, List<TrsTypeDefinitionTypeName>> cycleTestGraph, bool isAcTermGraph = false)
    {
      var visitedNodes = new HashSet<TrsTypeDefinitionTypeName>();
      var childNodes = new Stack<TrsTypeDefinitionTypeName>();
      foreach (var currentState in cycleTestGraph)
      {
        visitedNodes.Clear();
        childNodes.Clear();
        childNodes.Push(currentState.Key);
        while (childNodes.Count > 0)
        {
          var currentPosition = childNodes.Pop();
          if (visitedNodes.Contains(currentPosition))
          {
            ValidationMessages.Add(new InterpreterResultMessage
            {
              InputEntity = currentPosition,
              Message = isAcTermGraph ? "Found cycle in AC Term Type Graph for type name " + currentState.Key.ToSourceCode()
                : "Found cycle in type graph for type name " + currentState.Key.ToSourceCode(),
              MessageType = InterpreterMessageType.Error
            });
            break;
          }
          else
          {
            visitedNodes.Add(currentPosition);
            List<TrsTypeDefinitionTypeName> nextNodes = null;
            if (cycleTestGraph.TryGetValue(currentPosition, out nextNodes))
            {
              foreach (var nextNode in nextNodes) childNodes.Push(nextNode);
            }
          }
        }
      }
    }
  }
}
