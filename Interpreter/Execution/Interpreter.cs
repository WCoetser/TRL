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
using Interpreter.Validators;
using System.Diagnostics;
using Parser.AbstractSyntaxTree;
using Interpreter.Entities.Keywords;
using Interpreter.Entities.Terms;
using Parser.AbstractSyntaxTree.Terms;
using Interpreter.Entities.TypeDefinitions;

namespace Interpreter.Execution
{
  /// <summary>
  /// This class executes the term rewriting system by rewriting terms
  /// </summary>
  public class Interpreter
  {
    /// <summary>
    /// The actual program to execute
    /// </summary>
    private TrsProgramBlock initialProgram = null;

    public List<InterpreterResultMessage> ValidationMessages { get; private set; }

    /// <summary>
    /// Lookup from outer head term name to program rules. Faster than looping through everything all the time.
    /// This allows near-linear time evaluation. A term can have multiple rules.
    /// </summary>
    private List<TrsReductionRule> programRules = null;

    /// <summary>
    /// Use a double-linked list here: the terms will be rewritten all the time, therefore deleting old terms.
    /// I do not want to trigger a list compactation algorithm each time a term is removed.
    /// </summary>
    private List<InterpreterTerm> executionCache = null;

    /// <summary>
    /// Checks terms using a tree automaton.
    /// </summary>
    private InterpreterTypeChecker typeChecker = null;

    /// <summary>
    /// For execution step: avoid extra validation step for every rewrite step
    /// </summary>
    private bool hasErrors = false;

    /// <summary>
    /// Stop rewriting when no further rewriting took place
    /// </summary>
    bool rewritingTookPlace = true;

    private List<ITrsNativeFunction> nativeFunctions = null;

    private MguCalculation mguCalculation = null;

    /// <summary>
    /// Creates a new interpreter
    /// </summary>
    public Interpreter(TrsProgramBlock programBlock, List<ITrsNativeFunction> nativeFunctions = null,
      List<ITrsUnifierCalculation> customUnifiersCalculations = null)
    {
      if (nativeFunctions == null) nativeFunctions = new List<ITrsNativeFunction>();
      else this.nativeFunctions = nativeFunctions;
      mguCalculation = new MguCalculation(customUnifiersCalculations);
      this.initialProgram = programBlock;
      var validator = new TrsProgramBlockValidator();
      validator.Validate(this.initialProgram);
      ValidationMessages = new List<InterpreterResultMessage>();
      ValidationMessages.AddRange(validator.ValidationMessages);
      this.hasErrors = ValidationMessages.Where(msg => msg.MessageType == InterpreterMessageType.Error).Count() > 0;
      if (!hasErrors) ClassifyInput();
    }

    /// <summary>
    /// Build hash tables to optimize term matching, and a linked list that will serve as a execution cache.
    /// </summary>
    private void ClassifyInput()
    {
      executionCache = new List<InterpreterTerm>();
      programRules = new List<TrsReductionRule>();
      typeChecker = new InterpreterTypeChecker(initialProgram);
      foreach (var statement in initialProgram.Statements)
      {
        if (statement is TrsTermBase)
        {
          executionCache.Add(new InterpreterTerm(statement as TrsTermBase));
        }
        else if (statement is TrsReductionRule)
        {
          // All rules treated as term prodocuts to simplify rewriting
          var rule = statement as TrsReductionRule;
          if (rule.Head is TrsTermProduct)
          {
            programRules.Add(statement as TrsReductionRule);
          }
          else
          {
            programRules.Add(new TrsReductionRule(new TrsTermProduct(new[] { rule.Head }.ToList()), rule.Tail, (AstReductionRule)rule.AstSource));
          }
        }
        else if (!(statement is TrsTypeDefinition) && !(statement is TrsLimitStatement))
        {
          throw new ArgumentException("Unknown statement type.", "programBlockIn");
        }
      }
    }

    /// <summary>
    /// Call this to get the result of rewriting. Call ExecuteRewriteStep to execute a rewriting step. ExecuteRewriteStep 
    /// returns a boolean value indicating whether further rewriting took place. This method should preferrably only be 
    /// called after the rewriting process terminated. This will create copies of the current execution cache.
    /// </summary>    
    public InterpreterResult GetCurrentRewriteResult()
    {
      var retVal = new InterpreterResult(initialProgram);
      retVal.Messages.AddRange(ValidationMessages);
      retVal.ProgramOut.Statements.AddRange(typeChecker.TypeDefinitions);
      retVal.ProgramOut.Statements.AddRange(typeChecker.VariableMappings);
      foreach (var term in executionCache)
      {
        retVal.ProgramOut.Statements.Add(term.RootTerm.CreateCopy());
      }
      foreach (var rule in programRules)
      {
        // Term products of arity one must be turned back into normal terms
        if (rule.Head is TrsTermProduct && ((TrsTermProduct)rule.Head).TermList.Count == 1)
        {
          retVal.ProgramOut.Statements.Add(new TrsReductionRule(((TrsTermProduct)rule.Head).TermList[0], rule.Tail));
        }
        else
        {
          retVal.ProgramOut.Statements.Add(rule.CreateCopy());
        }
      }
      return retVal;
    }

