using System.Collections.Immutable;
using GuildScript.Analysis.Semantics.Symbols;
using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public abstract class ResolvedExpression : ResolvedNode
{
	public abstract ResolvedType? Type { get; }

	public interface IVisitor
	{
		void VisitAwaitExpression(Await expression);
		void VisitConditionalExpression(Conditional expression);
		void VisitBinaryExpression(Binary expression);
		void VisitTypeRelationExpression(TypeRelation expression);
		void VisitUnaryExpression(Unary expression);
		void VisitIdentifierExpression(Identifier expression);
		void VisitQualifierExpression(Qualifier expression);
		void VisitCallExpression(Call expression);
		void VisitLiteralExpression(Literal expression);
		void VisitInstantiateExpression(Instantiate expression);
		void VisitCastExpression(Cast expression);
		void VisitIndexExpression(Index expression);
		void VisitLambdaExpression(Lambda expression);
	}

	public abstract void AcceptVisitor(IVisitor visitor);

	public sealed class Await : ResolvedExpression
	{
		public override ResolvedType? Type => Expression.Type;

		public ResolvedExpression Expression { get; }

		public Await(ResolvedExpression expression)
		{
			Expression = expression;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitAwaitExpression(this);
		}
	}

	public sealed class Conditional : ResolvedExpression
	{
		public override ResolvedType? Type { get; }

		public ResolvedExpression Condition { get; }
		public ResolvedExpression TrueExpression { get; }
		public ResolvedExpression FalseExpression { get; }

		public Conditional(ResolvedExpression condition, ResolvedExpression trueExpression,
			ResolvedExpression falseExpression)
		{
			Condition = condition;
			TrueExpression = trueExpression;
			FalseExpression = falseExpression;

			Type = ResolveType();
		}

		private ResolvedType? ResolveType()
		{
			// True = False
			if (TrueExpression.Type == FalseExpression.Type)
				return TrueExpression.Type;

			// True = False?
			if (TrueExpression.Type is NullableResolvedType nullableTrueType)
			{
				if (nullableTrueType.BaseType == FalseExpression.Type)
					return nullableTrueType;
			}

			// True? = False
			if (FalseExpression.Type is NullableResolvedType nullableFalseType)
			{
				if (nullableFalseType.BaseType == TrueExpression.Type)
					return nullableFalseType;
			}

			// True = null, False = Type or Type? -> return Type?
			if (TrueExpression.Type is null)
			{
				switch (FalseExpression.Type)
				{
					case NullableResolvedType nullType:
						return nullType;
					case { } type:
						return new NullableResolvedType(type);
				}
			}

			// False = null, True = Type or Type? -> return Type?
			if (FalseExpression.Type is null)
			{
				switch (TrueExpression.Type)
				{
					case NullableResolvedType nullType:
						return nullType;
					case { } type:
						return new NullableResolvedType(type);
				}
			}

			return null;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitConditionalExpression(this);
		}
	}

	public sealed class Binary : ResolvedExpression
	{
		public override ResolvedType? Type { get; }

		public ResolvedExpression Left { get; }
		public BinaryOperator Operator { get; }
		public ResolvedExpression Right { get; }

		public Binary(ResolvedExpression left, BinaryOperator @operator, ResolvedExpression right, ResolvedType? type)
		{
			Left = left;
			Operator = @operator;
			Right = right;
			Type = type;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitBinaryExpression(this);
		}
	}

	public sealed class TypeRelation : ResolvedExpression
	{
		public override ResolvedType Type => SimpleResolvedType.Bool;

		public ResolvedExpression Operand { get; }
		public BinaryOperator Operator { get; }
		public ResolvedType? TypeQuery { get; }
		public LocalVariableSymbol? VariableSymbol { get; }

		public TypeRelation(ResolvedExpression operand, BinaryOperator @operator, ResolvedType? typeQuery,
							LocalVariableSymbol? variableSymbol)
		{
			Operand = operand;
			Operator = @operator;
			TypeQuery = typeQuery;
			VariableSymbol = variableSymbol;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitTypeRelationExpression(this);
		}
	}

	public sealed class Unary : ResolvedExpression
	{
		public ResolvedExpression Operand { get; }
		public UnaryOperator Operator { get; }
		public override ResolvedType? Type { get; }
		public bool IsPostfix { get; }
		public MethodSymbol? OperatorMethod { get; }

		public Unary(ResolvedExpression operand, UnaryOperator @operator, ResolvedType? type,
					 MethodSymbol? operatorMethod, bool isPostfix = false)
		{
			Operand = operand;
			Operator = @operator;
			Type = type;
			OperatorMethod = operatorMethod;
			IsPostfix = isPostfix;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitUnaryExpression(this);
		}
	}

	public sealed class Identifier : ResolvedExpression
	{
		public override ResolvedType Type { get; }
		public Symbol Symbol { get; }

		public Identifier(ResolvedType type, Symbol symbol)
		{
			Type = type;
			Symbol = symbol;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitIdentifierExpression(this);
		}

		public override string ToString()
		{
			return Symbol.Name;
		}
	}

	public sealed class Qualifier : ResolvedExpression
	{
		public override ResolvedType Type { get; }
		public Symbol Symbol { get; }

		public Qualifier(ResolvedType type, Symbol symbol)
		{
			Type = type;
			Symbol = symbol;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitQualifierExpression(this);
		}

		public override string ToString()
		{
			return Symbol.Name;
		}
	}

	public sealed class Call : ResolvedExpression
	{
		public override ResolvedType? Type => Function.ReturnType;
		public ICallable Function { get; }
		public ImmutableArray<TypeSymbol> TemplateArguments { get; }
		public ImmutableArray<ResolvedExpression> Arguments { get; }

		public Call(ICallable function, IEnumerable<TypeSymbol> templateArguments,
					IEnumerable<ResolvedExpression> arguments)
		{
			Function = function;
			TemplateArguments = templateArguments.ToImmutableArray();
			Arguments = arguments.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitCallExpression(this);
		}
	}

	public sealed class Literal : ResolvedExpression
	{
		public override ResolvedType? Type { get; }
		public SyntaxToken Token { get; }

		public Literal(SyntaxToken token, ResolvedType? type)
		{
			Token = token;
			Type = type;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitLiteralExpression(this);
		}
	}

	public sealed class Instantiate : ResolvedExpression
	{
		public override ResolvedType Type => InstanceType;
		public ResolvedType InstanceType { get; }
		public ImmutableArray<ResolvedExpression> Arguments { get; }
		public ImmutableArray<ResolvedExpression> Initializers { get; }

		public Instantiate(ResolvedType instanceType, IEnumerable<ResolvedExpression> arguments,
						   IEnumerable<ResolvedExpression> initializers)
		{
			InstanceType = instanceType;
			Arguments = arguments.ToImmutableArray();
			Initializers = initializers.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitInstantiateExpression(this);
		}
	}

	public sealed class Cast : ResolvedExpression
	{
		public override ResolvedType Type => TargetType;
		public ResolvedExpression Expression { get; }
		public ResolvedType TargetType { get; }
		public bool IsConditional { get; }
		public MethodSymbol? CastMethod { get; }

		public Cast(ResolvedExpression expression, ResolvedType targetType, bool isConditional, MethodSymbol? castMethod)
		{
			Expression = expression;
			TargetType = targetType;
			IsConditional = isConditional;
			CastMethod = castMethod;

			if (IsConditional && TargetType is not NullableResolvedType)
			{
				TargetType = new NullableResolvedType(TargetType);
			}
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitCastExpression(this);
		}
	}

	public sealed class Index : ResolvedExpression
	{
		public override ResolvedType Type { get; }
		public IndexerSymbol? IndexerSymbol { get; }
		public ResolvedExpression Key { get; }
		public bool IsConditional { get; }

		public Index(IndexerSymbol? indexerSymbol, ResolvedExpression key, bool isConditional, ResolvedType type)
		{
			IndexerSymbol = indexerSymbol;
			Key = key;
			IsConditional = isConditional;
			Type = type;

			if (IsConditional && Type is not NullableResolvedType)
			{
				Type = new NullableResolvedType(Type);
			}
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitIndexExpression(this);
		}
	}

	public sealed class Lambda : ResolvedExpression
	{
		public override ResolvedType? Type { get; }
		public LambdaSymbol Symbol { get; }
		public ResolvedStatement Body { get; }

		public Lambda(LambdaSymbol symbol, ResolvedType? returnType, ResolvedStatement body)
		{
			Symbol = symbol;
			Type = returnType;
			Body = body;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitLambdaExpression(this);
		}
	}
}
