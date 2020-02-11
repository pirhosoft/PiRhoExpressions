using PiRhoSoft.Variables;

namespace PiRhoSoft.Expressions
{
	public abstract class LogicOperator : InfixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			// TODO: This (and corresponding assign operators) doesn't short circuit. Should it?

			var left = Left.Evaluate(variables);
			var right = Right.Evaluate(variables);

			if (!left.IsBool || !right.IsBool)
				throw new TypeMismatchException(Symbol, left, right);

			return Test(left, right);
		}

		protected abstract Variable Test(Variable left, Variable right);
	}

	public class AndOperator : LogicOperator
	{
		protected override Variable Test(Variable left, Variable right) => Variable.Bool(left.AsBool && right.AsBool);
	}

	public class OrOperator : LogicOperator
	{
		protected override Variable Test(Variable left, Variable right) => Variable.Bool(left.AsBool || right.AsBool);
	}

	public class InvertOperator : PrefixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			var right = Right.Evaluate(variables);

			if (!right.IsBool)
				throw new TypeMismatchException(Symbol, right);

			return Variable.Bool(!right.AsBool);
		}
	}
}
