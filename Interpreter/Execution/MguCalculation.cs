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
using Interpreter.Entities.Terms;

namespace Interpreter.Execution
{
  /// <summary>
  /// Calculates the most general unifier for two terms
  /// </summary>
  public class MguCalculation : ITrsUnifierCalculation
  {
    /// <summary>
    /// Used to calculate unifiers for terms that are not just straight 
    /// assignments to a variable.
    /// </summary>
    private List<ITrsUnifierCalculation> customUnifiers = null;

    public MguCalculation(List<ITrsUnifierCalculation> customUnifiers = null)
    {
      if (customUnifiers != null) this.customUnifiers = customUnifiers;
      else this.customUnifiers = new List<ITrsUnifierCalculation>();
    }

    /// <summary>
    /// Applies the custom unifiers for the current equation in case of failiure ... 
    /// it applies the first working unifier from the list of custom unifiers.
    /// </summary>
    private List<UnificationContinuation> CustomFail(UnificationContinuation currentProblem, Equation currEq, List<TrsVariable> variableNamesToPreserve)
    {
      // First try custom unifiers ... use the first one that works.
      HashSet<UnificationResult> unificationResults = new HashSet<UnificationResult>();
      foreach (var unifFunc in customUnifiers)
      {
        // Prevent overwriting of input terms ...
        var input = currEq.CreateCopy();
        var unifiers = unifFunc.GetUnifier(input.Lhs, input.Rhs);
        if (unifiers != null)
        {
          foreach (var result in unifiers)
            if (result.Succeed)
            {
              if (result.Unifier == null) result.Unifier = new List<Substitution>(); 
              unificationResults.Add(result);
            }
        }
      }
      unificationResults = new HashSet<UnificationResult>(unificationResults.Where(result => result.Succeed));
      if (unificationResults.Count == 0)
      {
        return new List<UnificationContinuation>();
      }
      else
      {
        var retContinuations = new HashSet<UnificationContinuation>();
        foreach (var customUnifier in unificationResults)
        {
          bool succeed = true;
          foreach (var customSubstitution in customUnifier.Unifier)
          {
            // Do occurs check
            if (customSubstitution.SubstitutionTerm.ContainsVariable(customSubstitution.Variable))
            {
              succeed = false;
              break;
            }
            // Prevent conflicting mappings
            foreach (var substitution in currentProblem.CurrentSubstitutions)
            {
              if (substitution.Variable.Equals(customSubstitution.Variable)
                && !substitution.SubstitutionTerm.Equals(customSubstitution.SubstitutionTerm))
              {
                succeed = false;
                break;
              }
            }
          }
          // Apply the subtitutions
          if (succeed)
          {
            // Create continuations here for injection back into the main algorithm loop
            UnificationContinuation currentContinuation = currentProblem.CreateCopy();
            foreach (var substitution in customUnifier.Unifier)
              ProcessSubstitutionStep(currentContinuation, new Equation { Lhs = substitution.Variable, Rhs = substitution.SubstitutionTerm }, variableNamesToPreserve);
            retContinuations.Add(currentContinuation);
          }
        }
        return retContinuations.ToList();
      }
    }

