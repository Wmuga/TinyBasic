using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using TinyBasic.Tokens.CharTokens;
using TinyBasic.Tokens.EndTokens;
using TinyBasic.Tokens.IntermTokens;

namespace TinyBasic.VirtualMachine
{
	public delegate void PrintOutput(string output); 
	public delegate int GetInput();
	internal partial class CodeRunner
	{
		private List<LineToken> _program;
		private Dictionary<VarToken.VariableName, int> _variables = InitVariables();
		private int _pc;
		private PrintOutput _output;
		private GetInput _input;
		private Stack<int> _pcStack = new();

		private static Dictionary<VarToken.VariableName, int> InitVariables()
		{
			var vars = new Dictionary<VarToken.VariableName, int>();

			foreach(var name in Enum.GetValues(typeof(VarToken.VariableName)).Cast<VarToken.VariableName>())
			{
				vars.Add(name, 0);
			}

			return vars;
		}

		public CodeRunner(IEnumerable<LineToken> program, PrintOutput output, GetInput input)
		{
			_program = program.ToList();
			_input = input;
			_output = output;
		}

		public void Run()
		{
			_pc = 0;
			while(_pc < _program.Count)
			{
				EvalLine(_program[_pc++]);				
			}
		}

		private void EvalLine(LineToken line)
		{
			if (line.Statement is null)
			{
				throw new InvalidProgramException($"Exception on line {line.LineNumber}. No statement");
			}

			StatementEvalResult evalRes = EvalStatement(line.Statement);

			if (!evalRes.Equals(StatementEvalResults.Success))
			{
				var errMsg = string.Format("Error on line {0}: {1}", line.LineNumber, evalRes.ErrorMessage);
				throw new InvalidProgramException(errMsg);
			}
		}

		private StatementEvalResult EvalStatement(StatementToken statement)
		{
			return (statement.Type) switch
			{
				StatementType.Print => PrintStatement(statement),
				StatementType.If => IfStatement(statement),
				StatementType.Goto => GotoStatement(statement),
				StatementType.Input => InputStatement(statement),
				StatementType.Let => LetStatement(statement),
				StatementType.Gosub => GosubStatement(statement),
				StatementType.Return => ReturnStatement(statement),
				StatementType.List => ListStatement(statement),
				// Run not needed. Usually last line to start program. So I can cut everything after
				StatementType.Run => EndStatement(),
				StatementType.Rem => RemStatement(statement),
				StatementType.End => EndStatement(),
				_ => throw new NotImplementedException("Unknown Statement"),
			};
		}

		private StatementEvalResult ListStatement(StatementToken statement)
		{
			if (statement.Type != StatementType.List)
				return StatementEvalResults.WrongStatement;

			if (statement.Tokens.Count > 1)
				return StatementEvalResults.WrongArgumentCount;

			if (statement.Tokens.Count == 0)
			{
				for(int i = 0; i < _program.Count; i++)
				{
					LineToStr(_program[i]);
				}
				return StatementEvalResults.Success;
			}

			if (statement.Tokens[0] is not ExprListToken elt)
				return StatementEvalResults.NoExpressionList;

			var exprTokens = elt.GetExpressions().ToList();
			
			if (exprTokens.Count > 2)
				return StatementEvalResults.WrongArgumentCount;

			if (exprTokens[0] is not ExpressionToken expr1)
				return StatementEvalResults.NoExpression;

			int start = FindLineIndex(EvalExpressionToken(expr1));
			int end = start + 1;

			if (exprTokens.Count == 2)
			{
				if (exprTokens[1] is not ExpressionToken expr2)
					return StatementEvalResults.NoExpression;

				end = FindLineIndex(EvalExpressionToken(expr2)) + 1;
			}

			if (start == -1 || end == -1)
				return StatementEvalResults.NoLine;

			for (int i = start; i < end; i++)
			{
				LineToStr(_program[i]);
			}

			return StatementEvalResults.Success;
		}

		private StatementEvalResult GosubStatement(StatementToken statement)
		{
			if (statement.Type != StatementType.Gosub)
				return StatementEvalResults.WrongStatement;

			_pcStack.Push(_pc);
			statement.Type = StatementType.Goto;
			var res =  GotoStatement(statement);
			statement.Type = StatementType.Gosub;
			return res;
		}

		private StatementEvalResult ReturnStatement(StatementToken statement)
		{
			if (statement.Type != StatementType.Return)
				return StatementEvalResults.WrongStatement;

			if (statement.Tokens.Count != 0)
				return StatementEvalResults.WrongArgumentCount;

			if (!_pcStack.TryPop(out _pc))
			{
				return StatementEvalResults.EmptyStack;
			}

			return StatementEvalResults.Success;
		}

