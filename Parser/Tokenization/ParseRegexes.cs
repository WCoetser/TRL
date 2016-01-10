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
using System.Text.RegularExpressions;

namespace Parser.Tokenization
{
  /// <summary>
  /// Regular expressions for lookahead and content extraction.
  /// 
  /// Note: Use first regex group to select actual parsed content
  /// </summary>
  public static class ParseRegexes
  {
    public static Regex LookAheadConstAtomFormat = new Regex(@"[\s]*([a-zA-Z_]\w*)[\s]*");
    public static Regex LookAheadStringFormat = new Regex(@"[\s]*\""([^\""]*)\""[\s]*");
    public static Regex LookAheadNumberFormat = new Regex(@"[\s]*([+-]?[0-9]+[\.]?[0-9]*)[\s]*");
    public static Regex LookAheadOpenTerm = new Regex(@"[\s]*([a-zA-Z_]\w*)[\s]*" + Regex.Escape("(") + @"[\s]*");
    public static Regex LookAheadOpenAcTerm = new Regex(@"[\s]*([a-zA-Z_]\w*)[\s]*" + Regex.Escape("[") + @"[\s]*");
    public static Regex LookAheadComma = new Regex(@"[\s]*(,)[\s]*");
    public static Regex LookAheadCloseTerm = new Regex(@"[\s]*(" + Regex.Escape(")") + @")[\s]*");
    public static Regex LookAheadArrow = new Regex(@"[\s]*(=>)[\s]*");
    public static Regex LookAheadSemicolon = new Regex(@"[\s]*(;)[\s]*");
    public static Regex LookAheadSingleLineComment = new Regex(@"[\s]*//[^\r\n]*[\s]*");
    public static Regex LookAheadOpenTermProduct = new Regex(@"[\s]*(\[)[\s]*");
    public static Regex LookAheadCloseTermProduct = new Regex(@"[\s]*(\])[\s]*"); // NB: Also used for close AC-term
    public static Regex LookAheadEquals = new Regex(@"[\s]*(=)[\s]*");
    public static Regex LookAheadPipe = new Regex(@"[\s]*(\|)[\s]*");    

    // Types & Variables
    public static Regex LookAheadVariableFormat = new Regex(@"[\s]*\:(\w+)[\s]*");
    public static Regex LookAheadTypeName = new Regex(@"[\s]*\$(\w+)[\s]*");

    // Keywords
    public static Regex LookAheadType = new Regex(@"[\s]*(" + Keywords.Type + @")[\s]*");    
    public static Regex LookAheadNative = new Regex(@"[\s]*(" + Keywords.Native + @")[\s]*");
    public static Regex LookAheadLimit = new Regex(@"[\s]*"+ Keywords.Limit + @"[\s]*");
    public static Regex LookAheadTo = new Regex(@"[\s]*"+ Keywords.To + @"[\s]*");
    
    public static Tuple<Regex, TokenType>[] GetRegexToTokenTypeMappings()
    {
      // NB: Sequence is important here not to confuse atom and term start, "type" and atom constant.
      // NB: Arrow must come before Equals
      // NB: Keywords before constants
      return new[] 
      {
        // Keywords
        new Tuple<Regex, TokenType>(LookAheadNative, TokenType.NativeFunction),
        new Tuple<Regex, TokenType>(LookAheadType, TokenType.TypeDecleration),
        new Tuple<Regex, TokenType>(LookAheadLimit, TokenType.Limit),
        new Tuple<Regex, TokenType>(LookAheadTo, TokenType.To),
        // The rest
        new Tuple<Regex, TokenType>(LookAheadTypeName, TokenType.TypeName),
        new Tuple<Regex, TokenType>(LookAheadPipe, TokenType.Pipe),        
        new Tuple<Regex, TokenType>(LookAheadVariableFormat, TokenType.Variable),
        new Tuple<Regex, TokenType>(LookAheadOpenTerm, TokenType.StartTerm),        
        new Tuple<Regex, TokenType>(LookAheadOpenAcTerm, TokenType.StartAcTerm),
        new Tuple<Regex, TokenType>(LookAheadConstAtomFormat, TokenType.Atom),
        new Tuple<Regex, TokenType>(LookAheadStringFormat, TokenType.String),
        new Tuple<Regex, TokenType>(LookAheadNumberFormat, TokenType.Number),
        new Tuple<Regex, TokenType>(LookAheadCloseTerm, TokenType.EndTerm),
        new Tuple<Regex, TokenType>(LookAheadComma, TokenType.Comma),
        new Tuple<Regex, TokenType>(LookAheadArrow, TokenType.Arrow),
        new Tuple<Regex, TokenType>(LookAheadEquals, TokenType.Equals),
        new Tuple<Regex, TokenType>(LookAheadSemicolon, TokenType.Semicolon),
        new Tuple<Regex, TokenType>(LookAheadSingleLineComment, TokenType.SingleLineComment),
        new Tuple<Regex, TokenType>(LookAheadOpenTermProduct, TokenType.OpenTermProduct),
        new Tuple<Regex, TokenType>(LookAheadCloseTermProduct, TokenType.CloseTermProduct)
      };
    }
  }
}
