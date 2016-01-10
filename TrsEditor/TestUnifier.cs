using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interpreter.Execution;
using Interpreter.Entities.Terms;

namespace TrsEditor
{
  /// <summary>
  /// Small custom unifier for test purposes
  /// </summary>
  public class TestUnifier : ITrsUnifierCalculation
  {
    public List<UnificationResult> GetUnifier(TrsTermBase ruleHead, TrsTermBase matchTerm)
    {
      List<UnificationResult> unifiers = new List<UnificationResult>();
      for (int i = 0; i < 1; i++)
      {
        var tempResult = new UnificationResult();
        tempResult.Unifier = new List<Substitution>();
        foreach (var variable in ruleHead.GetVariables())
        {
          tempResult.Unifier.Add(new Substitution { SubstitutionTerm = new TrsNumber(i.ToString()), Variable = variable });
        }
        tempResult.Succeed = true;
        unifiers.Add(tempResult);
      }
      return unifiers;
    }
  }
}
