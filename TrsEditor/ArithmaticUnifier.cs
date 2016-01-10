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
using Interpreter.Execution;
using Interpreter.Entities;
using Parser.Grammer;
using Interpreter.Entities.Terms;
using Parser.AbstractSyntaxTree;

namespace TrsEditor
{
  // Solves simple linear equations equations in one variable, ex. 9 = add(mul(:x, 5), 7)
  public class ArithmaticUnifier : ITrsUnifierCalculation
  {
    const string solver = @"
// Term structure validations
type $var_or_num = $TrsNumber | $TrsVariable;
type $arith = add[$var_or_num, $var_or_num]
            | sub($var_or_num, $var_or_num) 
            | mul[$var_or_num, $var_or_num] 
            | div($TrsVariable, $TrsNumber);

// Map variables to types
limit :x to $TrsVariable;
limit :y,:c to $TrsNumber;
limit :a,:b to $TrsNumber;
limit :exp_l to $TrsNumber;
limit :exp_r to $arith;

[lhs(:exp_l),rhs(:exp_r)] => eq(:exp_l,:exp_r);

// Addition
eq(:y,add[:x,:c]) => eq(sub(:y,:c),:x);

// Subtraction
eq(:y,sub(:x,:c)) => eq(add[:y,:c],:x);
eq(:y,sub(:c,:x)) => eq(add[mul[-1,:y],:c],:x);

// Multiplication
eq(:y,mul[:x,:c]) => eq(div(:y,:c),:x);

// Div
eq(:y,div(:x,:c)) => eq(mul[:y,:c],:x);

// Native functions
add[:a,:b] => native;
sub(:a,:b) => native;
mul[:a,:b] => native;
div(:a,:b) => native;
";


    /// <summary>
    /// Get the resulting interpreter.
    /// </summary>
    Interpreter.Execution.Interpreter interpreter = null;

    public ArithmaticUnifier() {
      ProgramBlockParser parser = new ProgramBlockParser();
      var progIn = parser.Parse(solver);
      interpreter = new Interpreter.Execution.Interpreter(((AstProgramBlock)progIn.AstResult).Convert(), 
        new[] { new ArithmaticFunctions() }.Cast<ITrsNativeFunction>().ToList());
    }

    /// <summary>
    /// NB: this is not a general solution, only for c = b + x where + can be -, / or * and, b and x can be swapped arround.
    /// c is the match term. The rest is the head term. 
    /// </summary>
    public List<UnificationResult> GetUnifier(TrsTermBase termHead, TrsTermBase matchTerm)
    {
      UnificationResult result = new UnificationResult();
      result.Succeed = false;

      // Check input
      Substitution sRhs = new Substitution 
      {
        Variable = new TrsVariable("exp_r"),
        SubstitutionTerm = termHead
      };
      Substitution sLhs = new Substitution
      {
        Variable = new TrsVariable("exp_l"),
        SubstitutionTerm = matchTerm
      };
      var headTerm = termHead as TrsTerm;
      if (!interpreter.TypeChecker.IsSubstitutionValid(sLhs) ||
        !interpreter.TypeChecker.IsSubstitutionValid(sRhs))
      {
        return new List<UnificationResult> { result };
      }

      // Load problem
      interpreter.ClearExecutionCache();
      interpreter.LoadTerms(new [] 
      {
        new TrsTerm("rhs",new [] { termHead }), 
        new TrsTerm("lhs",new [] { matchTerm })
      });

      // Solve
      while (interpreter.ExecuteRewriteStep()) { };

      // Extract answer
      var runResults = interpreter.GetCurrentRewriteResult();
      foreach (var stm in runResults.ProgramOut.Statements)
      {
        var resEq = stm as TrsTerm;
        if (resEq != null
          && resEq.Name == "eq"
          && resEq.Arguments.Count == 2
          && resEq.Arguments[0] is TrsNumber
          && resEq.Arguments[1] is TrsVariable)
        {
          result.Succeed = true;
          result.Unifier = new List<Substitution>();
          result.Unifier.Add(new Substitution()
          {
            Variable = resEq.Arguments[1] as TrsVariable,
            SubstitutionTerm = resEq.Arguments[0]
          });
        }
      }
      return new List<UnificationResult> { result };
    }
  }
}
