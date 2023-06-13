using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TinyBasic.Tokens.BaseTokens;
using TinyBasic.Tokens.CharTokens;
using TinyBasic.Tokens.EndTokens;

namespace TinyBasic.Tokenizer
{
	internal partial class TokenStream
	{
		private int _listPosition;
		private List<string> _line = new();
		private readonly StreamReader _stream;
		private static readonly EOFToken _EOFToken = new();
		private bool _quotStarted = false;
		
		public TokenStream(string filename) { 
			_stream = new StreamReader(filename);
		}

		public TokenStream(Stream stream) { 
			_stream = new StreamReader(stream);
		}

		public TokenStream(StreamReader sr)
		{
			_stream = sr;
		}

		public IEnumerable<IToken> GetTokens()
		{
			while (true)
			{
				if (_line.Count == 0 || _listPosition == _line.Count)
				{
					var count = ReadLine();
					if (count == 0)
					{
						yield return _EOFToken;
						break;
					}
					yield return new LineToken();
				}

				var str = _line[_listPosition++];

				if (str.StartsWith('"') && str.EndsWith('"'))
				{
					yield return new QuotationToken();
					if (str.Length >2)
						yield return TokenConverter.FromString(str[1..^1], true);
					yield return new QuotationToken();
					continue;
				}

				if (str.StartsWith('"'))
				{
					if (_quotStarted)
					{
						throw new ArgumentException("Quotation already opened");
					}
					_quotStarted = true;
					yield return new QuotationToken();
					yield return TokenConverter.FromString(str[1..], true);
					continue;
				}

				if (str.EndsWith('"'))
				{
					if (!_quotStarted)
					{
						throw new ArgumentException("No opening quotation");
					}
					_quotStarted = false;
					yield return TokenConverter.FromString(str[..^1], true);
					yield return new QuotationToken();
					continue;
				}

				if (!_quotStarted && SignRegex().Match(str).Success)
				{
					IEnumerable<IToken> tokens = SplitBySign(str);
					foreach(var token in tokens)
					{
						yield return token;
					}
					continue;
				}
				yield return TokenConverter.FromString(str, _quotStarted);
			}
		}

		private IEnumerable<IToken> SplitBySign(string str)
		{
			StringBuilder sb = new();

			IToken SendNewToken()
			{
				IToken token = TokenConverter.FromString(sb.ToString(), _quotStarted);
				sb.Clear();
				return token;
			}

			foreach(char c in str)
			{
				switch (c)
				{
					case '+':
						yield return SendNewToken();
						yield return new PlusMinusToken { Type = PlusMinusType.Plus };
						break;
					case '-':
						yield return SendNewToken();
						yield return new PlusMinusToken { Type = PlusMinusType.Minus };
						break;
					case '*':
						yield return SendNewToken();
						yield return new MulDivToken { Type = MulDivType.Multiply };
						break;
					case '/':
						yield return SendNewToken();
						yield return new MulDivToken { Type = MulDivType.Division };
						break;
					case '<':
						yield return SendNewToken();
						yield return new LesserSingToken();
						break;
					case '>':
						yield return SendNewToken();
						yield return new GreaterSignToken();
						break;
					case '=':
						yield return SendNewToken();
						yield return new EqSignToken();
						break;
					default:
						sb.Append(c);
						break;
				}
			}
			
			if (sb.Length > 0)
			{
				yield return SendNewToken();
			}
		}

		private int ReadLine()
		{
			if (_stream.EndOfStream) return 0;

			_listPosition = 0;
			string line = _stream.ReadLine()?.Trim() ?? string.Empty;
			_line = line
				.Split(new[] {'\t',' '})
				.Where(x => x.Length > 0)
				.ToList();
			
			return _line.Count;
		}
		
		[GeneratedRegex("[+\\-*/<>=]", RegexOptions.Compiled)]
		private static partial Regex SignRegex();

	}
}
