using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.EndTokens
{
    internal class StatementToken : IToken
    {
        public StatementType Type { get; set; }
        public List<IToken> Tokens { get; set; } = new();

		public override string ToString()
		{
            return string.Format("StatementToken: {0}", Type);
		}
	}

    internal enum StatementType
    {
        Print,
        If,
        Goto,
        Input,
        Let,
        Gosub,
        Return,
        List,
        Run,
        Rem,
        End,
    };
}
