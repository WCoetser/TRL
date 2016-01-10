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
using Parser.Tokenization;
using Interpreter.Entities.Terms;
using Parser.Grammer;
using Interpreter.Entities;
using Parser.AbstractSyntaxTree.Terms;

namespace TestAcUnification
{
  class Program
  {
    static TrsTermBase GetTerm() 
    {
      var tIn = Console.ReadLine();
      TermParser parser = new TermParser();
      var parseResult = parser.Parse(tIn);
      if (!parseResult.Succeed) throw new Exception("Failed to parse string: " + tIn);
      return AstToTrsConverterExtensions.ConvertAstTermBase((AstTermBase)parseResult.AstResult);
    }

    static void Main(string[] args)
    {
      try
      {
        Console.Write("Enter first term: ");
        var lhs = GetTerm();
        Console.Write("Enter second term: ");
        var rhs = GetTerm();
        MguCalculation getMgu = new MguCalculation();
        var unifiers = getMgu.GetUnifier(lhs, rhs);
        if (unifiers.Count == 0) Console.WriteLine("Not Unifiable");
        else 
        {
          Console.WriteLine(string.Format("Found {0} unifiers",unifiers.Count));
          int i = 0;
          foreach (var unifier in unifiers)
          {
            i++;
            Console.WriteLine();            
            Console.WriteLine("Unifier " + i + ":");
            if (unifier.Unifier.Count == 0) Console.WriteLine("Unification succeeded with equal input terms given.");
            else
            {
              foreach (var sub in unifier.Unifier.OrderBy(s => s.Variable.Name)) 
                Console.WriteLine(sub.ToSourceCode());
            }
          }
        }
      }
      catch (Exception ex) 
      {
        Console.WriteLine("Error: " + ex.Message);
      }
      Console.WriteLine();
      Console.WriteLine("Press any key to exit");
      Console.ReadKey();
    }
  }
}
