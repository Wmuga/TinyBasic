using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;
using TinyBasic.Tokens.CharTokens;
using TinyBasic.Tokens.EndTokens;
using TinyBasic.Tokens.IntermTokens;
using static TinyBasic.Tokens.EndTokens.ExpressionToken;

namespace TinyBasic.VirtualMachine
{
	partial class CodeRunner
	{
		// private int _accInt;
		// private string _accStr = string.Empty;

		private int EvalExpressionToken(ExpressionToken expr)
		{
			int sum = 0;
			foreach (var token in expr.GetTerms())
			{
				if (token.Sign.Type == PlusMinusType.Plus)
				{
					sum += EvalTermToken(token.Term);
					continue;
				}
				sum -= EvalTermToken(token.Term);
			}
			return sum;
		}

		private int EvalTermToken(TermToken term)
		{
			int res = 1;
			foreach (var token in term.GetFactors())
			{
				if (token.Item1.Type == MulDivType.Multiply)
				{
					res *= EvalFactorToken(token.Item2);
					continue;
				}
				res /= EvalFactorToken(token.Item2);
			}
			return res;
		}

		private int EvalFactorToken(FactorToken token)
		{
			var factor = token.Factor;
			if (factor is ExpressionToken expr) return EvalExpressionToken(expr);
			if (factor is VarToken var) return _variables[var.Name];
			if (factor is NumberToken numb) return numb.Value;
			throw new InvalidProgramException("Empty factor");
		}

		private bool EvalLogExprToken(LogicalExpressionToken token)
		{
			var first = EvalExpressionToken(token.First);
			var second = EvalExpressionToken(token.Second);
			var relopVal = token.Relop.Value;
			if (relopVal is LesserSingToken lst)
			{
				if (lst.Next is EqSignToken)
				{
					return first <= second;
				}
				if (lst.Next is GreaterSignToken)
				{
					return first != second;
				}
				return first < second;

			}
			
			if(relopVal is GreaterSignToken gst)
			{
				if (gst.Next is EqSignToken)
				{
					return first >= second;
				}
				if (gst.Next is LesserSingToken)
				{
					return first != second;
				}
				return first > second;
			}

			return first == second;
		}
	}

	internal static class TokenExtensions
	{
		public static IEnumerable<SignedTerm> GetTerms(this ExpressionToken expr)
		{
			yield return expr.FirstTerm;
			foreach(var token in expr.Terms) 
				yield return token;
		}

		public static IEnumerable<VarToken> GetVars(this VarListToken vars)
		{
			yield return vars.FirstVar;
			foreach (var token in vars.Values)
			{
				if (token.Item1.Value != CommaToken.CommaType.Comma)
				{
					throw new InvalidProgramException("Variables should be comma separated");
				}
				yield return token.Item2;
			}
		}

		public static IEnumerable<(MulDivToken,FactorToken)> GetFactors(this TermToken term)
		{
			yield return (mulSign,term.FirstFactor);
			foreach (var token in term.Values)
				yield return token;
		}

		private static readonly MulDivToken mulSign = new() { Type = MulDivType.Multiply };
	}
}
