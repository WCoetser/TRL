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
using Parser.Tokenization;
using Parser.Grammer;
using Interpreter.Entities;
using Interpreter.Execution;
using Parser.AbstractSyntaxTree;

namespace TrsEditor
{
  public class MainPageController
  {
    private MainPageViewModel code;    

    public MainPageController(MainPageViewModel code)
    {
      this.code = code;
    }

    public string GetErrorLine(int parsePosition)
    {
      int end = parsePosition;
      int start = parsePosition;
      while (end < (code.ProgramBlock.Length - 1)
        && code.ProgramBlock[end] != '\n'
        && code.ProgramBlock[end] != '\r') end++;
      while (start > 0
        && code.ProgramBlock[start] != '\n'
        && code.ProgramBlock[start] != '\r') start--;
      return start == end ? string.Empty : code.ProgramBlock.Substring(start, end - start + 1);
    }

    public void ExecuteRewriteStep()
    {
      // Evaluate
      string prevInput = ParseRegexes.LookAheadSingleLineComment.Replace(code.ProgramBlock, Environment.NewLine).Trim();
      StringBuilder messages = new StringBuilder();
      // Execute
      Tokenizer tokenizer = new Tokenizer();
      ProgramBlockParser parser = new ProgramBlockParser();      
      var tokenResult = tokenizer.Tokenize(code.ProgramBlock);
      if (tokenResult.Succeed && tokenResult.Tokens.Count == 0)
      {
        messages.AppendLine("// No input statements given.");
        messages.AppendLine();
        messages.AppendLine(prevInput);
      }
      else if (tokenResult.Succeed)
      {
        var ast = parser.Parse(tokenResult.Tokens, 0);
        if (ast.Succeed)
        {
          var nativeFunctions = new [] { new ArithmaticFunctions() }.Cast<ITrsNativeFunction>().ToList();
          var externalUnifiers = new [] { new ArithmaticUnifier() /* new TestUnifier() */ }.Cast<ITrsUnifierCalculation>().ToList();
          var programBlock = ((AstProgramBlock)ast.AstResult).Convert();
          var interpreter = new Interpreter.Execution.Interpreter(programBlock, nativeFunctions, externalUnifiers.ToList());
          messages.AppendLine("// Interpreter validation messages: " + (interpreter.ValidationMessages.Count == 0 ? "None" : string.Empty));
          foreach (var msg in interpreter.ValidationMessages)
          {
            messages.AppendLine("// " + msg.Message);
          }
          if (interpreter.ValidationMessages.Where(msg => msg.MessageType == InterpreterMessageType.Error).FirstOrDefault() != null)
          {
            messages.AppendLine("// Errors found, exiting ...");
            messages.AppendLine();
            messages.AppendLine(prevInput);
          }
          else
          {
            if (!interpreter.ExecuteRewriteStep()) messages.AppendLine("// No further rewriting took place");
            messages.AppendLine();
            messages.AppendLine(interpreter.GetCurrentRewriteResult().ProgramOut.ToSourceCode());
          }
        }
        else
        {
          messages.AppendLine("// Parsing failed:\n// " + ast.ErrorMessage);
          var errPos = ast.StartPosition >= ast.SourceTokens.Count ? ast.SourceTokens.Count - 1 : ast.StartPosition;
          messages.AppendLine("// Input is: " + GetErrorLine(tokenResult.Tokens[errPos].From).Trim());
          messages.AppendLine();
          messages.AppendLine(prevInput);
        }
      }
      else
      {
        messages.AppendLine("// Tokenization failed:\n// " + tokenResult.ErrorMessage);
        messages.AppendLine();
        messages.AppendLine(prevInput);
      }
      code.ProgramBlock = messages.ToString();
    }    
  }
}
