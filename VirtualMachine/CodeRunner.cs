using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

			StatementEvalResult evalRes = (line.Statement.Type) switch
			{
				StatementType.Print => PrintStatement(line.Statement),
				StatementType.If => throw new NotImplementedException(),
				StatementType.Goto => GotoStatement(line.Statement),
				StatementType.Input => InputStatement(line.Statement),
				StatementType.Let => LetStatement(line.Statement),
				StatementType.Gosub => throw new NotImplementedException(),
				StatementType.Return => throw new NotImplementedException(),
				StatementType.Clear => throw new NotImplementedException(),
				StatementType.List => throw new NotImplementedException(),
				StatementType.Run => throw new NotImplementedException(),
				StatementType.Rem => throw new NotImplementedException(),
				StatementType.End => EndStatement(),
				_ => throw new NotImplementedException("Unknown Statement"),
			};

			if (!evalRes.Equals(StatementEvalResults.Success))
			{
				var errMsg = string.Format("Error on line {0}: {1}", line.LineNumber, evalRes.ErrorMessage);
				throw new InvalidProgramException(errMsg);
			}
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
