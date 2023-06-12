using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.IntermTokens
{
    internal class StringToken : IToken, IExprListArg
    {
        public string Value { get; set; } = string.Empty;
    }
}
