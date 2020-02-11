using PiRhoSoft.Variables;
using System;
using System.Text;

namespace PiRhoSoft.Expressions
{
	public interface IOperation
	{
		Variable Evaluate(IVariableDictionary variables);
		void Print(StringBuilder printer);
	}

	public interface IAssignableOperation
	{
		SetVariableResult Assign(IVariableDictionary variables, Variable value);
	}

	public abstract class Operator : IOperation
	{
		public override string ToString()
		{
			var builder = new StringBuilder();
			Print(builder);
			return builder.ToString();
		}

		public abstract Variable Evaluate(IVariableDictionary variables);
		public abstract void Print(StringBuilder printer);

		protected Variable Assign(IVariableDictionary variables, IOperation target, Variable value)
		{
			if (target is IAssignableOperation assignable)
			{
				var result = assignable.Assign(variables, value);

				switch (result)
				{
					case SetVariableResult.NotFound: throw new MissingAssignException(assignable, value);
					case SetVariableResult.ReadOnly: throw new ReadOnlyAssignException(assignable, value);
					case SetVariableResult.TypeMismatch: throw new TypeMismatchAssignException(assignable, value);
				}

				return value;
			}
			else
			{
				throw new InvalidAssignException(target, value);
			}
		}
	}

	public class OperationException : Exception
	{
		public OperationException(string error) : base(error) { }
		public OperationException(string errorFormat, params object[] arguments) : this(string.Format(errorFormat, arguments)) { }
	}

	public class TypeMismatchException : OperationException
	{
		private const string _message1 = "the operator '{0}' cannot be applied to a value of type {1}";
		private const string _message2 = "the operator '{0}' cannot be applied to values of type {1} and {2}";

		public TypeMismatchException(string symbol, Variable value) : base(_message1, symbol, value.Type) { }
		public TypeMismatchException(string symbol, Variable left, Variable right) : base(_message2, symbol, left.Type, right.Type) { }
		public TypeMismatchException(string symbol, Variable left, VariableType right) : base(_message2, symbol, left.Type, right) { }
	}

	public class AssignmentException : OperationException
	{
		public AssignmentException(string errorFormat, params object[] arguments) : base(errorFormat, arguments) { }
	}

	public class MissingAssignException : AssignmentException
	{
		private const string _message = "unable to assign '{0}' because '{1}' could not be found";
		public MissingAssignException(IAssignableOperation target, Variable value) : base(_message, value, target) { }
	}

	public class ReadOnlyAssignException : AssignmentException
	{
		private const string _message = "unable to assign '{0}' because '{1}' is read only";
		public ReadOnlyAssignException(IAssignableOperation target, Variable value) : base(_message, value, target) { }
	}

	public class TypeMismatchAssignException : AssignmentException
	{
		private const string _message = "unable to assign '{0}' because '{1}' cannot be assigned a value of type {2}";
		public TypeMismatchAssignException(IAssignableOperation target, Variable value) : base(_message, value, target, value.Type) { }
	}

	public class InvalidAssignException : AssignmentException
	{
		private const string _message = "unable to assign '{0}' because '{1}' is not assignable";
		public InvalidAssignException(IOperation target, Variable value) : base(_message, value, target) { }
	}
}
