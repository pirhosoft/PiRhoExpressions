using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System;

namespace PiRhoSoft.Expressions
{
	public enum VariableSourceType
	{
		Value,
		Expression
	}

	public class VariableSourceException : Exception
	{
		// TODO
	}

	[Serializable]
	public class VariableSource
	{
		[EnumButtons]
		[NoLabel]
		public VariableSourceType Type = VariableSourceType.Value;

		[Conditional(nameof(Type), (int)VariableSourceType.Expression, EnumTest.ShowIfEqual)]
		[NoLabel]
		public ReadOnlyExpression Expression = new ReadOnlyExpression();

		[Conditional(nameof(Type), (int)VariableSourceType.Value, EnumTest.ShowIfEqual)]
		public SerializedVariable Value = new SerializedVariable();

		public VariableSource()
		{
		}

		public VariableSource(Variable defautValue)
		{
			Value.Variable = defautValue;
			Type = VariableSourceType.Value;
		}

		public VariableSource(string defaultExpression)
		{
			Expression.Content = defaultExpression;
			Type = VariableSourceType.Expression;
		}

		public Variable Resolve(IVariableDictionary variables)
		{
			switch (Type)
			{
				case VariableSourceType.Value: return Value.Variable;
				case VariableSourceType.Expression: return Expression.Execute(variables);
			}

			return Variable.Empty;
		}

		public Variable Resolve(IVariableDictionary variables, VariableType expectedType)
		{
			var variable = Resolve(variables);

			if (variable.Type != expectedType)
				throw new VariableSourceException();

			return variable;
		}

		public ExpectedType Resolve<ExpectedType>(IVariableDictionary variables)
		{
			var variable = Resolve(variables);

			if (!variable.Is<ExpectedType>())
				throw new VariableSourceException();

			return variable.As<ExpectedType>();
		}
	}
}