		private StatementEvalResult IfStatement(StatementToken statement)
		{
			StatementToken? stmt2 = null;

			if (statement.Type != StatementType.If)
				return StatementEvalResults.WrongStatement;

			if (statement.Tokens.Count != 3 && statement.Tokens.Count != 5)
				return StatementEvalResults.WrongArgumentCount;

			if (statement.Tokens[0] is not LogicalExpressionToken logExpr)
				return StatementEvalResults.NoLogicalExpression;

			if (statement.Tokens[1] is not SpecialWordToken spWord1 || (spWord1.Value != SpecialWordToken.SpecialWord.Then && spWord1.Value != SpecialWordToken.SpecialWord.Else))
				return StatementEvalResults.NoThenElse;

			if (statement.Tokens[2] is not StatementToken stmt1)
				return StatementEvalResults.NoStatement;

			if (statement.Tokens.Count == 5)
			{
				if (spWord1.Value != SpecialWordToken.SpecialWord.Then)
					return StatementEvalResults.NoNewLine;

				if (statement.Tokens[3] is not SpecialWordToken spWord2 || spWord2.Value != SpecialWordToken.SpecialWord.Else)
					return StatementEvalResults.NoElse;

				if (statement.Tokens[4] is not StatementToken _stmt2)
					return StatementEvalResults.NoStatement;
				
				stmt2 = _stmt2;
			}

			bool logExpRes = EvalLogExprToken(logExpr);

			if (logExpRes && spWord1.Value == SpecialWordToken.SpecialWord.Then)
			{
				return EvalStatement(stmt1);
			}

			if (spWord1.Value == SpecialWordToken.SpecialWord.Else)
			{
				return EvalStatement(stmt1);
			}

			if (stmt2 is not null)
			{
				return EvalStatement(stmt2);
			}

			return StatementEvalResults.InvalidState;
		}

		private StatementEvalResult RemStatement(StatementToken statement)
		{
			if (statement.Type != StatementType.Rem)
				return StatementEvalResults.WrongStatement;

			if (statement.Tokens.Count != 1)
				return StatementEvalResults.WrongArgumentCount;

			if (statement.Tokens[0] is not StringToken)
				return StatementEvalResults.NoExpression;

			return StatementEvalResults.Success;
		}

		private StatementEvalResult PrintStatement(StatementToken statement)
		{
			if (statement.Type != StatementType.Print)
				return StatementEvalResults.WrongStatement;

			if (statement.Tokens.Count != 1)
				return StatementEvalResults.WrongArgumentCount;

			if (statement.Tokens[0] is not ExprListToken exprs) 
				return StatementEvalResults.NoExpressionList;

			StringBuilder sb = new();
			try
			{

				if (exprs.FirstExpression is StringToken strF)
				{
					sb.Append(strF.Value);
				}
				else
				{
					if (exprs.FirstExpression is not ExpressionToken expr)
						return StatementEvalResults.NoExpressionOrString;
					sb.Append(EvalExpressionToken(expr));
				}

				foreach (var exprPair in exprs.Values)
				{
					if (exprPair.Item1.Value == CommaToken.CommaType.Comma) sb.Append(' ');
					var exprArg = exprPair.Item2;
					if (exprArg is StringToken str)
					{
						sb.Append(str.Value);
						continue;
					}
					if (exprArg is not ExpressionToken expr)
						return StatementEvalResults.NoExpressionOrString;
					sb.Append(EvalExpressionToken(expr));
				}
			} 
			catch (InvalidProgramException ex)
			{
				return new StatementEvalResult() { Success = false, ErrorMessage = ex.Message};
			}

			_output.Invoke(sb.ToString());

			return StatementEvalResults.Success;
		}

		private StatementEvalResult GotoStatement(StatementToken statement)
		{
			if (statement.Type != StatementType.Goto)
				return StatementEvalResults.WrongStatement;

			if (statement.Tokens.Count != 1)
				return StatementEvalResults.WrongArgumentCount;

			if (statement.Tokens[0] is not ExpressionToken expr)
				return StatementEvalResults.NoExpression;

			var line = EvalExpressionToken(expr);
			var ind = FindLineIndex(line);
			if (ind == -1)
			{
				return StatementEvalResults.NoLine;
			}

			_pc = ind;
			return StatementEvalResults.Success;
		}

		private StatementEvalResult LetStatement(StatementToken statement)
		{
			if (statement.Type != StatementType.Let)
				return StatementEvalResults.WrongStatement;

			if (statement.Tokens.Count != 2)
				return StatementEvalResults.WrongArgumentCount;

			if (statement.Tokens[0] is not VarToken var)
				return StatementEvalResults.NoExpression;

			if (statement.Tokens[1] is not ExpressionToken expr)
				return StatementEvalResults.NoExpression;

			try
			{
				var evalRes = EvalExpressionToken(expr);
				_variables[var.Name] = evalRes;
			}
			catch (InvalidProgramException ex)
			{
				return new StatementEvalResult() { Success = false, ErrorMessage = ex.Message };
			}

			return StatementEvalResults.Success;
		}

		private StatementEvalResult InputStatement(StatementToken statement)
		{
			if (statement.Type != StatementType.Input)
				return StatementEvalResults.WrongStatement;

			if (statement.Tokens.Count != 1)
				return StatementEvalResults.WrongArgumentCount;

			if (statement.Tokens[0] is not VarListToken vars)
				return StatementEvalResults.NoVariableList;

			try
			{
				foreach(var variable in vars.GetVars())
				{
					_variables[variable.Name] = _input();
				}
			}
			catch (InvalidProgramException ex)
			{
				return new StatementEvalResult() { Success = false, ErrorMessage = ex.Message };
			}
			return StatementEvalResults.Success;
		}

		private StatementEvalResult EndStatement()
		{
			_pc = _program.Count;
			return StatementEvalResults.Success;
		}


		private int FindLineIndex(int line)
		{
			for(int i = 0; i < _program.Count; i++)
			{
				if (_program[i].LineNumber == line)
				{
					return i;
				}
			}
			return -1;
		}

		internal struct StatementEvalResult
		{
			public bool Success;
			public string ErrorMessage;

			public StatementEvalResult()
			{
				Success = true;
				ErrorMessage = string.Empty;
			}
		}
	}
}
