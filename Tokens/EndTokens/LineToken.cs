﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;

namespace TinyBasic.Tokens.EndTokens
{
    internal class LineToken : IToken
    {
        public int LineNumber { get; set; }
        public StatementToken Statement { get; set; } = new();

		public override string ToString()
		{
			return "LineToken";
		}
	}
}
