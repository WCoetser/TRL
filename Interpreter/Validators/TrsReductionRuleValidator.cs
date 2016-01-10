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
using Interpreter.Entities.Keywords;
using Interpreter.Entities.Terms;

namespace Interpreter.Validators
{
  public class TrsReductionRuleValidator : TrsValidatorBase<TrsReductionRule>
  {
    private TrsTermBaseValidator termValidator;

    public TrsReductionRuleValidator(Dictionary<string, bool> acTerms)
    {
      termValidator = new TrsTermBaseValidator(acTerms);
    }

    public override void Validate(TrsReductionRule validationInput)
    {
      if (validationInput.Head is TrsVariable)
      {
        ValidationMessages.Add(new InterpreterResultMessage
        {
          Message = "A reduction rule head may not only be a variable.",
          MessageType = InterpreterMessageType.Error,
          InputEntity = validationInput
        });
      }

      // All variables in tail must appear on head
      HashSet<TrsVariable> headVariables = new HashSet<TrsVariable>(validationInput.Head.GetVariables());
      HashSet<TrsVariable> tailVariables = new HashSet<TrsVariable>(validationInput.Tail.GetVariables());
      tailVariables.UnionWith(headVariables);
      if (tailVariables.Count != headVariables.Count)
      {
        ValidationMessages.Add(new InterpreterResultMessage
        {
          Message = "A reduction rule head must contain all variables that is in the reduction rule tail.",
          MessageType = InterpreterMessageType.Error,
          InputEntity = validationInput
        });
      }

      // Head validation
      if (validationInput.Head is TrsTermProduct)
      {
        foreach (var term in ((TrsTermProduct)validationInput.Head).TermList)
          termValidator.Validate(term);
      }
      else
      {
        termValidator.Validate(validationInput.Head);
      }
      
      // The tail validation must take the case into account where the "native" keywork have been used
      if (validationInput.Tail.GetType() != typeof(TrsNativeKeyword))
      {
        termValidator.Validate(validationInput.Tail);
      }
      ValidationMessages.AddRange(termValidator.ValidationMessages);
    }

    public override void ClearMessages()
    {
      ValidationMessages.Clear();
      termValidator.ClearMessages();
    }
  }
}