    /// <summary>
    /// Returns empty list if the terms in the equation cannot be unified or if Lhs = Rhs with no variables,
    /// otherwise a list of substitutions defining the MGU
    /// 
    /// Based on Unification chapter from Handbook of Automated Reasoning.
    /// </summary>
    private List<UnificationResult> GetMgu(Equation unificationProblem, List<TrsVariable> variableNamesToPreserve)
    {
      if (unificationProblem == null) throw new ArgumentException();
      if (unificationProblem.Lhs == null) throw new ArgumentException();
      if (unificationProblem.Rhs == null) throw new ArgumentException();
      if (variableNamesToPreserve == null) throw new ArgumentException();

      Equation initialEquation = unificationProblem.CreateCopy();
      UnificationContinuation currentProblem = new UnificationContinuation
      {
        CurrentEquations = new List<Equation>(),
        CurrentSubstitutions = new List<Substitution>()
      };
      currentProblem.CurrentEquations.Add(initialEquation);
      Queue<UnificationContinuation> currentContinuations = new Queue<UnificationContinuation>();
      currentContinuations.Enqueue(currentProblem);
      HashSet<UnificationResult> results = new HashSet<UnificationResult>();

      Func<UnificationContinuation, Equation, bool> processFail = delegate (UnificationContinuation curr, Equation currEq)
      {
        var failResult = CustomFail(currentProblem, currEq, variableNamesToPreserve);
        var succeed = true;
        if (failResult.Count == 0) succeed = false;
        else if (failResult.Count == 1) currentProblem = failResult.First();
        else foreach (var continuation in failResult) currentContinuations.Enqueue(continuation);
        return succeed;
      };

      while (currentContinuations.Count > 0)
      {
        currentProblem = currentContinuations.Dequeue();
        bool fail = false;
        while (currentProblem.CurrentEquations.Count > 0 && !fail)
        {
          var currEq = currentProblem.CurrentEquations[currentProblem.CurrentEquations.Count - 1];
          currentProblem.CurrentEquations.RemoveAt(currentProblem.CurrentEquations.Count - 1);
          if (currEq.Lhs.Equals(currEq.Rhs))
          {
            // Elimination by omission (this is a "succeed" case)
          }
          else if (currEq.Lhs is TrsAtom && currEq.Rhs is TrsAtom)
          {
            if (!currEq.Lhs.Equals(currEq.Rhs)) {
              fail = !processFail(currentProblem, currEq);
            }
          }
          else if (currEq.Lhs is TrsAtom && currEq.Rhs is TrsTerm
            || currEq.Lhs is TrsTerm && currEq.Rhs is TrsAtom
            || currEq.Lhs is TrsAtom && currEq.Rhs is TrsAcTerm
            || currEq.Lhs is TrsAcTerm && currEq.Rhs is TrsAtom
            || currEq.Lhs is TrsTerm && currEq.Rhs is TrsAcTerm
            || currEq.Lhs is TrsAcTerm && currEq.Rhs is TrsTerm)
          {
            fail = !processFail(currentProblem, currEq);
          }
          else if (currEq.Lhs is TrsTerm && currEq.Rhs is TrsTerm)
          {
            TrsTerm lhs = currEq.Lhs as TrsTerm;
            TrsTerm rhs = currEq.Rhs as TrsTerm;
            if (lhs.Name != rhs.Name || lhs.Arguments.Count != rhs.Arguments.Count)
              fail = !processFail(currentProblem, currEq);
            else
              currentProblem.CurrentEquations.AddRange(Enumerable.Range(0, lhs.Arguments.Count).
                Select(i => new Equation { Lhs = lhs.Arguments[i], Rhs = rhs.Arguments[i] }));
          }
          else if (currEq.Lhs is TrsAcTerm && currEq.Rhs is TrsAcTerm)
          {
            // Note: Failure is already processed internally in the next function call (ie. custom unifiers are called)
            fail = ProcessAcUnificationStep(currentProblem, currentContinuations, processFail, currEq);
          }
          else if (!(currEq.Lhs is TrsVariable) && (currEq.Rhs is TrsVariable))
          {
            TrsTermBase lhsSwap = currEq.Lhs;
            currEq.Lhs = currEq.Rhs;
            currEq.Rhs = lhsSwap;
            currentProblem.CurrentEquations.Add(currEq);
          }
          else if (currEq.Lhs is TrsVariable)
          {
            // Occurs check
            if (currEq.Rhs.ContainsVariable((TrsVariable)currEq.Lhs))
              fail = !processFail(currentProblem, currEq);
            else ProcessSubstitutionStep(currentProblem, currEq, variableNamesToPreserve);
          }
          else throw new Exception("Invalid program state");
        }
        if (!fail) 
          results.Add(new UnificationResult 
          { 
            Succeed = true, 
            Unifier = currentProblem.CurrentSubstitutions 
          });
      }
      return results.ToList();
    }

