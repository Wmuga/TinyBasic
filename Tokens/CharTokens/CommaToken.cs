using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.CharTokens
{
    internal class CommaToken : IToken
	{
		public CommaType Value { get; set; }
		public enum CommaType
		{
			Comma,
			Semicolon
		}

		public override string ToString()
		{
			return string.Format("CommaToken: {0}", Value == CommaType.Comma ? "," : ";");
		}
	}
}
