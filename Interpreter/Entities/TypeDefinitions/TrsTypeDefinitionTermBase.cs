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

namespace Interpreter.Entities.TypeDefinitions
{
  /// <summary>
  /// Common base class for type definitions.
  /// </summary>
  public abstract class TrsTypeDefinitionTermBase : TrsBase
  {
    public abstract override bool Equals(object other);

    public abstract override int GetHashCode();

    public abstract TrsTypeDefinitionTermBase CreateCopy();

    public abstract IEnumerable<TrsTypeDefinitionTypeName> GetReferencedTypeNames();

    /// <summary>
    /// Creates a copy of this term, replacing the termToReplace with the replacementTerm.
    /// Term equality is defined using Object.ReferenceEquals.
    /// </summary>
    /// <returns></returns>
    public abstract TrsTypeDefinitionTermBase CreateCopyAndReplaceSubTermRefEquals(TrsTypeDefinitionTermBase termToReplace, TrsTypeDefinitionTermBase replacementTerm);

    /// <summary>
    /// This function gets all the strings, numbers and constants from this term, recursively. This is part of the type system.
    /// </summary>
    public abstract List<TrsTypeDefinitionAtom> GetAllAtoms();

    /// <summary>
    /// Gets all the variables in this term and it's subterms.
    /// </summary>
    public abstract List<TrsTypeDefinitionVariable> GetAllVariables();
  }
}
