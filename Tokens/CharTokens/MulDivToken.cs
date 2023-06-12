using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.CharTokens
{
    internal class MulDivToken : IToken
    {
        public MulDivType Type { get; set; }
    }

    internal enum MulDivType
    {
        Multiply,
        Division,
    }
}
