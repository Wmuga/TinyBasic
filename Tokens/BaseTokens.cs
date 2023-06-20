using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyBasic.Tokens.BaseTokens
{
	internal interface IToken { }

	internal interface IExprListArg { }

	internal interface IFactorArg { }

	internal interface IRelopArg 
	{
		public IRelopArg? GetNext();
	}

	internal interface ILessSignNext { }
	internal interface IGreatSignNext { }
}
