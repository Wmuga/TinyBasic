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
	internal partial class CodeRunner
	{
		private List<LineToken> _program;
		private Dictionary<VarToken.VariableName, int> _variables = InitVariables();
		private int _pc;
		private PrintOutput _output;

		private static Dictionary<VarToken.VariableName, int> InitVariables()
		{
			var vars = new Dictionary<VarToken.VariableName, int>();

			foreach(var name in Enum.GetValues(typeof(VarToken.VariableName)).Cast<VarToken.VariableName>())
			{
				vars.Add(name, 0);
			}

			return vars;
		}

		public CodeRunner(IEnumerable<LineToken> program, PrintOutput output)
		{
			_program = program.ToList();
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
				StatementType.Goto => throw new NotImplementedException(),
				StatementType.Input => throw new NotImplementedException(),
				StatementType.Let => throw new NotImplementedException(),
				StatementType.Gosub => throw new NotImplementedException(),
				StatementType.Return => throw new NotImplementedException(),
				StatementType.Clear => throw new NotImplementedException(),
				StatementType.List => throw new NotImplementedException(),
				StatementType.Run => throw new NotImplementedException(),
				StatementType.Rem => throw new NotImplementedException(),
				StatementType.End => throw new NotImplementedException(),
				_ => throw new NotImplementedException("Unknown Statement"),
			};
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

			foreach(var exprPair in exprs.Values)
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

			_output.Invoke(sb.ToString());

			return StatementEvalResults.Success;
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
