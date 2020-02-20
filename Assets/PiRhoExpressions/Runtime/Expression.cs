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

		// TODO: Overload with 'this' that is assigned to a wrapper dictionary. Also 'value' as overload on
		// AssignmentExpression handled the same way

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
		public override bool IsValid => _operation is AssignmentOperator;
		public override Parser Parser => Parser.Assignment;
	}

	[Serializable]
	public class StringExpression : Expression
	{
		// TODO: Figure out how to make this not half to include ` at the start and end.
		public override Parser Parser => Parser.String;
	}
}
