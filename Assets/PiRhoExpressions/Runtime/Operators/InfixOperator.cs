using System.Text;

namespace PiRhoSoft.Expressions
{
	public interface IInfixOperator : IOperation
	{
		void Parse(IParseContext parser, Token token, IOperation left, Precedence precedence);
	}

	public abstract class InfixOperator : Operator, IInfixOperator
	{
		public IOperation Left { get; private set; }
		public string Symbol { get; private set; }
		public IOperation Right { get; private set; }

		public virtual void Parse(IParseContext parser, Token token, IOperation left, Precedence precedence)
		{
			Left = left;
			Symbol = token.Text;
			Right = parser.Parse(precedence.Right);
		}

		public override void Print(StringBuilder printer)
		{
			Left.Print(printer);
			printer.Append(Symbol);
			Right.Print(printer);
		}
	}
}
