using PiRhoSoft.Variables;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PiRhoSoft.Expressions
{
	public interface ILexerRule
	{
		Token Match(string input, int start, ref IReadOnlyList<ILexerRule> rules);
	}

	public class Lexer
	{
		#region Default

		public static readonly List<ILexerRule> DefaultRules = new List<ILexerRule>
		{
			// Number literals first so '-', '+', and '.' aren't detected as operators.
			// Float first so the whole number part isn't detected as an Int.
			new FloatLiteralLexerRule(),
			new IntLiteralLexerRule(),
			new StringLiteralLexerRule(),
			new ColorLiteralLexerRule(),
			new IdentifierLexerRule(),

			// Comments first so the opener symbols aren't detected as operators.
			new CommentLexerRule("//", "\n"),
			new CommentLexerRule("/*", "/*"),

			// Multiple character first so '+=' isn't detected as '+' then '='
			new AnyCharacterSequenceLexerRule(TokenType.Operator, @"!%^&*-+=/\|<>", StringComparison.Ordinal),
			new AnyCharacterLexerRule(TokenType.Operator, @"~?:.,()[]"),
			new StartInterpolateLexerRule('`'),
			new StartInterpolateLexerRule('}'),

			new WhitespaceLexerRule(),
			new CharacterLexerRule(TokenType.Sentinel, ';')
		};

		public static readonly Lexer Default = CreateDefault();

		private static Lexer CreateDefault()
		{
			return new Lexer();
		}

		#endregion

		#region String

		public static readonly IReadOnlyList<ILexerRule> InterpolateRules = new List<ILexerRule>
		{
			new EndInterpolateLexerRule('{'),
			new EndInterpolateLexerRule('`'),
			new InterpolateLexerRule()
		};

		#endregion

		#region Rules

		private Token MatchRule(string input, int start, ref IReadOnlyList<ILexerRule> rules)
		{
			var startingRules = rules;

			foreach (var rule in startingRules)
			{
				var token = rule.Match(input, start, ref rules);

				if (token != null)
					return token;
			}

			return null;
		}

		#endregion

		public List<Token> Tokenize(string input, bool preserveText, IReadOnlyList<ILexerRule> rules = null)
		{
			var location = 0;
			var length = input.Length;
			var tokens = new List<Token>();

			rules = rules ?? DefaultRules;

			while (location < length)
			{
				var token = MatchRule(input, location, ref rules);

				if (token != null)
				{
					if (preserveText || !token.IsIgnored)
						tokens.Add(token);

					location = token.End;
				}
				else
				{
					throw new LexerException(input, location);
				}
			}

			return tokens;
		}
	}

	public class LexerException : Exception
	{
		private const int _contextLength = 10;
		private const string _message = "Failed to tokenize input string: A rule could not be found to parse '{0}' at index {1}";

		public readonly string Input;
		public readonly int Location;

		public LexerException(string input, int location) : base(GetMessage(input, location))
		{
			Input = input;
			Location = location;
		}

		private static string GetMessage(string input, int location)
		{
			var context = location + _contextLength < input.Length
				? input.Substring(location, _contextLength) + "..."
				: input.Substring(location);

			return string.Format(_message, context, location);
		}
	}

	#region Base Rule Types

	public class CharacterLexerRule : ILexerRule
	{
		private TokenType _type;
		private char _character;

		public CharacterLexerRule(TokenType type, char c)
		{
			_type = type;
			_character = c;
		}

		public Token Match(string input, int start, ref IReadOnlyList<ILexerRule> rules)
		{
			var c = input[start];

			if (_character == c)
				return new Token(_type, input, start, 1, GetValue(c));
			else
				return null;
		}

		protected virtual Variable GetValue(char c) => Variable.Empty;
	}

	public class AnyCharacterLexerRule : ILexerRule
	{
		private TokenType _type;
		private string _characters;

		public AnyCharacterLexerRule(TokenType type, string characters)
		{
			_type = type;
			_characters = characters;
		}

		public Token Match(string input, int start, ref IReadOnlyList<ILexerRule> rules)
		{
			var c = input[start];

			if (_characters.IndexOf(c) >= 0)
				return new Token(_type, input, start, 1, GetValue(c));
			else
				return null;
		}

		protected virtual Variable GetValue(char c) => Variable.Empty;
	}

	public class CharacterSequenceLexerRule : ILexerRule
	{
		private TokenType _type;
		private string _sequence;
		private StringComparison _comparison;

		public CharacterSequenceLexerRule(TokenType type, string sequence, StringComparison comparison)
		{
			_type = type;
			_sequence = sequence;
			_comparison = comparison;
		}

		public Token Match(string input, int start, ref IReadOnlyList<ILexerRule> rules)
		{
			if (input.Length >= start + _sequence.Length && input.IndexOf(input, start, _sequence.Length, _comparison) == 0)
				return new Token(_type, input, start, _sequence.Length, GetValue(_sequence));
			else
				return null;
		}

		protected virtual Variable GetValue(string s) => Variable.Empty;
	}

	public class AnyCharacterSequenceLexerRule : ILexerRule
	{
		private TokenType _type;
		private Regex _regex;

		public AnyCharacterSequenceLexerRule(TokenType type, string characters, StringComparison comparison)
		{
			var options = RegexOptions.Compiled;

			if (comparison.HasFlag(StringComparison.InvariantCulture))
				options |= RegexOptions.CultureInvariant;

			if (comparison.HasFlag(StringComparison.CurrentCultureIgnoreCase))
				options |= RegexOptions.IgnoreCase;

			_type = type;
			_regex = new Regex($"\\G[{characters}]+", options);
		}

		public Token Match(string input, int start, ref IReadOnlyList<ILexerRule> rules)
		{
			var match = _regex.Match(input, start);

			if (match.Success)
				return new Token(_type, input, start, match.Length, GetValue(match.Value));
			else
				return null;
		}

		protected virtual Variable GetValue(string s) => Variable.Empty;
	}

	public abstract class RegexLexerRule : ILexerRule
	{
		private TokenType _type;
		private Regex _regex;

		public RegexLexerRule(TokenType type, Regex regex)
		{
			_type = type;
			_regex = regex;
		}

		public Token Match(string input, int start, ref IReadOnlyList<ILexerRule> rules)
		{
			var match = _regex.Match(input, start);

			if (match.Success)
				return new Token(_type, input, start, match.Length, GetValue(match));
			else
				return null;
		}

		protected virtual Variable GetValue(Match match) => Variable.Empty;
	}

	#endregion

	#region Default Rule Types

	public class IntLiteralLexerRule : RegexLexerRule
	{
		private static readonly Regex _regex = new Regex(@"\G[+-]?[0-9]+", RegexOptions.Compiled);

		public IntLiteralLexerRule() : base(TokenType.Literal, _regex) { }

		protected override Variable GetValue(Match match)
		{
			var i = int.Parse(match.Value);
			return Variable.Int(i);
		}
	}

	public class FloatLiteralLexerRule : RegexLexerRule
	{
		private static readonly Regex _regex = new Regex(@"\G[+-]?[0-9]*[.][0-9]+", RegexOptions.Compiled);

		public FloatLiteralLexerRule() : base(TokenType.Literal, _regex) { }

		protected override Variable GetValue(Match match)
		{
			var f = float.Parse(match.Value);
			return Variable.Float(f);
		}
	}

	public class StringLiteralLexerRule : RegexLexerRule
	{
		private static readonly Regex _regex = new Regex("\\G\"(.(?!(?<![\\\\])\"))*.?\"", RegexOptions.Compiled);

		public StringLiteralLexerRule() : base(TokenType.Literal, _regex) { }

		protected override Variable GetValue(Match match)
		{
			var s = match.Groups[1].Value.Replace("\\\"", "\""); // TODO: Unescape other stuff like unicode characters.
			return Variable.String(s);
		}
	}

	public class ColorLiteralLexerRule : RegexLexerRule
	{
		private const float _component1Ratio = 0x11 / 255.0f;
		private const float _component2Ratio = 1 / 255.0f;

		private static readonly Regex _regex = new Regex(@"\G#[A-Fa-f0-9]{3,8}", RegexOptions.Compiled);

		public ColorLiteralLexerRule() : base(TokenType.Literal, _regex) { }

		private float GetComponent1(string text, int index) => Convert.ToInt32(text.Substring(index, 1), 16) * _component1Ratio;
		private float GetComponent2(string text, int index) => Convert.ToInt32(text.Substring(index, 2), 16) * _component2Ratio;

		protected override Variable GetValue(Match match)
		{
			var r = 0.0f;
			var g = 0.0f;
			var b = 0.0f;
			var a = 1.0f;

			if (match.Length == 4) // #RGB
			{
				r = GetComponent1(match.Value, 1);
				g = GetComponent1(match.Value, 2);
				b = GetComponent1(match.Value, 3);
			}
			else if (match.Length == 5) // #RGBA
			{
				r = GetComponent1(match.Value, 1);
				g = GetComponent1(match.Value, 2);
				b = GetComponent1(match.Value, 3);
				a = GetComponent1(match.Value, 4);
			}
			else if (match.Length == 6) // #RGBAA
			{
				r = GetComponent1(match.Value, 1);
				g = GetComponent1(match.Value, 2);
				b = GetComponent1(match.Value, 3);
				a = GetComponent2(match.Value, 4);
			}
			else if (match.Length == 7) // #RRGGBB
			{
				r = GetComponent2(match.Value, 1);
				g = GetComponent2(match.Value, 3);
				b = GetComponent2(match.Value, 5);
			}
			else if (match.Length == 8) // #RRGGBBA
			{
				r = GetComponent2(match.Value, 1);
				g = GetComponent2(match.Value, 3);
				b = GetComponent2(match.Value, 5);
				a = GetComponent1(match.Value, 7);
			}
			else if (match.Length == 9) // #RRGGBBAA
			{
				r = GetComponent2(match.Value, 1);
				g = GetComponent2(match.Value, 3);
				b = GetComponent2(match.Value, 5);
				a = GetComponent2(match.Value, 7);
			}

			return Variable.Color(new Color(r, g, b, a));
		}
	}

	public class IdentifierLexerRule : RegexLexerRule
	{
		private static Regex _regex = new Regex(@"\G[_A-Za-z][_A-Za-z0-9]*", RegexOptions.Compiled);
		public IdentifierLexerRule() : base(TokenType.Identifier, _regex) { }
	}

	public class CommentLexerRule : ILexerRule
	{
		private readonly string _opening;
		private readonly string _closing;

		public CommentLexerRule(string opening, string closing)
		{
			_opening = opening;
			_closing = closing;
		}

		public Token Match(string input, int start, ref IReadOnlyList<ILexerRule> rules)
		{
			if (input.Length >= start + _opening.Length && input.IndexOf(input, start, _opening.Length) == 0)
			{
				var end = input.IndexOf(_closing, start);
				return new Token(TokenType.Comment, input, start, (end < start ? input.Length : end) - start, Variable.Empty);
			}
			else
			{
				return null;
			}
		}
	}

	public class WhitespaceLexerRule : RegexLexerRule
	{
		private static readonly Regex _regex = new Regex(@"\G\s+", RegexOptions.Compiled);
		public WhitespaceLexerRule() : base(TokenType.Whitespace, _regex) { }
	}

	#endregion

	#region String Interpolation

	public class StartInterpolateLexerRule : ILexerRule
	{
		private readonly char _character;

		public StartInterpolateLexerRule(char c)
		{
			_character = c;
		}

		public Token Match(string input, int start, ref IReadOnlyList<ILexerRule> rules)
		{
			if (input[start] == _character)
			{
				rules = Lexer.InterpolateRules;
				return new Token(TokenType.Operator, input, start, 1, Variable.Empty);
			}
			else
			{
				return null;
			}
		}
	}

	public class InterpolateLexerRule : RegexLexerRule
	{
		private static readonly Regex _regex = new Regex(@"\G(.(?!(?<![\\])[`\{]))+.?", RegexOptions.Compiled | RegexOptions.Singleline);
		public InterpolateLexerRule() : base(TokenType.Literal, _regex) { }
	}

	public class EndInterpolateLexerRule : ILexerRule
	{
		private readonly char _character;

		public EndInterpolateLexerRule(char c)
		{
			_character = c;
		}

		public Token Match(string input, int start, ref IReadOnlyList<ILexerRule> rules)
		{
			if (input[start] == _character)
			{
				rules = Lexer.DefaultRules;
				return new Token(TokenType.Operator, input, start, 1, Variable.Empty);
			}
			else
			{
				return null;
			}
		}
	}

	#endregion
}
