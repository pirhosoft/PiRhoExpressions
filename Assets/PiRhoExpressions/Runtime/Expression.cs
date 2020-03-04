using PiRhoSoft.Variables;
using System;
using UnityEngine;

namespace PiRhoSoft.Expressions
{
	[Serializable]
	public class Expression : ISerializationCallbackReceiver
	{
		[SerializeField] private string _content;
		protected IOperation _operation;

		public string Content
		{
			get => _content;
			set { _content = value; Compile(); }
		}

		public virtual bool IsValid => _operation != null;
		public virtual Lexer Lexer => Lexer.Default;
		public virtual Parser Parser => Parser.Default;

		public Variable Execute(IVariableDictionary variables)
		{
			return IsValid
				? _operation.Evaluate(variables)
				: Variable.Empty;
		}

		public Variable Execute(IVariableDictionary variables, VariableType expectedType)
		{
			var variable = Execute(variables);

			if (!variable.Is(expectedType))
				throw new VariableSourceException();

			return variable;
		}

		public ExpectedType Execute<ExpectedType>(IVariableDictionary variables)
		{
			var variable = Execute(variables);

			if (!variable.Is<ExpectedType>())
				throw new VariableSourceException();

			return variable.As<ExpectedType>();
		}

		public Variable Execute(IVariableDictionary variables, Variable thisObject)
		{
			variables.AddVariable("this", thisObject);
			var variable = Execute(variables);
			variables.RemoveVariable("this");

			return variable;
		}

		public Variable Execute(IVariableDictionary variables, VariableType expectedType, Variable thisObject)
		{
			variables.AddVariable("this", thisObject);
			var variable =  Execute(variables, expectedType);
			variables.RemoveVariable("this");

			return variable;
		}

		public ExpectedType Execute<ExpectedType>(IVariableDictionary variables, Variable thisObject)
		{
			variables.AddVariable("this", thisObject);
			var variable = Execute<ExpectedType>(variables);
			variables.RemoveVariable("this");

			return variable;
		}

		private void Compile()
		{
			_operation = null; // Cleared in case an exception is thrown.

			if (!string.IsNullOrEmpty(_content))
			{
				var tokens = Lexer.Tokenize(_content, false);
				_operation = Parser.Parse(tokens);
			}
		}

		#region ISerializationCallbackReceiver Implementation

		public void OnBeforeSerialize() { }
		public void OnAfterDeserialize() => Compile();

		#endregion
	}

	[Serializable]
	public class ReadOnlyExpression : Expression
	{
		public override Parser Parser => Parser.ReadOnly;
	}

	[Serializable]
	public class AssignmentExpression : Expression
	{
		public override bool IsValid => _operation is AssignOperator;
		public override Parser Parser => Parser.Assignment;

		public void Assign(IVariableDictionary variables, Variable value)
		{
			variables.AddVariable("value", value);
			_operation.Evaluate(variables);
			variables.RemoveVariable("value");
		}

		public void Assign(IVariableDictionary variables, Variable value, Variable thisObject)
		{
			variables.AddVariable("this", thisObject);
			Assign(variables, value);
			variables.RemoveVariable("this");
		}
	}

	[Serializable]
	public class StringExpression : Expression
	{
		// TODO: Figure out how to make this not have to include ` at the start and end.
		public override Parser Parser => Parser.String;
	}
}
