using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TinyBasic.VirtualMachine
{
	internal partial class CodeRunner
	{
		internal static class StatementEvalResults
		{
			public static readonly StatementEvalResult Success = new();
			public static readonly StatementEvalResult WrongStatement = new() {Success = false, ErrorMessage = "Machine got wrong statement" }; 
			public static readonly StatementEvalResult WrongArgumentCount = new() {Success = false, ErrorMessage = "Argument count" }; 
			public static readonly StatementEvalResult NoExpressionList = new() {Success = false, ErrorMessage = "Expected expression list" };
			public static readonly StatementEvalResult NoExpression = new() {Success = false, ErrorMessage = "Expected expression" };
			public static readonly StatementEvalResult NoExpressionOrString = new() {Success = false, ErrorMessage = "Expected expression or string" };
			public static readonly StatementEvalResult NoLogicalExpression = new() {Success = false, ErrorMessage = "Expected logical expression" };
			public static readonly StatementEvalResult NoTerm = new() {Success = false, ErrorMessage = "Expected term" };
			public static readonly StatementEvalResult NoFactor = new() {Success = false, ErrorMessage = "Expected factor" };
		}
	}
}
