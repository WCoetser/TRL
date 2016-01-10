using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Parser.AbstractSyntaxTree.TypeDefinitions;
using Interpreter.Entities.Terms;
using System.Diagnostics;

namespace Interpreter.Entities.TypeDefinitions
{
  public class TrsLimitStatement : TrsStatement
  {
    public TrsTypeDefinitionTypeName TypeDefinition { get; private set; }

    public List<TrsVariable> Variables { get; private set; }

    public TrsLimitStatement(List<TrsVariable> variables, TrsTypeDefinitionTypeName typeDefinition, AstLimitStatement astSource = null)
    {
      AstSource = astSource;
      TypeDefinition = typeDefinition;
      Variables = variables;
    }

    public override bool Equals(object other)
    {
      var typedOther = other as TrsLimitStatement;
      if (typedOther == null
        || !typedOther.TypeDefinition.Equals(this.TypeDefinition)) return false;
      var otherVars = new HashSet<TrsVariable>(typedOther.Variables);
      var thisVars = new HashSet<TrsVariable>(this.Variables);
      otherVars.IntersectWith(thisVars);
      if (otherVars.Count != thisVars.Count) return false;
      else return true;      
    }

    public override int GetHashCode()
    {
      var retVal = TypeDefinition.TypeName.GetHashCode();
      var variableList = new HashSet<TrsVariable>(Variables);
      foreach (var variable in variableList) retVal = retVal ^ variable.GetHashCode();
      return retVal;
    }

    public override TrsStatement CreateCopy()
    {
      return new TrsLimitStatement(Variables.Select(var => (TrsVariable)var.CreateCopy()).ToList(),
        (TrsTypeDefinitionTypeName)TypeDefinition.CreateCopy(),(AstLimitStatement)AstSource);
    }

    public override string ToSourceCode()
    {
      StringBuilder retVal = new StringBuilder();
      retVal.Append(Parser.Tokenization.Keywords.Limit);
      retVal.Append(" ");
      retVal.Append(Variables.First().ToSourceCode());
      foreach (var variable in Variables.Skip(1))
      {
        retVal.Append(",");
        retVal.Append(variable.ToSourceCode());
      }
      retVal.Append(" ");
      retVal.Append(Parser.Tokenization.Keywords.To);
      retVal.Append(" ");
      retVal.Append(TypeDefinition.ToSourceCode());
      return retVal.ToString(); 
    }
  }
}
