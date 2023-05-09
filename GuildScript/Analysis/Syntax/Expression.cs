using System.Collections.Immutable;

namespace GuildScript.Analysis.Syntax;

public abstract class Expression : SyntaxNode
{
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
	
	public interface IVisitor<out T>
	{
		T VisitAwaitExpression(Await expression);
		T VisitConditionalExpression(Conditional expression);
		T VisitBinaryExpression(Binary expression);
		T VisitTypeRelationExpression(TypeRelation expression);
		T VisitUnaryExpression(Unary expression);
		T VisitIdentifierExpression(Identifier expression);
		T VisitQualifierExpression(Qualifier expression);
		T VisitCallExpression(Call expression);
		T VisitLiteralExpression(Literal expression);
		T VisitInstantiateExpression(Instantiate expression);
		T VisitCastExpression(Cast expression);
		T VisitIndexExpression(Index expression);
		T VisitLambdaExpression(Lambda expression);
	}
	
	public abstract void AcceptVisitor(IVisitor visitor);
	public abstract T AcceptVisitor<T>(IVisitor<T> visitor);

	public sealed class Await : Expression
	{
		public Expression Expression { get; }
		
		public Await(Expression expression)
		{
			Expression = expression;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitAwaitExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitAwaitExpression(this);
		}

		public override int GetHashCode()
		{
			return Expression.GetHashCode() ^ 0xF0A8B17;
		}
	}

	public sealed class Conditional : Expression
	{
		public Expression Condition { get; }
		public Expression TrueExpression { get; }
		public Expression FalseExpression { get; }
		
		public Conditional(Expression condition, Expression trueExpression, Expression falseExpression)
		{
			Condition = condition;
			TrueExpression = trueExpression;
			FalseExpression = falseExpression;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitConditionalExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitConditionalExpression(this);
		}

		public override int GetHashCode()
		{
			var hash = 0;
			hash ^= Condition.GetHashCode();
			hash ^= TrueExpression.GetHashCode() << 1;
			hash ^= FalseExpression.GetHashCode() << 2;
			return hash;
		}
	}

	public sealed class Binary : Expression
	{
		public Expression Left { get; }
		public BinaryOperator Operator { get; }
		public Expression Right { get; }

		public Binary(Expression left, BinaryOperator @operator, Expression right)
		{
			Left = left;
			Operator = @operator;
			Right = right;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitBinaryExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitBinaryExpression(this);
		}

		public override int GetHashCode()
		{
			var hash = 0;
			hash ^= Left.GetHashCode();
			hash ^= Operator.GetHashCode() << 1;
			hash ^= Right.GetHashCode() << 2;
			return hash;
		}
	}

	public sealed class TypeRelation : Expression
	{
		public Expression Operand { get; }
		public BinaryOperator Operator { get; }
		public TypeSyntax? Type { get; }
		public SyntaxToken? IdentifierToken { get; }

		public TypeRelation(Expression operand, BinaryOperator @operator, TypeSyntax? type, SyntaxToken? identifierToken)
		{
			Operand = operand;
			Operator = @operator;
			Type = type;
			IdentifierToken = identifierToken;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitTypeRelationExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitTypeRelationExpression(this);
		}

		public override int GetHashCode()
		{
			var hash = 0;
			hash ^= Operand.GetHashCode();
			hash ^= Operator.GetHashCode() << 1;
			hash ^= Type?.GetHashCode() << 2 ?? 0;
			hash ^= IdentifierToken?.Type.GetHashCode() << 3 ?? 0;
			return hash;
		}
	}

	public sealed class Unary : Expression
	{
		public Expression Operand { get; }
		public SyntaxToken OperatorToken { get; }
		public bool IsPostfix { get; }

		public Unary(Expression operand, SyntaxToken operatorToken, bool isPostfix = false)
		{
			Operand = operand;
			OperatorToken = operatorToken;
			IsPostfix = isPostfix;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitUnaryExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitUnaryExpression(this);
		}

		public override int GetHashCode()
		{
			var hash = 0;
			hash ^= Operand.GetHashCode();
			hash ^= OperatorToken.Type.GetHashCode() << 1;
			hash ^= IsPostfix.GetHashCode() << 2;
			return hash;
		}
	}

	public sealed class Identifier : Expression
	{
		public SyntaxToken NameToken { get; }

		public Identifier(SyntaxToken nameToken)
		{
			NameToken = nameToken;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitIdentifierExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitIdentifierExpression(this);
		}

		public override string ToString()
		{
			return NameToken.Text;
		}
		
		public override int GetHashCode()
		{
			return NameToken.Text.GetHashCode();
		}
	}

	public sealed class Qualifier : Expression
	{
		public SyntaxToken NameToken { get; }

		public Qualifier(SyntaxToken nameToken)
		{
			NameToken = nameToken;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitQualifierExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitQualifierExpression(this);
		}

