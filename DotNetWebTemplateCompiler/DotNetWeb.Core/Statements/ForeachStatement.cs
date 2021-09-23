using DotNetWeb.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core.Statements
{
    public class ForeachStatement : Statement
    {
        public ForeachStatement(List<Token> Token1, Statement statement)
        {
            token1 = Token1;
            Statement = statement;
        }

        public List<Token> token1 { get; }

        public Statement Statement { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(1);

            foreach (var result in token1)
            {
                code += $"<li>{result.Lexeme}{Environment.NewLine}</li>";
            }
            //code += $"<div>{token1}{Environment.NewLine}</div>";
            //   code += $"{token2}{Environment.NewLine}";
            code += $"{Statement.Generate(tabs)}{Environment.NewLine}";
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
