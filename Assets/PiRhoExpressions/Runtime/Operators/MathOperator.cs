using PiRhoSoft.Variables;
using System.Text;

namespace PiRhoSoft.Expressions
{
	public abstract class MathOperator : InfixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			var left = Left.Evaluate(variables);
			var right = Right.Evaluate(variables);
			var result = Calculate(left, right);

			if (result.IsEmpty)
				throw new TypeMismatchException(Symbol, left, right);

			return result;
		}

		protected abstract Variable Calculate(Variable left, Variable right);
	}

	public class AddOperator : MathOperator
	{
		protected override Variable Calculate(Variable left, Variable right) => Variable.Add(left, right);
	}

	public class SubtractOperator : MathOperator
	{
		protected override Variable Calculate(Variable left, Variable right) => Variable.Subtract(left, right);
	}

	public class MultiplyOperator : MathOperator
	{
		protected override Variable Calculate(Variable left, Variable right) => Variable.Multiply(left, right);
	}

	public class DivideOperator : MathOperator
	{
		protected override Variable Calculate(Variable left, Variable right) => Variable.Divide(left, right);
	}

	public class ExponentOperator : MathOperator
	{
		protected override Variable Calculate(Variable left, Variable right) => Variable.Exponent(left, right);
	}

	public class ModuloOperator : MathOperator
	{
		protected override Variable Calculate(Variable left, Variable right) => Variable.Modulo(left, right);
	}

	public class NegateOperator : PrefixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			var right = Right.Evaluate(variables);
			var result = Variable.Negate(right);

			if (result.IsEmpty)
				throw new TypeMismatchException(Symbol, right);

			return result;
		}
	}

	public class GroupOperator : PrefixOperator
	{
		private string _closeSymbol;

		public GroupOperator(string closeSymbol)
		{
			_closeSymbol = closeSymbol;
		}

		public override void Parse(IParseContext parser, Token token)
		{
			base.Parse(parser, token);

			var close = parser.TakeToken();

			if (close.Type != TokenType.Operator || close.Text != _closeSymbol)
				throw new UnexpectedTokenException(token, _closeSymbol);
		}

		public override Variable Evaluate(IVariableDictionary variables)
		{
			return Right.Evaluate(variables);
		}

		public override void Print(StringBuilder printer)
		{
			base.Print(printer);
			printer.Append(_closeSymbol);
		}
	}
}
