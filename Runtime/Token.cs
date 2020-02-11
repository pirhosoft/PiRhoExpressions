using PiRhoSoft.Variables;

namespace PiRhoSoft.Expressions
{
	public enum TokenType
	{
		Literal,
		Identifier,
		Operator,
		Sentinel,
		Comment,
		Whitespace,
		Other
	}

	public class Token
	{
		public TokenType Type { get; private set; }
		public int Start { get; private set; }
		public int End { get; private set; }
		public string Text { get; private set; } // Use ReadOnlySpan<char> for this when System.Memory is available in Unity's .net runtime
		public Variable Value { get; private set; }

		public bool IsIgnored => Type == TokenType.Comment || Type == TokenType.Whitespace;

		public Token(TokenType type, string input, int location, int length, Variable value)
		{
			Type = type;
			Start = location;
			End = location + length;
			Text = input.Substring(location, length); // input.AsSpan(location, length);
			Value = value;
		}
	}
}
