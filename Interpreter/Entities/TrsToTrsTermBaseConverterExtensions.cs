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
using Interpreter.Entities.TypeDefinitions;
using Interpreter.Entities.Terms;
using Parser.AbstractSyntaxTree.TypeDefinitions;

namespace Interpreter.Entities
{
  /// <summary>
  /// Converts terms to their corresponding type definitions. This is 
  /// for type checking with the type checker class.
  /// </summary>
  public static class TrsToTrsTermBaseConverterExtensions
  {
    public static TrsTypeDefinitionAtom Convert(this TrsAtom atom)
    {
      TrsConstant cons = atom as TrsConstant;
      TrsString str = atom as TrsString;
      TrsNumber num = atom as TrsNumber;
      if (cons != null) return new TrsTypeDefinitionConstant(cons.Value);
      else if (str != null) return new TrsTypeDefinitionString(str.Value);
      else if (num != null) return new TrsTypeDefinitionNumber(num.Value);
      else throw new Exception("Unexpected type: " + atom.GetType().FullName);
    }

    public static TrsTypeDefinitionVariable Convert(this TrsVariable variable) 
    {
      return new TrsTypeDefinitionVariable(variable.Name);
    }

    public static TrsTypeDefinitionTerm Convert(this TrsTerm term) 
    {
      return new TrsTypeDefinitionTerm(term.Name, 
        term.Arguments.Select(arg => arg.Convert()).ToList());
    }

    public static TrsTypeDefinitionAcTerm Convert(this TrsAcTerm term)
    {
      return new TrsTypeDefinitionAcTerm(term.Name,
        term.OnfArguments.Select(arg => new TrsTypeDefinitionOnfAcTermArgument 
        { 
          Cardinality = arg.Cardinality, 
          Term = arg.Term.Convert() 
        }));
    }

    public static TrsTypeDefinitionTermBase Convert(this TrsTermBase termIn)
    {
      TrsAtom atom = termIn as TrsAtom;
      TrsVariable variable = termIn as TrsVariable;
      TrsTerm term = termIn as TrsTerm;
      TrsAcTerm acTerm = termIn as TrsAcTerm;
      if (atom != null) return atom.Convert();
      else if (variable != null) return variable.Convert();
      else if (term != null) return term.Convert();
      else if (acTerm != null) return acTerm.Convert();
      else throw new Exception("Unexpected type: " + termIn.GetType().FullName);
    }
  }
}