    /// <summary>
    /// Executes a single rewrite step. The program block is executed, and a new program block is produced in the process with the 
    /// rewritten terms and the rules from the input program. This can then be executed again.
    /// </summary>
    /// <returns>true if rewriting took place, false otherwise</returns>
    public bool ExecuteRewriteStep()
    {
      if (hasErrors || !rewritingTookPlace) return false;
      rewritingTookPlace = false;
      // Rewrite step
      if (executionCache.Count > 0)
      {
        foreach (var rule in programRules)
          ApplyReductionRuleToCache(rule);

        RemoveDuplicatesAndDeleted();
      }
      return rewritingTookPlace;
    }

    public InterpreterTypeChecker TypeChecker
    {
      get
      {
        return typeChecker;
      }
    }

    private void ExecuteRewriteForRule(InterpreterEvaluationTerm termToRewrite, TrsReductionRule rule, UnificationResult composedUnifier)
    {
      bool rewritingTookPlaceLocal = false;
      if (composedUnifier.Succeed)
      {
        if (rule.Tail.GetType() == typeof(TrsNativeKeyword))
        {
          // Replacing term value with a native function generated value
          var nativeTermInHead = ((TrsTermBase)termToRewrite.CurrentSubTerm.CreateCopy()).ApplySubstitutions(composedUnifier.Unifier);
          foreach (var native in nativeFunctions)
          {
            var processedTerm = native.Evaluate(nativeTermInHead);
            if (processedTerm == null) throw new Exception(string.Format("Native function in type {0} returned null", native.GetType().FullName));
            // If the rewrite result is the same, no rewriting took place ...
            if (!nativeTermInHead.Equals(processedTerm))
            {
              var newTerm = termToRewrite.RootTerm.CreateCopyAndReplaceSubTerm(termToRewrite.CurrentSubTerm, processedTerm);
              rewritingTookPlaceLocal = true;
              var newTermWrapper = new InterpreterTerm(newTerm);
              newTermWrapper.IsNewTerm = true;
              executionCache.Add(newTermWrapper);
            }
          }
        }
        else
        {
          // Normal rewriting without native eval functions
          var replacementTerm = ((TrsTermBase)rule.Tail.CreateCopy()).ApplySubstitutions(composedUnifier.Unifier);
          if (!termToRewrite.CurrentSubTerm.Equals(replacementTerm))
          {
            var newTerm = termToRewrite.RootTerm.CreateCopyAndReplaceSubTerm(termToRewrite.CurrentSubTerm, replacementTerm);
            rewritingTookPlaceLocal = true;
            var newTermWrapper = new InterpreterTerm(newTerm);
            newTermWrapper.IsNewTerm = true;
            executionCache.Add(newTermWrapper);
          }
        }
      }
      if (rewritingTookPlaceLocal)
      {
        rewritingTookPlace = true; // stops rewriting on next rewrite step
        termToRewrite.CacheSourceTerm.MustDeletFromCache = true;
      }
    }

