using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TinyBasic.Tokens.BaseTokens;
using TinyBasic.Tokens.EndTokens;
using TinyBasic.Tokens.IntermTokens;
using static TinyBasic.Tokens.EndTokens.SpecialWordToken;
using static TinyBasic.Tokens.EndTokens.VarToken;

namespace TinyBasic.Tokenizer
{
	internal static partial class TokenConverter
	{
		public static IToken FromString(string str, bool inQuotation = false)
		{
			var match = NumberRegex().Match(str);
			if (!inQuotation && match.Success)
			{
				if(int.TryParse(str, out var res))
				{
					return new NumberToken { Value = res };
				}
				throw new ArgumentException(string.Format("Can't convert {0} to int", str));
			}

			if (!inQuotation && _var.ContainsKey(str))
			{
				return new VarToken() { Name = _var[str] };
			}

			if (!inQuotation && _statements.ContainsKey(str))
			{
				return new StatementToken() { Type = _statements[str] };
			}

			if (!inQuotation && _specials.ContainsKey(str))
			{
				return new SpecialWordToken() { Value = _specials[str] };
			}

			return new WordToken() { Value = str };
		}

		private static readonly Dictionary<string, StatementType> _statements = InitDictFormEnum<StatementType>();
		private static readonly Dictionary<string, VariableName> _var = InitDictFormEnum<VariableName>();
		private static readonly Dictionary<string, SpecialWord> _specials = InitDictFormEnum<SpecialWord>();

		private static Dictionary<string,T> InitDictFormEnum<T>() where T : Enum
		{
			Dictionary<string, T> dict = new();

			foreach (var value in Enum.GetValues(typeof(T)).Cast<T>())
			{
				dict.Add(value.ToString().ToUpper(), value);
			}

			return dict;
		}


		[GeneratedRegex("\\A\"([A-Za-z!@#$%^&*()\\-_+=,./\\?\"<>\\[\\]{}:;']+?)\"\\z", RegexOptions.Compiled)]
		private static partial Regex StringRegex();
		[GeneratedRegex("\\A(\\d+)\\z", RegexOptions.Compiled)]
		private static partial Regex NumberRegex();
	}
}
