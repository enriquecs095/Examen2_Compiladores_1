using DotNetWeb.Core;
using DotNetWeb.Core.Expressions;
using DotNetWeb.Core.Interfaces;
using Type = DotNetWeb.Core.Type;

using System;
using DotNetWeb.Core.Statements;
using System.Collections.Generic;
using System.IO;

namespace DotNetWeb.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner scanner;
        private Token lookAhead;
        List<Token> listdata;
        List<VariableType> listVariables = new List<VariableType>();
        List<InsideFor> listDataFor = new List<InsideFor>();
        List<Token> listToken = new List<Token>();

        Token actualVariable;
        Token VariableList;
        public Parser(IScanner scanner)
        {
            this.scanner = scanner;
            this.Move();
        }
        public void Parse()
        {
           
            var result=Program();
            result.ValidateSemantic();///valido la semantica
           // result.Interpret();
            var result2= result.Generate(1);
            var path = "C:\\Users\\enrik\\Documents\\compiladores 1\\examen 2\\DotNetWebTemplateCompiler\\result.html";
            File.WriteAllText(path, result2);
        }

        private Statement Program()
        {
            Init();
            var result = Template();
            return result;
            //    return new SequenceStatement(Init(), Template());
        }

        private Statement Template()
        {
           return  new SequenceStatement(Tag(),InnerTemplate());
        }
        
        private Statement InnerTemplate()
        {
            if (this.lookAhead.TokenType == TokenType.LessThan)
            {
                return Template();
            }

            return null;
        }
        private Statement Tag()
        {
            Match(TokenType.LessThan);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
           var result= Stmts();
            Match(TokenType.LessThan);
            Match(TokenType.Slash);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            return result;
        }

        private Statement Stmts()
        {
            if (this.lookAhead.TokenType == TokenType.OpenBrace)
            {
                return new SequenceStatement(Stmt(), Stmts());
               // Stmt();
                //Stmts();
            }
            return null;
        }

        private Statement Stmt()
        {
            if (lookAhead.TokenType == TokenType.OpenBrace)
            {
                Match(TokenType.OpenBrace);////
            }
            switch (this.lookAhead.TokenType)
            {

                case TokenType.OpenBrace:
                    Match(TokenType.OpenBrace);
                    var resultValue=Eq();
                    Match(TokenType.CloseBrace);
                    Match(TokenType.CloseBrace);
                    return new InsideIf(resultValue as TypedExpression);
                   
                case TokenType.Percentage:
                //    var expression = IfStmt();
                    Match(TokenType.Percentage);
                    Match(TokenType.IfKeyword);
                    var result = Eq();
                    Match(TokenType.Percentage);
                    Match(TokenType.CloseBrace);
                     var statement = Template();////
                    Match(TokenType.OpenBrace);
                    Match(TokenType.Percentage);
                    Match(TokenType.EndIfKeyword);
                    Match(TokenType.Percentage);
                    Match(TokenType.CloseBrace);
                 //   var statement = Stmt(); ////aqui
                    return new IfStatement(result as TypedExpression, statement);

                case TokenType.Hyphen:
                    //var forEach= ForeachStatement();
                    Match(TokenType.Hyphen);
                    Match(TokenType.Percentage);
                    Match(TokenType.ForEeachKeyword);
                    var token1 = lookAhead;//la variable
                    Match(TokenType.Identifier);
                    Match(TokenType.InKeyword);
                    var token2 = lookAhead;//el array
                    var  id = new Id(token2, Type.Int);
                    var symbol = Environment_Variables.GetSymbol(token2.Lexeme);
                    Match(TokenType.Identifier);
                    Match(TokenType.Percentage);
                    Match(TokenType.CloseBrace);
                    Environment_Variables.AddVariable(token1.Lexeme, id);
                    var resultTemplateFor= Template();////
                    Match(TokenType.OpenBrace);
                    Match(TokenType.Percentage);
                    Match(TokenType.EndForEachKeyword);
                    Match(TokenType.Percentage);
                    Match(TokenType.CloseBrace);
                    foreach (var data in listDataFor) {
                        listToken.Add(data.dataVariable);
                    }
                    return new ForeachStatement(listToken, resultTemplateFor);
                //    return null;
                    //break;

                default:
                    throw new ApplicationException("Unrecognized statement");
            }
          

        }
  

        private Expression Eq()
        {
            var result= Rel();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.NotEqual)
            {
                var token = lookAhead;
                Move();

                result = new RelationalExpression(token, result as TypedExpression, Rel() as TypedExpression);
            }
            return result;
        }

        private Expression Rel()
        {
           var result= Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan)
            {
                var token=lookAhead;
                Move();
               // Expr();
                result= new RelationalExpression(token, result as TypedExpression, Expr() as TypedExpression);
            }
            return result;
        }

        private Expression Expr()
        {
            var result = Term();
            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Hyphen)
            {
                var token = lookAhead;
                Move();
                //Term();
                result = new ArithmeticOperator(token, result as TypedExpression, Term() as TypedExpression);
            }
            return result;
        }

        private Expression Term()
        {
           var result= Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Slash)
            {
                var token = lookAhead;
                Move();
                Factor();
                result = new ArithmeticOperator(token, result as TypedExpression, Factor() as TypedExpression);
            }
            return result;
        }

        private Expression Factor()
        {
           
            switch (this.lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        var resultEq=Eq();
                        Match(TokenType.RightParens);
                        return resultEq;
                    }
             
                case TokenType.IntConstant:

                    Environment_Variables.UpdateVariable(actualVariable.Lexeme, lookAhead.Lexeme);
                    var constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                   // break;
                case TokenType.FloatConstant:
     
                    Environment_Variables.UpdateVariable(actualVariable.Lexeme, lookAhead.Lexeme);
                    constant = new Constant(lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                //    break;
                case TokenType.StringConstant:
                    Environment_Variables.UpdateVariable(actualVariable.Lexeme, lookAhead.Lexeme);
                    constant = new Constant(lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                //    break;
                case TokenType.OpenBracket:
                    Match(TokenType.OpenBracket);
                    var resultExpreList = ExprList();
                    Match(TokenType.CloseBracket);
                    return resultExpreList;
                  //  break;
                default:
                    var symbol = Environment_Variables.GetSymbol(lookAhead.Lexeme);
                    Match(TokenType.Identifier);
                    return symbol.Id;
                  //  break;
            }

        }



        private Expression ExprList()
        {
           var result= Eq();
            var resultFind3 = listVariables.Find(x => x.variable == VariableList.Lexeme);
            if (resultFind3 != null)
            {
                listDataFor.Add(new InsideFor { Namevariable = VariableList, dataVariable = result.Token });
            }
            if (this.lookAhead.TokenType != TokenType.Comma)
            {
                return null;
            }
            Match(TokenType.Comma);
            return ExprList();
        }

        private void Init()
        {
            Environment_Variables.PushContext();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.InitKeyword);
            Code();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
        }

        private void Code()
        {
            Decls();
            Assignations();
        }

        private Statement Assignations()
        {
            if (this.lookAhead.TokenType == TokenType.Identifier)
            {
                var symbol = Environment_Variables.GetSymbol(this.lookAhead.Lexeme);
                return new SequenceStatement(Assignation(symbol.Id), Assignations());
            }
            return null;
        }

        private Statement Assignation(Id id)
        {
            actualVariable = lookAhead;
            Match(TokenType.Identifier);
            Match(TokenType.Assignation);
            var expression=Eq();
            Match(TokenType.SemiColon);
            return new AssignationStatement(id, expression as TypedExpression);///////////////////
        }

        private void Decls()
        {
            Decl();
            InnerDecls();
        }

        private void InnerDecls()
        {
            if (this.LookAheadIsType())
            {
                Decls();
            }
        }

        private void Decl()
        {
            Id id;
            switch (this.lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = lookAhead;
                    VariableList = lookAhead;
                    Match(TokenType.Identifier);
                    var isInitialize = lookAhead;
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Float);
                    Environment_Variables.AddVariable(token.Lexeme, id);
 
                    break;
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    token = lookAhead;
                    VariableList = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.String);
                    Environment_Variables.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.IntKeyword:
                    Match(TokenType.IntKeyword);
                    token = lookAhead;
                    VariableList = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Int);
                    Environment_Variables.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.FloatListKeyword:
                    Match(TokenType.FloatListKeyword);
                    token = lookAhead;
                    VariableList = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    this.listVariables.Add(new VariableType { variable = token.Lexeme, typeVariable = TokenType.FloatConstant });
                    id = new Id(token, Type.Float);
                    Environment_Variables.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.IntListKeyword:
                    Match(TokenType.IntListKeyword);
                    token = lookAhead;
                    VariableList = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    this.listVariables.Add(new VariableType { variable = token.Lexeme, typeVariable = TokenType.IntConstant });
                    id = new Id(token, Type.Int);
                    Environment_Variables.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.StringListKeyword:
                    Match(TokenType.StringListKeyword);
                    token = lookAhead;
                    VariableList = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    this.listVariables.Add(new VariableType { variable = token.Lexeme, typeVariable = TokenType.StringConstant });
                    id = new Id(token, Type.String);
                    Environment_Variables.AddVariable(token.Lexeme, id);
                    break;
                default:
                    throw new ApplicationException($"Unsupported type {this.lookAhead.Lexeme}");
            }
        }

        private void Move()
        {
            this.lookAhead = this.scanner.GetNextToken();
        }

        private void Match(TokenType tokenType)
        {
            if (this.lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this.lookAhead.TokenType}. Line: {this.lookAhead.Line}, Column: {this.lookAhead.Column}");
            }
            this.Move();
        }



        private void Message() {

            throw new ApplicationException("Error en asignacion de datos en variable");

        }
        private bool LookAheadIsType()
        {
            return this.lookAhead.TokenType == TokenType.IntKeyword ||
                this.lookAhead.TokenType == TokenType.StringKeyword ||
                this.lookAhead.TokenType == TokenType.FloatKeyword ||
                this.lookAhead.TokenType == TokenType.IntListKeyword ||
                this.lookAhead.TokenType == TokenType.FloatListKeyword ||
                this.lookAhead.TokenType == TokenType.StringListKeyword;

        }
    }
}
