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
using Parser.Tokenization;
using Interpreter.Entities.Terms;

namespace Interpreter.Validators
{
  public class TrsTermBaseValidator : TrsValidatorBase<TrsTermBase>
  {
    private Dictionary<string, bool> acDictionary = null;

    public TrsTermBaseValidator(Dictionary<string, bool> acDictionary)
    {
      this.acDictionary = acDictionary;
    }

    public override void Validate(TrsTermBase validationInput)
    {
      var native = validationInput as TrsNativeKeyword;
      var termProduct = validationInput as TrsTermProduct;
      var term = validationInput as TrsTerm;
      var acTerm = validationInput as TrsAcTerm;

      if (term != null || acTerm != null)
      {
        string strComp = term != null ? term.Name : acTerm.Name;
        var isAc = acTerm != null;
        if (!acDictionary.ContainsKey(strComp)) 
          acDictionary.Add(strComp, isAc);
        if (isAc != acDictionary[strComp])
        {
          ValidationMessages.Add(new InterpreterResultMessage
          {
            Message = string.Format("The '{0}' term may not be an AC and non-AC term at the same time.", strComp),
            InputEntity = validationInput,
            MessageType = InterpreterMessageType.Error
          });
        }
      }

      if (native != null)
      {
        ValidationMessages.Add(new InterpreterResultMessage 
        { 
          Message = string.Format("The '{0}' keyword can only be used in reduction rule tails.",Keywords.Native), 
          InputEntity = validationInput, 
          MessageType = InterpreterMessageType.Error 
        });
      }
      else if (termProduct != null)
      {
        ValidationMessages.Add(new InterpreterResultMessage
        {
          Message = "Term product can only be used as a reduction rule head.",
          InputEntity = validationInput,
          MessageType = InterpreterMessageType.Error
        });
      }
      else if (term != null)
      {
        foreach (var arg in term.Arguments) Validate(arg);
      }
      else if (acTerm != null)
      {
        if (acTerm.OnfArguments.Count == 0
          || acTerm.OnfArguments.Sum(arg => arg.Cardinality) < 2)
        {
          ValidationMessages.Add(new InterpreterResultMessage
          {
            Message = string.Format("AC terms must have at last 2 arguments: '{0}' does not.", acTerm.Name),
            InputEntity = validationInput,
            MessageType = InterpreterMessageType.Error
          });
        }
        foreach (var arg in acTerm.OnfArguments.Select(arg => arg.Term)) Validate(arg);
      }
    }

    public override void ClearMessages()
    {
      ValidationMessages.Clear();
    }
  }
}
