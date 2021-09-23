using DotNetWeb.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetWeb.Core
{
    public enum SymbolType
    {
        Variable,
        List,
        Method,
        Library
    }

    public class Symbol
    {
        public Symbol(SymbolType symbolType, Id id, dynamic value)
        {
            SymbolType = symbolType;
            Id = id;
            Value = value;
        }

        public Symbol(SymbolType symbolType, Id id)
        {
            SymbolType = symbolType;
            Id = id;

        }
        /// <summary>
        /// ////
        /// </summary>
        public SymbolType SymbolType { get; }
        public Id Id { get; }
        public dynamic Value { get; set; }
        public Expression Attributes { get; }

    }
}
