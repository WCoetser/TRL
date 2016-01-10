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
using Parser.AbstractSyntaxTree;
using Interpreter.Entities.Terms;
using Interpreter.Entities.TypeDefinitions;

namespace Interpreter.Entities
{
  public class TrsProgramBlock : TrsBase
  {
    public List<TrsStatement> Statements { get; private set; }

    public TrsProgramBlock(AstProgramBlock astSource) : this()
    {
      AstSource = astSource;
    }

    public TrsProgramBlock()
    {
      Statements = new List<TrsStatement>();
    }

    public override string ToSourceCode()
    {
      StringBuilder program = new StringBuilder();
      Action<Type> dumpStatements = delegate(Type typeIn) 
      {
        foreach (var statement in Statements.Where(stm => stm.GetType().Equals(typeIn) 
          || stm.GetType().IsSubclassOf(typeIn)))
        {
          program.Append(statement.ToSourceCode());
          program.AppendLine(";");
        }
        program.AppendLine();
      };
      
      program.Append("// Type definitions");
      if (Statements.Count(stm => stm is TrsTypeDefinition) == 0) program.AppendLine(": None");
      else program.AppendLine();
      dumpStatements(typeof(TrsTypeDefinition));

      program.Append("// Variable mappings");
      if (Statements.Count(stm => stm is TrsLimitStatement) == 0) program.AppendLine(": None");
      else program.AppendLine();
      dumpStatements(typeof(TrsLimitStatement));

      program.Append("// Terms");
      if (Statements.Count(stm => stm is TrsTermBase) == 0) program.AppendLine(": None");
      else program.AppendLine();
      dumpStatements(typeof(TrsTermBase));

      program.Append("// Reduction Rules");
      if (Statements.Count(stm => stm is TrsReductionRule) == 0) program.AppendLine(": None");
      else program.AppendLine();
      dumpStatements(typeof(TrsReductionRule));

      return program.ToString();
    }
  }
}