		public override string ToString()
		{
			return NameToken.Text;
		}
		
		public override int GetHashCode()
		{
			return NameToken.Text.GetHashCode();
		}
	}

	public sealed class Call : Expression
	{
		public Expression Function { get; }
		public ImmutableArray<Expression> TemplateArguments { get; }
		public ImmutableArray<Expression> Arguments { get; }
		
		public Call(Expression function, IEnumerable<Expression> templateArguments, IEnumerable<Expression> arguments)
		{
			Function = function;
			TemplateArguments = templateArguments.ToImmutableArray();
			Arguments = arguments.ToImmutableArray();
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitCallExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitCallExpression(this);
		}

		public override int GetHashCode()
		{
			var hash = Function.GetHashCode();

			for (var i = 0; i < TemplateArguments.Length; i++)
			{
				hash ^= TemplateArguments[i].GetHashCode() << (i + 1);
			}
			
			for (var i = 0; i < Arguments.Length; i++)
			{
				hash ^= Arguments[i].GetHashCode() << (i + 1 + TemplateArguments.Length);
			}

			return hash;
		}
	}

	public sealed class Literal : Expression
	{
		public SyntaxToken Token { get; }

		public Literal(SyntaxToken token)
		{
			Token = token;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitLiteralExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitLiteralExpression(this);
		}

		public override int GetHashCode()
		{
			return Token.Text.GetHashCode();
		}
	}

	public sealed class Instantiate : Expression
	{
		public TypeSyntax InstanceType { get; }
		public ImmutableArray<Expression> Arguments { get; }
		public ImmutableArray<Expression> Initializers { get; }
		
		public Instantiate(TypeSyntax instanceType, IEnumerable<Expression> arguments,
						   IEnumerable<Expression> initializers)
		{
			InstanceType = instanceType;
			Arguments = arguments.ToImmutableArray();
			Initializers = initializers.ToImmutableArray();
		}
		
		public Instantiate(TypeSyntax instanceType, IEnumerable<Expression> arguments)
		{
			InstanceType = instanceType;
			Arguments = arguments.ToImmutableArray();
			Initializers = ImmutableArray<Expression>.Empty;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitInstantiateExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitInstantiateExpression(this);
		}

		public override int GetHashCode()
		{
			var hash = InstanceType.GetHashCode();

			for (var i = 0; i < Initializers.Length; i++)
			{
				hash ^= Initializers[i].GetHashCode() << (i + 1);
			}
			
			for (var i = 0; i < Arguments.Length; i++)
			{
				hash ^= Arguments[i].GetHashCode() << (i + 1 + Initializers.Length);
			}

			return hash;
		}
	}

	public sealed class Cast : Expression
	{
		public Expression Expression { get; }
		public TypeSyntax TargetType { get; }
		public bool IsConditional { get; }
		
		public Cast(Expression expression, TypeSyntax targetType, bool isConditional)
		{
			Expression = expression;
			TargetType = targetType;
			IsConditional = isConditional;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitCastExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitCastExpression(this);
		}

		public override int GetHashCode()
		{
			var hash = 0;
			hash ^= Expression.GetHashCode();
			hash ^= TargetType.GetHashCode() << 1;
			hash ^= IsConditional.GetHashCode() << 2;
			return hash;
		}
	}

	public sealed class Index : Expression
	{
		public Expression Expression { get; }
		public Expression Key { get; }
		public bool IsConditional { get; }
		
		public Index(Expression expression, Expression key, bool isConditional)
		{
			Expression = expression;
			Key = key;
			IsConditional = isConditional;
		}
		
		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitIndexExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitIndexExpression(this);
		}

		public override int GetHashCode()
		{
			var hash = 0;
			hash ^= Expression.GetHashCode();
			hash ^= Key.GetHashCode() << 1;
			hash ^= IsConditional.GetHashCode() << 2;
			return hash;
		}
	}

	public sealed class Lambda : Expression
	{
		public ImmutableArray<Variable> ParameterList { get; }
		public TypeSyntax? ReturnType { get; }
		public Statement Body { get; }
		
		public Lambda(IEnumerable<Variable> parameterList, TypeSyntax? returnType, Statement body)
		{
			ParameterList = parameterList.ToImmutableArray();
			ReturnType = returnType;
			Body = body;
		}

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitLambdaExpression(this);
		}

		public override T AcceptVisitor<T>(IVisitor<T> visitor)
		{
			return visitor.VisitLambdaExpression(this);
		}

		public override int GetHashCode()
		{
			var hash = 0;
			hash ^= ReturnType?.GetHashCode() ?? 0;
			hash ^= Body.GetHashCode() << 1;

			for (var i = 0; i < ParameterList.Length; i++)
			{
				hash ^= ParameterList[i].GetHashCode() << (2 + i);
			}
			
			return hash;
		}
	}
}
