namespace PiRhoSoft.Expressions
{
	public struct Precedence
	{
		public static Precedence Default = LeftAssociative(0);
		public static Precedence Assignment = RightAssociative(10);
		public static Precedence Ternary = RightAssociative(20);
		public static Precedence Or = LeftAssociative(30);
		public static Precedence And = LeftAssociative(40);
		public static Precedence Equality = LeftAssociative(50);
		public static Precedence Comparison = LeftAssociative(60);
		public static Precedence Addition = LeftAssociative(70);
		public static Precedence Multiplication = LeftAssociative(80);
		public static Precedence Exponentiation = RightAssociative(90);
		public static Precedence Prefix = LeftAssociative(100);
		public static Precedence Postfix = LeftAssociative(110);
		public static Precedence Execute = LeftAssociative(150);
		public static Precedence MemberAccess = LeftAssociative(200);

		public static Precedence LeftAssociative(int value) => new Precedence { Left = value, Right = value };
		public static Precedence RightAssociative(int value) => new Precedence { Left = value, Right = value - 1 };

		public int Left { get; private set; }
		public int Right { get; private set; }
	}
}
