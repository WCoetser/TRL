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
using Parser.AbstractSyntaxTree.Keywords;
using Interpreter.Entities.Keywords;
using Interpreter.Entities.Terms;
using Interpreter.Entities.TypeDefinitions;
using Parser.AbstractSyntaxTree.Terms;
using Parser.AbstractSyntaxTree.TypeDefinitions;

namespace Interpreter.Entities
{
  /// <summary>
  /// Converts parse results into entity classes for interpreter. The mappings between AST and TRS classes are not always 1-to-1.
  /// </summary>
  public static class AstToTrsConverterExtensions
  {
    public static TrsNativeKeyword Convert(this AstNativeKeyword astIn)
    {
      return new TrsNativeKeyword(astIn);
    }

    public static TrsConstant Convert(this AstConstant astIn)
    {
      var token = astIn.AtomName;
      return new TrsConstant(token.TokenString, astIn);
    }

    public static TrsNumber Convert(this AstNumber astIn)
    {
      var token = astIn.Number;
      return new TrsNumber(token.TokenString, astIn);
    }

    public static TrsString Convert(this AstString astIn)
    {
      var token = astIn.StringContent;
      return new TrsString(token.TokenString, astIn);
    }

    public static TrsTermProduct Convert(this AstTermProduct astIn)
    {
      List<TrsTermBase> termList = new List<TrsTermBase>(astIn.TermList.Select(t => ConvertAstTermBase(t)).Cast<TrsTermBase>());
      var trsTermProduct = new TrsTermProduct(termList, astIn);
      return trsTermProduct;
    }

    public static TrsTermBase ConvertAstTermBase(AstTermBase arg)
    {
      if (arg is AstConstant) return ((AstConstant)arg).Convert();
      else if (arg is AstVariable) return ((AstVariable)arg).Convert();
      else if (arg is AstString) return ((AstString)arg).Convert();
      else if (arg is AstNumber) return ((AstNumber)arg).Convert();
      else if (arg is AstNativeKeyword) return ((AstNativeKeyword)arg).Convert();
      else if (arg is AstTermProduct) return ((AstTermProduct)arg).Convert();
      else if (arg is AstTerm) return ((AstTerm)arg).Convert();
      else if (arg is AstAcTerm) return ((AstAcTerm)arg).Convert();
      else throw new ArgumentException("Unexpected type: " + arg.GetType().FullName);
    }

    private static TrsTypeDefinitionTermBase ConvertAstTypeDefinitionTermBase(AstTypeDefinitionTermBase astIn)
    {
      if (astIn is AstTypeDefinitionAtom) return ((AstTypeDefinitionAtom)astIn).Convert();
      else if (astIn is AstTypeDefinitionVariable) return ((AstTypeDefinitionVariable)astIn).Convert();
      else if (astIn is AstTypeDefinitionTerm) return ((AstTypeDefinitionTerm)astIn).Convert();
      else if (astIn is AstTypeDefinitionAcTerm) return ((AstTypeDefinitionAcTerm)astIn).Convert();
      else if (astIn is AstTypeDefinitionName) return ((AstTypeDefinitionName)astIn).Convert();
      else throw new ArgumentException("Found unexpected AST type: " + astIn.GetType().FullName);
    }

    public static TrsAcTerm Convert(this AstAcTerm astIn)
    {
      var tokenTermName = astIn.TermName;
      return new TrsAcTerm(tokenTermName.TokenString,
                          astIn.TermArguments.Arguments.Select(astTermArg => ConvertAstTermBase(astTermArg)), astIn);
    }

    public static TrsTypeDefinitionAcTerm Convert(this AstTypeDefinitionAcTerm astIn)
    {
      return new TrsTypeDefinitionAcTerm(astIn.TermName.TokenString, astIn.SubTypeArgumentNames.
        Select(arg => arg.Convert()).Cast<TrsTypeDefinitionTermBase>().ToList(), astIn);
    }
    
    public static TrsTypeDefinitionTerm Convert(this AstTypeDefinitionTerm astIn)
    {
      return new TrsTypeDefinitionTerm(astIn.TermName.TokenString, astIn.SubTypeArgumentNames.
        Select(arg => arg.Convert()).Cast<TrsTypeDefinitionTermBase>().ToList(), astIn);
    }

    public static TrsTypeDefinitionVariable Convert(this AstTypeDefinitionVariable astIn)
    {
      return new TrsTypeDefinitionVariable(astIn.VariableName.TokenString, astIn);
    }

    public static TrsTerm Convert(this AstTerm astIn) 
    {
      var tokenTermName = astIn.TermName;
      return new TrsTerm(tokenTermName.TokenString,
                        astIn.TermArguments.Arguments.Select(astTermArg => ConvertAstTermBase(astTermArg)), astIn);
    }

    public static TrsVariable Convert(this AstVariable astIn)
    {
      var token = astIn.VariableName;
      return new TrsVariable(token.TokenString, astIn);
    }

    public static TrsReductionRule Convert(this AstReductionRule astIn) 
    {
      var head = ConvertAstTermBase(astIn.Head);
      var tail = ConvertAstTermBase(astIn.Tail);
      return new TrsReductionRule((TrsTermBase)head, (TrsTermBase)tail, astIn);
    }

    public static TrsProgramBlock Convert(this AstProgramBlock astIn)
    {
      var trsProgramBlock = new TrsProgramBlock(astIn);
      foreach (var statement in astIn.Statements)
      {
        if (statement is AstTermBase) trsProgramBlock.Statements.Add(ConvertAstTermBase((AstTermBase)statement));
        else if (statement is AstReductionRule) trsProgramBlock.Statements.Add(((AstReductionRule)statement).Convert());
        else if (statement is AstTypeDefinitionStatement) trsProgramBlock.Statements.Add(((AstTypeDefinitionStatement)statement).Convert());
        else if (statement is AstLimitStatement) trsProgramBlock.Statements.Add(((AstLimitStatement)statement).Convert());
        else throw new ArgumentException("Enexpected AST type: " + statement.GetType().FullName);
      }
      return trsProgramBlock;
    }

    public static TrsTypeDefinitionTypeName Convert(this AstTypeDefinitionName astIn)
    {
      return new TrsTypeDefinitionTypeName(astIn.TermName.TokenString, astIn);
    }

    public static TrsTypeDefinitionAtom Convert(this AstTypeDefinitionAtom astIn)
    {
      if (astIn is AstTypeDefinitionConstant) return new TrsTypeDefinitionConstant(astIn.SourceToken.TokenString, astIn);
      else if (astIn is AstTypeDefinitionNumber) return new TrsTypeDefinitionNumber(astIn.SourceToken.TokenString, astIn);
      else if (astIn is AstTypeDefinitionString) return new TrsTypeDefinitionString(astIn.SourceToken.TokenString, astIn);
      else throw new ArgumentException("Unexpected type: " + astIn.GetType().FullName);
    }

    public static TrsTypeDefinition Convert(this AstTypeDefinitionStatement astIn)
    {
      return new TrsTypeDefinition(astIn.TypeName.Convert(), astIn.CandidateTerms.Select(condidate => ConvertAstTypeDefinitionTermBase(condidate)).ToList(), astIn);
    }

    public static TrsLimitStatement Convert(this AstLimitStatement astIn)
    {
      return new TrsLimitStatement(astIn.Variables.Select(v => v.Convert()).ToList(), astIn.TypeName.Convert(), astIn);
    }
  }
}
