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
	
	public abstract void AcceptVisitor(IVisitor visitor);

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

		public override string ToString()
		{
			return NameToken.Text;
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

		public override string ToString()
		{
			return NameToken.Text;
		}
	}

	public sealed class Call : Expression
	{
		public Call(Expression function, IEnumerable<Expression> templateArguments, IEnumerable<Expression> arguments)
		{
			Function = function;
			TemplateArguments = templateArguments.ToImmutableArray();
			Arguments = arguments.ToImmutableArray();
		}

		public Expression Function { get; }
		public ImmutableArray<Expression> TemplateArguments { get; }
		public ImmutableArray<Expression> Arguments { get; }

		public override void AcceptVisitor(IVisitor visitor)
		{
			visitor.VisitCallExpression(this);
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
	}
}
