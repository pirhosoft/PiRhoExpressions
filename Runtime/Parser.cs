using PiRhoSoft.Utilities;
using PiRhoSoft.Variables;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PiRhoSoft.Expressions
{
	public interface IParseContext
	{
		IOperation Parse(int precedence);
		Token ViewToken();
		Token TakeToken();
	}

	public class Parser
	{
		#region Default

		public static readonly Dictionary<string, Variable> DefaultConstants = new Dictionary<string, Variable>
		{
			{ "null", Variable.Object(null) },
			{ "true", Variable.Bool(true) },
			{ "false", Variable.Bool(false) }
		};

		public static readonly Dictionary<string, Func<Variable>> DefaultVariables = new Dictionary<string, Func<Variable>>
		{
			{ "Time", () => Variable.Float(Time.time) },
			{ "Realtime", () => Variable.Float(Time.realtimeSinceStartup) },
			{ "UnscaledTime", () => Variable.Float(Time.unscaledTime) }
		};

		public static readonly Dictionary<string, IVariableFunction> DefaultCommands = new Dictionary<string, IVariableFunction>
		{
			{ "Ceil", new VariableFunction(new Func<float, float>(Mathf.Ceil)) },
			{ "CeilToInt", new VariableFunction(new Func<float, int>(Mathf.CeilToInt)) },
			{ "Clamp", new VariableOverload(new Func<float, float, float, float>(Mathf.Clamp), new Func<int, int, int, int>(Mathf.Clamp)) },
			{ "Floor", new VariableFunction(new Func<float, float>(Mathf.Floor)) },
			{ "FloorToInt", new VariableFunction(new Func<float, int>(Mathf.FloorToInt)) },
			{ "Max", new VariableOverload(new Func<float, float, float>(Mathf.Max), new Func<int, int, int>(Mathf.Max)) },
			{ "Min", new VariableOverload(new Func<float, float, float>(Mathf.Min), new Func<int, int, int>(Mathf.Min)) },
			{ "Random", new VariableOverload(new Func<float>(() => Random.value), new Func<float, float, float>(Random.Range), new Func<int, int, int>(Random.Range)) },
			{ "Round", new VariableFunction(new Func<float, float>(Mathf.Round)) },
			{ "RoundToInt", new VariableFunction(new Func<float, int>(Mathf.RoundToInt)) },
			{ "Sign", new VariableFunction(new Func<float, float>(Mathf.Sign)) }
		};

		public static readonly Dictionary<string, IVariableFunction> DefaultConstructors = new Dictionary<string, IVariableFunction>
		{
			{ "Vector2", new VariableOverload(
				new Func<Vector2>(() => new Vector2()),
				new Func<float, float, Vector2>((x, y) => new Vector2(x, y))
			)},
			{ "Vector3", new VariableOverload(
				new Func<Vector3>(() => new Vector3()),
				new Func<float, float, float, Vector3>((x, y, z) => new Vector3(x, y, z))
			)},
			{ "Vector4", new VariableOverload(
				new Func<Vector4>(() => new Vector4()),
				new Func<float, float, float, Vector4>((x, y, z) => new Vector4(x, y, z)),
				new Func<float, float, float, float, Vector4>((x, y, z, w) => new Vector4(x, y, z, w))
			)},
			{ "Quaternion", new VariableOverload(
				new Func<Quaternion>(() => new Quaternion()),
				new Func<float, float, float, float, Quaternion>((x, y, z, w) => new Quaternion(x, y, z, w))
			)},
			{ "Rect", new VariableOverload(
				new Func<Rect>(() => new Rect()),
				new Func<Vector2, Vector2, Rect>((position, size) => new Rect(position, size)),
				new Func<float, float, float, float, Rect>((x, y, w, h) => new Rect(x, y, w, h))
			)},
			{ "Bounds", new VariableOverload(
				new Func<Bounds>(() => new Bounds()),
				new Func<Vector3, Vector3, Bounds>((center, size) => new Bounds(center, size))
			)},
			{ "Vector2Int", new VariableOverload(
				new Func<Vector2Int>(() => new Vector2Int()),
				new Func<int, int, Vector2Int>((x, y) => new Vector2Int(x, y))
			)},
			{ "Vector3Int", new VariableOverload(
				new Func<Vector3Int>(() => new Vector3Int()),
				new Func<int, int, int, Vector3Int>((x, y, z) => new Vector3Int(x, y, z))
			)},
			{ "RectInt", new VariableOverload(
				new Func<RectInt>(() => new RectInt()),
				new Func<Vector2Int, Vector2Int, RectInt>((position, size) => new RectInt(position, size)),
				new Func<int, int, int, int, RectInt>((x, y, w, h) => new RectInt(x, y, w, h))
			)},
			{ "BoundsInt", new VariableOverload(
				new Func<BoundsInt>(() => new BoundsInt()),
				new Func<Vector3Int, Vector3Int, BoundsInt>((center, size) => new BoundsInt(center, size))
			)},
			{ "Color", new VariableOverload(
				new Func<Color>(() => new Color()),
				new Func<float, float, float, Color>((r, g, b) => new Color(r, g, b)),
				new Func<float, float, float, float, Color>((r, g, b, a) => new Color(r, g, b, a))
			)}
		};

		public static readonly Dictionary<string, Variable> MathConstants = new Dictionary<string, Variable>
		{
			{ "Pi", Variable.Float(Mathf.PI) },
			{ "Deg2Rad", Variable.Float(Mathf.Deg2Rad) },
			{ "Rad2Deg", Variable.Float(Mathf.Rad2Deg) }
		};

		public static readonly Dictionary<string, IVariableFunction> MathCommands = new Dictionary<string, IVariableFunction>
		{
			{ "Abs", new VariableOverload(new Func<int, int>(Mathf.Abs), new Func<float, float>(Mathf.Abs)) },
			{ "Acos", new VariableFunction(new Func<float, float>(Mathf.Acos)) },
			{ "Asin", new VariableFunction(new Func<float, float>(Mathf.Asin)) },
			{ "ATan", new VariableFunction(new Func<float, float>(Mathf.Atan)) },
			{ "Cos", new VariableFunction(new Func<float, float>(Mathf.Cos)) },
			{ "Lerp", new VariableFunction(new Func<float, float, float, float>(Mathf.Lerp)) },
			{ "Log", new VariableOverload(new Func<float, float>(Mathf.Log), new Func<float, float, float>(Mathf.Log)) },
			{ "Log10", new VariableFunction(new Func<float, float>(Mathf.Log10)) },
			{ "Pow", new VariableFunction(new Func<float, float, float>(Mathf.Pow)) },
			{ "Sin", new VariableFunction(new Func<float, float>(Mathf.Sin)) },
			{ "Sqrt", new VariableFunction(new Func<float, float>(Mathf.Sqrt)) },
			{ "Tan", new VariableFunction(new Func<float, float>(Mathf.Tan)) },
		};

		public static readonly Dictionary<string, Func<IPrefixOperator>> DefaultPrefixOperators = new Dictionary<string, Func<IPrefixOperator>>
		{
			{ "`", () => new InterpolateOperator("{", "}", "`") },
			{ "(", () => new GroupOperator(")") },
			{ "-", () => new NegateOperator() },
			{ "++", () => new IncrementOperator() },
			{ "--", () => new DecrementOperator() },
			{ "!", () => new InvertOperator() }
		};

		public static readonly Dictionary<string, (Precedence Precedence, Func<IInfixOperator> Creator)> DefaultInfixOperators = new Dictionary<string, (Precedence, Func<IInfixOperator>)>
		{
			{ "+", (Precedence.Addition, () => new AddOperator()) },
			{ "-", (Precedence.Addition, () => new SubtractOperator()) },
			{ "*", (Precedence.Multiplication, () => new MultiplyOperator()) },
			{ "/", (Precedence.Multiplication, () => new DivideOperator()) },
			{ "%", (Precedence.Multiplication, () => new ModuloOperator()) },
			{ "^", (Precedence.Exponentiation, () => new ExponentOperator()) },
			{ "++", (Precedence.Postfix, () => new PostIncrementOperator()) },
			{ "--", (Precedence.Postfix, () => new PostDecrementOperator()) },
			{ "&&", (Precedence.And, () => new AndOperator()) },
			{ "||", (Precedence.Or, () => new OrOperator()) },
			{ "?", (Precedence.Ternary, () => new TernaryOperator(":")) },
			{ "==", (Precedence.Equality, () => new EqualOperator()) },
			{ "!=", (Precedence.Equality, () => new InequalOperator()) },
			{ "<", (Precedence.Comparison, () => new LessOperator()) },
			{ ">", (Precedence.Comparison, () => new GreaterOperator()) },
			{ "<=", (Precedence.Comparison, () => new LessOrEqualOperator()) },
			{ ">=", (Precedence.Comparison, () => new GreaterOrEqualOperator()) },
			{ ".", (Precedence.MemberAccess, () => new AccessOperator()) },
			{ "[", (Precedence.MemberAccess, () => new LookupOperator("]")) },
			{ "(", (Precedence.Execute, () => new CallOperator(",", ")")) }
		};

		public static readonly Dictionary<string, (Precedence Precedence, Func<IInfixOperator> Creator)> AssignmentOperators = new Dictionary<string, (Precedence, Func<IInfixOperator>)>
		{
			{ "=", (Precedence.Assignment, () => new AssignOperator()) },
			{ "+=", (Precedence.Assignment, () => new AddAssignOperator()) },
			{ "-=", (Precedence.Assignment, () => new SubtractAssignOperator()) },
			{ "*=", (Precedence.Assignment, () => new MultiplyAssignOperator()) },
			{ "/=", (Precedence.Assignment, () => new DivideAssignOperator()) },
			{ "%=", (Precedence.Assignment, () => new ModuloAssignOperator()) },
			{ "^=", (Precedence.Assignment, () => new ExponentAssignOperator()) },
			{ "&=", (Precedence.Assignment, () => new AndAssignOperator()) },
			{ "|=", (Precedence.Assignment, () => new OrAssignOperator()) },
		};

		public static readonly Parser Default = CreateDefault();

		private static Parser CreateDefault()
		{
			var parser = new Parser();
			parser.AddDefaultConstants();
			parser.AddDefaultVariables();
			parser.AddDefaultCommands();
			parser.AddDefaultConstructors();
			parser.AddDefaultOperators();
			parser.AddMathConstants();
			parser.AddMathCommands();
			parser.AddAssignmentOperators();
			return parser;
		}

		#endregion

		#region Read Only

		public static readonly Parser ReadOnly = CreateReadOnly();

		private static Parser CreateReadOnly()
		{
			var parser = new Parser();
			parser.AddDefaultConstants();
			parser.AddDefaultVariables();
			parser.AddDefaultCommands();
			parser.AddDefaultConstructors();
			parser.AddDefaultOperators();
			parser.AddMathConstants();
			parser.AddMathCommands();
			return parser;
		}

		#endregion

		#region Assignment

		public static readonly Parser Assignment = CreateAssignment();

		private static Parser CreateAssignment()
		{
			var parser = new Parser();
			parser.AddDefaultConstants();
			parser.AddDefaultVariables();
			parser.AddDefaultCommands();
			parser.AddDefaultConstructors();
			parser.AddDefaultOperators();
			parser.AddMathConstants();
			parser.AddMathCommands();
			parser.AddAssignmentOperators();
			return parser;
		}

		#endregion

		#region String Interpolate

		public static readonly Parser String = CreateString();

		private static Parser CreateString()
		{
			var parser = new Parser();
			parser.AddDefaultConstants();
			parser.AddDefaultVariables();
			parser.AddDefaultOperators();
			parser.AddMathConstants();
			parser.AddMathCommands();
			return parser;
		}

		#endregion

		#region Constants

		private Dictionary<string, Variable> _constants = new Dictionary<string, Variable>();
		private Dictionary<string, Func<Variable>> _variables = new Dictionary<string, Func<Variable>>();

		public void AddDefaultConstants()
		{
			foreach (var constant in DefaultConstants)
				SetConstant(constant.Key, constant.Value);
		}

		public void AddDefaultVariables()
		{
			foreach (var variable in DefaultVariables)
				SetVariable(variable.Key, variable.Value);
		}

		public void AddDefaultCommands()
		{
			foreach (var command in DefaultCommands)
				SetConstant(command.Key, Variable.Function(command.Value));
		}

		public void AddDefaultConstructors()
		{
			foreach (var constructor in DefaultConstructors)
				SetConstant(constructor.Key, Variable.Function(constructor.Value));
		}

		public void AddMathConstants()
		{
			foreach (var constant in MathConstants)
				SetConstant(constant.Key, constant.Value);
		}

		public void AddMathCommands()
		{
			foreach (var command in MathCommands)
				SetConstant(command.Key, Variable.Function(command.Value));
		}

		public void SetConstant(string name, Variable value)
		{
			_constants[name] = value;
		}

		public void SetVariable(string name, Func<Variable> variable)
		{
			_variables[name] = variable;
		}

		#endregion

		#region Operators

		private Dictionary<string, ClassPool<IPrefixOperator>> _prefixOperators = new Dictionary<string, ClassPool<IPrefixOperator>>();
		private Dictionary<string, ClassPool<IInfixOperator>> _infixOperators = new Dictionary<string, ClassPool<IInfixOperator>>();
		private Dictionary<string, Precedence> _precedences = new Dictionary<string, Precedence>();

		public void AddDefaultOperators()
		{
			foreach (var pool in DefaultPrefixOperators)
				SetPrefixOperator(pool.Key, pool.Value);

			foreach (var pool in DefaultInfixOperators)
				SetInfixOperator(pool.Key, pool.Value.Precedence, pool.Value.Creator);
		}

		public void AddAssignmentOperators()
		{
			foreach (var pool in AssignmentOperators)
				SetInfixOperator(pool.Key, pool.Value.Precedence, pool.Value.Creator);
		}

		public void SetPrefixOperator(string symbol, Func<IPrefixOperator> creator)
		{
			_prefixOperators[symbol] = new ClassPool<IPrefixOperator>(creator);
		}

		public void SetInfixOperator(string symbol, Precedence precedence, Func<IInfixOperator> creator)
		{
			_infixOperators[symbol] = new ClassPool<IInfixOperator>(creator);
			_precedences[symbol] = precedence;
		}

		#endregion

		#region ParseContext Implementation

		private class LiteralOperation : IPrefixOperator
		{
			private Token _token;

			public void Parse(IParseContext parser, Token token) => _token = token;
			public Variable Evaluate(IVariableDictionary variables) => _token.Value;
			public void Print(StringBuilder printer) => printer.Append(_token.Text);
			public override string ToString() => _token.Text;
		}

		private class ConstantOperation : IPrefixOperator
		{
			private Token _token;
			private Variable _value;

			public ConstantOperation(Variable value) => _value = value;
			public void Parse(IParseContext parser, Token token) => _token = token;
			public Variable Evaluate(IVariableDictionary variables) => _value;
			public void Print(StringBuilder printer) => printer.Append(_token.Text);
			public override string ToString() => _token.Text;
		}

		private class VariableOperation : IPrefixOperator
		{
			private Token _token;
			private Func<Variable> _function;

			public VariableOperation(Func<Variable> function) => _function = function;
			public void Parse(IParseContext parser, Token token) => _token = token;
			public Variable Evaluate(IVariableDictionary variables) => _function();
			public void Print(StringBuilder printer) => printer.Append(_token.Text);
			public override string ToString() => _token.Text;
		}

		private class IdentifierOperation : IPrefixOperator, IAssignableOperation
		{
			private Token _token;

			public IdentifierOperation(Token token) => _token = token;
			public void Parse(IParseContext parser, Token token) { }
			public Variable Evaluate(IVariableDictionary variables) => variables.GetVariable(_token.Text);
			public SetVariableResult Assign(IVariableDictionary variables, Variable value) => variables.SetVariable(_token.Text, value);
			public void Print(StringBuilder printer) => printer.Append(_token.Text);
			public override string ToString() => _token.Text;
		}

		private IPrefixOperator CreateIdentifier(Token token)
		{
			if (_constants.TryGetValue(token.Text, out var constant))
				return new ConstantOperation(constant);
			else if (_variables.TryGetValue(token.Text, out var variable))
				return new VariableOperation(variable);
			else
				return new IdentifierOperation(token);
		}

		private IPrefixOperator CreateLiteral()
		{
			return new LiteralOperation();
		}

		private IPrefixOperator CreatePrefixOperator(Token token)
		{
			if (_prefixOperators.TryGetValue(token.Text, out var pool))
				return pool.Reserve();
			else
				return null;
		}

		private IInfixOperator CreateInfixOperator(Token token)
		{
			if (_infixOperators.TryGetValue(token.Text, out var pool))
				return pool.Reserve();
			else
				return null;
		}

		private class ParseContext : IParseContext
		{
			public Parser Parser;
			public List<Token> Tokens;
			public int Index;

			public IOperation Parse(int precedence)
			{
				var operation = ParsePrefix();

				while (precedence < NextPrecedence())
					operation = ParseInfix(operation);

				return operation;
			}

			private int NextPrecedence()
			{
				var token = ViewToken();

				if (token == null)
					return int.MinValue;
				else if (token.Type == TokenType.Operator)
					return Parser._precedences.TryGetValue(token.Text, out var precedence) ? precedence.Left : int.MinValue;
				else
					return int.MaxValue;
			}

			private IOperation ParsePrefix()
			{
				var token = TakeToken();
				var operation = CreatePrefixOperation(token);

				if (operation == null)
					throw new InvalidTokenException(token);

				operation.Parse(this, token);

				return operation;
			}

			private IOperation ParseInfix(IOperation left)
			{
				var token = TakeToken();
				var operation = CreateInfixOperation(token);

				if (operation == null)
					throw new InvalidTokenException(token);

				Parser._precedences.TryGetValue(token.Text, out var precedence);
				operation.Parse(this, token, left, precedence);

				return operation;
			}

			private IPrefixOperator CreatePrefixOperation(Token token)
			{
				switch (token.Type)
				{
					case TokenType.Literal: return Parser.CreateLiteral();
					case TokenType.Identifier: return Parser.CreateIdentifier(token);
					case TokenType.Operator: return Parser.CreatePrefixOperator(token);
					default: return null;
				}
			}

			private IInfixOperator CreateInfixOperation(Token token)
			{
				switch (token.Type)
				{
					case TokenType.Operator: return Parser.CreateInfixOperator(token);
					default: return null;
				}
			}

			public Token ViewToken()
			{
				while (Index < Tokens.Count && Tokens[Index].IsIgnored)
					Index++;

				return Index < Tokens.Count ? Tokens[Index] : null;
			}

			public Token TakeToken()
			{
				while (Index < Tokens.Count && Tokens[Index].IsIgnored)
					Index++;

				if (Index >= Tokens.Count)
					throw new UnexpectedEndException();

				return Tokens[Index++];
			}
		}

		#endregion

		public IOperation Parse(List<Token> tokens)
		{
			var context = new ParseContext
			{
				Parser = this,
				Tokens = tokens,
				Index = 0
			};

			return context.Parse(Precedence.Default.Left);
		}
	}

	public class ParserException : Exception
	{
		public ParserException(string error) : base(error) { }
		public ParserException(string errorFormat, params object[] arguments) : this(string.Format(errorFormat, arguments)) { }
	}

	public class InvalidTokenException : ParserException
	{
		private const string _message = "unable to find an operation to handle '{0}'";
		public InvalidTokenException(Token token) : base(_message, token.Text) { }
	}

	public class UnexpectedTokenException : ParserException
	{
		private const string _messageType = "expected a '{0}' instead of a '{1}'";
		private const string _messageText = "expected '{0}' instead of a '{1}'";

		public UnexpectedTokenException(Token token, TokenType expectedType) : base(_messageType, expectedType, token.Type) { }
		public UnexpectedTokenException(Token token, string expectedText) : base(_messageText, expectedText, token.Text) { }
	}

	public class UnexpectedEndException : ParserException
	{
		private const string _message = "expression is incomplete";
		public UnexpectedEndException() : base(_message) { }
	}
}
