using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.IntermTokens
{
	internal class WordToken : IToken
	{
		public string Value { get; set; } = string.Empty;

		public override string ToString()
		{
			return string.Format("WordToken: {0}", Value);
		}
	}
}
