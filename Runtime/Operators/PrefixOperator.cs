using System.Text;

namespace PiRhoSoft.Expressions
{
	public interface IPrefixOperator : IOperation
	{
		void Parse(IParseContext parser, Token token);
	}

	public abstract class PrefixOperator : Operator, IPrefixOperator
	{
		public string Symbol { get; private set; }
		public IOperation Right { get; private set; }

		public virtual void Parse(IParseContext parser, Token token)
		{
			Symbol = token.Text;
			Right = parser.Parse(Precedence.Prefix.Left);
		}

		public override void Print(StringBuilder printer)
		{
			printer.Append(Symbol);
			Right.Print(printer);
		}
	}
}
