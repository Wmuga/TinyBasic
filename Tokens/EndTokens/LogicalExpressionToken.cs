using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;
using TinyBasic.Tokens.IntermTokens;

namespace TinyBasic.Tokens.EndTokens
{
	internal class LogicalExpressionToken : IToken
	{
		public ExpressionToken First { get; set; }
		public ExpressionToken Second { get; set; }
		public RelopToken Relop { get; set; }
	}
}
