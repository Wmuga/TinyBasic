using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;
using TinyBasic.Tokens.CharTokens;

namespace TinyBasic.Tokens.IntermTokens
{
    internal class TermToken : IToken
	{
		public FactorToken FirstFactor { get; set; }
		public List<(MulDivToken, FactorToken)> Values { get; set; } = new();
	}
}
