using PiRhoSoft.Variables;
using System.Text;

namespace PiRhoSoft.Expressions
{
	public class TernaryOperator : InfixOperator
	{
		private string _alternationSymbol;
		private IOperation _rightAlternative;

		public TernaryOperator(string alternationSymbol)
		{
			_alternationSymbol = alternationSymbol;
		}

		public override void Parse(IParseContext parser, Token token, IOperation left, Precedence precedence)
		{
			base.Parse(parser, token, left, precedence);

			var alternation = parser.TakeToken();

			if (alternation.Type != TokenType.Operator || alternation.Text != _alternationSymbol)
				throw new UnexpectedTokenException(token, _alternationSymbol);

			_rightAlternative = parser.Parse(Precedence.Ternary.Right);
		}

		public override void Print(StringBuilder printer)
		{
			base.Print(printer);
			printer.Append(_alternationSymbol);
			_rightAlternative.Print(printer);
		}

		public override Variable Evaluate(IVariableDictionary variables)
		{
			var left = Left.Evaluate(variables);
			
			if (left.Type != VariableType.Bool)
				throw new TypeMismatchException(Symbol, left, VariableType.Bool);

			if (left.AsBool)
				return Right.Evaluate(variables);
			else
				return _rightAlternative.Evaluate(variables);
		}
	}
}
