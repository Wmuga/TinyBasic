using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Shapes;
using TinyBasic.Tokens.BaseTokens;
using TinyBasic.Tokens.CharTokens;
using TinyBasic.Tokens.EndTokens;
using TinyBasic.Tokens.IntermTokens;

namespace TinyBasic.VirtualMachine
{
	partial class CodeRunner
	{
		private static readonly Dictionary<StatementType, string> _statementStrs = EnumToStr<StatementType>();
		private static readonly Dictionary<SpecialWordToken.SpecialWord, string> _spWordsStrs = EnumToStr<SpecialWordToken.SpecialWord>();
		private static readonly Dictionary<VarToken.VariableName, string> _varStrs = EnumToStr<VarToken.VariableName>();
		private static readonly Dictionary<CommaToken.CommaType, char> _commaStr = new()
		{
			{ CommaToken.CommaType.Comma, ',' },
			{ CommaToken.CommaType.Semicolon, ';' },
		};
		private static readonly Dictionary<PlusMinusType, char> _signStr = new()
		{
			{PlusMinusType.Plus, '+' },
			{PlusMinusType.Minus, '-' },
		};
		private static readonly Dictionary<MulDivType, char> _sign2Str = new()
		{
			{ MulDivType.Multiply, '*' },
			{ MulDivType.Division, '/' },
		};

		private void LineToStr(LineToken line)
		{
			StringBuilder sb = new();
			
			sb.Append(line.LineNumber);
			sb.Append(' ');
			
			TokenToStr(line.Statement, sb);
			
			_output(sb.ToString());
		}

		private static void TokenToStr(StatementToken stmt, StringBuilder sb)
		{
			sb.Append(_statementStrs[stmt.Type]);
			sb.Append(' ');
			foreach (var token in stmt.Tokens)
			{
				switch (token)
				{
					case ExpressionToken expr:
						TokenToStr(expr, sb);
						break;
					case ExprListToken exprList:
						TokenToStr(exprList, sb);
						break;
					case LogicalExpressionToken logExpr:
						TokenToStr(logExpr, sb);
						break;
					case SpecialWordToken spWord:
						sb.Append(_spWordsStrs[spWord.Value]);
						sb.Append(' ');
						break;
					case VarListToken varList:
						TokenToStr(varList, sb);
						break;
					case VarToken var:
						TokenToStr(var, sb);
						break;
					case StringToken str:
						TokenToStr(str, sb);
						break;
					case StatementToken stmt1:
						TokenToStr(stmt1, sb);
						break;
					default:
						throw new NotImplementedException();
				}
			}
		}

		private static void TokenToStr(StringToken str, StringBuilder sb)
		{
			sb.Append('\"');
			sb.Append(str.Value);
			sb.Append('\"');
			sb.Append(' ');
		}

		private static void TokenToStr(VarToken var, StringBuilder sb)
		{
			sb.Append(_varStrs[var.Name]);
			sb.Append(' ');
		}

		private static void TokenToStr(VarListToken varList, StringBuilder sb)
		{
			TokenToStr(varList.FirstVar, sb);
			foreach(var token in varList.Values)
			{
				TokenToStr(token.Item1, sb);
				TokenToStr(token.Item2, sb);
			}
		}

		private static void TokenToStr(CommaToken comma, StringBuilder sb)
		{
			sb.Append(_commaStr[comma.Value]);
			sb.Append(' ');
		}

		private static void TokenToStr(LogicalExpressionToken logExpr, StringBuilder sb)
		{
			TokenToStr(logExpr.First, sb);
			TokenToStr(logExpr.Relop, sb);
			TokenToStr(logExpr.Second, sb);
		}

		private static void TokenToStr(RelopToken relop, StringBuilder sb)
		{
			RelopToStr(relop.Value);

			var next = relop.Value.GetNext();
			if (next is not null)
				RelopToStr(next);
			
			sb.Append(' ');
			return;

			void RelopToStr(IRelopArg relop)
			{
				switch (relop)
				{
					case LesserSingToken:
						sb.Append('<');
						break;
					case GreaterSignToken:
						sb.Append('>');
						break;
					case EqSignToken:
						sb.Append('=');
						break;
					default:
						throw new NotImplementedException();
				}
			}
		}

		private static void TokenToStr(ExprListToken exprList, StringBuilder sb)
		{
			ExprListArg(exprList.FirstExpression);

			foreach(var token in exprList.Values)
			{
				TokenToStr(token.Item1, sb);
				ExprListArg(token.Item2);
			}

			void ExprListArg(IExprListArg exprListArg)
			{
				switch(exprListArg)
				{
					case ExpressionToken expr:
						TokenToStr(expr, sb);
						break;
					case StringToken str:
						TokenToStr(str, sb);
						break;
					default:
						throw new NotImplementedException();
				}
			}
		}

		private static void TokenToStr(ExpressionToken expr, StringBuilder sb)
		{
			foreach(var token in expr.GetTerms())
			{
				sb.Append(_signStr[token.Sign.Type]);
				sb.Append(' ');
				TokenToStr(token.Term, sb);
			}
			sb.Append(' ');
		}

		private static void TokenToStr(TermToken term, StringBuilder sb)
		{
			TokenToStr(term.FirstFactor, sb);
			foreach(var token in term.Values)
			{
				sb.Append(' ');
				sb.Append(_sign2Str[token.Item1.Type]);
				sb.Append(' ');
				TokenToStr(token.Item2, sb);
			}
			sb.Append(' ');
		}

		private static void TokenToStr(FactorToken factor, StringBuilder sb)
		{
			switch (factor.Factor)
			{
				case ExpressionToken expr:
					sb.Append('(');
					TokenToStr(expr, sb); 
					sb.Append(')');
					break;
				case VarToken var:
					TokenToStr(var, sb);
					break;
				case NumberToken number:
					sb.Append(number.Value);
					sb.Append(' ');
					break;
				default:
					throw new NotImplementedException();
			}
		}

		private static Dictionary<T, string> EnumToStr<T>() where T : Enum
		{
			var dict = new Dictionary<T, string>();
			foreach(T val in Enum.GetValues(typeof(T)).Cast<T>())
			{
				dict.Add(val, val.ToString().ToUpper());
			}
			return dict;
		}
	}
}
