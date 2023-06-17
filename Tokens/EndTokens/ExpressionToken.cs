using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;
using TinyBasic.Tokens.CharTokens;
using TinyBasic.Tokens.IntermTokens;

namespace TinyBasic.Tokens.EndTokens
{
    internal class ExpressionToken : IToken, IExprListArg, IFactorArg
	{
        public SignedTerm FirstTerm { get; set; }
        public List<SignedTerm> Terms { get; set; } = new();
        internal record SignedTerm
        {
            public PlusMinusToken Sign { get; set; }
            public TermToken Term { get; set; }
        }
    }

    internal class ExprListToken : IToken
    {
        public IExprListArg FirstExpression { get; set; }
        public List<(CommaToken, IExprListArg)> Values { get; set; } = new();
    }
}
