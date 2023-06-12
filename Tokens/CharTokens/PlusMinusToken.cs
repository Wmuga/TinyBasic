using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.CharTokens
{
    internal class PlusMinusToken : IToken, IGreatSignNext, ILessSignNext
    {
        public PlusMinusType Type { get; set; }
    }

    internal enum PlusMinusType
    {
        None,
        Plus,
        Minus,
    }
}
