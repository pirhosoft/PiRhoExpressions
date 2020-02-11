using PiRhoSoft.Variables;

namespace PiRhoSoft.Expressions
{
	public abstract class ComparisonOperator : InfixOperator
	{
		private const string _comparisonTypeMismatchException = "unable to compare types {0} and {1} with operator '{2}'";

		public override Variable Evaluate(IVariableDictionary variables)
		{
			var left = Left.Evaluate(variables);
			var right = Right.Evaluate(variables);
			var result = Compare(variables, left, right);

			if (!result.HasValue)
				throw new OperationException(_comparisonTypeMismatchException, left, right, Symbol);

			return Variable.Bool(result.Value);
		}

		protected abstract bool? Compare(IVariableDictionary variables, Variable left, Variable right);
	}

	public class EqualOperator : ComparisonOperator
	{
		protected override bool? Compare(IVariableDictionary variables, Variable left, Variable right) => Variable.IsEqual(left, right);
	}

	public class InequalOperator : ComparisonOperator
	{
		// The ! operator works as expected with null but comparison operators do not.
		protected override bool? Compare(IVariableDictionary variables, Variable left, Variable right) => !Variable.IsEqual(left, right);
	}

	public class LessOperator : ComparisonOperator
	{
		protected override bool? Compare(IVariableDictionary variables, Variable left, Variable right)
		{
			var comparison = Variable.Compare(left, right);
			return comparison.HasValue ? comparison.Value < 0 : (bool?)null;
		}
	}

	public class LessOrEqualOperator : ComparisonOperator
	{
		protected override bool? Compare(IVariableDictionary variables, Variable left, Variable right)
		{
			var comparison = Variable.Compare(left, right);
			return comparison.HasValue ? comparison.Value <= 0 : (bool?)null;
		}
	}

	public class GreaterOperator : ComparisonOperator
	{
		protected override bool? Compare(IVariableDictionary variables, Variable left, Variable right)
		{
			var comparison = Variable.Compare(left, right);
			return comparison.HasValue ? comparison.Value > 0 : (bool?)null;
		}
	}

	public class GreaterOrEqualOperator : ComparisonOperator
	{
		protected override bool? Compare(IVariableDictionary variables, Variable left, Variable right)
		{
			var comparison = Variable.Compare(left, right);
			return comparison.HasValue ? comparison.Value >= 0 : (bool?)null;
		}
	}
}
