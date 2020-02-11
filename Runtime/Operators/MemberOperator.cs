using PiRhoSoft.Variables;
using System.Text;

namespace PiRhoSoft.Expressions
{
	public abstract class MemberOperator : Operator, IInfixOperator
	{
		public IOperation Left { get; private set; }
		public string Symbol { get; private set; }

		public virtual void Parse(IParseContext parser, Token token, IOperation left, Precedence precedence)
		{
			Left = left;
			Symbol = token.Text;
			ParseMember(parser);
		}

		public override Variable Evaluate(IVariableDictionary variables)
		{
			var left = Left.Evaluate(variables);
			var right = GetMember(variables);
			var value = Variable.Lookup(left, right);

			if (value.IsEmpty)
				throw new MemberNotFoundException(left, right);

			return value;
		}

		public override void Print(StringBuilder printer)
		{
			Left.Print(printer);
			printer.Append(Symbol);
		}

		protected abstract void ParseMember(IParseContext parser);
		protected abstract Variable GetMember(IVariableDictionary variables);
		protected abstract void PrintMember(StringBuilder printer);
	}

	public class AccessOperator : MemberOperator
	{
		private string _member;

		protected override void ParseMember(IParseContext parser)
		{
			var token = parser.TakeToken();

			if (token.Type != TokenType.Identifier)
				throw new UnexpectedTokenException(token, TokenType.Identifier);

			_member = token.Text;
		}

		protected override Variable GetMember(IVariableDictionary variables) => Variable.String(_member);
		protected override void PrintMember(StringBuilder printer) => printer.Append(_member);
	}

	public class LookupOperator : MemberOperator
	{
		private string _endSymbol;
		private IOperation _right;

		public LookupOperator(string endSymbol)
		{
			_endSymbol = endSymbol;
		}

		protected override void ParseMember(IParseContext parser)
		{
			_right = parser.Parse(Precedence.Default.Left);

			var token = parser.TakeToken();

			if (token.Type != TokenType.Operator || token.Text != _endSymbol)
				throw new UnexpectedTokenException(token, _endSymbol);
		}

		protected override Variable GetMember(IVariableDictionary variables) => _right.Evaluate(variables);
		protected override void PrintMember(StringBuilder printer) => _right.Print(printer);
	}

	public class MemberNotFoundException : OperationException
	{
		private const string _message = "the member '{0}' could not be found on type {1}";
		public MemberNotFoundException(Variable owner, Variable member) : base(_message, member, owner.IsObject ? owner.ObjectType.Name : owner.Type.ToString()) { }
	}
}
