using System.Text;

namespace PiRhoSoft.Expressions
{
	public abstract class PostfixOperator : Operator, IInfixOperator
	{
		public IOperation Left { get; private set; }
		public string Symbol { get; private set; }

		public virtual void Parse(IParseContext parser, Token token, IOperation left, Precedence precedence)
		{
			Left = left;
			Symbol = token.Text;
		}

		public override void Print(StringBuilder printer)
		{
			Left.Print(printer);
			printer.Append(Symbol);
		}
	}
}
