using System;
using System.Collections.Generic;
using System.Text;
using DotNetWeb.Core.Expressions;

namespace DotNetWeb.Core.Statements
{
    public class InsideIf : Statement
    {
        public InsideIf(TypedExpression expression)
        {
            Expression = expression;
        }

        public TypedExpression Expression { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
          //  code += $"<div>{Expression.Generate()}</div>{Environment.NewLine}";
            code += $"<div>{Expression.Evaluate()}</div>{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
           
        }

        public override void ValidateSemantic()
        {
        }
    }
}