    private static bool ProcessAcUnificationStep(UnificationContinuation currentProblem, Queue<UnificationContinuation> currentContinuations, 
      Func<UnificationContinuation, Equation, bool> processFail, Equation currEq)
    {
      var acLhs = currEq.Lhs as TrsAcTerm;
      var acRhs = currEq.Rhs as TrsAcTerm;      
      bool fail = false;
      if (acLhs.Name != acRhs.Name)
      {
        fail = !processFail(currentProblem, currEq);
      }
      else
      {
        // Reduce cardinalities to reduce number of permutations
        foreach (var pair in (from lhsTerm in acLhs.OnfArguments
                             join rhsTerm in acRhs.OnfArguments
                               on lhsTerm.Term equals rhsTerm.Term
                              where lhsTerm.Term.IsGroundTerm
                                && rhsTerm.Term.IsGroundTerm 
                              select new { 
                                Lhs = lhsTerm, 
                                Rhs = rhsTerm 
                              }).ToList())
        {
          var diff = Math.Min(pair.Lhs.Cardinality, pair.Rhs.Cardinality);
          if (pair.Lhs.Cardinality > diff) pair.Lhs.Cardinality -= diff;
          else acLhs.OnfArguments.Remove(pair.Lhs);
          if (pair.Rhs.Cardinality > diff) pair.Rhs.Cardinality -= diff;
          else acRhs.OnfArguments.Remove(pair.Rhs);
        }
        // AC partitioning can only take place from the small to the large cardinalities
        List<TrsTermBase> fromArgs = null;
        List<TrsTermBase> toArgs = null;
        if (acLhs.TotalCardinality > acRhs.TotalCardinality)
        {
          fromArgs = acRhs.ExpandedArguments;
          toArgs = acLhs.ExpandedArguments;
        }
        else
        {
          fromArgs = acLhs.ExpandedArguments;
          toArgs = acRhs.ExpandedArguments;
        }
        // Generate mappings ... this is where the AC semantics are applied
        var setPartitionMappings = new SetMapPartitionEnumerator<int, int>(new HashSet<int>(Enumerable.Range(0, fromArgs.Count)),
          new HashSet<int>(Enumerable.Range(0, toArgs.Count)));
        // The first mapping will map to the first continuation, this must be added to the
        // current problem's equations in order to preserve algorithm correctness when the unification result 
        // contains no substitutions.
        var isFirstContinuation = true;
        var initialCurrentProblem = currentProblem.CreateCopy(); // currentProblem gets updated in the first iteration
        foreach (var mappings in setPartitionMappings)
        {
          // Cater for multiple unification problems generated by the current AC problem
          UnificationContinuation clone = null;
          if (!isFirstContinuation) clone = initialCurrentProblem.CreateCopy();
          foreach (var mapping in mappings)
          {
            Equation eq = new Equation();
            eq.Lhs = fromArgs[mapping.FromElement];
            if (mapping.ToPartition.Count == 1) eq.Rhs = toArgs[mapping.ToPartition.First()];
            else eq.Rhs = new TrsAcTerm(acLhs.Name, mapping.ToPartition.Select(i => toArgs[i]));
            if (!isFirstContinuation) clone.CurrentEquations.Add(eq);
            else currentProblem.CurrentEquations.Add(eq);
          }
          if (!isFirstContinuation) currentContinuations.Enqueue(clone);
          else isFirstContinuation = false;
        }
      }
      return fail;
    }

    private void ProcessSubstitutionStep(UnificationContinuation currentProblem, Equation currEq, List<TrsVariable> variableNamesToPreserve)
    {      
      // LHS will always be variable here ... preserve matched term variables
      Substitution newSub = null;
      if (currEq.Rhs is TrsVariable
        && variableNamesToPreserve.Contains(currEq.Lhs))
      {
        newSub = new Substitution
        {
          Variable = (TrsVariable)currEq.Rhs,
          SubstitutionTerm = currEq.Lhs
        };
      }
      else 
      {
        newSub = new Substitution
        {
          Variable = (TrsVariable)currEq.Lhs,
          SubstitutionTerm = currEq.Rhs
        };
      }
      currentProblem.ComposeAndApplySubstitutions(newSub);
    }

    public List<UnificationResult> GetUnifier(TrsTermBase termHead, TrsTermBase matchTerm)
    {
      return GetMgu(new Equation { Lhs = termHead, Rhs = matchTerm }, matchTerm.GetVariables());
    }
  }
}