    private void ApplyReductionRuleToCache(TrsReductionRule rule)
    {
      // This data structure stores the Cartesian product used for term products
      // The length of each sub-List is the size of an alphabet for calculating a "Godel string" of terms.
      // This avoids the need for seperate sub-enumerators.
      var rewriteCandidates = new List<List<InterpreterEvaluationTerm>>();

      // Populate the rewriteCandidates
      TrsTermProduct ruleHead = (TrsTermProduct)rule.Head;
      int productLength = ruleHead.TermList.Count;
      for (int termProductIndex = 0; termProductIndex < productLength; termProductIndex++)
      {
        rewriteCandidates.Add(new List<InterpreterEvaluationTerm>());
        foreach (var termInCache in executionCache)
        {
          // Do not rewrite new terms, we are doing one rewrite step at a time
          if (termInCache.IsNewTerm) continue;

          // Test all sub-terms
          var expantionStack = new Stack<TrsTermBase>();
          expantionStack.Push(termInCache.RootTerm);
          while (expantionStack.Count > 0)
          {
            var current = expantionStack.Pop();

            // Ignore the "variable only" case to avoid matching all rewrite rules to a sub-term.
            if (current is TrsVariable) continue;

            // Type rules applied here ... cater for multiple unification results by duplicating candidates
            foreach (var unifier in mguCalculation.GetUnifier(ruleHead.TermList[termProductIndex], current))
            {
              if (IsUnifierValid(unifier, ruleHead.TermList[termProductIndex]))
              {
                rewriteCandidates[termProductIndex].Add(new InterpreterEvaluationTerm(termInCache.RootTerm, current, termInCache, unifier));
              }
            }

            // Apply rewrite rule to subterms
            if (current is TrsTerm)
            {
              foreach (var subTerm in ((TrsTerm)current).Arguments)
                expantionStack.Push(subTerm);
            }
            else if (current is TrsAcTerm)
            {
              foreach (var subTerm in ((TrsAcTerm)current).OnfArguments)
                expantionStack.Push(subTerm.Term);
            }
          }
        }
      }

      // Execute rewite step ... iterate over cartesian term product
      // This iterationCount variable will prevent rewriting in the case where any of the lists are empty
      int iterationCount = rewriteCandidates.First().Count;
      foreach (var termList in rewriteCandidates.Skip(1)) iterationCount *= termList.Count;
      var matchTupple = new List<InterpreterEvaluationTerm>(rewriteCandidates.Count);
      for (int tuppleNumber = 0; tuppleNumber < iterationCount; tuppleNumber++)
      {
        var currDiv = tuppleNumber;
        UnificationResult currentUnifier = null;
        UnificationResult testUnifier = null;
        matchTupple.Clear();
        // In order to do a substitution, all variables must bind to the same values across the tupple members
        for (int termColumn = 0; termColumn < rewriteCandidates.Count; termColumn++)
        {
          var currMod = currDiv % rewriteCandidates[termColumn].Count;
          currDiv = currDiv / rewriteCandidates[termColumn].Count;
          var targetTerm = rewriteCandidates[termColumn][currMod];
          currentUnifier = targetTerm.Unifier;
          if (testUnifier == null) testUnifier = currentUnifier;
          matchTupple.Add(targetTerm);
        }
        var termProductUnifier = ComposeUnifiers(matchTupple.Select(term => term.Unifier));
        if (termProductUnifier.Succeed)
        {
          foreach (var term in matchTupple) ExecuteRewriteForRule(term, rule, termProductUnifier);
        }
      }
    }

