using PiRhoSoft.Variables;

namespace PiRhoSoft.Expressions
{
	public abstract class AssignmentOperator : InfixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			var left = Left.Evaluate(variables);
			var right = Right.Evaluate(variables);
			var result = GetValue(left, right);

			if (result.IsEmpty)
				throw new TypeMismatchException(Symbol, left, right);

			return Assign(variables, Left, result);
		}

		protected abstract Variable GetValue(Variable left, Variable right);
	}

	public class AssignOperator : InfixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			var right = Right.Evaluate(variables);
			return Assign(variables, Left, right);
		}
	}

	public class AddAssignOperator : AssignmentOperator
	{
		protected override Variable GetValue(Variable left, Variable right) => Variable.Add(left, right);
	}

	public class SubtractAssignOperator : AssignmentOperator
	{
		protected override Variable GetValue(Variable left, Variable right) => Variable.Subtract(left, right);
	}

	public class MultiplyAssignOperator : AssignmentOperator
	{
		protected override Variable GetValue(Variable left, Variable right) => Variable.Multiply(left, right);
	}

	public class DivideAssignOperator : AssignmentOperator
	{
		protected override Variable GetValue(Variable left, Variable right) => Variable.Divide(left, right);
	}

	public class ExponentAssignOperator : AssignmentOperator
	{
		protected override Variable GetValue(Variable left, Variable right) => Variable.Exponent(left, right);
	}

	public class ModuloAssignOperator : AssignmentOperator
	{
		protected override Variable GetValue(Variable left, Variable right) => Variable.Modulo(left, right);
	}

	public class AndAssignOperator : AssignmentOperator
	{
		protected override Variable GetValue(Variable left, Variable right) => left.IsBool && right.IsBool ? Variable.Bool(left.AsBool && right.AsBool) : Variable.Empty;
	}

	public class OrAssignOperator : AssignmentOperator
	{
		protected override Variable GetValue(Variable left, Variable right) => left.IsBool && right.IsBool ? Variable.Bool(left.AsBool || right.AsBool) : Variable.Empty;
	}

	public class IncrementOperator : PrefixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			var right = Right.Evaluate(variables);
			var value = Variable.Add(right, Variable.Int(1));

			if (value.IsEmpty)
				throw new TypeMismatchException(Symbol, right);

			return Assign(variables, Right, value);
		}
	}

	public class DecrementOperator : PrefixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			var right = Right.Evaluate(variables);
			var value = Variable.Add(right, Variable.Int(-1));

			if (value.IsEmpty)
				throw new TypeMismatchException(Symbol, right);

			return Assign(variables, Right, value);
		}
	}

	public class PostIncrementOperator : PostfixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			var left = Left.Evaluate(variables);
			var value = Variable.Add(left, Variable.Int(1));

			if (value.IsEmpty)
				throw new TypeMismatchException(Symbol, left);

			Assign(variables, Left, value);
			return left;
		}
	}
	public class PostDecrementOperator : PostfixOperator
	{
		public override Variable Evaluate(IVariableDictionary variables)
		{
			var left = Left.Evaluate(variables);
			var value = Variable.Add(left, Variable.Int(-1));

			if (value.IsEmpty)
				throw new TypeMismatchException(Symbol, left);

			Assign(variables, Left, value);
			return left;
		}
	}
}
