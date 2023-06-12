using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.IntermTokens
{
    internal class NumberToken : IToken, IFactorArg
    {
        public int Value { get; set; }
    }
}