    /// <summary>
    /// Checks that the given unifier is valid in terms of the type definitions.
    /// </summary>
    private bool IsUnifierValid(UnificationResult unifier, TrsTermBase matchedRuleHead)
    {
      // Variables at this level must be validated in a way that is sensitive to AC term type definitions
      // to account for semantics. If it is not add[:x,:y] will not match add[1,2,3] with limit :x,:y to $TrsNumber
      if (!unifier.Succeed) return false;
      if (unifier.Unifier.Count == 0) return true; // equal terms, nothing to validate

      // Keep track of variable parent cases
      Stack<TrsTermBase> evalStack = new Stack<TrsTermBase>();
      Dictionary<TrsVariable, bool> isAcParent = new Dictionary<TrsVariable, bool>();
      Dictionary<TrsVariable, bool> isNonAcParent = new Dictionary<TrsVariable, bool>();
      Dictionary<TrsVariable, HashSet<string>> variableTermNames = new Dictionary<TrsVariable, HashSet<string>>();

      evalStack.Push(matchedRuleHead);
      Action<TrsVariable, bool, Dictionary<TrsVariable, bool>> updateLookups =
        delegate(TrsVariable v, bool b, Dictionary<TrsVariable, bool> target)
        {
          if (target.ContainsKey(v)) target[v] = b;
          else target.Add(v, b);
        };
      while (evalStack.Count > 0)
      {
        var current = evalStack.Pop();
        // Variable only case ... this should not happen unless variable only reduction rule heads are allowed in the future
        var currentVariable = current as TrsVariable;
        if (currentVariable != null)
        {
          updateLookups(currentVariable, false, isAcParent);
          updateLookups(currentVariable, false, isNonAcParent);
        }
        else
        {
          // Check arguments
          var curTerm = current as TrsTerm;
          var curAcTerm = current as TrsAcTerm;
          foreach (var variable in Enumerable.Concat(curTerm == null ? new TrsVariable[0] : curTerm.Arguments.Where(arg => arg is TrsVariable).Cast<TrsVariable>(),
            curAcTerm == null ? new TrsVariable[0] : curAcTerm.OnfArguments.Where(arg => arg.Term is TrsVariable).Select(arg => arg.Term).Cast<TrsVariable>()))
          {
            updateLookups(variable, curTerm != null, isNonAcParent);
            updateLookups(variable, curAcTerm != null, isAcParent);
            if (curAcTerm != null)
            {
              HashSet<string> termNames;
              if (!variableTermNames.TryGetValue(variable, out termNames))
                variableTermNames.Add(variable, termNames = new HashSet<string>());
              termNames.Add(curAcTerm.Name);
            }
          }
        }
      }

      bool isValid = true;
      foreach (var substitution in unifier.Unifier)
      {
        // It is possible that the variable being tested does not occur in the term head ...
        if (!isNonAcParent.ContainsKey(substitution.Variable)
          && !isAcParent.ContainsKey(substitution.Variable))
        {
          isValid = isValid && typeChecker.IsSubstitutionValid(substitution);
          continue;
        }

        // AC term case
        if (isNonAcParent.ContainsKey(substitution.Variable) &&
          isNonAcParent[substitution.Variable])
          isValid = isValid && typeChecker.IsSubstitutionValid(substitution);

        // Non-AC term case
        if (isAcParent.ContainsKey(substitution.Variable)
          && isAcParent[substitution.Variable])
        {
          var acSubstitutionTerm = substitution.SubstitutionTerm as TrsAcTerm;
          if (acSubstitutionTerm == null
            || !variableTermNames[substitution.Variable].Contains(acSubstitutionTerm.Name))
          {
            isValid = isValid && typeChecker.IsSubstitutionValid(substitution);
          }
          else
          {
            // In this case, test each nested argument of the AC substitution term to match the term head variable.
            // This is due to the ONF convertion for AC terms. It keeps type checking in line with AC semantics.
            foreach (var argTerm in acSubstitutionTerm.OnfArguments.Select(arg => arg.Term))
            {
              var testSubstitution = new Substitution { Variable = substitution.Variable, SubstitutionTerm = argTerm };
              isValid = isValid && typeChecker.IsSubstitutionValid(testSubstitution);
            }
          }
        }
      }
      return isValid;
    }

    /// <summary>
    /// Combines a list of unifiers for the term product
    /// </summary>
    private UnificationResult ComposeUnifiers(IEnumerable<UnificationResult> unifierList)
    {
      if (unifierList.Count() == 1) return unifierList.First();
      UnificationResult retVal = new UnificationResult();
      var composedUnifier = new Dictionary<TrsVariable, TrsTermBase>();
      foreach (var unifier in unifierList)
      {
        foreach (var substitution in unifier.Unifier)
        {
          TrsTermBase subValue = null;
          if (!composedUnifier.TryGetValue(substitution.Variable, out subValue))
          {
            composedUnifier.Add(substitution.Variable, substitution.SubstitutionTerm);
          }
          else if (!subValue.Equals(substitution.SubstitutionTerm))
          {
            // Conflicting mapping found.
            retVal.Succeed = false;
            return retVal;
          }
        }
      }
      retVal.Succeed = true;
      retVal.Unifier = new List<Substitution>();
      retVal.Unifier.AddRange(composedUnifier.Select(pair => new Substitution
      {
        Variable = pair.Key,
        SubstitutionTerm = pair.Value
      }));
      return retVal;
    }

    /// <summary>
    /// Removes duplicate terms from the execution cache.
    /// </summary>
    private void RemoveDuplicatesAndDeleted()
    {
      HashSet<TrsTermBase> uniqueSet = new HashSet<TrsTermBase>(executionCache.Where(term => !term.MustDeletFromCache).Select(term => term.RootTerm));
      executionCache.Clear();
      executionCache.AddRange(uniqueSet.Select(term => new InterpreterTerm(term)));
    }

    /// <summary>
    /// Clears the execution cache.
    /// </summary>
    public void ClearExecutionCache()
    {
      rewritingTookPlace = true; // this must be set to true to allow rewriting to take place. When is is false, the interpreter assumes that nothing further can be done.
      executionCache.Clear();
    }

    /// <summary>
    /// Loads the given terms into the execution cache
    /// </summary>
    /// <param name="terms"></param>
    public void LoadTerms(IEnumerable<TrsTermBase> terms)
    {
      foreach (var term in terms) executionCache.Add(new InterpreterTerm(term));
    }
  }
}
