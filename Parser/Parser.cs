using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyBasic.Tokens.BaseTokens;
using TinyBasic.Tokens.CharTokens;
using TinyBasic.Tokens.EndTokens;
using TinyBasic.Tokens.IntermTokens;
using static TinyBasic.Tokens.EndTokens.ExpressionToken;
using StatementType = TinyBasic.Tokens.EndTokens.StatementType;

namespace TinyBasic.Parser
{
	internal class Parser
	{
		private List<LineToken> _lines = new();
		private readonly List<IToken> _tokens;
		private int _position = 0;
		private int _lineNumber = 0;
		private bool _programEnded = false;
		private IToken? _buffer;

		private readonly static PlusMinusToken _plusToken = new PlusMinusToken() { Type = PlusMinusType.Plus };

		public List<LineToken> Parsed { get { return _lines; } }

		public Parser(IEnumerable<IToken> tokens)
		{
			_tokens = tokens.ToList();
			Line();
		}

		private IToken GetNext()
		{
			if (_buffer == null)
			{
				PeekNext();
			}
			var ret = _buffer!;
			_buffer = null;
			return ret;
		}

		private IToken PeekNext()
		{
			while (_buffer == null)
			{
				if (_position == _tokens.Count)
				{
					_buffer = _tokens[^1];
					break;
				}
				_buffer = _tokens[_position++];
			}
			return _buffer!;
		}

		private void Line()
		{
			while (PeekNext() is not EOFToken)
			{
				if (GetNext() is not LineToken line)
				{
					throw new InvalidProgramException("Line state should start with 'Line' token");
				}

				if (_programEnded)
				{
					if (PeekNext() is not EOFToken)
						throw new InvalidProgramException("There should be nothing after 'END'");
					break;
				}

				if (PeekNext() is NumberToken number)
				{
					GetNext();
					_lineNumber = number.Value;
				}
				else
				{
					_lineNumber += 10;
				}
				line.LineNumber = _lineNumber;

				line.Statement = Statement();

				_lines.Add(line);
			}
		}

		private StatementToken Statement()
		{
			if (GetNext() is not StatementToken statement)
			{
				throw new InvalidProgramException("Statement state should start with 'Statement' token");
			}

			switch (statement.Type)
			{
				case StatementType.Print:
					statement.Tokens.Add(ExpressionList());
					break;
				case StatementType.If:
					IfStatement(statement);
					break;
				case StatementType.Goto:
					statement.Tokens.Add(Expression());
					break;
				case StatementType.Input:
					statement.Tokens.Add(VarList());
					break;
				case StatementType.Let:
					LetStatement(statement);
					break;
				case StatementType.Gosub:
					statement.Tokens.Add(Expression());
					break;
				case StatementType.Return:
					break;
				case StatementType.Clear:
					break;
				case StatementType.List:
					if (PeekNext() is not LineToken)
					{
						statement.Tokens.Add(Expression());
					}
					break;
				case StatementType.Run:
					break;
				case StatementType.Rem:
					statement.Tokens.Add(String());
					break;
				case StatementType.End:
					_programEnded = true;
					break;
				default:
					break;
			}

			return statement;
		}

		private void IfStatement(StatementToken statement)
		{
			statement.Tokens.Add(LogicalExpression());
			if (GetNext() is not SpecialWordToken specialWord
				|| (specialWord.Value != SpecialWordToken.SpecialWord.Then
				&& specialWord.Value != SpecialWordToken.SpecialWord.Else))
			{
				throw new InvalidProgramException("'IF' should contain 'THEN' or 'ELSE'");
			}
			statement.Tokens.Add(specialWord);
			statement.Tokens.Add(Statement());
			if (specialWord.Value == SpecialWordToken.SpecialWord.Then && PeekNext() is SpecialWordToken swt)
			{
				GetNext();
				if (swt.Value != SpecialWordToken.SpecialWord.Else)
				{
					throw new InvalidProgramException("After 'THEN statement' should be 'ELSE' or nothing");
				}
				statement.Tokens.Add(swt);
				statement.Tokens.Add(Statement());
			}
		}

