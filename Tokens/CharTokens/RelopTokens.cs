using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.CharTokens
{
	internal class LesserSingToken : IToken, IRelopArg, IGreatSignNext { 
		public ILessSignNext? Next { get; set; }
	}

	internal class GreaterSignToken : IToken, IRelopArg, ILessSignNext {
		public IGreatSignNext? Next { get; set; }
	}
}
