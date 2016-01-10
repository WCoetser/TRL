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

namespace Parser.AbstractSyntaxTree.Terms
{
  /// <summary>
  /// Represents a comma seperated list of terms, enclosed in brackets. This list cannot be empty. If it should be empty, use an atom instead
  /// </summary>
  public class AstArgumentList : AstBase
  {
    public List<AstTermBase> Arguments { get; private set; }

    public AstArgumentList(List<AstTermBase> sourceList)
    {
      if (sourceList.Count == 0) throw new ArgumentException("sourceList");
      Arguments = sourceList;
    }

    public override string ToSourceCode()
    {
      StringBuilder retSource = new StringBuilder();      
      retSource.Append(Arguments[0].ToSourceCode());
      foreach (var termSource in Arguments.Skip(1)) retSource.AppendFormat(",{0}", termSource.ToSourceCode());
      return retSource.ToString();
    }
  }
}
