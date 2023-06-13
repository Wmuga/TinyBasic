using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.EndTokens
{
	internal class SpecialWordToken : IToken
	{
		public SpecialWord Value { get; set; }

		public override string ToString()
		{
			return string.Format("SpecialWordToken: {0}", Value);
		}
		public enum SpecialWord
		{
			Then,
			Else
		}
	}
}
