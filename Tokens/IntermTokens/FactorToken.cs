using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.IntermTokens
{
    internal class FactorToken : IToken
	{
		public IFactorArg Factor { get; set; }

		public override string ToString()
		{
			return "FactorToken";
		}
	}
}
