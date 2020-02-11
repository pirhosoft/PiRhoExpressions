using PiRhoSoft.Variables;
using System.Collections.Generic;
using System.Text;

namespace PiRhoSoft.Expressions
{
	public class CallOperator : Operator, IInfixOperator
	{
		private IOperation _command;
		private string _openSymbol;
		private List<IOperation> _parameters;
		private string _separatorSymbol = ",";
		private string _endSymbol = ")";

		private Variable[] _parameterValues;

		public CallOperator(string separatorSymbol, string endSymbol)
		{
			_separatorSymbol = separatorSymbol;
			_endSymbol = endSymbol;
		}

		public void Parse(IParseContext parser, Token token, IOperation left, Precedence precedence)
		{
			_command = left;
			_openSymbol = token.Text;
			_parameters = new List<IOperation>();

			token = parser.ViewToken();

			while (token.Type != TokenType.Operator || token.Text != _endSymbol)
			{
				var parameter = parser.Parse(Precedence.Default.Left);
				_parameters.Add(parameter);

				var separator = parser.TakeToken();
				if (separator.Type != TokenType.Operator || separator.Text != _separatorSymbol)
					throw new UnexpectedTokenException(separator, _separatorSymbol);

				token = parser.ViewToken();
			}

			parser.TakeToken();
			_parameterValues = new Variable[_parameters.Count];
		}

		public override Variable Evaluate(IVariableDictionary variables)
		{
			var command = _command.Evaluate(variables);

			if (!command.IsFunction)
				throw new InvalidCallException(_command);

			for (var i = 0; i < _parameters.Count; i++)
				_parameterValues[i] = _parameters[i].Evaluate(variables);

			return command.AsFunction.Invoke(_parameterValues);
		}

		public override void Print(StringBuilder printer)
		{
			_command.Print(printer);
			printer.Append(_openSymbol);

			for (var i = 0; i < _parameters.Count; i++)
			{
				if (i != 0)
					printer.Append(_separatorSymbol);

				_parameters[i].Print(printer);
			}

			printer.Append(_endSymbol);
		}
	}

	public class CallException : OperationException
	{
		public CallException(string errorFormat, params object[] arguments) : base(errorFormat, arguments) { }
	}

	public class InvalidCallException : CallException
	{
		private const string _message = "unable to call '{0}' because it is not a function";
		public InvalidCallException(IOperation target) : base(_message, target) { }
	}
}