		private void LetStatement(StatementToken statement)
		{
			if (GetNext() is not VarToken var)
			{
				throw new InvalidProgramException("'LET' should be followed by variable name");
			}
			statement.Tokens.Add(var);
			if (GetNext() is not EqSignToken)
			{
				throw new InvalidProgramException("Variable name should be followed by equals sign");
			}
			statement.Tokens.Add(Expression());
		}
		private ExprListToken ExpressionList()
		{
			ExprListToken exprList = new();
			List<IExprListArg> exprs = new();
			List<CommaToken> commas = new();
			while (true)
			{

				if (PeekNext() is QuotationToken)
				{
					exprs.Add(String());
				}
				else
				{
					exprs.Add(Expression());
				}
				
				if(PeekNext() is CommaToken comma)
				{
					commas.Add(comma);
					GetNext();
					continue;
				}

				break;
			}

			if (exprs.Count == 0)
				throw new InvalidProgramException("ExprList token should contain at least 1 item");

			exprList.FirstExpression = exprs[0];
			exprList.Values
				.AddRange(
					exprs.Skip(1)
					.Select((v, i) =>
					{
						return (commas[i], v);
					})
				);
			return exprList;
		}


		private VarListToken VarList()
		{
			VarListToken varList = new();

			List<VarToken> vars = new();
			List<CommaToken> commas = new();
			while (true)
			{

				if (PeekNext() is VarToken v)
				{
					GetNext();
					vars.Add(v);
				}

				if (PeekNext() is CommaToken comma)
				{
					if (comma.Value != CommaToken.CommaType.Comma)
						throw new InvalidOperationException("Variables should be divided by comma");

					GetNext();
					commas.Add(comma);
					continue;
				}

				break;
			}

			if (vars.Count == 0)
				throw new InvalidProgramException("VarList token should contain at least 1 item");

			varList.FirstVar = vars[0];
			varList.Values
				.AddRange(
					vars.Skip(1)
					.Select((v, i) =>
					{
						return (commas[i], v);
					})
				);

			return varList;
		}

		private ExpressionToken Expression()
		{
			ExpressionToken exp = new();
			PlusMinusToken pmf = _plusToken;
			if (PeekNext() is PlusMinusToken pm)
			{
				GetNext();
				pmf = pm;	
			}
			exp.FirstTerm = new SignedTerm() { Sign = pmf, Term = Term() };
			while (PeekNext() is PlusMinusToken pmt)
			{
				GetNext();
				exp.Terms.Add(new SignedTerm() { Sign = pmt, Term = Term() });
			}
			return exp;
		}

		private LogicalExpressionToken LogicalExpression()
		{
			var fExp = Expression();
			var relop = Relop();
			var sExp = Expression();
			return new LogicalExpressionToken
			{
				First = fExp,
				Relop = relop,
				Second = sExp
			};
		}

		private TermToken Term()
		{
			TermToken term = new() { 
				FirstFactor = Factor(),
			};
			while(PeekNext() is MulDivToken md)
			{
				GetNext();
				term.Values.Add((md, Factor()));
			}
			return term;
		}

		private FactorToken Factor()
		{
			var next = PeekNext();
			if(next is VarToken vart)
			{
				GetNext();
				return new FactorToken() { Factor = vart};
			}

			if (next is NumberToken number)
			{
				GetNext();
				return new FactorToken() { Factor = number };
			}

			if (next is ParenthesisToken rbt)
			{
				GetNext();
				if (!rbt.Open)
					throw new InvalidProgramException("Expression should be started with open brackets");
				var ft = new FactorToken() { Factor = Expression()};
				if (GetNext() is not ParenthesisToken rbt2 || rbt2.Open)
					throw new InvalidProgramException("Expression should be closed with closing brackets");
				return ft;
			}

			throw new InvalidProgramException("Factor token should be variable, number or bracketed expression");
		}

		private RelopToken Relop()
		{
			var next = PeekNext();
			if (next is EqSignToken est)
			{
				GetNext();
				return new RelopToken() { Value = est };
			}

			if(next is LesserSingToken lst)
			{
				GetNext();
				if (PeekNext() is ILessSignNext it)
				{
					GetNext();
					lst.Next = it;
				}
				return new RelopToken() { Value = lst };
			}

			if (next is GreaterSignToken gst)
			{
				GetNext();
				if (PeekNext() is IGreatSignNext it)
				{
					GetNext();
					gst.Next = it;
				}
				return new RelopToken() { Value = gst };
			}

			throw new InvalidProgramException("Relop token should contain at lest one of ( < | > | = )");
		}

		private StringToken String()
		{
			var st = new StringToken();
			var sb = new StringBuilder();

			if (GetNext() is not QuotationToken)
				throw new InvalidProgramException("String token should start with \"");

			while(PeekNext() is WordToken wt)
			{
				GetNext();
				sb.Append(wt.Value);
				sb.Append(' ');
			}
			if (GetNext() is not QuotationToken)
				throw new InvalidProgramException("String token should end with \"");

			st.Value = sb.Length > 0 ?  sb.ToString()[..^1] : string.Empty;

			return st;
		}
	}
}
