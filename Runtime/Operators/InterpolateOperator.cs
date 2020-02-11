using PiRhoSoft.Variables;
using System.Collections.Generic;
using System.Text;

namespace PiRhoSoft.Expressions
{
	public class InterpolateOperator : IPrefixOperator
	{
		private string _startExpressionSymbol;
		private string _endExpressionSymbol;
		private string _closeSymbol;

		private string _format;
		private IOperation[] _operations;
		private string[] _results;

		public InterpolateOperator(string startExpressionSymbol, string endExpressionSymbol, string closeSymbol)
		{
			_startExpressionSymbol = startExpressionSymbol;
			_endExpressionSymbol = endExpressionSymbol;
			_closeSymbol = closeSymbol;
		}

		public void Parse(IParseContext parser, Token token)
		{
			var builder = new StringBuilder();
			var operations = new List<IOperation>();

			token = parser.TakeToken();

			while (token.Type != TokenType.Operator || token.Text != _closeSymbol)
			{
				if (token.Type == TokenType.Literal)
				{
					builder.Append(token.Text);
				}
				else if (token.Type == TokenType.Operator && token.Text == _startExpressionSymbol)
				{
					builder.Append('{');
					builder.Append(operations.Count);
					builder.Append('}');

					var operation = ParseOperation(parser);
					operations.Add(operation);
				}
				else
				{
					throw new UnexpectedTokenException(token, _closeSymbol);
				}

				token = parser.TakeToken();
			}

			_format = builder.ToString();
			_operations = operations.ToArray();
			_results = new string[_operations.Length];
		}

		private IOperation ParseOperation(IParseContext parser)
		{
			var operation = parser.Parse(Precedence.Default.Left);
			var token = parser.TakeToken();

			if (token.Type != TokenType.Operator || token.Text != _endExpressionSymbol)
				throw new UnexpectedTokenException(token, _endExpressionSymbol);

			return operation;
		}

		public void Print(StringBuilder printer)
		{
			printer.AppendFormat(_format, _operations);
		}

		public Variable Evaluate(IVariableDictionary variables)
		{
			for (var i = 0; i < _operations.Length; i++)
			{
				_results[i] = _operations[i]
					.Evaluate(variables)
					.ToString();
			}

			var result = string.Format(_format, _results);
			return Variable.String(result);
		}
	}
}
